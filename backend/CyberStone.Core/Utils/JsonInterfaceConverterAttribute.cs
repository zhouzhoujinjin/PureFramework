using System;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Utils
{
  [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
  public class JsonInterfaceConverterAttribute : JsonConverterAttribute
  {
    public JsonInterfaceConverterAttribute(Type converterType)
        : base(converterType)
    {
    }
  }
}