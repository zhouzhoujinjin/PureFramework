using System.ComponentModel.DataAnnotations.Schema;

namespace CyberStone.Core.Entities
{
  public class ValueSpaceEntity
  {
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Title { get; set; }

    public ConfigureLevel ConfigureLevel { get; set; }
    public ValueSpaceType ValueSpaceType { get; set; }
    public string Items { get; set; } = "[]";

    [NotMapped]
    public object? ItemList { get; set; }
  }
}