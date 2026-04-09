using UnityEngine;

namespace Mini3.Editor.ExportExcel
{
    public sealed class ExcelExportSettings : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/src/Editor/ExportExcel/ExcelExportSettings.asset";

        [Header("Input")]
        public string excelFolder = "Assets/src/ExportExcel/Excels";

        [Header("Generated Code")]
        public string dataRowCodeFolder = "Assets/src/ExportExcel/Generated/DataTables";
        public string jsonModelCodeFolder = "Assets/src/ExportExcel/Generated/JsonModels";

        [Header("Output")]
        public string bytesOutputFolder = "Assets/Resources/DataTables/Bytes";
        public string jsonOutputFolder = "Assets/Resources/DataTables/Json";

        [Header("Namespace")]
        public string dataRowNamespace = "Mini3.DataTables";
        public string jsonModelNamespace = "Mini3.ExcelJson";

        [Header("Options")]
        public bool exportBytes = true;
        public bool exportJson = true;
        public bool generateDataRowCode = true;
        public bool generateJsonModelCode = true;
    }
}
