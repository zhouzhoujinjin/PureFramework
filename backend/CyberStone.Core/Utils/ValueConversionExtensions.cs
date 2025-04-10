using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CyberStone.Core.Utils
{
  public static class ValueConversionExtensions
  {
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
      Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      PropertyNameCaseInsensitive = false
    };

    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
    {
      ValueConverter<T, string> converter = new ValueConverter<T, string>
      (
          v => JsonSerializer.Serialize(v, JsonSerializerOptions),
          v => JsonSerializer.Deserialize<T>(v, JsonSerializerOptions) ?? new T()
      );

      ValueComparer<T?> comparer = new ValueComparer<T?>
      (
          (left, right) => left != null && right != null
            ? JsonSerializer.Serialize(left, JsonSerializerOptions) == JsonSerializer.Serialize(right, JsonSerializerOptions)
            : left == right,
          v => JsonSerializer.Serialize(v, JsonSerializerOptions).GetHashCode(),
          v => v == null ? null : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, JsonSerializerOptions), JsonSerializerOptions)
      );

      propertyBuilder.HasConversion(converter);
      propertyBuilder.Metadata.SetValueConverter(converter);
      propertyBuilder.Metadata.SetValueComparer(comparer);
      return propertyBuilder;
    }
  }
}