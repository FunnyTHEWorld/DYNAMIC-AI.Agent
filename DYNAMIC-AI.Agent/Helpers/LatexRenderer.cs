using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Aspose.TeX.IO;
using Aspose.TeX.Presentation.Image;

namespace DYNAMIC_AI.Agent.Helpers;

public abstract class RenderedContent
{
}

public class TextContent : RenderedContent
{
    public string Text { get; set; }
}

public class LatexImageContent : RenderedContent
{
    public byte[] ImageData { get; set; }
    public string Latex { get; set; }
}

public static class LatexRenderer
{
    public static List<RenderedContent> Render(string text)
    {
        var result = new List<RenderedContent>();
        var regex = new Regex(@"\$\$(.*?)\$\$");
        var lastIndex = 0;

        foreach (Match match in regex.Matches(text))
        {
            if (match.Index > lastIndex)
            {
                result.Add(new TextContent { Text = text.Substring(lastIndex, match.Index - lastIndex) });
            }

            var latex = match.Groups[1].Value;
            try
            {
                var imageData = RenderLatexToBytes(latex);
                result.Add(new LatexImageContent { ImageData = imageData, Latex = latex });
            }
            catch (Exception)
            {
                // If rendering fails, just add the raw LaTeX string.
                result.Add(new TextContent { Text = match.Value });
            }

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            result.Add(new TextContent { Text = text.Substring(lastIndex) });
        }

        return result;
    }

    private static byte[] RenderLatexToBytes(string latex)
    {
        using var stream = new MemoryStream();
        var options = new PngSaveOptions();
        options.Resolution = 300;
        Aspose.TeX.Presentation.Image.PngMathRenderer.Render(latex, stream, options, out _);
        return stream.ToArray();
    }
}
