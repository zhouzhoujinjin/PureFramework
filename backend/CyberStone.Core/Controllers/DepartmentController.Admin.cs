using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  #region v6
  public partial class DepartmentController
  {



    [HttpGet("/api/admin/departments", Name = "部门树")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetAdminDepartments()
    {
      var department = await _departmentManager.GetDepartmentTreeAsync();
      return new AjaxResp<Department>
      {
        Data = department
      };
    }

    [HttpPost("/api/admin/departments", Name = "添加部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> AddBaseDepartment([FromBody] Department department)
    {
      var creatorId = HttpContext.GetUserId();
      var currentDepartment = await _departmentManager.GetDepartmentTreeAsync();
      var result = await _departmentManager.CreateAsync(department.Title, creatorId, currentDepartment?.Id);
      return new AjaxResp<Department>
      {
        Data = result
      };
    }

    [HttpPost("/api/admin/departments/struct", Name = "更新部门结构")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdateDepartmentStruct([FromBody] Dictionary<long, IEnumerable<long>> structs)
    {
      await _departmentManager.UpdateStructsAsync(structs);
      return new AjaxResp { };
    }

    [HttpPut("/api/admin/departments/{departmentId}", Name = "更新部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdateDepartment(long departmentId, [FromBody] Department department)
    {
      await _departmentManager.UpdateDepartmentAsync(departmentId, department);
      return new AjaxResp
      {
        Message = "操作成功"
      };
    }

    [HttpDelete("/api/admin/departments/{departmentId}", Name = "删除部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> DeleteDepartment(int departmentId)
    {
      await _departmentManager.DeleteAsync(departmentId);
      return new AjaxResp();
    }

  }
  #endregion

  #region v7
  //public partial class DepartmentController : ControllerBase
  //{
  //  [HttpGet("/api/admin/departments", Name = "管理员 - 部门树")]
  //  public async Task<AjaxResp<ICollection<Department>?>> TreeAsync()
  //  {
  //    var departmentTree = await departmentManager.GetDepartmentTree();
  //    return new AjaxResp<ICollection<Department>?> { Data = departmentTree };
  //  }

  //  [HttpGet("/api/admin/departments/{depId}", Name = "管理员 - 部门用户列表")]
  //  public async Task<AjaxResp<Department>?> DepartmentUsersAsync(long depId)
  //  {
  //    var department = await departmentManager.GetDepartmentWithUsers(depId);
  //    return new AjaxResp<Department> { Data = department };
  //  }

  //  [HttpGet("/api/admin/departments/users/{userId}", Name = "管理员 - 用户部门列表")]
  //  public async Task<AjaxResp<ICollection<UserDepartment>>?> UserDepartmentsAsync(long userId)
  //  {
  //    var departments = await departmentManager.GetUserDepartments(userId);
  //    return new AjaxResp<ICollection<UserDepartment>> { Data = departments };
  //  }

  //  [HttpPost("/api/admin/departments", Name = "管理员 - 创建部门")]
  //  public async Task<AjaxResp> CreateAsync([FromBody] Department department)
  //  {
  //    var currentUserId = HttpContext.GetUserId();
  //    await departmentManager.AddDepartment(department, currentUserId);
  //    return new AjaxResp { Message = "创建部门成功" };
  //  }

  //  [HttpPost("api/admin/departments/{departmentId}/update", Name = "管理员 - 更新部门及部门人员信息")]
  //  public async Task<AjaxResp> UpdateAsync(long departmentId, [FromBody] Department department)
  //  {
  //    await departmentManager.UpdateDepartment(departmentId, department);
  //    await departmentManager.UpdateDepartmentUser(departmentId, department);
  //    return new AjaxResp { Message = "更新部门信息成功" };
  //  }

  //  [HttpPost("api/admin/departments/{departmentId}/parent", Name = "管理员 - 更新部门结构")]
  //  public async Task<AjaxResp> UpdateAsync(long departmentId, [FromBody] long? parentDepartmentId)
  //  {
  //    await departmentManager.UpdateHierarchical(departmentId, parentDepartmentId);
  //    return new AjaxResp { };
  //  }

  //  [HttpPost("api/admin/departments/{deptId}/delete", Name = "管理端 - 删除部门")]
  //  public async Task<AjaxResp> DeleteAsync(long deptId)
  //  {
  //    await departmentManager.DeleteDepartment(deptId);
  //    return new AjaxResp { Message = "删除成功" };
  //  }
  //}
  #endregion
}