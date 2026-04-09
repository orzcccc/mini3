using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Mini3.Editor.ExportExcel
{
    public sealed class PlaceholderExcelParser : IExcelParser
    {
        public List<ExcelTableData> Parse(string filePath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(filePath))
            {
                List<string> sharedStrings = ReadSharedStrings(archive);
                List<SheetReference> sheetReferences = ReadSheetReferences(archive, filePath);
                List<ExcelTableData> tables = new List<ExcelTableData>(sheetReferences.Count);
                for (int i = 0; i < sheetReferences.Count; i++)
                {
                    SheetReference sheetReference = sheetReferences[i];
                    string worksheetPath = ResolveWorksheetPath(archive, sheetReference.RelationshipId);
                    tables.Add(ReadWorksheet(archive, worksheetPath, filePath, sheetReference.SheetName, sharedStrings));
                }

                return tables;
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
            List<string> results = new List<string>();
            if (entry == null)
            {
                return results;
            }

            XDocument document = LoadXml(entry);
            XNamespace ns = document.Root.Name.Namespace;
            foreach (XElement item in document.Root.Elements(ns + "si"))
            {
                IEnumerable<XElement> textNodes = item.Descendants(ns + "t");
                results.Add(string.Concat(textNodes.Select(node => node.Value)));
            }

            return results;
        }

        private static List<SheetReference> ReadSheetReferences(ZipArchive archive, string filePath)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/workbook.xml");
            XDocument document = LoadXml(entry);
            XNamespace ns = document.Root.Name.Namespace;
            XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            IEnumerable<XElement> sheetElements = document.Root.Element(ns + "sheets")?.Elements(ns + "sheet");
            if (sheetElements == null)
            {
                throw new InvalidDataException("Excel 中没有可读取的 Sheet。");
            }

            List<SheetReference> results = new List<SheetReference>();
            foreach (XElement sheetElement in sheetElements)
            {
                XAttribute relationAttribute = sheetElement.Attribute(relNs + "id");
                if (relationAttribute == null)
                {
                    throw new InvalidDataException("无法找到 Sheet 的关系 ID。");
                }

                string sheetName = (string)sheetElement.Attribute("name");
                if (string.IsNullOrWhiteSpace(sheetName))
                {
                    sheetName = Path.GetFileNameWithoutExtension(filePath);
                }

                results.Add(new SheetReference(sheetName, relationAttribute.Value));
            }

            if (results.Count == 0)
            {
                throw new InvalidDataException("Excel 中没有可读取的 Sheet。");
            }

            return results;
        }

        private static string ResolveWorksheetPath(ZipArchive archive, string relationshipId)
        {
            ZipArchiveEntry entry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            XDocument document = LoadXml(entry);
            XNamespace ns = document.Root.Name.Namespace;
            XElement relation = document.Root.Elements(ns + "Relationship").FirstOrDefault(item => (string)item.Attribute("Id") == relationshipId);
            if (relation == null)
            {
                throw new InvalidDataException($"无法解析 Sheet 关系 {relationshipId}。");
            }

            string target = (string)relation.Attribute("Target");
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new InvalidDataException($"Sheet 关系 {relationshipId} 没有目标路径。");
            }

            return "xl/" + target.Replace("\\", "/").TrimStart('/');
        }

        private static ExcelTableData ReadWorksheet(ZipArchive archive, string worksheetPath, string filePath, string sheetName, List<string> sharedStrings)
        {
            ZipArchiveEntry entry = archive.GetEntry(worksheetPath);
            if (entry == null)
            {
                throw new FileNotFoundException($"找不到工作表文件: {worksheetPath}");
            }

            XDocument document = LoadXml(entry);
            XNamespace ns = document.Root.Name.Namespace;
            XElement sheetData = document.Root.Element(ns + "sheetData");
            if (sheetData == null)
            {
                throw new InvalidDataException("工作表中没有 sheetData。");
            }

            List<List<string>> rows = ParseRows(sheetData, ns, sharedStrings);
            if (rows.Count < 3)
            {
                throw new InvalidDataException($"Excel 文件 {Path.GetFileName(filePath)} 至少需要 3 行表头。");
            }

            ExcelSheetSchema schema = BuildSchema(filePath, sheetName, rows[0], rows[1], rows[2]);
            ExcelTableData tableData = new ExcelTableData { Schema = schema };
            for (int i = 3; i < rows.Count; i++)
            {
                List<string> rowValues = rows[i];
                if (IsEmptyRow(rowValues))
                {
                    continue;
                }

                ExcelRowData rowData = new ExcelRowData { RowIndex = i + 1 };
                for (int j = 0; j < schema.Fields.Count; j++)
                {
                    int columnIndex = schema.Fields[j].ColumnIndex;
                    rowData.RawValues.Add(columnIndex < rowValues.Count ? rowValues[columnIndex] : string.Empty);
                }

                tableData.Rows.Add(rowData);
            }

            return tableData;
        }

        private static ExcelSheetSchema BuildSchema(string filePath, string sheetName, List<string> names, List<string> rawTypes, List<string> comments)
        {
            string tableName = string.IsNullOrWhiteSpace(sheetName) ? Path.GetFileNameWithoutExtension(filePath) : sheetName.Trim();
            ExcelSheetSchema schema = new ExcelSheetSchema
            {
                TableName = tableName,
                SourceFilePath = filePath,
                SheetName = tableName
            };

            int columnCount = new[] { names.Count, rawTypes.Count, comments.Count }.Max();
            for (int i = 0; i < columnCount; i++)
            {
                string fieldName = i < names.Count ? names[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(fieldName) || fieldName.StartsWith("#"))
                {
                    continue;
                }

                string rawType = i < rawTypes.Count ? rawTypes[i].Trim() : string.Empty;
                string comment = i < comments.Count ? comments[i].Trim() : string.Empty;
                if (!ExcelTypeParser.TryParse(rawType, out bool isList, out FieldTypeKind kind, out string error))
                {
                    throw new InvalidDataException($"表 {schema.TableName} 的字段 {fieldName} 类型非法: {error}");
                }

                schema.Fields.Add(new ExcelFieldSchema
                {
                    Name = fieldName,
                    RawType = rawType,
                    Comment = comment,
                    ColumnIndex = i,
                    IsList = isList,
                    TypeKind = kind
                });
            }

            return schema;
        }

        private static List<List<string>> ParseRows(XElement sheetData, XNamespace ns, List<string> sharedStrings)
        {
            List<List<string>> rows = new List<List<string>>();
            foreach (XElement rowElement in sheetData.Elements(ns + "row"))
            {
                List<string> rowValues = new List<string>();
                int currentColumn = 0;
                foreach (XElement cell in rowElement.Elements(ns + "c"))
                {
                    string cellReference = (string)cell.Attribute("r");
                    int columnIndex = GetColumnIndex(cellReference);
                    while (currentColumn < columnIndex)
                    {
                        rowValues.Add(string.Empty);
                        currentColumn++;
                    }

                    rowValues.Add(ReadCellValue(cell, ns, sharedStrings));
                    currentColumn++;
                }

                rows.Add(rowValues);
            }

            return rows;
        }

        private static string ReadCellValue(XElement cell, XNamespace ns, List<string> sharedStrings)
        {
            string cellType = (string)cell.Attribute("t");
            XElement valueElement = cell.Element(ns + "v");
            if (valueElement == null)
            {
                XElement inlineString = cell.Element(ns + "is");
                if (inlineString != null)
                {
                    return string.Concat(inlineString.Descendants(ns + "t").Select(item => item.Value));
                }

                return string.Empty;
            }

            string rawValue = valueElement.Value;
            if (string.Equals(cellType, "s"))
            {
                int sharedIndex = int.Parse(rawValue);
                return sharedIndex >= 0 && sharedIndex < sharedStrings.Count ? sharedStrings[sharedIndex] : string.Empty;
            }

            return rawValue;
        }

        private static int GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrWhiteSpace(cellReference))
            {
                return 0;
            }

            int index = 0;
            for (int i = 0; i < cellReference.Length; i++)
            {
                char character = cellReference[i];
                if (!char.IsLetter(character))
                {
                    break;
                }

                index = index * 26 + (char.ToUpperInvariant(character) - 'A' + 1);
            }

            return index - 1;
        }

        private static bool IsEmptyRow(List<string> rowValues)
        {
            for (int i = 0; i < rowValues.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(rowValues[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static XDocument LoadXml(ZipArchiveEntry entry)
        {
            if (entry == null)
            {
                throw new FileNotFoundException("Excel 压缩包中缺少必要的 XML 文件。");
            }

            using (Stream stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }

        private sealed class SheetReference
        {
            public SheetReference(string sheetName, string relationshipId)
            {
                SheetName = sheetName;
                RelationshipId = relationshipId;
            }

            public string SheetName { get; }

            public string RelationshipId { get; }
        }
    }
}
