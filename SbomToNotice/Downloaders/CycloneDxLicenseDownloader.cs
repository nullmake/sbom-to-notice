using CycloneDX.Models;

namespace SbomToNotice.Downloaders;

/// <summary>
/// Downloads license information for components based on CycloneDX SBOM models.
/// </summary>
internal sealed class CycloneDxLicenseDownloader : SbomDownloader
{
    /// <summary>
    /// Downloads license information for the specified CycloneDX component.
    /// </summary>
    /// <param name="component">The <see cref="Component"/> to download licenses for.</param>
    /// <returns>A list of strings containing the downloaded license texts.</returns>
    public IReadOnlyList<string> Download(Component component)
    {
        foreach (var func in new Func<Component, IEnumerable<string>>[]
        {
            DownloadFromLicenseUrl,
            DownloadFromVcs,
            DownloadFromSpdx
        })
        {
            var list = func(component).ToArray();
            if (list.Length > 0)
            {
                return list;
            }
        }
        return [];
    }

    /// <summary>
    /// Attempts to download license text from URLs defined in the component's license field.
    /// </summary>
    /// <param name="component">The <see cref="Component"/> to process.</param>
    /// <returns>An enumerable collection of downloaded license texts.</returns>
    private IEnumerable<string> DownloadFromLicenseUrl(Component component)
    {
        var licenses = component.Licenses;
        if (licenses is null || licenses.Count == 0)
        {
            yield break;
        }

        var downloader = new UriLicenseDownloader(this);
        foreach (var uri in licenses
            .Where(l => l.License.Url is not null)
            .Select(l => new Uri(l.License.Url)))
        {
            AddSource(uri.OriginalString);
            if (downloader.TryDownload(uri, out var text))
            {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Attempts to download license text from VCS URLs associated with the component.
    /// </summary>
    /// <param name="component">The <see cref="Component"/> to process.</param>
    /// <returns>An enumerable collection of downloaded license texts.</returns>
    private IEnumerable<string> DownloadFromVcs(Component component)
    {
        var externalReferences = component.ExternalReferences;
        if (externalReferences is null || externalReferences.Count == 0)
        {
            yield break;
        }

        var downloader = new UriLicenseDownloader(this);
        foreach (var uri in externalReferences
            .Where(r => r.Type == ExternalReference.ExternalReferenceType.Vcs)
            .Select(r => new Uri(r.Url)))
        {
            AddSource(uri.OriginalString);
            if (downloader.TryDownload(uri, out var text))
            {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Attempts to download license text using the SPDX identifier.
    /// </summary>
    /// <param name="component">The <see cref="Component"/> to process.</param>
    /// <returns>An enumerable collection of downloaded license texts.</returns>
    private IEnumerable<string> DownloadFromSpdx(Component component)
    {
        var licenses = component.Licenses;
        if (licenses is null || licenses.Count == 0)
        {
            yield break;
        }

        var downloader = new SpdxLicenseDownloader(this);
        foreach (var id in licenses
            .Where(l => l.License.Id is not null)
            .Select(l => l.License.Id))
        {
            if (downloader.TryDownload(id, out var text))
            {
                yield return text;
            }
        }
    }
}
