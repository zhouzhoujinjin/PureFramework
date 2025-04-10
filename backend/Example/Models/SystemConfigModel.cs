using CyberStone.Core.Models;

namespace Backend.Models
{
  public class SystemConfigModel : GlobalSettings
  {
    public DateTime OpenStartDate { get; set; }
    public DateTime OpenEndDate { get; set; }
  }
}
