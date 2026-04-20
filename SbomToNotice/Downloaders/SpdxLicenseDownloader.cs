namespace SbomToNotice.Downloaders;

/// <summary>
/// Provides functionality to download license text from the SPDX license list.
/// </summary>
/// <param name="httpDownloader">The HTTP downloader implementation used to fetch the license.</param>
internal sealed class SpdxLicenseDownloader(IHttpDownloader httpDownloader)
{
    private readonly IHttpDownloader _downloader = httpDownloader;

    /// <summary>
    /// Attempts to download the license text for the specified SPDX identifier.
    /// </summary>
    /// <param name="identifier">The SPDX license identifier.</param>
    /// <param name="text">When this method returns, contains the downloaded license content if successful; otherwise, an empty string.</param>
    /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryDownload(string identifier, out string text)
    {
        var url = $"https://raw.githubusercontent.com/spdx/license-list-data/refs/heads/main/text/{identifier}.txt";
        return _downloader.TryDownload(new Uri(url), out text);
    }
}
