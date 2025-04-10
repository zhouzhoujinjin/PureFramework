using System;

namespace CyberStone.Core
{
  public interface IEntity
  {
    long Id { get; set; }
  }

  public interface IEntityWithCreator : IEntity, IHasCreationTime, IHasCreator
  {
  }

  public interface IEntityWithCreatorAndModifier : IEntity, IHasCreationTime, IHasLastModifiedTime, IHasCreator, IHasModifier 
  {
  }

  public interface IEntityWithCreatorAndDeleter : IEntity, IHasCreationTime, IHasDeletionTime, IHasCreator, IHasDeleter
  {
  }

  public interface IHasCreationTime
  {
    DateTime CreatedTime { get; set; }
  }

  public interface IHasCreator
  {
    ulong CreatorId { get; set; }
  }

  public interface IHasLastModifiedTime
  {
    DateTime LastModifiedTime { get; set; }
  }

  public interface IHasModifier
  {
    ulong ModifierId { get; set; }
  }

  public interface IHasDeletionTime
  {
    bool IsDeleted { get; set; }
    DateTime? DeletedTime { get; set; }
  }

  public interface IHasDeleter
  {
    ulong DeleterId { get; set; }
  }
}