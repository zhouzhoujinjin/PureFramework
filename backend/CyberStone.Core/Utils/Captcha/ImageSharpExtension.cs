using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace CyberStone.Core.Utils.Captcha
{
  public static class ImageSharpExtension
  {

    /// <summary>
    /// 绘制中文字符（可以绘制字母数字，但样式可能需要改）
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    /// <param name="processingContext"></param>
    /// <param name="containerWidth"></param>
    /// <param name="containerHeight"></param>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <param name="font"></param>
    /// <returns></returns>
    public static IImageProcessingContext DrawingCnText(this IImageProcessingContext processingContext,
      int containerWidth, int containerHeight, string text, Rgba32 color, Font font)
    {

      if (string.IsNullOrEmpty(text)) return processingContext;

      var size = processingContext.GetCurrentSize();
      var random = new Random();
      var textWidth = size.Width / text.Length;
      var img2Size = Math.Min(textWidth, size.Height);
      var fontMiniSize = (int)(img2Size * 0.6);
      var fontMaxSize = (int)(img2Size * 0.95);

      for (int i = 0; i < text.Length; i++)
      {
        using Image img2 = new Image<Rgba32>(img2Size, img2Size);
        var scaledFont = new Font(font, random.Next(fontMiniSize, fontMaxSize));
        var point = new Point(i * textWidth, (containerHeight - img2.Height) / 2);
        var textGraphicsOptions = new RichTextOptions(font)
        {
          Origin = new Point(0, 0),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Top
        };

        img2.Mutate(ctx => ctx.DrawText(textGraphicsOptions, text[i].ToString(), color: color)
          .Rotate(random.Next(-45, 45))
        );
        processingContext.DrawImage(img2, point, 1);
      }

      return processingContext;
    }

    public static IImageProcessingContext DrawingEnText(this IImageProcessingContext processingContext,
      int containerWidth, int containerHeight, string text, string[] colorHexArr, Font[] fonts)
    {
      if (string.IsNullOrEmpty(text))
      {
        return processingContext;
      }

      var size = processingContext.GetCurrentSize();

      var random = new Random();
      var textWidth = (size.Width / text.Length);
      var img2Size = Math.Min(textWidth, size.Height);
      var fontMiniSize = (int)(img2Size * 0.9);
      var fontMaxSize = (int)(img2Size * 1.37);
      var fontStyleArr = Enum.GetValues(typeof(FontStyle));

      for (int i = 0; i < text.Length; i++)
      {
        using Image img2 = new Image<Rgba32>(img2Size, img2Size);
        var scaledFont = new Font(fonts[random.Next(0, fonts.Length)], random.Next(fontMiniSize, fontMaxSize),
          (FontStyle)fontStyleArr.GetValue(random.Next(fontStyleArr.Length))!);
        var point = new Point(i * textWidth, (containerHeight - img2.Height) / 2);
        var colorHex = colorHexArr[random.Next(0, colorHexArr.Length)];
        var textGraphicsOptions = new RichTextOptions(scaledFont)
        {
          Origin = new Point(0, 0),
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Top
        };

        img2.Mutate(ctx => ctx
          .DrawText(textGraphicsOptions, text[i].ToString(), Rgba32.ParseHex(colorHex))
          .DrawGrid(containerWidth, containerHeight, Rgba32.ParseHex(colorHex), 6, 1)
          .Rotate(random.Next(-45, 45))
        );
        processingContext.DrawImage(img2, point, 1);
      }

      return processingContext;
    }



  /// <summary>
  /// 画圆圈（泡泡）
  /// </summary>
  /// <typeparam name="TPixel"></typeparam>
  /// <param name="processingContext"></param>
  /// <param name="containerWidth"></param>
  /// <param name="containerHeight"></param>
  /// <param name="count"></param>
  /// <param name="miniR"></param>
  /// <param name="maxR"></param>
  /// <param name="color"></param>
  /// <param name="canOverlap"></param>
  /// <returns></returns>
  public static IImageProcessingContext DrawingCircles(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, int count, int miniR, int maxR, Rgba32 color, bool canOverlap = false)
  {
    var random = new Random();

    var points = new List<PointF>();

    if (count > 0)
    {
      for (int i = 0; i < count; i++)
      {
        PointF tempPoint;
        if (canOverlap)
        {
          tempPoint = new PointF(random.Next(0, containerWidth), random.Next(0, containerHeight));
        }
        else
        {
          tempPoint = GetCirclePoint(containerWidth, containerHeight, (miniR + maxR), ref points);
        }
        var ep = new EllipsePolygon(tempPoint, random.Next(miniR, maxR));

        processingContext.Draw(color, (float)(random.Next(94, 145) / 100.0), ep.Clip());
      }
    }

    return processingContext;
  }
  /// <summary>
  /// 画杂线
  /// </summary>
  /// <typeparam name="TPixel"></typeparam>
  /// <param name="processingContext"></param>
  /// <param name="containerWidth"></param>
  /// <param name="containerHeight"></param>
  /// <param name="color"></param>
  /// <param name="count"></param>
  /// <param name="thickness"></param>
  /// <returns></returns>
  public static IImageProcessingContext DrawGrid(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, Rgba32 color, int count, float thickness)
  {
    var points = new List<PointF> { new(0, 0) };
    for (int i = 0; i < count; i++)
    {
      GetCirclePoint(containerWidth, containerHeight, 9, ref points);
    }
    points.Add(new PointF(containerWidth, containerHeight));

    processingContext.DrawLine(color, thickness, points.ToArray());

    return processingContext;
  }

  /// <summary>
  /// 散 随机点
  /// </summary>
  /// <param name="containerWidth"></param>
  /// <param name="containerHeight"></param>
  /// <param name="lapR"></param>
  /// <param name="list"></param>
  /// <returns></returns>
  private static PointF GetCirclePoint(int containerWidth, int containerHeight, double lapR, ref List<PointF> list)
  {
    var random = new Random();
    var newPoint = new PointF();
    var retryTimes = 10;

    do
    {
      newPoint.X = random.Next(0, containerWidth);
      newPoint.Y = random.Next(0, containerHeight);
      bool tooClose = false;
      foreach (var p in list)
      {
        tooClose = false;
        var tempDistance = Math.Sqrt((Math.Pow((p.X - newPoint.X), 2) + Math.Pow((p.Y - newPoint.Y), 2)));
        if (tempDistance < lapR)
        {
          tooClose = true;
          break;
        }
      }
      if (tooClose == false)
      {
        list.Add(newPoint);
        break;
      }
    }
    while (retryTimes-- > 0);

    if (retryTimes <= 0)
    {
      list.Add(newPoint);
    }
    return newPoint;
  }
}
}
