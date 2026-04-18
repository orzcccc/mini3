using System.Collections.Generic;

public sealed class ExcelTableData
{
    public ExcelSheetSchema Schema;
    public readonly List<ExcelRowData> Rows = new List<ExcelRowData>();
}
