using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyberStone.Core.Utils
{
  public static class HttpClientExtensions
  {
    /// <summary>
    /// 执行HTTP POST请求。
    /// </summary>
    /// <param name="client">HttpClient</param>
    /// <param name="url">请求地址</param>
    /// <param name="textParams">请求参数</param>
    /// <param name="converter"></param>
    /// <returns>HTTP响应内容</returns>
    public static async Task<string> PostAsync(this HttpClient client, string url, IDictionary<string, string> textParams, Func<IDictionary<string, string>, string> converter)
    {
      using var reqContent = new StringContent(converter(textParams), Encoding.UTF8, "application/xml");
      using var resp = await client.PostAsync(url, reqContent);
      using var respContent = resp.Content;
      return await respContent.ReadAsStringAsync();
    }

    /// <summary>
    /// 执行HTTP POST请求。
    /// </summary>
    /// <param name="client">HttpClient</param>
    /// <param name="url">请求地址</param>
    /// <param name="queryStrings">请求参数</param>
    /// <param name="postData"></param>
    /// <returns>HTTP响应内容</returns>
    public static async Task<string> PostAsync(this HttpClient client, string url, IDictionary<string, string>? queryStrings, IDictionary<string, string> postData)
    {
      if (queryStrings is { Count: > 0 })
      {
        url = QueryHelpers.AddQueryString(url, queryStrings!);
      }
      //有一些已转换为json string的内容进行替换
      var str = JsonSerializer.Serialize(postData).Replace("\"@@", "").Replace("@@\"", "").Replace("\\u0022", "\"").Replace("\\\\", "\\");
      using var reqContent = new StringContent(str, Encoding.UTF8, "application/json");
      using var resp = await client.PostAsync(url, reqContent);
      using var respContent = resp.Content;
      return await respContent.ReadAsStringAsync();
    }

    public static async Task<object> GetAsync(this HttpClient client, string url, IDictionary<string, string>? queryStrings)
    {
      if (queryStrings is { Count: > 0 })
      {
        url = QueryHelpers.AddQueryString(url, queryStrings!);
      }
      using var resp = await client.GetAsync(url);
      using var respContent = resp.Content;
      if (respContent.Headers.ContentType?.MediaType
        is MediaTypeNames.Application.Json
        or MediaTypeNames.Text.Plain
        or MediaTypeNames.Text.Html
        or MediaTypeNames.Text.Xml)
      {
        return await respContent.ReadAsStringAsync();
      }

      var content = await respContent.ReadAsByteArrayAsync();
      return (content, respContent.Headers.ContentDisposition?.FileName, respContent.Headers.ContentType);
    }
  }
}