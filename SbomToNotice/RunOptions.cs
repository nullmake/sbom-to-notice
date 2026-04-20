namespace SbomToNotice;

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
}
