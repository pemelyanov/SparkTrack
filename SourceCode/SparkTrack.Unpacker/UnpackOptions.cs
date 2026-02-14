namespace SparkTrack.Unpacker;

using CommandLine;

public record UnpackOptions
{
    #region Properties

    /// <summary>
    /// </summary>
    [Option(
        'p',
        "process",
        Required = true,
        HelpText = "Название процесса основной программы. Необходимо чтобы дождаться ее закрытия"
    )]
    public required string LauncherProcessName { get; init; }

    /// <summary>
    /// </summary>
    [Option(
        'l',
        "launcher",
        Required = true,
        HelpText = "Путь до исполняемого файла основной программы, которая должна быть запущена после распаковки"
    )]
    public required string LauncherPath { get; init; }

    /// <summary>
    /// </summary>
    [Option('u', "update", Required = true, HelpText = "Путь до ZIP архива с обновлением")]
    public required string UpdateZipPath { get; init; }

    #endregion
}