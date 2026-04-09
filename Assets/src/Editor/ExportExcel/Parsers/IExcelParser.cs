using System.Collections.Generic;

namespace Mini3.Editor.ExportExcel
{
    public interface IExcelParser
    {
        List<ExcelTableData> Parse(string filePath);
    }
}
