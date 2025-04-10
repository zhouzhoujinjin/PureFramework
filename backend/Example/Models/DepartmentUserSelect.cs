using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Backend.Models
{
  public class DepartmentUserSelect
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("value")]
    public long Value { get; set; }

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }

    [JsonPropertyName("children")]
    public ICollection<DepartmentUserSelect> Children { get; set; }

    public DepartmentUserSelect()
    {
      Children = new List<DepartmentUserSelect>();
    }
  }
}