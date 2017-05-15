﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class BulkInsertExtension
    {
        internal static void BulkInsertToTable<TEntity>(this IDbContextWrapper context, IList<TEntity> entities,
            string tableName, Operation operationType, BulkOptions options) where TEntity : class
        {
            var properties = context.EntityMapping.Properties
                .FilterPropertiesByOperation(operationType)
                .ToList();
            if (operationType == Operation.InsertOrUpdate && options.HasFlag(BulkOptions.OutputIdentity))
            {
                properties.Add(new PropertyMapping
                {
                    ColumnName = SqlHelper.Identity,
                    PropertyName = SqlHelper.Identity
                });
            }
            var dataReader = entities.ToDataReader(context.EntityMapping, operationType, options);

            using (var bulkcopy = new SqlBulkCopy((SqlConnection) context.Connection,
                SqlBulkCopyOptions.Default | SqlBulkCopyOptions.KeepIdentity,
                (SqlTransaction) context.Transaction))
            {
                foreach (var column in properties)
                {
                    bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BulkCopyTimeout = context.Connection.ConnectionTimeout;
                bulkcopy.WriteToServer(dataReader);
            }
        }
    }
}