using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Utils
{
  public class NumberUtility
  {
    private static object _lock = new object();
    //生成申请单号
    //年月日时分秒+4位随机数
    public static string CreateRandomCode()
    {
      lock (_lock)
      {
        string datePart = DateTime.Now.ToString("yyyyMMddHHmmss");
        Random ran = new Random();
        int r = ran.Next(int.Parse("1".PadRight(4, '0')), int.Parse("9".PadRight(4, '9')));
        return $"{datePart}{r}";
      }
    }
  }
}
