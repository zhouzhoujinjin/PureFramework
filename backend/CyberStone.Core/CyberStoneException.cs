using System;

namespace CyberStone.Core
{
  public class CyberStoneException : Exception
  {
    public CyberStoneException() : base()
    {
    }

    public CyberStoneException(string message) : base(message)
    {
    }
  }
}