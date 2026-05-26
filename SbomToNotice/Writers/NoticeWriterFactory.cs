using SbomToNotice.Options;

namespace SbomToNotice.Writers;

/// <summary>
/// The factory class of the <see cref="INoticeWriter"/> class.
/// </summary>
/// <param name="Options">The execution options for the application.</param>
internal sealed class NoticeWriterFactory(RunOptions Options)
{
    public INoticeWriter Create()
        => Options.OutputFormat switch
        {
            OutputFormat.Markdown => new NoticeMarkdownWriter(),
            OutputFormat.Html => new NoticeHtmlWriter(),
            _ => throw new NotSupportedException($"{Options.OutputFormat} is not supported.")
        };
}
