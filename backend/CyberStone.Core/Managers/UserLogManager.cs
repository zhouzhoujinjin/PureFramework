using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Entities;
using CyberStone.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public class UserLogManager
  {
    private readonly CyberStoneDbContext context;
    private readonly UserManager userManager;

    public UserLogManager(CyberStoneDbContext context, UserManager userManager)
    {
      this.context = context;
      this.userManager = userManager;
    }

    public async Task AddLog(UserLog userLog)
    {
      var entity = new UserLogEntity
      {
        UserId = userLog.UserId,
        Url = userLog.Url ?? "",
        CreatedTime = userLog.CreatedTime,
        Ip = userLog.Ip ?? "",
        Device = userLog.Device ?? "",
        Duration = userLog.Duration,
        Method = userLog.Method ?? "",
        Data = userLog.Data ?? "",
        UserAgent = userLog.UserAgent ?? ""
      };

      context.Add(entity);
      await context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<UserLog>, int)> ListUserLogsAsync(Dictionary<string, string> conditions, int page, int size)
    {
      IQueryable<UserLogEntity> userLogs = context.UserLogs;

      foreach (var kv in conditions)
      {
        if (kv.Key == "data")
        {
          userLogs = userLogs.Where(u => u.Data.Contains(kv.Value));
        }
        else if (kv.Key == "device")
        {
          userLogs = userLogs.Where(u => u.Device.Contains(kv.Value));
        }
        else if (kv.Key == "userId")
        {
          if (long.TryParse(kv.Value, out var userId))
          {
            userLogs = userLogs.Where(u => u.UserId == userId);
          }
        }
        else if (kv.Key == "startDate")
        {
          if (DateTime.TryParse(kv.Value, out var startDate))
          {
            userLogs = userLogs.Where(u => u.CreatedTime >= startDate);
          }
        }
        else if (kv.Key == "endDate")
        {
          if (DateTime.TryParse(kv.Value, out var endDate))
          {
            userLogs = userLogs.Where(u => u.CreatedTime < endDate);
          }
        }
        else if (kv.Key == "url")
        {
          userLogs = userLogs.Where(u => u.Url.StartsWith(kv.Value));
        }
      }

      var count = await userLogs.CountAsync();
      var data = await userLogs.OrderByDescending(u => u.CreatedTime).Skip(Math.Max(page - 1, 0) * size).Take(size).ToArrayAsync();
      var list = new List<UserLog>();

      foreach (var u in data)
      {
        var ul = new UserLog
        {
          Id = u.Id,
          User = await userManager.GetBriefUserAsync(u.Id),
          UserId = u.UserId,
          CreatedTime = u.CreatedTime,
          Url = u.Url,
          UserAgent = u.UserAgent,
          Ip = u.Ip,
          Method = u.Method,
          Device = u.Device,
          Duration = u.Duration,
          Data = u.Method == "GET" || u.Method == "DELETE" ? u.Data : "«请求可能过大，通过详细查看»"
        };

        list.Add(ul);
      };
      return (list, count);
    }

    public async Task<UserLog?> GetUserLog(long userLogId)
    {
      var entity = await context.UserLogs.Where(x => x.Id == userLogId).FirstOrDefaultAsync();
      if (entity == null)
      {
        return null;
      }
      var userLog = new UserLog
      {
        Id = entity.Id,
        User = await userManager.GetBriefUserAsync(entity.Id),
        UserId = entity.UserId,
        CreatedTime = entity.CreatedTime,
        Url = entity.Url,
        UserAgent = entity.UserAgent,
        Ip = entity.Ip,
        Method = entity.Method,
        Device = entity.Device,
        Duration = entity.Duration,
        Data = entity.Data
      };
      return userLog;
    }

    public async Task RemoveUserLog(IEnumerable<long> userLogIds)
    {
      var entities = context.UserLogs.Where(x => userLogIds.Contains(x.Id));
      context.RemoveRange(entities);
      await context.SaveChangesAsync();
    }
  }
}