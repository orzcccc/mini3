using System.Collections.Generic;
using System.IO;
using System.Text;

public static class BytesTableExporter
{
    public static byte[] Export(ExcelTableData tableData)
    {
        using (MemoryStream tableStream = new MemoryStream())
        using (BinaryWriter tableWriter = new BinaryWriter(tableStream, Encoding.UTF8))
        {
            for (int i = 0; i < tableData.Rows.Count; i++)
            {
                byte[] rowBytes = BuildRowBytes(tableData.Schema, tableData.Rows[i]);
                Write7BitEncodedInt32(tableWriter, rowBytes.Length);
                tableWriter.Write(rowBytes);
            }

            return tableStream.ToArray();
        }
    }

    private static byte[] BuildRowBytes(ExcelSheetSchema schema, ExcelRowData row)
    {
        using (MemoryStream rowStream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(rowStream, Encoding.UTF8))
        {
            for (int i = 0; i < schema.Fields.Count; i++)
            {
                ExcelFieldSchema field = schema.Fields[i];
                object value = ExcelFieldUtility.ParseValue(field, row.RawValues[i]);
                WriteField(writer, field, value);
            }

            return rowStream.ToArray();
        }
    }

    private static void WriteField(BinaryWriter writer, ExcelFieldSchema field, object value)
    {
        if (field.IsList)
        {
            List<object> items = (List<object>)value;
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                WriteScalar(writer, field.TypeKind, items[i]);
            }

            return;
        }

        WriteScalar(writer, field.TypeKind, value);
    }

    private static void WriteScalar(BinaryWriter writer, FieldTypeKind kind, object value)
    {
        switch (kind)
        {
            case FieldTypeKind.Int:
                writer.Write((int)value);
                break;
            case FieldTypeKind.Long:
                writer.Write((long)value);
                break;
            case FieldTypeKind.Float:
                writer.Write((float)value);
                break;
            case FieldTypeKind.Double:
                writer.Write((double)value);
                break;
            case FieldTypeKind.Bool:
                writer.Write((bool)value);
                break;
            default:
                writer.Write((string)value ?? string.Empty);
                break;
        }
    }

    private static void Write7BitEncodedInt32(BinaryWriter writer, int value)
    {
        uint num = (uint)value;
        while (num >= 0x80)
        {
            writer.Write((byte)(num | 0x80));
            num >>= 7;
        }

        writer.Write((byte)num);
    }
}
