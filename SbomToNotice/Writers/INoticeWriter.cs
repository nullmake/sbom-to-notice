using SbomToNotice.Components;

namespace SbomToNotice.Writers;

internal interface INoticeWriter
{
    /// <summary>
    /// Writes the license notice information for the provided components to the specified <see cref="StreamWriter"/>.
    /// </summary>
    /// <param name="components">The collection of components to write.</param>
    /// <param name="streamWriter">The <see cref="StreamWriter"/> to output the notice text.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteAsync(IEnumerable<Component> components, StreamWriter streamWriter);
}
