using System.Text;

public static class JsonModelCodeGenerator
{
    public static string GenerateRow(ExcelSheetSchema schema, string nameSpace)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine();
        builder.AppendLine("[Serializable]");
        builder.AppendFormat("public class {0}Row", schema.TableName).AppendLine();
        builder.AppendLine("{");
        for (int i = 0; i < schema.Fields.Count; i++)
        {
            ExcelFieldSchema field = schema.Fields[i];
            builder.AppendFormat("    public {0} {1};", ExcelFieldUtility.GetCSharpType(field), field.Name).AppendLine();
        }
        builder.AppendLine("}");
        return builder.ToString();
    }

    public static string GenerateTable(ExcelSheetSchema schema, string nameSpace)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
        builder.AppendLine("[Serializable]");
        builder.AppendFormat("public class {0}Table", schema.TableName).AppendLine();
        builder.AppendLine("{");
        builder.AppendLine("    public string tableName;");
        builder.AppendFormat("    public List<{0}Row> rows = new List<{0}Row>();", schema.TableName).AppendLine();
        builder.AppendLine("}");
        return builder.ToString();
    }
}
