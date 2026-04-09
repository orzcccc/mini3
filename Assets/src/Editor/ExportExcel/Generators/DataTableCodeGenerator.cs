using System.Text;

namespace Mini3.Editor.ExportExcel
{
    public static class DataTableCodeGenerator
    {
        public static string Generate(ExcelSheetSchema schema, string nameSpace)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Globalization;");
            builder.AppendLine("using System.IO;");
            builder.AppendLine("using System.Text;");
            builder.AppendLine("using UnityGameFramework.Runtime;");
            builder.AppendLine();
            builder.AppendFormat("namespace {0}", nameSpace).AppendLine();
            builder.AppendLine("{");
            builder.AppendFormat("    public class DR{0} : DataRowBase", schema.TableName).AppendLine();
            builder.AppendLine("    {");
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                builder.AppendFormat("        private {0} {1};", ExcelFieldUtility.GetCSharpType(field), ExcelFieldUtility.ToMemberName(field.Name)).AppendLine();
            }
            builder.AppendLine();
            builder.AppendLine("        public override int Id => m_Id;");
            for (int i = 1; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                builder.AppendFormat("        public {0} {1} => {2};", ExcelFieldUtility.GetReadonlyType(field), ExcelFieldUtility.ToPropertyName(field.Name), ExcelFieldUtility.ToMemberName(field.Name)).AppendLine();
            }

            builder.AppendLine();
            builder.AppendLine("        public override bool ParseDataRow(string dataRowString, object userData)");
            builder.AppendLine("        {");
            builder.AppendLine("            string[] columns = dataRowString.Split('\\t');");
            builder.AppendLine("            int index = 0;");
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                string memberName = ExcelFieldUtility.ToMemberName(field.Name);
                if (field.IsList)
                {
                    builder.AppendFormat("            {0} = Parse{1}List(columns[index++]);", memberName, ExcelFieldUtility.ToPropertyName(ExcelFieldUtility.GetScalarCSharpType(field.TypeKind))).AppendLine();
                }
                else
                {
                    builder.AppendFormat("            {0} = {1}(columns[index++]);", memberName, ExcelFieldUtility.GetParseMethodName(field.TypeKind)).AppendLine();
                }
            }

            builder.AppendLine("            return true;");
            builder.AppendLine("        }");
            builder.AppendLine();
            builder.AppendLine("        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)");
            builder.AppendLine("        {");
            builder.AppendLine("            using (MemoryStream stream = new MemoryStream(dataRowBytes, startIndex, length, false))");
            builder.AppendLine("            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))");
            builder.AppendLine("            {");
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                string memberName = ExcelFieldUtility.ToMemberName(field.Name);
                if (field.IsList)
                {
                    string scalarType = ExcelFieldUtility.GetScalarCSharpType(field.TypeKind);
                    string readMethod = ExcelFieldUtility.GetBinaryReaderMethod(field.TypeKind);
                    builder.AppendFormat("                int {0}Count = reader.ReadInt32();", field.Name).AppendLine();
                    builder.AppendFormat("                {0} = new List<{1}>({2}Count);", memberName, scalarType, field.Name).AppendLine();
                    builder.AppendFormat("                for (int i = 0; i < {0}Count; i++)", field.Name).AppendLine();
                    builder.AppendLine("                {");
                    builder.AppendFormat("                    {0}.Add(reader.{1}());", memberName, readMethod).AppendLine();
                    builder.AppendLine("                }");
                }
                else
                {
                    builder.AppendFormat("                {0} = reader.{1}();", memberName, ExcelFieldUtility.GetBinaryReaderMethod(field.TypeKind)).AppendLine();
                }
            }

            builder.AppendLine();
            builder.AppendLine("                return true;");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
            builder.AppendLine();
            AppendHelpers(builder, schema);
            builder.AppendLine("    }");
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendHelpers(StringBuilder builder, ExcelSheetSchema schema)
        {
            bool hasIntList = false;
            bool hasLongList = false;
            bool hasFloatList = false;
            bool hasDoubleList = false;
            bool hasBoolList = false;
            bool hasStringList = false;

            for (int i = 0; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                if (!field.IsList)
                {
                    continue;
                }

                switch (field.TypeKind)
                {
                    case FieldTypeKind.Int:
                        hasIntList = true;
                        break;
                    case FieldTypeKind.Long:
                        hasLongList = true;
                        break;
                    case FieldTypeKind.Float:
                        hasFloatList = true;
                        break;
                    case FieldTypeKind.Double:
                        hasDoubleList = true;
                        break;
                    case FieldTypeKind.Bool:
                        hasBoolList = true;
                        break;
                    default:
                        hasStringList = true;
                        break;
                }
            }

            builder.AppendLine("        private static int ParseInt(string value) => int.Parse(value, CultureInfo.InvariantCulture);");
            builder.AppendLine("        private static long ParseLong(string value) => long.Parse(value, CultureInfo.InvariantCulture);");
            builder.AppendLine("        private static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);");
            builder.AppendLine("        private static double ParseDouble(string value) => double.Parse(value, CultureInfo.InvariantCulture);");
            builder.AppendLine("        private static bool ParseBool(string value) => value == \"1\" || bool.Parse(value);");
            builder.AppendLine("        private static string ParseString(string value) => value;");
            builder.AppendLine();

            if (hasIntList)
            {
                builder.AppendLine("        private static List<int> ParseIntList(string value) => ParseList(value, ParseInt);");
            }

            if (hasLongList)
            {
                builder.AppendLine("        private static List<long> ParseLongList(string value) => ParseList(value, ParseLong);");
            }

            if (hasFloatList)
            {
                builder.AppendLine("        private static List<float> ParseFloatList(string value) => ParseList(value, ParseFloat);");
            }

            if (hasDoubleList)
            {
                builder.AppendLine("        private static List<double> ParseDoubleList(string value) => ParseList(value, ParseDouble);");
            }

            if (hasBoolList)
            {
                builder.AppendLine("        private static List<bool> ParseBoolList(string value) => ParseList(value, ParseBool);");
            }

            if (hasStringList)
            {
                builder.AppendLine("        private static List<string> ParseStringList(string value) => ParseList(value, ParseString);");
            }

            if (hasIntList || hasLongList || hasFloatList || hasDoubleList || hasBoolList || hasStringList)
            {
                builder.AppendLine();
                builder.AppendLine("        private static List<T> ParseList<T>(string value, Func<string, T> parser)");
                builder.AppendLine("        {");
                builder.AppendLine("            List<T> results = new List<T>();");
                builder.AppendLine("            if (string.IsNullOrWhiteSpace(value))");
                builder.AppendLine("            {");
                builder.AppendLine("                return results;");
                builder.AppendLine("            }");
                builder.AppendLine();
                builder.AppendLine("            string[] segments = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);");
                builder.AppendLine("            for (int i = 0; i < segments.Length; i++)");
                builder.AppendLine("            {");
                builder.AppendLine("                results.Add(parser(segments[i].Trim()));");
                builder.AppendLine("            }");
                builder.AppendLine();
                builder.AppendLine("            return results;");
                builder.AppendLine("        }");
            }
        }
    }
}
