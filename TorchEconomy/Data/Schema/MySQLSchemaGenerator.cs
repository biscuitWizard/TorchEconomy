using System;
using System.Reflection;

namespace TorchEconomy.Data.Schema
{
    public class MySQLSchemaGenerator : SchemaGenerator
    {
        protected override string GetFieldMarker()
        {
            return "`";
        }

        protected override string ResolvePropertyType(PropertyInfo property)
        {
            var propertyType = property.PropertyType;
            
            if (propertyType == typeof(long))
                return "bigint(8)";
            if (propertyType == typeof(ulong))
                return "decimal(20,0)";
            if (propertyType == typeof(decimal))
                return "decimal(20,2)";
            if (propertyType == typeof(string))
            {
                var lengthAttribute = propertyType.GetCustomAttribute<StringLengthAttribute>();
                if (lengthAttribute == null)
                    return "text";
                return $"varchar({lengthAttribute.Length})";
            }
            if (propertyType == typeof(bool))
                return "tinyint(1)";
            if (propertyType == typeof(int))
                return "int(8)";
            if (propertyType == typeof(float))
                return "float";

            return "blob";
        }

        protected override string AfterFields(PropertyInfo[] properties, PropertyInfo primaryKey)
        {
            return $"PRIMARY KEY ({GetFieldMarker()}{primaryKey.Name}{GetFieldMarker()})";
        }
        
        protected override string GetCreateTableString(string tableName)
        {
            return $"CREATE TABLE IF NOT EXISTS {GetFieldMarker()}{tableName}{GetFieldMarker()} (";
        }
    }
}