using System;
using System.Collections.Generic;
using System.Reflection;
using GameFramework.DataTable;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Mini3
{
    /// <summary>
    /// 数据表业务入口，负责从 Resources 加载表并提供强类型访问。
    /// </summary>
    public class TableMgr : Singleton<TableMgr>
    {
        private readonly Dictionary<string, string> m_ResourcePathByTableKey = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> m_LoadedTableKeys = new HashSet<string>(StringComparer.Ordinal);

        public void Register<T>(string resourcePath = null, string name = null) where T : class, IDataRow, new()
        {
            string tableKey = GetTableKey(typeof(T), name);
            m_ResourcePathByTableKey[tableKey] = string.IsNullOrWhiteSpace(resourcePath)
                ? GetDefaultResourcePath(typeof(T))
                : resourcePath;

            EnsureTableCreated<T>(name);
        }

        public bool Load<T>(string resourcePath = null, string name = null) where T : class, IDataRow, new()
        {
            string tableKey = GetTableKey(typeof(T), name);
            string finalResourcePath = ResolveResourcePath(typeof(T), resourcePath, name);
            TextAsset textAsset = Resources.Load<TextAsset>(finalResourcePath);
            if (textAsset == null)
            {
                Log.Warning("Can not load data table asset from Resources path '{0}'.", finalResourcePath);
                return false;
            }

            EnsureTableCreated<T>(name);
            IDataTable<T> table = GetDataTableComponent().GetDataTable<T>(name);
            if (table == null)
            {
                Log.Warning("Data table '{0}' is invalid.", tableKey);
                return false;
            }

            bool success = ((DataTableBase)table).ParseData(textAsset.bytes, null);
            if (success)
            {
                m_ResourcePathByTableKey[tableKey] = finalResourcePath;
                m_LoadedTableKeys.Add(tableKey);
            }

            return success;
        }

        public void LoadMany(params Type[] rowTypes)
        {
            if (rowTypes == null)
            {
                return;
            }

            for (int i = 0; i < rowTypes.Length; i++)
            {
                LoadByType(rowTypes[i]);
            }
        }

        public int LoadAll()
        {
            return LoadAll(GetAutoDiscoverRowTypes());
        }

        public int LoadAll(params Type[] rowTypes)
        {
            if (rowTypes == null || rowTypes.Length == 0)
            {
                return 0;
            }

            int successCount = 0;
            for (int i = 0; i < rowTypes.Length; i++)
            {
                if (LoadByType(rowTypes[i]))
                {
                    successCount++;
                }
            }

            return successCount;
        }

        public T Get<T>(int id, string name = null) where T : class, IDataRow
        {
            IDataTable<T> table = GetDataTableComponent().GetDataTable<T>(name);
            if (table == null)
            {
                Log.Warning("Data table '{0}' has not been created or loaded.", GetTableKey(typeof(T), name));
                return null;
            }

            return table.GetDataRow(id);
        }

        public bool IsLoaded<T>(string name = null) where T : class, IDataRow
        {
            return m_LoadedTableKeys.Contains(GetTableKey(typeof(T), name));
        }

        private bool LoadByType(Type rowType)
        {
            if (rowType == null)
            {
                return false;
            }

            string defaultPath = GetDefaultResourcePath(rowType);
            TextAsset textAsset = Resources.Load<TextAsset>(defaultPath);
            if (textAsset == null)
            {
                Log.Warning("Can not load data table asset from Resources path '{0}'.", defaultPath);
                return false;
            }

            DataTableComponent dataTableComponent = GetDataTableComponent();
            if (!dataTableComponent.HasDataTable(rowType))
            {
                dataTableComponent.CreateDataTable(rowType);
            }

            DataTableBase table = dataTableComponent.GetDataTable(rowType);
            if (table == null)
            {
                Log.Warning("Data table '{0}' is invalid.", rowType.FullName);
                return false;
            }

            if (table.ParseData(textAsset.bytes, null))
            {
                string tableKey = GetTableKey(rowType, null);
                m_ResourcePathByTableKey[tableKey] = defaultPath;
                m_LoadedTableKeys.Add(tableKey);
                return true;
            }

            return false;
        }

        private void EnsureTableCreated<T>(string name) where T : class, IDataRow, new()
        {
            DataTableComponent dataTableComponent = GetDataTableComponent();
            if (!dataTableComponent.HasDataTable<T>(name))
            {
                dataTableComponent.CreateDataTable<T>(name);
            }
        }

        private string ResolveResourcePath(Type rowType, string resourcePath, string name)
        {
            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                return resourcePath;
            }

            string tableKey = GetTableKey(rowType, name);
            if (m_ResourcePathByTableKey.TryGetValue(tableKey, out string cachedPath))
            {
                return cachedPath;
            }

            return GetDefaultResourcePath(rowType);
        }

        private static string GetTableKey(Type rowType, string name)
        {
            return string.IsNullOrWhiteSpace(name) ? rowType.FullName : $"{rowType.FullName}:{name}";
        }

        private static string GetDefaultResourcePath(Type rowType)
        {
            return $"DataTables/Bytes/{GetTableName(rowType)}";
        }

        private static Type[] GetAutoDiscoverRowTypes()
        {
            List<Type> rowTypes = new List<Type>();
            Type dataRowInterfaceType = typeof(IDataRow);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;
                try
                {
                    types = assemblies[i].GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                if (types == null)
                {
                    continue;
                }

                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    if (type == null || type.IsAbstract)
                    {
                        continue;
                    }

                    if (!string.Equals(type.Namespace, "Mini3.DataTables", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!type.Name.StartsWith("DR", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!dataRowInterfaceType.IsAssignableFrom(type))
                    {
                        continue;
                    }

                    rowTypes.Add(type);
                }
            }

            rowTypes.Sort((left, right) => string.CompareOrdinal(left.FullName, right.FullName));
            return rowTypes.ToArray();
        }

        private static string GetTableName(Type rowType)
        {
            string typeName = rowType.Name;
            return typeName.StartsWith("DR", StringComparison.Ordinal) ? typeName.Substring(2) : typeName;
        }

        private static DataTableComponent GetDataTableComponent()
        {
            DataTableComponent dataTableComponent = GameEntry.GetComponent<DataTableComponent>();
            if (dataTableComponent == null)
            {
                throw new Exception("DataTableComponent is not found. Please check GameEntry scene setup.");
            }

            return dataTableComponent;
        }
    }
}
