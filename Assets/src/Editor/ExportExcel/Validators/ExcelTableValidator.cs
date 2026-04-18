using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class ExcelTableValidator
{
    private static readonly Regex FieldNamePattern = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

    public static void Validate(ExcelTableData tableData)
    {
        if (tableData == null || tableData.Schema == null)
        {
            throw new ArgumentNullException(nameof(tableData), "表数据不能为空。");
        }

        ExcelSheetSchema schema = tableData.Schema;
        if (string.IsNullOrWhiteSpace(schema.TableName))
        {
            throw new Exception("表名不能为空。");
        }

        if (schema.Fields.Count == 0)
        {
            throw new Exception($"表 {schema.TableName} 没有可导出的字段。");
        }

        ValidateFields(schema);
        ValidateRows(tableData);
    }

    private static void ValidateFields(ExcelSheetSchema schema)
    {
        HashSet<string> fieldNames = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < schema.Fields.Count; i++)
        {
            ExcelFieldSchema field = schema.Fields[i];
            if (string.IsNullOrWhiteSpace(field.Name))
            {
                throw new Exception($"表 {schema.TableName} 存在空字段名。");
            }

            if (!FieldNamePattern.IsMatch(field.Name))
            {
                throw new Exception($"表 {schema.TableName} 的字段名 {field.Name} 不是合法的 C# 标识符。");
            }

            if (!fieldNames.Add(field.Name))
            {
                throw new Exception($"表 {schema.TableName} 的字段名 {field.Name} 重复。");
            }
        }

        ExcelFieldSchema firstField = schema.Fields[0];
        if (!string.Equals(firstField.Name, "id", StringComparison.Ordinal))
        {
            throw new Exception($"表 {schema.TableName} 的第一列必须命名为 id。");
        }

        if (firstField.IsList || firstField.TypeKind != FieldTypeKind.Int)
        {
            throw new Exception($"表 {schema.TableName} 的第一列必须为 int 类型的 id。");
        }
    }

    private static void ValidateRows(ExcelTableData tableData)
    {
        HashSet<int> ids = new HashSet<int>();
        foreach (ExcelRowData row in tableData.Rows)
        {
            if (row.RawValues.Count < tableData.Schema.Fields.Count)
            {
                throw new Exception($"表 {tableData.Schema.TableName} 第 {row.RowIndex} 行列数不足。");
            }

            string idValue = row.RawValues[0];
            if (!int.TryParse(idValue, out int id))
            {
                throw new Exception($"表 {tableData.Schema.TableName} 第 {row.RowIndex} 行的 id 不是有效 int: {idValue}");
            }

            if (!ids.Add(id))
            {
                throw new Exception($"表 {tableData.Schema.TableName} 存在重复 id: {id}");
            }

            for (int i = 0; i < tableData.Schema.Fields.Count; i++)
            {
                ExcelFieldUtility.ValidateValue(tableData.Schema.Fields[i], row.RawValues[i], tableData.Schema.TableName, row.RowIndex);
            }
        }
    }
}
