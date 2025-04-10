using System;
using System.Collections.Generic;

namespace CyberStone.Core
{
  public class AjaxResp<T>
  {
    public int Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
  }

  public class AjaxResp : AjaxResp<object>
  {
    //public static AjaxResp<T> Create<T>(T data, string? message = null, int code = 0)
    //{
    //  return new AjaxResp<T>
    //  {
    //    Code = code,
    //    Message = message,
    //    Data = data
    //  };
    //}
    public static AjaxResp Create(object? data, string? message = null, int code = 0)
    {
      return new AjaxResp
      {
        Code = code,
        Message = message,
        Data = data
      };
    }
  }

  public class PagedAjaxResp<T> : AjaxResp<IEnumerable<T>>
  {
    /// <summary>
    /// 总记录条数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 是否还有更多记录
    /// </summary>
    public bool HasMore { get; set; } = false;

    public static PagedAjaxResp<T> Create(IEnumerable<T> data, int totalRecordCount, int page = 1, int size = 20, int code = 0, string? message = null)
    {
      var total = (int)Math.Ceiling(totalRecordCount / (double)size);
      return new PagedAjaxResp<T>
      {
        Data = data,
        Page = page,
        Total = totalRecordCount,
        HasMore = page < total,
        Code = code,
        Message = message
      };
    }
  }

  public class PagedAjaxResp : PagedAjaxResp<object>
  { }
}