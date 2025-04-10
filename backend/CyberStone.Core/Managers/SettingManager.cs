using Microsoft.EntityFrameworkCore;
using CyberStone.Core.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CyberStone.Core.Managers
{
  public class SettingManager
  {
    private readonly DbSet<SettingEntity> settings;
    private readonly CyberStoneDbContext context;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
      Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      PropertyNameCaseInsensitive = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      Converters =
      {
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
      }
    };

    public SettingManager(CyberStoneDbContext context)
    {
      this.context = context;
      settings = context.Settings;
    }

    public async Task SaveInstanceSettingAsync<T>(string instanceType, long instanceId, T? setting) where T : new()
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.ClassName == type.FullName && x.InstanceId == instanceId);
      if (setting == null && entity != null)
      {
        context.Remove(entity);
        await context.SaveChangesAsync();
        return;
      }

      if (entity == null)
      {
        settings.Add(new SettingEntity
        {
          InstanceType = instanceType,
          InstanceId = instanceId,
          ClassName = type.FullName!,
          Value = JsonSerializer.Serialize(setting, jsonSerializerOptions)
        });
      }
      else
      {
        entity.Value = JsonSerializer.Serialize(setting, jsonSerializerOptions);
      }

      await context.SaveChangesAsync();
    }

    public async Task SaveGlobalSettingAsync<T>(T setting) where T : new()
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.InstanceType == null && x.ClassName == type.FullName && x.InstanceId == 0);
      if (entity == null)
      {
        settings.Add(new SettingEntity
        {
          InstanceId = 0,
          ClassName = type.FullName!,
          Value = JsonSerializer.Serialize(setting, jsonSerializerOptions)
        });
      }
      else
      {
        entity.Value = JsonSerializer.Serialize(setting, jsonSerializerOptions);
      }

      await context.SaveChangesAsync();
    }

    public async Task ResetInstanceSettingAsync<T>(string instanceType, long instanceId)
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.InstanceType == instanceType && x.InstanceId == instanceId && x.ClassName == type.FullName);
      if (entity != null)
      {
        settings.Remove(entity);
        await context.SaveChangesAsync();
      }
    }

    public async Task<T> GetGlobalSettings<T>(T defaultValue) where T : new()
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.InstanceType == null && x.InstanceId == 0 && x.ClassName == type.FullName);
      if (entity is { Value: { } }) return JsonSerializer.Deserialize<T>(entity.Value, jsonSerializerOptions)!;
      var value = defaultValue ?? new T();
      entity = new SettingEntity()
      {
        InstanceType = null,
        InstanceId = 0,
        ClassName = type.FullName!,
        Value = JsonSerializer.Serialize(value, jsonSerializerOptions)
      };
      settings.Add(entity);
      await context.SaveChangesAsync();
      return value;
    }

    public async Task<T> GetGlobalSettings<T>() where T : new()
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.InstanceId == 0 && x.ClassName == type.FullName);
      if (entity is { Value: { } }) return JsonSerializer.Deserialize<T>(entity.Value, jsonSerializerOptions)!;
      var value = new T();
      entity = new SettingEntity()
      {
        InstanceType = null,
        InstanceId = 0,
        ClassName = type.FullName!,
        Value = JsonSerializer.Serialize(value, jsonSerializerOptions)
      };
      settings.Add(entity);
      await context.SaveChangesAsync();
      return value;
    }

    public async Task<T> GetInstanceSettingAsync<T>(string instanceType, long instanceId) where T : new()
    {
      var type = typeof(T);
      var entity = await settings.SingleOrDefaultAsync(x => x.InstanceType == instanceType && x.InstanceId == instanceId && x.ClassName == type.FullName);
      if (entity is { Value: { } }) return JsonSerializer.Deserialize<T>(entity.Value, jsonSerializerOptions)!;
      var value = new T();
      entity = new SettingEntity
      {
        InstanceType = instanceType,
        InstanceId = instanceId,
        ClassName = type.FullName!,
        Value = JsonSerializer.Serialize(value, jsonSerializerOptions)
      };
      settings.Add(entity);
      await context.SaveChangesAsync();
      return value;
    }
  }
}