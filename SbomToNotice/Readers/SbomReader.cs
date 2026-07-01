using SbomToNotice.Downloaders;
using SbomToNotice.Repositories;
using System.Text;
using System.Text.Json;

namespace SbomToNotice.Readers;

/// <summary>
/// Provides functionality to read and parse SBOM files.
/// </summary>
internal static class SbomReader
{
    /// <summary>
    /// Detects the <see cref="SbomType"/> of the SBOM file at the specified path.
    /// </summary>
    /// <param name="path">The file path to the SBOM.</param>
    /// <returns>The detected <see cref="SbomType"/>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the SBOM format is not supported or cannot be detected.</exception>
    public static async Task<SbomType> DetectSbomTypeAsync(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        var document = await JsonDocument.ParseAsync(fs).ConfigureAwait(false);
        var root = document.RootElement;
        if (root.TryGetProperty("bomFormat", out var bomFormat))
        {
            var value = bomFormat.GetString();
            if (value == "CycloneDX")
            {
                return SbomType.CycloneDX;
            }
            throw new NotSupportedException($"\"bomFormat: {bomFormat}\" is not supported.");
        }
        throw new NotSupportedException();
    }

    /// <summary>
    /// Loads the BOM components from the specified CycloneDX file path.
    /// </summary>
    /// <param name="path">The file path to the CycloneDX SBOM.</param>
    /// <param name="refreshCache">whether the local cache for the components data is overwritten.</param>
    /// <returns>An asynchronous stream of <see cref="Components.Component"/> objects.</returns>
    public static async IAsyncEnumerable<Components.Component> LoadCycloneDxAsync(string path, bool refreshCache)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var sw = new StreamReader(fs, new UTF8Encoding(false, true));
        var text = await sw.ReadToEndAsync().ConfigureAwait(false);
        var bom = CycloneDX.Json.Serializer.Deserialize(text);
        var licenseRepository = new LicenseCacheRepository();
        foreach (var component in bom.Components.OrderBy(c => c.Name))
        {
            var licenses = refreshCache
                ? null
                : await licenseRepository
                    .GetLicensesAsync(component.Name, component.Version)
                    .ConfigureAwait(false);

            if (licenses is null)
            {
                var licenseDownloader = new CycloneDxLicenseDownloader();
                licenses = licenseDownloader.Download(component);
                foreach (var error in licenseDownloader.Errors)
                {
                    await Console.Error.WriteAsync(error).ConfigureAwait(false);
                }

                if (licenses.Count == 0)
                {
                    if (licenseDownloader.Sources.Length == 0)
                    {
                        continue;
                    }
                    licenses = [string.Join(Environment.NewLine, licenseDownloader.Sources)];
                }
                else
                {
                    if (licenseDownloader.HasError)
                    {
                        await licenseRepository
                            .SaveLicensesAsync(component.Name, component.Version, licenses)
                            .ConfigureAwait(false);
                    }
                }
            }

            yield return new Components.Component(component.Name, component.Version)
            {
                Copyright = !string.IsNullOrEmpty(component.Copyright)
                    ? [component.Copyright]
                    : [],
                Authors = [.. component.Authors.Select(a => a.Name)],
                Licenses = [.. licenses]
            };
        }
    }
}
