using System.Collections.Generic;

namespace CyberStone.Core.Entities
{
  public class TreeNodeEntity
  {
    public long Id { get; set; }
    public string InstanceType { get; set; } = string.Empty;
    public long InstanceId { get; set; }
    public long? ParentId { get; set; }
    public string ParentIds { get; set; } = string.Empty;
    public int Seq { get; set; }

    public Dictionary<string, object>? ExtendData { get; set; } = new Dictionary<string, object>();

    public TreeNodeEntity? Parent { get; set; }

    public List<TreeNodeEntity> Children { get; set; } = new List<TreeNodeEntity>();
  }
}