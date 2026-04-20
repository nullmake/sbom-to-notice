namespace SbomToNotice.Downloaders;

/// <summary>
/// Defines the contract for downloading content over HTTP.
/// </summary>
internal interface IHttpDownloader
{
    /// <summary>
    /// Attempts to download the contents from the specified URI.
    /// </summary>
    /// <param name="uri">The <see cref="Uri"/> to download from.</param>
    /// <param name="text">When this method returns, contains the downloaded content if successful; otherwise, an empty string.</param>
    /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
    bool TryDownload(Uri uri, out string text);

    /// <summary>
    /// Records an error message occurred during the download process.
    /// </summary>
    /// <param name="message">The error message to record.</param>
    void AddError(string message);
}
