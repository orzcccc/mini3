using System.Collections.Generic;

public interface IExcelParser
{
    List<ExcelTableData> Parse(string filePath);
}
