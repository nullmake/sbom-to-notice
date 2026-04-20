using System.Collections.Immutable;

namespace SbomToNotice.Components;

/// <summary>
/// Represents a software component within an SBOM.
/// </summary>
/// <param name="Name">The name of the component.</param>
/// <param name="Version">The version of the component.</param>
internal sealed record Component(string Name, string Version)
{
    /// <summary>
    /// Gets the name of the component.
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// Gets the version of the component.
    /// </summary>
    public string Version { get; } = Version;

    /// <summary>
    /// Gets or sets the copyright information.
    /// </summary>
    public ImmutableArray<string> Copyright { get; init; } = [];

    /// <summary>
    /// Gets or sets the authors of the component.
    /// </summary>
    public ImmutableArray<string> Authors { get; init; } = [];

    /// <summary>
    /// Gets or sets the list of licenses associated with the component.
    /// </summary>
    public ImmutableArray<string> Licenses { get; init; } = [];
}
