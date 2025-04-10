using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Converters
{
  public class JsonTypeConverter : JsonConverter<Type>
  {
    public override bool CanConvert(Type typeToConvert) => typeof(Type) == typeToConvert;

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      try
      {
        return string.IsNullOrEmpty(reader.GetString()) ? null : Type.GetType(reader.GetString()!);
      }
      catch
      {
        return null;
      }
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
      writer.WriteStringValue(value.FullName);
    }
  }
}