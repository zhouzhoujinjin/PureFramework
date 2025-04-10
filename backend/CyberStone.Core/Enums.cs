namespace CyberStone.Core
{
  public enum ConfigureLevel
  {
    System = 1,               // 系统级，不可修改
    Configurable = 2          // 可配置的
  }

  public enum ValueSpaceType
  {
    Code = 1,                   // 代码
    Regex = 2,                  // 正则表达式
    Range = 3                   // 范围
  }

  public enum MenuItemType
  {
    /// <summary>
    /// 菜单路由项
    /// </summary>
    Route = 1,

    /// <summary>
    /// 菜单权限项
    /// </summary>
    Action = 2
  }


  public enum UserLogLevel
  {
    None = 0,
    Info,
    Classified
  }

  public enum CaptchaCheckResult
  {
    Success,
    NotFound,
    Failure
  }
}