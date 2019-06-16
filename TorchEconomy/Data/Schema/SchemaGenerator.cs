using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TorchEconomy.Data.DataObjects;

namespace TorchEconomy.Data.Schema
{
    public abstract class SchemaGenerator
    {
        public virtual string Generate()
        {
            var types = new List<Type>();
            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("TorchEconomy.") 
                            || a.GetName().Name == "TorchEconomy")
                .ToList();
            foreach (var assembly in assemblies)
            {
                types.AddRange(assembly
                    .GetTypes()
                    .Where(t => typeof(IDataObject).IsAssignableFrom(t))
                    .Where(t => !t.IsInterface)
                    .Where(t => !t.IsAbstract));
            }
            
            var stringBuilder = new StringBuilder();
            foreach (var doType in types)
            {
                // Start table create statement.
                var tableName = doType.Name.Replace("DataObject", "");
                stringBuilder.AppendLine(GetCreateTableString(tableName));

                var properties = doType
                    .GetProperties()
                    .Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null)
                    .ToArray();

                var fields = new List<string>();
                PropertyInfo primaryKey = null;
                foreach (var property in properties)
                {
                    var attributes = ResolveAttributes(property);
                    var attributesString = string.Join(" ", attributes);

                    if (property.GetCustomAttribute<PrimaryKeyAttribute>() != null)
                        primaryKey = property;
                    
                    fields.Add(
                        $"\t{GetFieldMarker()}{property.Name}{GetFieldMarker()} {ResolvePropertyType(property)} {attributesString}");
                }

                var afterFields = AfterFields(properties, primaryKey);
                if(!string.IsNullOrEmpty(afterFields))
                    fields.Add(afterFields);

                // Add to query statement.
                stringBuilder.AppendLine(string.Join("," + Environment.NewLine, fields));

                // End table create statement.
                stringBuilder.AppendLine($");");
                stringBuilder.AppendLine(); // whitespace
            }

            return stringBuilder.ToString();
        }

        protected abstract string GetFieldMarker();
        protected abstract string ResolvePropertyType(PropertyInfo property);

        protected virtual string GetCreateTableString(string tableName)
        {
            return $"CREATE TABLE {GetFieldMarker()}{tableName}{GetFieldMarker()} (";
        }

        protected virtual string[] ResolveAttributes(PropertyInfo property)
        {
            var attributes = new List<string>();
            
            var defaultValue = property.GetCustomAttribute<DefaultValueAttribute>();
            var isPrimaryKey = property.GetCustomAttribute<PrimaryKeyAttribute>() != null;
            var isRequired = property.GetCustomAttribute<RequiredAttribute>() != null;

            if(isRequired || isPrimaryKey)
                attributes.Add(ResolveRequiredAttribute(true));
            if (isPrimaryKey)
                attributes.Add(ResolvePrimaryKeyAttribute(true));
            if (!isPrimaryKey && defaultValue != null)
                attributes.Add(ResolveDefaultValueAttribute(defaultValue));

            return attributes.ToArray();
        }

        protected virtual string ResolvePrimaryKeyAttribute(bool isPrimary)
        {
            return isPrimary ? "AUTO_INCREMENT" : "";
        }

        protected virtual string ResolveRequiredAttribute(bool isRequired)
        {
            return isRequired ? "NOT NULL" : "";
        }

        protected virtual string ResolveDefaultValueAttribute(DefaultValueAttribute attribute)
        {
            if (attribute != null)
            {
                var defaultValueString = "";
                if (attribute.DefaultValue is bool)
                    defaultValueString = (bool) attribute.DefaultValue ? "1" : "0";
                else
                    defaultValueString = attribute.DefaultValue.ToString();
                
                return $"DEFAULT '{defaultValueString}'";
            }

            return "DEFAULT NULL";
        }

        protected virtual string AfterFields(PropertyInfo[] properties, PropertyInfo primaryKey)
        {
            return null;
        }
    }
}