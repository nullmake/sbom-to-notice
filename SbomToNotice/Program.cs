using SbomToNotice.Readers;
using SbomToNotice.Writers;
using System.CommandLine;
using System.Text;

namespace SbomToNotice;

/// <summary>
/// The main application class responsible for handling execution.
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// The main entry point method for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task that represents the asynchronous operation, returning the process exit code.</returns>
    static async Task<int> Main(string[] args)
    {
#if DEBUG
        // args = [@"..\manifest.cyclonedx.json", "-o", @"..\ThirdPartyNotices.txt.md"];
#endif
        var input = new Argument<string>("file")
        {
            Description = "Path to the SBOM file for generating the license notice.",
            Arity = ArgumentArity.ExactlyOne
        };
        var output = new Option<string>("--output", "-o")
        {
            Description = "File path for outputting the license notice.",
            Required = false
        };

        var rootCommand = new RootCommand(
            "Generates a license notice using a Software Bill of Materials as its source.")
        {
            input,
            output
        };

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            Console.WriteLine(string.Join(Environment.NewLine, parseResult.Errors.Select(e => e.Message)));
            return 1;
        }

        return await HandleAsync(new RunOptions
        {
            SbomPath = parseResult.GetValue(input)!,
            Output = parseResult.GetValue(output)
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the license notice generation process based on the provided <see cref="RunOptions"/>.
    /// </summary>
    /// <param name="options">The execution options.</param>
    /// <returns>A task that represents the asynchronous operation, returning the process exit code.</returns>
    public static async Task<int> HandleAsync(RunOptions options)
    {
        var sbomType = await SbomReader.DetectSbomTypeAsync(options.SbomPath).ConfigureAwait(false);
        if (sbomType == SbomType.CycloneDX)
        {
            var components = await SbomReader.LoadCycloneDxAsync(options.SbomPath).ToArrayAsync().ConfigureAwait(false);
            using var sw = GetStreamWriter(options.Output);
            await NoticeWriter.WriteAsync(components, sw).ConfigureAwait(false);
            return 0;
        }
        throw new NotImplementedException($"{sbomType} is not implemented.");
    }

    /// <summary>
    /// Creates a <see cref="StreamWriter"/> for the specified file path, or defaults to standard output if the path is null or empty.
    /// </summary>
    /// <param name="path">The optional output file path.</param>
    /// <returns>A <see cref="StreamWriter"/> instance for the output.</returns>
    private static StreamWriter GetStreamWriter(string? path)
    => string.IsNullOrWhiteSpace(path)
        ? new StreamWriter(Console.OpenStandardOutput())
        : new StreamWriter(path, false, new UTF8Encoding(false));
}
