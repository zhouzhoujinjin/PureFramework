using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CyberStone.Core.Entities
{
  public class DepartmentEntity:IEntity
  {
    public long Id { get; set; }
    public string Title { get; set; } = "";

    [JsonIgnore]
    public DepartmentEntity? Parent { get; set; }

    public long? ParentId { get; set; }
    public int Seq { get; set; }

    public long CreatorId { get; set; }
    public UserEntity? Creator { get; set; }
    public DateTime CreatedTime { get; set; }
    public List<DepartmentUserEntity> Users { get; set; } = [];
    public List<DepartmentEntity> Children { get; set; } = [];
    //long IEntity.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
  }

  public class DepartmentUserEntity
  {
    public long Id { get; set; }
    public long DepartmentId { get; set; }
    public DepartmentEntity? Department { get; set; }
    public long UserId { get; set; }
    public UserEntity? User { get; set; }
    public string? Position { get; set; }
    public short Level { get; set; }
    public bool IsUserMajorDepartment { get; set; }
  }
}