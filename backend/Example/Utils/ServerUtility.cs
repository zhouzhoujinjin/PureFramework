using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Utils
{
  public class ServerUtility
  {
    public static string ContentRootMapPath(string virtualPath)
    {
      if (virtualPath == null)
      {
        return "";
      }
      else
      {
        var pathSeparator = Path.DirectorySeparatorChar.ToString();
        var altPathSeparator = Path.AltDirectorySeparatorChar.ToString();
        var rootDictionaryPath = AppContext.BaseDirectory;
        if (!rootDictionaryPath.EndsWith(pathSeparator) && !rootDictionaryPath.EndsWith(altPathSeparator))
        {
          rootDictionaryPath += pathSeparator;
        }

        if (virtualPath.StartsWith("~/"))
        {
          return virtualPath.Replace("~/", rootDictionaryPath).Replace("/", pathSeparator);
        }
        else
        {
          return Path.Combine(rootDictionaryPath, virtualPath);
        }
      }
    }
  }
}
