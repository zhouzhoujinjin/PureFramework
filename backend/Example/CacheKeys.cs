using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend
{
  static class CacheKeys
  {
    public const string CurrentSemester = "CurrentSemester";
    public const string SemesterStudents = "SemesterStudents_{0}";
    public const string UserProfileWithDepartmentCacheKeyValue = "UsersWithDepartmentCacheKeyValue";
  }
}
