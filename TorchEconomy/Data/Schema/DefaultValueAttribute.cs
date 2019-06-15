using System;

namespace TorchEconomy.Data.Schema
{
    public class DefaultValueAttribute : Attribute
    {
        public object DefaultValue { get;  }
        public DefaultValueAttribute(object value)
        {
            DefaultValue = value;
        }   
    }
}