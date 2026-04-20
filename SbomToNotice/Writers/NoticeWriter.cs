using SbomToNotice.Components;
using System.Text;

namespace SbomToNotice.Writers;

/// <summary>
/// Provides functionality to write license notice information to an output stream.
/// </summary>
internal static class NoticeWriter
{
    /// <summary>
    /// Writes the license notice information for the provided components to the specified <see cref="StreamWriter"/>.
    /// </summary>
    /// <param name="components">The collection of components to write.</param>
    /// <param name="streamWriter">The <see cref="StreamWriter"/> to output the notice text.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteAsync(IEnumerable<Component> components, StreamWriter streamWriter)
    {
        await streamWriter.WriteLineAsync("# THIRD-PARTY SOFTWARE NOTICES AND INFORMATION"
            + Environment.NewLine + "Do Not Translate or Localize").ConfigureAwait(false);

        foreach (var component in components.OrderBy(c => c.Name))
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.Append("## ").Append(component.Name).Append(' ').AppendLine(component.Version);
            if (component.Copyright.Length > 0)
            {
                foreach (var copyright in component.Copyright)
                {
                    sb.AppendLine(copyright);
                }
            }
            else
            {
                if (component.Authors.Length > 0)
                {
                    sb.Append("Authors: ").AppendLine(string.Join(", ", component.Authors));
                }
            }
            sb.AppendLine();
            foreach (var text in component.Licenses)
            {
                sb.AppendLine("```");
                sb.AppendLine(text);
                sb.AppendLine("```");
            }
            await streamWriter.WriteAsync(sb.ToString()).ConfigureAwait(false);
        }
    }
}
