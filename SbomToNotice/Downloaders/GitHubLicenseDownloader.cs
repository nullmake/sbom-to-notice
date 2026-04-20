using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SbomToNotice.Downloaders;

/// <summary>
/// Provides functionality to download license information from GitHub repositories.
/// </summary>
/// <param name="httpDownloader">The HTTP downloader implementation.</param>
internal sealed partial class GitHubLicenseDownloader(IHttpDownloader httpDownloader)
{
    private readonly IHttpDownloader _downloader = httpDownloader;

    /// <summary>
    /// Gets the compiled regex to strip the .git suffix from repository URLs.
    /// </summary>
    [GeneratedRegex(@"\.git$")]
    private static partial Regex RegexTailGit();

    /// <summary>
    /// Gets the compiled regex to parse GitHub URLs.
    /// </summary>
    [GeneratedRegex("/(.+)/blob/(.+)")]
    private static partial Regex RegexGitHubUrl();

    /// <summary>
    /// Attempts to download license text from a GitHub repository URL.
    /// </summary>
    /// <param name="uri">The GitHub repository <see cref="Uri"/>.</param>
    /// <param name="text">When this method returns, contains the downloaded license content if successful; otherwise, an empty string.</param>
    /// <returns><see langword="true"/> if the download was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryDownload(Uri uri, out string text)
    {
        if (uri.Segments.Length == 3)
        {
            var owner = uri.Segments[1].Trim('/');
            var repo = RegexTailGit().Replace(uri.Segments[2].Trim('/'), "");
            var filenames = new string[]
            {
                "LICENSE.txt", "LICENSE.TXT", "License.txt",
                "LICENSE", "License",
                "LICENSE.md", "License.md"
            };
            foreach (var filename in filenames)
            {
                var url = $"https://raw.githubusercontent.com/{owner}/{repo}/refs/heads/main/{filename}";
                if (_downloader.TryDownload(new Uri(url), out text))
                {
                    return true;
                }
            }

            foreach (var filename in filenames)
            {
                var url = $"https://raw.githubusercontent.com/{owner}/{repo}/refs/heads/master/{filename}";
                if (_downloader.TryDownload(new Uri(url), out text))
                {
                    return true;
                }
            }

            var api = $"https://api.github.com/repos/{owner}/{repo}/license";
            if (_downloader.TryDownload(new Uri(api), out var json))
            {
                var deserialized = JsonSerializer.Deserialize<GitHubLicense>(json)?.GetLicenseText();
                if (deserialized is not null)
                {
                    text = deserialized;
                    return true;
                }
            }
        }
        else
        {
            var match = RegexGitHubUrl().Match(uri.AbsolutePath);
            if (match.Success)
            {
                var url = $"https://raw.githubusercontent.com/{match.Groups[1]}/refs/tags/{match.Groups[2]}";
                if (_downloader.TryDownload(new Uri(url), out text))
                {
                    return true;
                }
            }
        }
        text = "";
        return false;
    }

    /// <summary>
    /// Represents the GitHub license information structure.
    /// </summary>
    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Used via a serializer.")]
    private sealed record GitHubLicense
    {
        /// <summary>
        /// Gets or sets the name of the license file.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the license file.
        /// </summary>
        [JsonPropertyName("path")]
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets the SHA hash of the file content.
        /// </summary>
        [JsonPropertyName("sha")]
        public string? SHA { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the type of the object (e.g., "file").
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the encoded file content.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the encoding type (e.g., "base64").
        /// </summary>
        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        /// <summary>
        /// Decodes and returns the license text.
        /// </summary>
        /// <returns>The decoded license text.</returns>
        /// <exception cref="NotSupportedException">Thrown when the encoding type is not supported.</exception>
        public string GetLicenseText()
        {
            if (Encoding is null || Content is null)
            {
                return "";
            }
            if (Encoding == "base64")
            {
                var data = Convert.FromBase64String(Content);
                return System.Text.Encoding.UTF8.GetString(data).TrimEnd('\r', '\n');
            }
            throw new NotSupportedException();
        }
    }
}
