using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace JBAUtils
{
    public class DataConversionService
    {
        private delegate void ColumnMap(ref object obj, DataRow row);
        private delegate void TableMap<in T>(T obj, DataSet dataset);
        private delegate void PropertyMap(ref DataRow row, object obj);

        public IEnumerable<T> DataTableToEnumerable<T>(DataTable table)
        {
            return DataTableToEnumerable(table, typeof(T)).Cast<T>();
        }

        private IEnumerable<object> DataTableToEnumerable(DataTable table, Type type)
        {
            List<string> columns = (from DataColumn column in table.Columns select column.ColumnName).ToList();

            List<ColumnMap> columnMaps = GetColumnMaps(columns, type).ToList();

            foreach (DataRow row in table.AsEnumerable())
            {
                object obj = Activator.CreateInstance(type);

                columnMaps.ForEach(columnMap => columnMap(ref obj, row));

                yield return obj;
            }
        }

        public DataTable CreateTableForList<T>(List<T> data, dynamic columnNames)
        {
            DataTable table = new DataTable();

            Dictionary<string, Type> typeDictionary = GetTypeDictionary<T>(columnNames);

            foreach (string columnName in typeDictionary.Select(t => t.Key))
            {
                Type type = typeDictionary[columnName];
                Type safeType = Nullable.GetUnderlyingType(type) ?? type;

                if (typeof(Enum).IsAssignableFrom(safeType))
                {
                    safeType = typeof(int);
                }

                DataColumn column = new DataColumn(columnName, safeType);

                if (safeType == type)
                {
                    column.AllowDBNull = true;
                }

                table.Columns.Add(column);
            }

            return table;
        }

        public DataTable PopulateTable<T>(DataTable table, List<T> data, dynamic columnNames)
        {
            IEnumerable<PropertyMap> p = GetPropertyMaps<T>(columnNames);
            List<PropertyMap> propertyMaps = p.ToList();

            if (data != null)
            {
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();

                    propertyMaps.ForEach(propertyMap => propertyMap(ref row, item));

                    table.Rows.Add(row);
                }
            }

            return table;
        }

        public DataTable CreateAndPopulateTableForList<T>(List<T> data, dynamic columnNames)
        {
            DataTable table = CreateTableForList<T>(data, columnNames);

            return PopulateTable(table, data, columnNames);
        }

        private IEnumerable<ColumnMap> GetColumnMaps(List<string> columns, Type type)
        {
            if (columns.Count == 1 && !type.IsClass)
            {
                return new List<ColumnMap>
                {
                    (ref object obj, DataRow row) => obj = Convert.ChangeType(row[columns.First()], type)
                };
            }

            return type.GetProperties()
                .Where(property => property.CanWrite)
                .Select(property => GetColumnMap(property, columns))
                .Where(columnMap => columnMap != null);
        }

        private static PropertyMap GetPropertyMap(string propertyName, string columnName, ICollection<PropertyInfo> propertyStack, PropertyInfo previousPropertyInfo = null, Type type = null)
        {
            string firstColumnName = columnName.Split('.').First();

            string remainingColumnName = firstColumnName != columnName ?
                columnName.Substring(firstColumnName.Length + 1, columnName.Length - firstColumnName.Length - 1) :
                string.Empty;

            PropertyInfo propertyInfo = (previousPropertyInfo?.PropertyType ?? type)?.GetProperty(firstColumnName);

            if (propertyInfo != null)
            {
                if (remainingColumnName.Length > 0)
                {
                    propertyStack.Add(propertyInfo);
                    return GetPropertyMap(propertyName, remainingColumnName, propertyStack, propertyInfo);
                }

                return (ref DataRow row, object obj) =>
                {
                    obj = propertyStack.Aggregate(obj, (current, currentPropertyInfo) => currentPropertyInfo.GetValue(current));

                    if (obj == null)
                    {
                        row[propertyName] = DBNull.Value;
                    }
                    else
                    {
                        row[propertyName] = propertyInfo.GetValue(obj) ?? DBNull.Value;
                    }
                };
            }

            return null;
        }

        private Type GetTypeForColumn(string columnName, Type type)
        {
            string firstColumnName = columnName.Split('.').First();
            string remainingColumnName = firstColumnName != columnName ? columnName.Substring(firstColumnName.Length + 1, columnName.Length - firstColumnName.Length - 1) : string.Empty;

            return remainingColumnName.Length > 0 ?
                GetTypeForColumn(remainingColumnName, type.GetProperty(firstColumnName)?.PropertyType) :
                type.GetProperty(firstColumnName)?.PropertyType;
        }

        private ColumnMap GetColumnMap(PropertyInfo property, List<string> columns, string prefix = "")
        {
            Type safeType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            bool isEnum = false;
            Type enumType = null;

            if (safeType.IsEnum)
            {
                enumType = safeType;
                safeType = typeof(int);
                isEnum = true;
            }

            if (columns.Any(column => column.Equals(property.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return (ref object obj, DataRow row) =>
                {
                    if (!(row[$"{prefix}{property.Name}"] is DBNull))
                    {
                        object safeValue = row[$"{prefix}{property.Name}"] == null ? null : Convert.ChangeType(row[$"{prefix}{property.Name}"], safeType);

                        if (isEnum)
                        {
                            safeValue = Enum.ToObject(enumType, safeValue);
                        }

                        property.SetValue(obj, safeValue, null);
                    }
                };
            }

            List<ColumnMap> columnMaps = GetCompositeColumnMaps(property, columns, prefix).ToList();

            if (columnMaps.Any())
            {
                bool hasId = property.PropertyType.GetProperty("Id") != null;

                return (ref object obj, DataRow row) =>
                {
                    if (!hasId || row[$"{prefix}{property.Name}.Id"] != DBNull.Value)
                    {
                        object propertyValue = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(obj, propertyValue);

                        columnMaps.ForEach(columnMap => columnMap(ref propertyValue, row));
                    }
                };
            }

            return null;
        }

        private IEnumerable<ColumnMap> GetCompositeColumnMaps(PropertyInfo property, IEnumerable<string> columns, string prefix = "")
        {
            List<string> compositeColumns = columns
                .Where(column => column.StartsWith($"{property.Name}."))
                .Select(compositeColumn => compositeColumn.Substring(property.Name.Length + 1, compositeColumn.Length - property.Name.Length - 1)).ToList();

            return property.PropertyType
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(propertyInfo => GetColumnMap(propertyInfo, compositeColumns, $"{prefix}{property.Name}."))
                .Where(columnMap => columnMap != null);
        }

        private Dictionary<string, Type> GetTypeDictionary<T>(dynamic columnNames)
        {
            Dictionary<string, Type> typeDictionary = new Dictionary<string, Type>();

            foreach (PropertyInfo property in columnNames.GetType().GetProperties())
            {
                typeDictionary.Add(property.Name, GetTypeForColumn(property.GetValue(columnNames), typeof(T)));
            }

            return typeDictionary;
        }

        private static IEnumerable<PropertyMap> GetPropertyMaps<T>(dynamic columnNames)
        {
            foreach (PropertyInfo property in columnNames.GetType().GetProperties())
            {
                yield return GetPropertyMap(property.Name, property.GetValue(columnNames), new List<PropertyInfo>(), type: typeof(T));
            }
        }
    }
}
