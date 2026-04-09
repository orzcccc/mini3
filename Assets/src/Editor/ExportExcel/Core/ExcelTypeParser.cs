using System;

namespace Mini3.Editor.ExportExcel
{
    public static class ExcelTypeParser
    {
        public static bool TryParse(string rawType, out bool isList, out FieldTypeKind kind, out string error)
        {
            isList = false;
            kind = FieldTypeKind.String;
            error = null;

            if (string.IsNullOrWhiteSpace(rawType))
            {
                error = "字段类型不能为空。";
                return false;
            }

            string normalized = rawType.Trim().ToLowerInvariant();
            if (normalized.StartsWith("list<", StringComparison.Ordinal) && normalized.EndsWith(">", StringComparison.Ordinal))
            {
                string elementType = normalized.Substring(5, normalized.Length - 6).Trim();
                if (!TryParseScalar(elementType, out kind))
                {
                    error = $"暂不支持的列表元素类型: {rawType}";
                    return false;
                }

                isList = true;
                return true;
            }

            if (!TryParseScalar(normalized, out kind))
            {
                error = $"暂不支持的字段类型: {rawType}";
                return false;
            }

            return true;
        }

        private static bool TryParseScalar(string rawType, out FieldTypeKind kind)
        {
            switch (rawType)
            {
                case "int":
                    kind = FieldTypeKind.Int;
                    return true;
                case "long":
                    kind = FieldTypeKind.Long;
                    return true;
                case "float":
                    kind = FieldTypeKind.Float;
                    return true;
                case "double":
                    kind = FieldTypeKind.Double;
                    return true;
                case "bool":
                    kind = FieldTypeKind.Bool;
                    return true;
                case "string":
                    kind = FieldTypeKind.String;
                    return true;
                default:
                    kind = FieldTypeKind.String;
                    return false;
            }
        }
    }
}
