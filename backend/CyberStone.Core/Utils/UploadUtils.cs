using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace CyberStone.Core.Utils
{
  public static class UploadUtils
  {
    public static string CreateBase64Image(string base64, string rootPath, bool useDate = true)
    {
      if (string.IsNullOrEmpty(base64))
      {
        throw new ArgumentNullException(nameof(base64));
      }

      var indexOfSemiColon = base64.IndexOf(";", StringComparison.OrdinalIgnoreCase);

      var dataLabel = base64.Substring(0, indexOfSemiColon);

      var contentType = dataLabel.Split(':').Last();
      var extension = contentType.Split('/').Last();

      var startIndex = base64.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;

      var fileContents = base64.Substring(startIndex);
      return CreateFile(fileContents, "." + extension, rootPath, useDate);
    }

    public static string CreateFile(string base64, string extension, string rootPath, bool useDate = true)
    {
      string path = rootPath;
      if (useDate)
      {
        path = Path.Combine(path, DateTime.Now.ToString("yyyyMM"));
      }

      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      var fileName = $"{DateTimeOffset.Now.Ticks}".ComputeMd5() + extension;
      var filePath = Path.Combine(path, fileName);

      byte[] byteBuffer = Convert.FromBase64String(base64);
      File.WriteAllBytes(filePath, byteBuffer);
      return filePath;
    }

    public static string MoveFile(IFormFile file, string rootPath, bool useDate = true, string forceExt = "")
    {
      string path = rootPath;
      if (useDate)
      {
        path = Path.Combine(path, DateTime.Now.ToString("yyyyMMdd"));
      }

      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }

      var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTimeOffset.Now:yyyyMMddHHmmss}" + Path.GetExtension(file.FileName)+forceExt;
      var filePath = Path.Combine(path, fileName);

      using var stream = new FileStream(filePath, FileMode.Create);
      file.CopyTo(stream);
      stream.Flush();

      return filePath;
    }

    public static string GetUrl(string filePath, string rootPath, string webRoot)
    {
      var url = filePath.Replace(rootPath, webRoot);
      url = url.Replace(Path.DirectorySeparatorChar, '/');
      return url;
    }

    public static string GetUrl(string filePath, UploadOptions options)
    {
      return GetUrl(filePath, options.AbsolutePath, options.WebRoot);
    }

    public static string GetPhysicalPath(string url, string webRoot, string absoluteBasePath)
    {
      return url.Replace(webRoot, absoluteBasePath);
    }

    public static string GetPhysicalPath(string url, UploadOptions options)
    {
      return url.Replace(options.WebRoot, options.AbsolutePath).Replace('/', Path.DirectorySeparatorChar);
    }
  }
}