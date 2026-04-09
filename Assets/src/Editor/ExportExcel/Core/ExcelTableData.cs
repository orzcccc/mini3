using System.Collections.Generic;

namespace Mini3.Editor.ExportExcel
{
    public sealed class ExcelTableData
    {
        public ExcelSheetSchema Schema;
        public readonly List<ExcelRowData> Rows = new List<ExcelRowData>();
    }
}
