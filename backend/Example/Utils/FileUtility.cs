using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Backend.Utils
{
  public class FileUtility
  {
    //获得文件大小(字节)
    public static long GetFileSize(string filePath = "")
    {
      if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return 0;
      FileInfo fileInfo = new FileInfo(filePath);
      return fileInfo.Length;
    }
  }
}
