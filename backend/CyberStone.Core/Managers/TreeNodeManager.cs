using CyberStone.Core.Entities;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberStone.Core.TreeFeature
{
  public class TreeNodeManager
  {
    private readonly DbSet<TreeNodeEntity> treeNodeSet;
    private readonly CyberStoneDbContext context;
    private readonly SettingManager settingManager;

    public TreeNodeManager(CyberStoneDbContext context, SettingManager settingManager)
    {
      this.treeNodeSet = context.Set<TreeNodeEntity>();
      this.context = context;
      this.settingManager = settingManager;
    }

    public async Task<TreeNodeEntity> CreateNodeAsync(string instanceType, long instanceId,long? parentInstanceId = null, Dictionary<string, object>? extendData = null)
    {
      var entity = new TreeNodeEntity
      {
        InstanceType = instanceType,
        InstanceId = instanceId,
        ExtendData = extendData
      };
      await treeNodeSet.AddAsync(entity);
      if (parentInstanceId != null)
      {
        var parent = await GetTreeNodeAsync(instanceType, parentInstanceId);
        entity.Parent = parent;
      }
      await context.SaveChangesAsync();
      return entity;
    }

    /// <summary>
    /// 根据实体Id获取其全部父实体
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="instanceType"></param>
    /// <param name="childInstanceId"></param>
    /// <param name="fetchListFunc">注入函数，根据实体Id列表获取实体的方法</param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetParentsAsync<TEntity>(string instanceType, long childInstanceId, Func<IEnumerable<long>, Task<IEnumerable<TEntity>>> fetchListFunc) where TEntity : IEntity
    {
      var parentIds = await GetParentIdsAsync(instanceType, childInstanceId);
      return await fetchListFunc(parentIds);
    }

    /// <summary>
    /// 根据实体Id获取其全部父实体Id
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="childInstanceId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> GetParentIdsAsync(string instanceType, long childInstanceId)
    {
      var child = await GetTreeNodeAsync(instanceType, childInstanceId);
      if (child != null && !string.IsNullOrEmpty(child.ParentIds))
      {
        var parentIds = child.ParentIds.Split(",").Where(x => !string.IsNullOrEmpty(x)).Select(x => long.Parse(x));
        var parentInstanceIds = await treeNodeSet.Where(x => x.InstanceType == instanceType && parentIds.Contains(x.Id)).Select(x => x.InstanceId).ToListAsync();
        return parentInstanceIds;
      }
      return Enumerable.Empty<long>();
    }

    /// <summary>
    /// 根据子实体获取其全部父实体列表
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="childInstance"></param>
    /// <param name="fetchListFunc">注入函数，根据实体Id列表获取实体的方法</param>
    /// <returns>父实体列表，如无父实体，返回一个空列表</returns>
    public async Task<IEnumerable<TEntity>> GetParentsAsync<TEntity>(TEntity childInstance, Func<IEnumerable<long>, Task<IEnumerable<TEntity>>> fetchListFunc) where TEntity : IEntity
    {
      var settings = await settingManager.GetGlobalSettings<TreeTypeSettings>();
      var type = typeof(TEntity).FullName;
      if (settings.Value.TryGetValue(type!, out var typeName))
      {
        type = typeName;
      }
      return await GetParentsAsync(type!, childInstance.Id, fetchListFunc);
    }

    /// <summary>
    /// 根据父实体获取其全部子实体列表
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="parentInstance"></param>
    /// <param name="fetchListFunc"></param>
    /// <param name="allOffspring">为 True 返回全部子实体，为 False 返回直接子实体（不包含孙代及以后的子实体）</param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetChildrenAsync<TEntity>(TEntity parentInstance, Func<IEnumerable<long>, Task<IEnumerable<TEntity>>> fetchListFunc, bool allOffspring = false) where TEntity : IEntity
    {
      var settings = await settingManager.GetGlobalSettings<TreeTypeSettings>();
      var type = typeof(TEntity).FullName;
      if (settings.Value.TryGetValue(type!, out var typeName))
      {
        type = typeName;
      }

      return await GetChildrenAsync(type!, parentInstance.Id, fetchListFunc, allOffspring);
    }

    /// <summary>
    /// 获取子代的实体数据
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="instanceType"></param>
    /// <param name="parentInstanceId"></param>
    /// <param name="fetchListFunc"></param>
    /// <param name="allOffspring">是否获取全部子代的数据，默认为 false</param>
    /// <returns></returns>
    public async Task<IEnumerable<TOutput>> GetChildrenDataAsync<TEntity, TOutput>(string instanceType, long? parentInstanceId, Func<IEnumerable<long>, Task<IEnumerable<TOutput>>> fetchListFunc, bool allOffspring = false) where TEntity : IEntity
    {
      var instanceIds = await GetChildrenIdsAsync(instanceType, parentInstanceId, allOffspring);

      return await fetchListFunc(instanceIds);
    }

    /// <summary>
    /// 根据实体 Id 获取 TreeNode 实体
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="instanceId"></param>
    /// <param name="withParent">是否包含其父节点</param>
    /// <returns></returns>
    public async Task<TreeNodeEntity?> GetTreeNodeAsync(string instanceType, long? instanceId, bool withParent = false)
    {
      var query = treeNodeSet.Where(x => x.InstanceType == instanceType && x.InstanceId == instanceId);
      if (withParent)
      {
        query = query.Include(x => x.Parent);
      }
      return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 根据实体 Id 获取以其为根节点的树型结构
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TreeNodeModel<TreeNodeEntity>>?> GetTreeHierarchicalAsync(string instanceType, long? instanceId)
    {
      var parent = await GetTreeNodeAsync(instanceType, instanceId);

      if (instanceId != null && parent == null)
      {
        return null;
      }

      var query = treeNodeSet.Where(x => x.InstanceType == instanceType);

      var tree = new Dictionary<long, TreeNodeModel<TreeNodeEntity>>();
      if (parent != null)
      {
        query = treeNodeSet.Where(x => x.ParentIds.Contains($",{parent.Id},"));
        tree.Add(parent.Id, new TreeNodeModel<TreeNodeEntity> { Data = parent });
      }

      var nodes = await query.OrderBy(x => x.Seq).ToListAsync();
      var nodesMap = nodes.ToDictionary(x => x.Id, x => x);

      var roots = new List<TreeNodeModel<TreeNodeEntity>>();
      foreach (var node in nodes)
      {
        if (nodesMap.TryGetValue(node.InstanceId, out var instance))
        {
          if (!tree.TryGetValue(node.Id, out var treeData))
          {
            treeData = new TreeNodeModel<TreeNodeEntity>();
            tree[node.Id] = treeData;
          }
          treeData.Data = instance;
          treeData.ExtendData = node.ExtendData;

          if (node.ParentId == null)
          {
            roots.Add(treeData);
          }
          else
          {
            if (!tree.TryGetValue(node.ParentId.Value, out _))
            {
              tree[node.ParentId.Value] = new TreeNodeModel<TreeNodeEntity>
              {
                Children = new List<TreeNodeModel<TreeNodeEntity>>()
              };
              (tree[node.ParentId.Value].Children as List<TreeNodeModel<TreeNodeEntity>>)!.Add(treeData);
            }
          }
        }
      }

      return parent == null ? roots : new List<TreeNodeModel<TreeNodeEntity>> { tree[parent.Id] };
    }

    /// <summary>
    /// 根据 instanceType 获取完整的树型结构
    /// </summary>
    /// <param name="instanceType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TreeNodeModel<TreeNodeEntity>>?> GetTreeHierarchicalAsync(string instanceType)
    {
      var query = treeNodeSet.Where(x => x.InstanceType == instanceType);

      var nodes = await query.OrderBy(x => x.Seq).ToListAsync();
      var nodesMap = nodes.ToDictionary(x => x.Id, x => x);

      var tree = new Dictionary<long, TreeNodeModel<TreeNodeEntity>>();
      var roots = new List<TreeNodeModel<TreeNodeEntity>>();
      foreach (var node in nodes)
      {
        if (nodesMap.TryGetValue(node.InstanceId, out var instance))
        {
          if (!tree.TryGetValue(node.Id, out var treeData))
          {
            treeData = new TreeNodeModel<TreeNodeEntity>();
            tree[node.Id] = treeData;
          }
          treeData.Data = instance;
          treeData.ExtendData = node.ExtendData;

          if (node.ParentId == null)
          {
            roots.Add(treeData);
          }
          else
          {
            if (!tree.TryGetValue(node.ParentId.Value, out var parentNode))
            {
              tree[node.ParentId.Value] = parentNode = new TreeNodeModel<TreeNodeEntity>
              {
                Children = new List<TreeNodeModel<TreeNodeEntity>>()
              };
              (tree[node.ParentId.Value].Children as List<TreeNodeModel<TreeNodeEntity>>)!.Add(treeData);
            }
          }
        }
      }
      return roots;
    }

    /// <summary>
    /// 获取实体树
    /// </summary>
    /// <remarks>
    /// <para>此方法有可能产生大量SQL查询，建议对结果进行缓存。</para>
    /// </remarks>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="instanceType"></param>
    /// <param name="parentInstanceId"></param>
    /// <param name="fetchListFunc"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TreeNodeModel<TEntity>>> GetHierarchicalAsync<TEntity>(string instanceType, long? parentInstanceId, Func<IEnumerable<long>, Task<IEnumerable<TEntity>>> fetchListFunc) where TEntity : IEntity
    {
      var parent = await GetTreeNodeAsync(instanceType, parentInstanceId);

      var query = treeNodeSet.Where(x => x.InstanceType == instanceType);

      if (parentInstanceId != null && parent == null)
      {
        return new List<TreeNodeModel<TEntity>>();
      }

      if (parent != null)
      {
        query = treeNodeSet.Where(x => x.ParentIds.Contains($",{parent.Id},"));
      }

      var nodes = await query.OrderBy(x => x.Seq).ToListAsync();
      var instanceIds = nodes.Select(x => x.InstanceId).ToList();
      var instances = (await fetchListFunc(instanceIds)).ToDictionary(x => x.Id, x => x);

      var tree = new Dictionary<long, TreeNodeModel<TEntity>>();
      var roots = new List<TreeNodeModel<TEntity>>();
      foreach (var node in nodes)
      {
        if (instances.TryGetValue(node.InstanceId, out var instance))
        {
          if (!tree.TryGetValue(node.Id, out var treeData))
          {
            treeData = new TreeNodeModel<TEntity>();
            tree[node.Id] = treeData;
          }
          treeData.Data = instance;
          treeData.ExtendData = node.ExtendData;

          if (node.ParentId == null)
          {
            roots.Add(treeData);
          }
          else
          {
            if (!tree.TryGetValue(node.ParentId.Value, out var parentNode))
            {
              tree[node.ParentId.Value] = parentNode = new TreeNodeModel<TEntity>
              {
                Children = new List<TreeNodeModel<TEntity>>()
              };
            }
            else if (tree[node.ParentId.Value].Children == null)
            {
              tree[node.ParentId.Value].Children = new List<TreeNodeModel<TEntity>>();
            }
            (tree[node.ParentId.Value].Children as List<TreeNodeModel<TEntity>>)!.Add(treeData);
          }
        }
      }

      return parent == null ? roots : new List<TreeNodeModel<TEntity>> { tree[parent.Id] };
    }

    /// <summary>
    /// 获取子代的实体 Id 列表
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="parentInstanceId"></param>
    /// <param name="allOffspring">是否获取全部子代的数据，默认为 false</param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> GetChildrenIdsAsync(string instanceType, long? parentInstanceId, bool allOffspring = false)
    {
      var parent = treeNodeSet.Where(x => x.InstanceType == instanceType && x.InstanceId == parentInstanceId).FirstOrDefault();
      if (parentInstanceId != null && parent == null)
      {
        return Enumerable.Empty<long>();
      }

      var query = treeNodeSet.Where(x => x.InstanceType == instanceType);
      if (parent != null)
      {
        query = allOffspring ? query.Where(x => x.ParentIds.Contains($",{parent.Id},")) : query.Where(x => x.ParentId == parent.Id);
      }
      else if (!allOffspring)
      {
        query = treeNodeSet.Where(x => x.ParentId == null);
      }
      var nodes = await query.OrderBy(x => x.ParentId).ThenBy(x => x.Seq).Select(x => x.InstanceId).ToListAsync();
      return nodes;
    }

    /// <summary>
    /// 根据实体 Id 获取其子实体
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="instanceType"></param>
    /// <param name="parentInstanceId"></param>
    /// <param name="fetchListFunc"></param>
    /// <param name="allOffspring">是否获取全部子代的数据，默认为 false</param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetChildrenAsync<TEntity>(string instanceType, long? parentInstanceId, Func<IEnumerable<long>, Task<IEnumerable<TEntity>>> fetchListFunc, bool allOffspring = false) where TEntity : IEntity
    {
      var instanceIds = await GetChildrenIdsAsync(instanceType, parentInstanceId, allOffspring);

      return await fetchListFunc(instanceIds);
    }

    /// <summary>
    /// 更新某个实体的层级关系
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="childInstanceId"></param>
    /// <param name="parentInstanceId">父节点 Id，为 null 代表根节点</param>
    /// <returns></returns>
    public async Task<bool> UpdateHierarchicalAsync(string instanceType, long childInstanceId, long? parentInstanceId)
    {
      var tree = await GetTreeHierarchicalAsync(instanceType, childInstanceId);
      if (tree == null)
      {
        return false;
      }
      TreeNodeEntity? parent = null;
      if (parentInstanceId != null)
      {
        parent = await GetTreeNodeAsync(instanceType, parentInstanceId);
      }

      CalculateHierarchical(parent, tree);
      return await context.SaveChangesAsync() > 0;
    }

    public async Task UpdateExtendDataAsync(long id, Dictionary<string, object> extendData)
    {
      var entity = await treeNodeSet.FirstOrDefaultAsync(x => x.Id == id);
      if (entity != null)
      {
        entity.ExtendData = extendData;
        treeNodeSet.Update(entity);
        await context.SaveChangesAsync();
      }
    }

    /// <summary>
    /// 删除节点
    /// </summary>
    /// <param name="instanceType"></param>
    /// <param name="instanceId"></param>
    /// <param name="withChildren">是否删除子节点，为否时，子节点的父节点将指向待删除节点的父节点。</param>
    /// <returns></returns>
    public async Task<bool> RemoveNodeAsync(string instanceType, long instanceId, bool withChildren = false)
    {
      var root = await GetTreeNodeAsync(instanceType, instanceId, true);
      if (root == null)
      {
        return false;
      }
      var tree = await GetTreeHierarchicalAsync(instanceType, instanceId);
      if (tree == null)
      {
        return false;
      }

      if (withChildren)
      {
        LoopNode(tree, model =>
        {
          treeNodeSet.Remove(model.Data!);
        });
      }
      else
      {
        CalculateHierarchical(root.Parent, tree);
      }

      treeNodeSet.Remove(root);
      return await context.SaveChangesAsync() > 0;
    }

    private static void CalculateHierarchical(TreeNodeEntity? parent, IEnumerable<TreeNodeModel<TreeNodeEntity>> children)
    {
      var i = 1;
      foreach (var child in children)
      {
        child.Data!.ParentIds = GenerateParentIdsStr(parent?.ParentIds, parent?.Id);
        child.Data.ParentId = parent?.Id;
        child.Data.Seq = i++;
        if (child.Children != null && child.Children.Any())
        {
          LoopNode(
            child.Children,
            loopBeforeAction: (treeNode) =>
              {
                treeNode.Data!.ParentIds = GenerateParentIdsStr(treeNode.Data.Parent?.ParentIds, treeNode.Data.Id);
              }
          );
        }
      }
    }

    private static string GenerateParentIdsStr(string? parentIdsOfParent, long? parentId)
    {
      if (parentIdsOfParent != null && parentId == null)
      {
        throw new ArgumentException($"数据错误, {nameof(parentIdsOfParent)} != null && {nameof(parentId)} == null");
      }

      var parentIds = string.IsNullOrEmpty(parentIdsOfParent) ? ["", ""] : parentIdsOfParent.Split(",").ToList();
      if (parentId != null)
      {
        parentIds.Insert(parentIds.Count - 1, parentId.ToString()!);
      }

      return string.Join(",", parentIds);
    }

    /// <summary>
    /// 遍历树型结构
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="list"></param>
    /// <param name="loopBeforeAction">遍历子节点前的操作</param>
    /// <param name="loopAfterAction">遍历子节点后的操作</param>
    public static void LoopNode<TData>(IEnumerable<TreeNodeModel<TData>> list, Action<TreeNodeModel<TData>>? loopBeforeAction = null, Action<TreeNodeModel<TData>>? loopAfterAction = null)
    {
      foreach (var node in list)
      {
        loopBeforeAction?.Invoke(node);
        if (node.Children != null && node.Children.Any())
        {
          LoopNode(node.Children, loopBeforeAction);
        }
        loopAfterAction?.Invoke(node);
      }
    }

    public static void LoopMapNode<TData, TOutput>(IEnumerable<TreeNodeModel<TData>> list, Func<TData?, TOutput> func, out List<TreeNodeModel<TOutput>> result)
    {
      result = new List<TreeNodeModel<TOutput>>();
      foreach (var node in list)
      {
        var output = func.Invoke(node.Data);
        if (output != null)
        {
          var treeData = new TreeNodeModel<TOutput>
          {
            Data = output,
            ExtendData = node.ExtendData
          };
          result.Add(treeData);
          if (node.Children != null && node.Children.Any())
          {
            LoopMapNode(node.Children, func, out var children);
            treeData.Children = children;
          }
        }
      }
    }

    public static void LoopNodeToOtherType<TData, TOutput>(IEnumerable<TreeNodeModel<TData>> list, Func<TreeNodeModel<TData>, TOutput> func, List<TOutput> result) where TOutput : ITreeNode<TOutput>
    {
      foreach (var node in list)
      {
        var output = func.Invoke(node);
        if (output != null)
        {
          result.Add(output);
          if (node.Children != null && node.Children.Any())
          {
            var children = new List<TOutput>();
            LoopNodeToOtherType(node.Children, func, children);
            output.Children = children;
          }
        }
      }
    }

    public static async Task LoopMapNodeAsync<TData, TOutput>(IEnumerable<TreeNodeModel<TData>> list, Func<TData?, Task<TOutput>> func, List<TreeNodeModel<TOutput>> result)
    {
      foreach (var node in list)
      {
        var output = await func.Invoke(node.Data);
        if (output != null)
        {
          var treeData = new TreeNodeModel<TOutput>
          {
            Data = output,
            ExtendData = node.ExtendData
          };
          result.Add(treeData);
          if (node.Children != null && node.Children.Any())
          {
            var children = new List<TreeNodeModel<TOutput>>();
            await LoopMapNodeAsync(node.Children, func, children);
            treeData.Children = children;
          }
        }
      }
    }
  }
}