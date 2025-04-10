namespace CyberStone.Core.Utils
{
  public class UploadOptions
  {
    public string AbsolutePath { get; set; } = "";
    public string Path { get; set; } = "./";
    public string FolderMask { get; set; } = "yyyyMMdd";
    public string WebRoot { get; set; } = "/uploads";
    public string FileNameGenerator { get; set; } = "md5";
  }
}