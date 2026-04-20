namespace SbomToNotice.Downloaders;

/// <summary>
/// Provides functionality to download license text from a given URI.
/// </summary>
/// <param name="httpDownloader">The HTTP downloader implementation used to fetch the license.</param>
internal sealed partial class UriLicenseDownloader(IHttpDownloader httpDownloader)
{
    private readonly IHttpDownloader _downloader = httpDownloader;

    private readonly GitHubLicenseDownloader _gitHubLicenseDownloader = new(httpDownloader);

    /// <summary>
    /// Attempts to download the license text from the specified URI.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to download from.</param>
    /// <param name="text">When this method returns, contains the downloaded license content if successful; otherwise, an empty string.</param>
    /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryDownload(Uri uri, out string text)
    {
        if (uri.Host == "github.com")
        {
            if (_gitHubLicenseDownloader.TryDownload(uri, out text))
            {
                return true;
            }
        }
        else if (uri.Host == "go.microsoft.com")
        {
            var url = "https://raw.githubusercontent.com/dotnet/core/refs/heads/main/LICENSE.TXT";
            if (_downloader.TryDownload(new Uri(url), out var result))
            {
                text = "On Linux and macOS:" + Environment.NewLine + result;
                return true;
            }
        }
        else
        {
            _downloader.AddError($"\"{uri.Host}\" is not supported.");
        }
        text = "";
        return false;
    }
}
