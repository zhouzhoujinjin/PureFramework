using CyberStone.Core.TreeFeature;
using System.Collections.Generic;

namespace CyberStone.Core.Models
{
  public class TreeNodeModel<TData> : ITreeNode<TreeNodeModel<TData>>
  {
    public TData? Data { get; set; }
    public Dictionary<string, object>? ExtendData { get; set; }
    public List<TreeNodeModel<TData>>? Children { get; set; }
  }
}