using System.Text;

namespace Mini3.Editor.ExportExcel
{
    public static class JsonTableExporter
    {
        public static string Export(ExcelTableData tableData)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendFormat("  \"tableName\": \"{0}\",", tableData.Schema.TableName).AppendLine();
            builder.AppendLine("  \"rows\": [");

            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                builder.Append("    {");
                for (int j = 0; j < tableData.Schema.Fields.Count; j++)
                {
                    ExcelFieldSchema field = tableData.Schema.Fields[j];
                    string jsonValue = ExcelFieldUtility.ToJsonLiteral(field, tableData.Rows[i].RawValues[j]);
                    builder.AppendFormat("\"{0}\": {1}", field.Name, jsonValue);
                    if (j < tableData.Schema.Fields.Count - 1)
                    {
                        builder.Append(", ");
                    }
                }

                builder.Append("}");
                if (i < tableData.Rows.Count - 1)
                {
                    builder.Append(",");
                }

                builder.AppendLine();
            }

            builder.AppendLine("  ]");
            builder.AppendLine("}");
            return builder.ToString();
        }
    }
}
