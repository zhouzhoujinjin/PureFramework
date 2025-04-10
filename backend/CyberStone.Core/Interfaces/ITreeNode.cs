using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberStone.Core.TreeFeature
{
  public interface ITreeNode<T>
  {
    List<T>? Children { get; set; }
  }
}