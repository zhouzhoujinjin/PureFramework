using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CyberStone.Core.Controllers
{
  #region v6  

  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
  [Route("/api/departments")]
  [ApiController]
  public partial class DepartmentController : ControllerBase
  {
    private readonly DepartmentManager _departmentManager;

    public DepartmentController(DepartmentManager departmentManager)
    {
      _departmentManager = departmentManager;
    }

    [HttpGet("/api/departments", Name = "部门列表")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetDepartments()
    {
      var department = await _departmentManager.GetDepartmentTreeAsync();
      return new AjaxResp<Department>
      {
        Data = department
      };
    }

    [HttpGet("/api/departments/{departmentId}", Name = "部门信息")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetDepartment(long departmentId)
    {
      var model = await _departmentManager.GetDepartmentWithUsersAsync(departmentId);

      return new AjaxResp<Department>
      {
        Data = model
      };
    }
  }
  #endregion

  #region v7
  //[ApiController]
  //[Route("", Name = "部门管理")]
  ////[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
  //public partial class DepartmentController(DepartmentManager departmentManager) : ControllerBase
  //{
  //  [HttpGet("/api/departments", Name = "用户 - 部门列表")]
  //  public async Task<AjaxResp<ICollection<Department>?>> ListAsync()
  //  {
  //    var departments = await departmentManager.ListAllAsync();
  //    return new AjaxResp<ICollection<Department>?> { Data = departments };
  //  }

  //  [HttpGet("/api/departments/users", Name = "用户 - 全部用户（部门）列表")]
  //  public async Task<AjaxResp<ICollection<User>?>> ListWithDepartment()
  //  {
  //    var userList = await departmentManager.GetAllUsersWithDepartmentsRoles();
  //    return new AjaxResp<ICollection<User>?>
  //    {
  //      Data = userList,
  //    };
  //  }

  //}
  #endregion
}