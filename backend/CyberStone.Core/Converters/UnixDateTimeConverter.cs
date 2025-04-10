using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Converters
{
  public class UnixDateTimeConverter : JsonConverter<DateTime>
  {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return DateTime.UnixEpoch.ToLocalTime().AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
      writer.WriteNumberValue((value.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds);
    }
  }
}