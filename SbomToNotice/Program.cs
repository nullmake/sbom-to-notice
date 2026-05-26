using SbomToNotice.Options;
using SbomToNotice.Readers;
using SbomToNotice.Writers;
using System.CommandLine;
using System.Globalization;
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
        var culture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

#if DEBUG
        //args = [@"..\manifest.cyclonedx.json", "-o", @"..\ThirdPartyNotices.html", "--output-format", "html"];
        //args = ["--help"];
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
        var outputFormat = new Option<OutputFormat>("--output-format", "--ofmt")
        {
            Description = "File format for outputting the license notice.",
            DefaultValueFactory = _ => OutputFormat.Markdown,
            Required = false,
        };

        var rootCommand = new RootCommand(
            "Generates a license notice using a Software Bill of Materials as its source.")
        {
            input,
            output,
            outputFormat
        };
        rootCommand.SetAction(AppAction);

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync().ConfigureAwait(false);

        Task<int> AppAction(ParseResult parseResult, CancellationToken _)
            => HandleAsync(new RunOptions
            {
                SbomPath = parseResult.GetValue(input)!,
                Output = parseResult.GetValue(output),
                OutputFormat = parseResult.GetValue(outputFormat)
            });
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
            await new NoticeWriterFactory(options).Create().WriteAsync(components, sw).ConfigureAwait(false);
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
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return new StreamWriter(Console.OpenStandardOutput());
        }

        var dir = Path.GetDirectoryName(path);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }
        return new StreamWriter(path, false, new UTF8Encoding(false));
    }
}
