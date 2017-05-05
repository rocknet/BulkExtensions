﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using EntityFramework.BulkExtensions.BulkOperations;
using EntityFramework.MappingExtension;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DataTableExtension
    {
        /// <summary>
        /// </summary>
        /// <param name="mapping"></param>
        /// <param name="entities"></param>
        /// <param name="operationType"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        internal static DataTable ToDataTable<TEntity>(this IEnumerable<TEntity> entities, IEntityMapping mapping, OperationType operationType) where TEntity : class
        {
            var tableColumns = mapping.Properties.FilterProperties(operationType).ToList();

            var tb = CreateDataTable(mapping, tableColumns);

            foreach (var item in entities)
            {
                var props = item.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                var values = new List<object>();
                foreach (var column in tableColumns)
                {
                    var prop = props.SingleOrDefault(info => info.Name == column.PropertyName);
                    if (prop != null)
                        values.Add(prop.GetValue(item, null));
                    else if (column.IsHierarchyMapping)
                        values.Add(mapping.HierarchyMapping[item.GetType().Name]);
                    else
                        values.Add(null);
                }

                tb.Rows.Add(values.ToArray());
            }

            return tb;
        }

        private static DataTable CreateDataTable(IEntityMapping mapping, IEnumerable<IPropertyMapping> IPropertyMappings)
        {
            var table = new DataTable();
            foreach (var prop in IPropertyMappings)
            {
                table.Columns.Add(prop.ColumnName, Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);
            }

            table.TableName = mapping.EntityName;
            return table;
        }
    }
}