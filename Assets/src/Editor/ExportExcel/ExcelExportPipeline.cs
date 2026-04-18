using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ExcelExportPipeline
{
    public static void ExportAll(ExcelExportSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        EnsureFolder(settings.excelFolder);
        EnsureFolder(settings.dataRowCodeFolder);
        EnsureFolder(settings.jsonModelCodeFolder);
        EnsureFolder(settings.bytesOutputFolder);
        EnsureFolder(settings.jsonOutputFolder);

        string[] excelFiles = Directory.GetFiles(settings.excelFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
        if (excelFiles.Length == 0)
        {
            Debug.LogWarning($"[Excel导表] 在目录 {settings.excelFolder} 下没有找到 .xlsx 文件。");
            return;
        }

        IExcelParser parser = new PlaceholderExcelParser();
        int successCount = 0;

        foreach (string excelFile in excelFiles)
        {
            try
            {
                successCount += ExportSingle(excelFile, settings, parser);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Excel导表] 导出失败: {excelFile}\n{exception}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[Excel导表] 导出流程完成，成功导出 {successCount} 张表，源文件数 {excelFiles.Length}。");
    }

    private static int ExportSingle(string excelFile, ExcelExportSettings settings, IExcelParser parser)
    {
        List<ExcelTableData> tables = parser.Parse(excelFile);
        int exportCount = 0;
        for (int i = 0; i < tables.Count; i++)
        {
            ExcelTableData tableData = tables[i];
            ExcelTableValidator.Validate(tableData);

            string tableName = tableData.Schema.TableName;

            if (settings.generateDataRowCode)
            {
                string code = DataTableCodeGenerator.Generate(tableData.Schema, settings.dataRowNamespace);
                File.WriteAllText(Path.Combine(settings.dataRowCodeFolder, $"DR{tableName}.cs"), code);
            }

            if (settings.generateJsonModelCode)
            {
                string rowCode = JsonModelCodeGenerator.GenerateRow(tableData.Schema, settings.jsonModelNamespace);
                string tableCode = JsonModelCodeGenerator.GenerateTable(tableData.Schema, settings.jsonModelNamespace);
                File.WriteAllText(Path.Combine(settings.jsonModelCodeFolder, $"{tableName}Row.cs"), rowCode);
                File.WriteAllText(Path.Combine(settings.jsonModelCodeFolder, $"{tableName}Table.cs"), tableCode);
            }

            if (settings.exportBytes)
            {
                byte[] bytes = BytesTableExporter.Export(tableData);
                File.WriteAllBytes(Path.Combine(settings.bytesOutputFolder, $"{tableName}.bytes"), bytes);
            }

            if (settings.exportJson)
            {
                string json = JsonTableExporter.Export(tableData);
                File.WriteAllText(Path.Combine(settings.jsonOutputFolder, $"{tableName}.json"), json);
            }

            exportCount++;
        }

        return exportCount;
    }

    private static void EnsureFolder(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            throw new Exception("导表路径配置不能为空。");
        }

        if (!Directory.Exists(assetPath))
        {
            Directory.CreateDirectory(assetPath);
        }
    }
}
