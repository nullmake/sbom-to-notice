namespace SbomToNotice.Options;

/// <summary>
/// Represents the execution options for the application.
/// </summary>
internal sealed record RunOptions
{
    /// <summary>
    /// Gets or sets the path to the input SBOM file.
    /// </summary>
    public string SbomPath { get; init; } = null!;

    /// <summary>
    /// Gets or sets the optional path to the output file.
    /// </summary>
    public string? Output { get; init; }

    /// <summary>
    /// Gets or sets the file format of the output file.
    /// </summary>
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Markdown;

    /// <summary>
    /// Gets or sets whether the local cache for the components data is overwritten.
    /// </summary>
    public bool RefreshCache { get; set; }
}
