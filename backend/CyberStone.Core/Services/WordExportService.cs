using Aspose.Words;
using Aspose.Words.MailMerging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace CyberStone.Core.Services
{
  public class WordExportService
  {
    public WordExportService()
    {
      new License().SetLicense(
        new MemoryStream(
          Convert.FromBase64String("PExpY2Vuc2U+CiAgPERhdGE+CiAgICA8TGljZW5zZWRUbz5TdXpob3UgQXVuYm94IFNvZnR3YXJlIENvLiwgTHRkLjwvTGljZW5zZWRUbz4KICAgIDxFbWFpbFRvPnNhbGVzQGF1bnRlYy5jb208L0VtYWlsVG8+CiAgICA8TGljZW5zZVR5cGU+RGV2ZWxvcGVyIE9FTTwvTGljZW5zZVR5cGU+CiAgICA8TGljZW5zZU5vdGU+TGltaXRlZCB0byAxIGRldmVsb3BlciwgdW5saW1pdGVkIHBoeXNpY2FsIGxvY2F0aW9uczwvTGljZW5zZU5vdGU+CiAgICA8T3JkZXJJRD4yMDA2MDIwMTI2MzM8L09yZGVySUQ+CiAgICA8VXNlcklEPjEzNDk3NjAwNjwvVXNlcklEPgogICAgPE9FTT5UaGlzIGlzIGEgcmVkaXN0cmlidXRhYmxlIGxpY2Vuc2U8L09FTT4KICAgIDxQcm9kdWN0cz4KICAgICAgPFByb2R1Y3Q+QXNwb3NlLlRvdGFsIGZvciAuTkVUPC9Qcm9kdWN0PgogICAgPC9Qcm9kdWN0cz4KICAgIDxFZGl0aW9uVHlwZT5FbnRlcnByaXNlPC9FZGl0aW9uVHlwZT4KICAgIDxTZXJpYWxOdW1iZXI+OTM2ZTVmZDEtODY2Mi00YWJmLTk1YmQtYzhkYzBmNTNhZmE2PC9TZXJpYWxOdW1iZXI+CiAgICA8U3Vic2NyaXB0aW9uRXhwaXJ5PjIwMjEwODI3PC9TdWJzY3JpcHRpb25FeHBpcnk+CiAgICA8TGljZW5zZVZlcnNpb24+My4wPC9MaWNlbnNlVmVyc2lvbj4KICAgIDxMaWNlbnNlSW5zdHJ1Y3Rpb25zPmh0dHBzOi8vcHVyY2hhc2UuYXNwb3NlLmNvbS9wb2xpY2llcy91c2UtbGljZW5zZTwvTGljZW5zZUluc3RydWN0aW9ucz4KICA8L0RhdGE+CiAgPFNpZ25hdHVyZT5wSkpjQndRdnYxV1NxZ1kyOHFJYUFKSysvTFFVWWRrQ2x5THE2RUNLU0xDQ3dMNkEwMkJFTnh5L3JzQ1V3UExXbjV2bTl0TDRQRXE1aFAzY2s0WnhEejFiK1JIWTBuQkh1SEhBY01TL1BSeEJES0NGbWg1QVFZRTlrT0FxSzM5NVBSWmJRSGowOUNGTElVUzBMdnRmVkp5cUhjblJvU3dPQnVqT1oyeDc4WFE9PC9TaWduYXR1cmU+CjwvTGljZW5zZT4=")));
    }

    public Document GetDocument()
    {
      return new Document();
    }

    public Document GetDocument(string path)
    {
      return new Document(path);
    }

    /// <summary>
    /// 使用 Word MergeField （合并域）功能进行数据填充。
    /// 创建好模板docx，设置对应的MergeField（在 [插入] -> [文档部件] -> [域...] 中选择 [MergeField]，设置域名）。
    /// 如果要插入图片将域名设置为 `Image:域名` 的格式。
    /// </summary>
    /// <param name="templatePath">模板路径，可以为绝对路径，或相对于主程序的相对路径。</param>
    /// <param name="outputPath">输出路径，可以为绝对路径，或相对于主程序的相对路径。可以为doc、docx、pdf。</param>
    /// <param name="fields">传入的域字典。注意如果是要插入图片仅将key设为域名，不要包含 `Image:`。</param>
    /// <returns></returns>
    public void ExportTemplate(string templatePath, string outputPath, Dictionary<string, string> fields)
    {
      var document = new Document(templatePath);
      var keys = fields.Keys.ToArray();
      var values = fields.Values.ToArray();
      document.MailMerge.FieldMergingCallback = new HandleMergeImageField();
      document.MailMerge.Execute(keys, values);
      document.Save(outputPath);
    }

    /// <summary>
    /// 根据模板样式导出列表数据。
    /// 循环体需包裹在域 `TableStart:${listName}` 和域 `TableEnd:${listName}`之间
    /// </summary>
    /// <param name="templatePath">模板路径，可以为绝对路径，或相对于主程序的相对路径。</param>
    /// <param name="listName">模板中的循环列表名称</param>
    /// <param name="outputPath">输出路径，可以为绝对路径，或相对于主程序的相对路径。可以为doc、docx、pdf。</param>
    /// <param name="array">数据列表，列表元素为传入的域字典。注意如果是要插入图片仅将key设为域名，不要包含 `Image:`。</param>
    public void ExportList(string templatePath, string listName, string outputPath, List<Dictionary<string, string>> array)
    {
      var document = new Document(templatePath);
      var keys = array.First().Keys.ToArray();
      document.MailMerge.FieldMergingCallback = new HandleMergeImageField();

      var dataTable = new DataTable(listName);
      dataTable.Columns.AddRange(keys.Select(x => new DataColumn(x)).ToArray());
      array.ForEach(x => dataTable.Rows.Add(x.Values));
      document.MailMerge.ExecuteWithRegions(dataTable);
      document.Save(outputPath);
    }

    private class HandleMergeImageField : IFieldMergingCallback
    {
      public void FieldMerging(FieldMergingArgs args)
      {
      }

      public void ImageFieldMerging(ImageFieldMergingArgs args)
      {
        var path = args.FieldValue.ToString();
        using var fileStream = File.OpenRead(path);
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        args.ImageStream = memoryStream;
      }
    }
  }
}