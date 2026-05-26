using SbomToNotice.Components;
using System.Text;
using System.Text.RegularExpressions;

namespace SbomToNotice.Writers;

/// <summary>
/// Provides functionality to write license notice information to an output stream.
/// </summary>
internal sealed partial class NoticeHtmlWriter : INoticeWriter
{
    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex NewLineRegex();

    /// <summary>
    /// The opening elemnts.
    /// </summary>
    private readonly string OpeningElements = NewLineRegex().Replace("""
        <!doctype html>
        <html lang=\"en\">
            <head>
                <meta charset="UTF-8" />
                <title>THIRD-PARTY SOFTWARE NOTICES AND INFORMATION</title>
                <style>
                    .tpn-main-container {
                        margin: 24px;
                    }
                    .tpn-title {
                        font-size: 2em;
                    }
                    .tpn-title-container {
                        margin-left: 0.5em;
                    }
                    .tpn-component-title {
                        font-size: 1.5em;
                        margin-top: 1.5em;
                    }
                    .tpn-component-title-container {
                        margin-left: 0.5em;
                    }
                    .tpn-license {
                        padding: 16px;
                        width: max-content;
                        max-width: 100%;
                        overflow: auto;
                        background: #f6f6f6;
                        border: 1px solid #eee;
                    }
                </style>
            </head>
            <body class="tpn-main-container">
                <h1 class="tpn-title">THIRD-PARTY SOFTWARE NOTICES AND INFORMATION</h1>
                <div class="tpn-title-container">
                    <p>Do Not Translate or Localize</p>
        """, Environment.NewLine);

    /// <summary>
    /// The closing elements.
    /// </summary>
    private readonly string ClosingElements = NewLineRegex().Replace("""
                </div>
            </body>
        </html>
        """, Environment.NewLine);

    /// <inheritdoc/>
    public async Task WriteAsync(IEnumerable<Component> components, StreamWriter streamWriter)
    {
        await streamWriter.WriteLineAsync(OpeningElements).ConfigureAwait(false);

        foreach (var component in components.OrderBy(c => c.Name))
        {
            var sb = new StringBuilder();
            sb.Append("        ").AppendLine("<div class=\"tpn-component\">");
            sb.Append("        ").AppendLine("<div class=\"tpn-component-title-container\">");
            sb.Append("            ").Append("<h2 class=\"tpn-component-title\">")
                .Append(component.Name).Append(' ').Append(component.Version)
                .AppendLine("</h2>");
            if (component.Copyright.Length > 0)
            {
                foreach (var copyright in component.Copyright)
                {
                    sb.Append("                ").Append("<p class=\"tpn-copyright\">")
                        .Append(copyright)
                        .AppendLine("</p>");
                }
            }
            else
            {
                if (component.Authors.Length > 0)
                {
                    sb.Append("                ").Append("<p class=\"tpn-authors\">")
                        .Append("Authors: ").Append(string.Join(", ", component.Authors))
                        .AppendLine("</p>");
                }
            }
            foreach (var text in component.Licenses)
            {
                sb.Append("                ").AppendLine("<pre class=\"tpn-license\"><code>");
                sb.AppendLine(text);
                sb.AppendLine("</code></pre>");
            }
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");
            await streamWriter.WriteAsync(sb.ToString()).ConfigureAwait(false);
        }
        await streamWriter.WriteLineAsync(ClosingElements).ConfigureAwait(false);
    }
}
