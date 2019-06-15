using System;
using System.Reflection;

namespace TorchEconomy.Data.Schema
{
    public class SQLiteSchemaGenerator : SchemaGenerator
    {
        protected override string GetFieldMarker()
        {
            return "\"";
        }

        protected override string ResolvePropertyType(PropertyInfo property)
        {
            var propertyType = property.PropertyType;

            // SQLite primary keys always have to be integer.
            if (property.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                return "INTEGER";
            
            if (propertyType == typeof(string))
                return "TEXT";
            if (propertyType == typeof(bool)
                || propertyType == typeof(int)
                || propertyType == typeof(short)
                || propertyType == typeof(byte)
                || propertyType == typeof(bool))
                return "INTEGER";
            if (propertyType == typeof(float)
                || propertyType == typeof(long)
                || propertyType == typeof(ulong)
                || propertyType == typeof(decimal))
                return "NUMERIC";
            return "BLOB";
        }

        protected override string ResolvePrimaryKeyAttribute(bool isPrimary)
        {
            return "PRIMARY KEY AUTOINCREMENT";
        }
    }
}