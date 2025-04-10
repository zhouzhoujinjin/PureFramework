using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberStone.Core.Utils
{
  public class PasswordSecurityOptions
  {
    public string? FrontendHash { get; set; } = "none";
    public string? AesKey { get; set; }
    public string? DefaultPassword { get; set; } = "123456";
  }
}
