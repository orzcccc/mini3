using System.Collections.Generic;

namespace Mini3.Editor.ExportExcel
{
    public sealed class ExcelRowData
    {
        public int RowIndex;
        public readonly List<string> RawValues = new List<string>();
    }
}
