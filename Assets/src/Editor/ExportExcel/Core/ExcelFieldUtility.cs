using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class ExcelFieldUtility
{
    private static readonly char[] ListSeparator = { ',' };

    public static string GetCSharpType(ExcelFieldSchema field)
    {
        string scalarType = GetScalarCSharpType(field.TypeKind);
        return field.IsList ? $"List<{scalarType}>" : scalarType;
    }

    public static string GetReadonlyType(ExcelFieldSchema field)
    {
        string scalarType = GetScalarCSharpType(field.TypeKind);
        return field.IsList ? $"IReadOnlyList<{scalarType}>" : scalarType;
    }

    public static string GetScalarCSharpType(FieldTypeKind kind)
    {
        switch (kind)
        {
            case FieldTypeKind.Int:
                return "int";
            case FieldTypeKind.Long:
                return "long";
            case FieldTypeKind.Float:
                return "float";
            case FieldTypeKind.Double:
                return "double";
            case FieldTypeKind.Bool:
                return "bool";
            default:
                return "string";
        }
    }

    public static string ToMemberName(string fieldName)
    {
        return $"m_{char.ToUpperInvariant(fieldName[0])}{fieldName.Substring(1)}";
    }

    public static string ToPropertyName(string fieldName)
    {
        return char.ToUpperInvariant(fieldName[0]) + fieldName.Substring(1);
    }

    public static object ParseValue(ExcelFieldSchema field, string rawValue)
    {
        if (field.IsList)
        {
            return ParseList(field.TypeKind, rawValue);
        }

        return ParseScalar(field.TypeKind, rawValue);
    }

    public static void ValidateValue(ExcelFieldSchema field, string rawValue, string tableName, int rowIndex)
    {
        try
        {
            ParseValue(field, rawValue);
        }
        catch (Exception exception)
        {
            throw new Exception($"表 {tableName} 第 {rowIndex} 行字段 {field.Name} 的值 '{rawValue}' 无法解析为 {field.RawType}。", exception);
        }
    }

    public static string ToJsonLiteral(ExcelFieldSchema field, string rawValue)
    {
        object value = ParseValue(field, rawValue);
        return ToJsonLiteral(field, value);
    }

    public static string ToJsonLiteral(ExcelFieldSchema field, object value)
    {
        if (field.IsList)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            IList<object> items = (IList<object>)value;
            for (int i = 0; i < items.Count; i++)
            {
                builder.Append(ToScalarJsonLiteral(field.TypeKind, items[i]));
                if (i < items.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(']');
            return builder.ToString();
        }

        return ToScalarJsonLiteral(field.TypeKind, value);
    }

    public static string GetBinaryReaderMethod(FieldTypeKind kind)
    {
        switch (kind)
        {
            case FieldTypeKind.Int:
                return "ReadInt32";
            case FieldTypeKind.Long:
                return "ReadInt64";
            case FieldTypeKind.Float:
                return "ReadSingle";
            case FieldTypeKind.Double:
                return "ReadDouble";
            case FieldTypeKind.Bool:
                return "ReadBoolean";
            default:
                return "ReadString";
        }
    }

    public static string GetParseMethodName(FieldTypeKind kind)
    {
        switch (kind)
        {
            case FieldTypeKind.Int:
                return "ParseInt";
            case FieldTypeKind.Long:
                return "ParseLong";
            case FieldTypeKind.Float:
                return "ParseFloat";
            case FieldTypeKind.Double:
                return "ParseDouble";
            case FieldTypeKind.Bool:
                return "ParseBool";
            default:
                return "ParseString";
        }
    }

    private static object ParseScalar(FieldTypeKind kind, string rawValue)
    {
        string value = rawValue ?? string.Empty;
        switch (kind)
        {
            case FieldTypeKind.Int:
                return int.Parse(value, CultureInfo.InvariantCulture);
            case FieldTypeKind.Long:
                return long.Parse(value, CultureInfo.InvariantCulture);
            case FieldTypeKind.Float:
                return float.Parse(value, CultureInfo.InvariantCulture);
            case FieldTypeKind.Double:
                return double.Parse(value, CultureInfo.InvariantCulture);
            case FieldTypeKind.Bool:
                return ParseBoolValue(value);
            default:
                return value;
        }
    }

    private static List<object> ParseList(FieldTypeKind kind, string rawValue)
    {
        List<object> values = new List<object>();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return values;
        }

        string[] segments = rawValue.Split(ListSeparator, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < segments.Length; i++)
        {
            values.Add(ParseScalar(kind, segments[i].Trim()));
        }

        return values;
    }

    private static bool ParseBoolValue(string value)
    {
        if (string.Equals(value, "1", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(value, "0", StringComparison.Ordinal))
        {
            return false;
        }

        return bool.Parse(value);
    }

    private static string ToScalarJsonLiteral(FieldTypeKind kind, object value)
    {
        switch (kind)
        {
            case FieldTypeKind.Int:
            case FieldTypeKind.Long:
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            case FieldTypeKind.Float:
                return ((float)value).ToString("R", CultureInfo.InvariantCulture);
            case FieldTypeKind.Double:
                return ((double)value).ToString("R", CultureInfo.InvariantCulture);
            case FieldTypeKind.Bool:
                return ((bool)value) ? "true" : "false";
            default:
                return $"\"{EscapeString(Convert.ToString(value, CultureInfo.InvariantCulture))}\"";
        }
    }

    private static string EscapeString(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
