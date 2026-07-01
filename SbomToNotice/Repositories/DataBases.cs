using System.Reflection;

namespace SbomToNotice.Repositories;

/// <summary>
/// The information of the databases.
/// </summary>
internal static class DataBases
{
    /// <summary>
    /// The tool name.
    /// </summary>
    private readonly static string _toolName = Assembly.GetExecutingAssembly().GetName().Name ?? "SbomToNotice";

    /// <summary>
    /// The local database directory.
    /// </summary>
    public static string LocalDbDirectory
        => field ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _toolName);

    /// <summary>
    /// The connection string of the local database.
    /// </summary>
    public static string LocalDbConnectionString
    {
        get
        {
            Directory.CreateDirectory(LocalDbDirectory);
            return field ??= "Data Source=" + Path.Combine(LocalDbDirectory, _toolName + "_local.db") + ';';
        }
    }
}
