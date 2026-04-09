using System.Collections.Generic;

namespace Mini3.Editor.ExportExcel
{
    public sealed class ExcelSheetSchema
    {
        public string TableName;
        public string SourceFilePath;
        public string SheetName;
        public readonly List<ExcelFieldSchema> Fields = new List<ExcelFieldSchema>();
    }
}
