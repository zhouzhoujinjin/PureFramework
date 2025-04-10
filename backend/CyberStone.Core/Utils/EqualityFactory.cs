using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CyberStone.Core.Utils
{
  public static class EqualityFactory
  {
    private sealed class Impl<T> : EqualityComparer<T>
    {
      private readonly Func<T?, T?, bool> m_del;
      private readonly IEqualityComparer<T> m_comp;

      public Impl(Func<T?, T?, bool> del)
      {
        m_del = del;
        m_comp = EqualityComparer<T>.Default;
      }

      public override bool Equals(T? x, T? y)
      {
        return m_del(x, y);
      }

      public override int GetHashCode([DisallowNull] T obj)
      {
        return m_comp.GetHashCode(obj);
      }
    }

    public static EqualityComparer<T> Create<T>(Func<T?, T?, bool> del)
    {
      return new Impl<T>(del);
    }
  }
}