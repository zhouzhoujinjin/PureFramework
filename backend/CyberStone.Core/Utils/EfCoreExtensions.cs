using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CyberStone.Core.Utils
{
  public static class EfCoreExtensions
  {
    public static string? ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
    {
      var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
      var relationalCommandCache = enumerator.Private("_relationalCommandCache");
      if (relationalCommandCache == null) return null;
      var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
      var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

      if (factory == null) return null;
      var sqlGenerator = factory.Create();
      if (selectExpression == null) return null;
      var command = sqlGenerator.GetCommand(selectExpression);

      string sql = command.CommandText;
      return sql;
    }

    private static object? Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);

    private static T? Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj)!;
  }
}