namespace CyberStone.Core
{
  internal static class CacheKeys
  {
    public const string Users = ":Users:{0}";
    public const string BriefProfileKeys = ":BriefProfileKeys";
    public const string AllProfileKeys = ":AllProfileKeys";
    public const string UserClaims = ":UserClaims:{0}";
    public const string UserProfiles = ":UserProfiles:{0}";
    public const string Menu = ":Menu";
    public const string CaptchaKey = ":Captchaes:{0}";
    public const string DepartmentList = ":DepartmentList";
    public const string DepartmentTree = ":DepartmentTree";
    public const string AllUsersWithDepartmentsRoles = ":AllUsersWithDepartmentsRoles";
  }

  public static class ClaimNames
  {
    public const string ApiPermission = "api";
    public const string ActionPermission = "action";
    public const string RoutePermission = "route";
    public const string DefaultCorsPolicy = "cors";
  }

  public static class SystemProfileKeyCategory
  {
    public const string Public = "public";
    public const string Secret = "secret";
    public const string Contact = "contact";
    public const string Family = "family";
    public const string Personal = "personal";
  }
}