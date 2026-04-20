using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SbomToNotice.Downloaders;

/// <summary>
/// Provides a base implementation for downloading license information.
/// </summary>
internal abstract class SbomDownloader : IHttpDownloader
{
    /// <summary>
    /// Gets a list of errors encountered during the download process.
    /// </summary>
    public ImmutableArray<string> Errors => [.. _errors];
    private readonly List<string> _errors = [];

    /// <summary>
    /// Gets a list of source URLs used for downloading licenses.
    /// </summary>
    public ImmutableArray<string> Sources => [.. _sources];
    private readonly List<string> _sources = [];

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "To prevent exceptions from being thrown.")]
    bool IHttpDownloader.TryDownload(Uri uri, out string text)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(typeof(CycloneDxLicenseDownloader).Assembly.GetName().Name);
        try
        {
            text = client.GetStringAsync(uri).Result.TrimEnd('\r', '\n');
            return !string.IsNullOrWhiteSpace(text);
        }
        catch (HttpRequestException ex) when (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            ((IHttpDownloader)this).AddError($"{ex.StatusCode}: {ex.Message}");
        }
        catch (AggregateException ex)
        {
            foreach (var iex in ex.InnerExceptions)
            {
                if (iex is HttpRequestException hrex
                    && hrex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    continue;
                }
                ((IHttpDownloader)this).AddError(ex.Message);
            }
        }
        catch (Exception ex)
        {
            ((IHttpDownloader)this).AddError(ex.Message);
        }
        text = "";
        return false;
    }

    /// <inheritdoc/>
    void IHttpDownloader.AddError(string message)
    {
        _errors.Add(message);
        System.Diagnostics.Debug.WriteLine(message);
    }

    /// <summary>
    /// Records a source URL used for the download.
    /// </summary>
    /// <param name="source">The source URL.</param>
    protected void AddSource(string source)
    {
        _sources.Add(source);
    }
}
