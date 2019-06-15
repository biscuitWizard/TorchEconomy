using System;

namespace TorchEconomy.Data.Schema
{
    
    public class StringLengthAttribute : Attribute
    {
        public int Length { get;  }
        
        public StringLengthAttribute(int length)
        {
            Length = length;
        }
    }
}