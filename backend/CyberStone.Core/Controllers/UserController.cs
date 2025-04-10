using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [Route("/api/admin/users", Name = "用户")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly UserManager userManager;

    public UserController(
      UserManager userManager
    )
    {
      this.userManager = userManager;
    }

    [HttpGet("/api/users", Name = "用户搜索")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<User>> FindUser(string? query = null, int page = 1, int size = 1000)
    {
      Dictionary<string, string> dict;
      if (query != null)
      {
        dict = JsonSerializer.Deserialize<Dictionary<string, string>>(query) ?? new Dictionary<string, string>();
      }
      else
      {
        dict = new Dictionary<string, string>();
      }

      var users = await userManager.FindUsersAsync(dict, page, size);
      var total = await userManager.FindUsersCountAsync(dict);
      return new PagedAjaxResp<User>
      {
        Code = 0,
        Total = total,
        Page = page,
        Data = users
      };
    }

    [HttpGet(Name = "用户列表")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<AdminUser>> AdminUsers(string? query = null, int page = 1, int size = 20)
    {
      var dict = new Dictionary<string, string>
      {
        { "deleted", "false" },
        { "visible", "true" }
      };
      if (query != null)
      {
        dict["userName"] = query;
      }

      var (users, total) = await userManager.ListUsersWithRolesAsync(dict, page, size);

      return new PagedAjaxResp<AdminUser>
      {
        Total = total,
        Page = page,
        Data = users
      };
    }

    [HttpGet("deleted", Name = "已删除用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<AdminUser>> AdminUsersDeleted(string? query = null, int page = 1, int size = 20)
    {
      var dict = new Dictionary<string, string>
      {
        { "deleted", "true" }
      };
      if (query != null)
      {
        dict["fullName"] = query;
        dict["userName"] = query;
      }

      var (users, total) = await userManager.ListUsersWithRolesAsync(dict, page, size);

      return new PagedAjaxResp<AdminUser>
      {
        Total = total,
        Page = page,
        Data = users
      };
    }

    [HttpGet("invisible", Name = "已隐藏用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<PagedAjaxResp<AdminUser>> AdminUsersInvisible(string? query = null, int page = 1,
      int size = 20)
    {
      var dict = new Dictionary<string, string>
      {
        { "deleted", "false" },
        { "visible", "false" }
      };
      if (query != null)
      {
        dict["fullName"] = query;
        dict["userName"] = query;
      }

      var (users, total) = await userManager.ListUsersWithRolesAsync(dict, page, size);

      return new PagedAjaxResp<AdminUser>
      {
        Total = total,
        Page = page,
        Data = users
      };
    }

    [HttpPost(Name = "添加用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> Add([FromBody] User user)
    {
      var entity = (await userManager.AddUserAsync(user.UserName, true));
      var userId = entity?.Id ?? 0;
      var profiles = new Dictionary<string, JsonElement>();
      foreach (var profile in user.Profiles)
      {
        var value = (JsonElement?)profile.Value;
        if (value != null)
        {
          profiles[profile.Key] = value.Value;
        }
      }
      if (userId > 0)
      {
        await userManager.AddProfilesAsync(userId, profiles);
      }

      user.Id = userId;
      return new AjaxResp<User>
      {
        Message = "保存成功",
        Data = user
      };
    }

    [HttpPost("{userName}", Name = "修改用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> Update(string userName, [FromBody] User user)
    {
      var userId = (await userManager.FindByNameAsync(userName))?.Id;
      if (userId == null)
      {
        return new AjaxResp<User>
        {
          Message = "未找到用户"
        };
      }

      var profiles = new Dictionary<string, JsonElement>();
      if (user.Profiles != null)
      {
        foreach (var profile in user.Profiles)
        {
          var value = (JsonElement?)profile.Value;
          if (value != null)
          {
            profiles[profile.Key] = value.Value;
          }
        }
      }
      await userManager.AddProfilesAsync(userId.Value, profiles);

      return new AjaxResp<User>
      {
        Message = "保存成功",
        Data = user
      };
    }

    [HttpPost("{userName}/delete", Name = "停用用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<bool>> DeleteUser(string userName)
    {
      var data = await userManager.SetDeletedAsync(userName, true);
      return new AjaxResp<bool> { Data = data };
    }

    [HttpPost("{userName}/active", Name = "激活用户")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<bool>> ActiveUser(string userName)
    {
      var data = await userManager.SetDeletedAsync(userName, false);
      return new AjaxResp<bool> { Data = data };
    }

    [HttpPost("{userName}/resetPassword", Name = "重置密码")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<bool>> ResetPassword(string userName)
    {
      var data = await userManager.ResetPasswordAsync(userName);
      return new AjaxResp<bool> { Data = data };
    }

    //[HttpPost("batch", Name = "批量添加用户")]
    //[UserLog(UserLogLevel.Classified)]
    //public async Task<AjaxResp<IEnumerable<User>>> BatchAdd(IEnumerable<IFormFile> files)
    //{
    //  return null;
    //}

    [HttpGet("{userName}/isExist", Name = "判断用户是否存在")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<bool>> IsExistUser(string userName)
    {
      var data = await userManager.IsExistUserAsync(userName);
      return new AjaxResp<bool> { Data = data };
    }

    [HttpPost("/api/users/avatar", Name = "上传头像")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public AjaxResp UploadAvatar(IFormFile avatar)
    {
      var data = userManager.UploadImage(avatar);
      return new AjaxResp { Data = data };
    }

    [HttpGet("{userName}", Name = "管理员 - 用户基础信息")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> GetBaseUserAsync(string userName)
    {
      var user = await userManager.FindByNameAsync(userName);
      if (user == null)
      {
        return new AjaxResp<User>
        {
          Code = 404,
          Message = $"没有找到此用户 [{userName}]"
        };
      }
      var data = await userManager.GetBriefUserAsync(user, ["public", "surname", "givenName", "phoneNumber"]);
      return new AjaxResp<User> { Data = data };
    }

    [HttpGet("/api/users/{userName}", Name = "用户扼要信息")]
    [Authorize(Policy = ClaimNames.ApiPermission)]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<User>> GetBriefUserAsync(string userName)
    {
      var user = await userManager.FindByNameAsync(userName);
      if (user == null)
      {
        return new AjaxResp<User>
        {
          Code = 404,
          Message = $"没有找到此用户 [{userName}]"
        };
      }
      var data = await userManager.GetBriefUserAsync(user, new List<string>() { "public", "SurName", "GivenName" });
      return new AjaxResp<User> { Data = data };
    }
  }
}