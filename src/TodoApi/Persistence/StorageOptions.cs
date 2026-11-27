using Microsoft.Extensions.Options;

namespace TodoApi.Persistence;

public sealed class StorageOptions
{
    public const string Section = "Storage";

    /// <summary>Path of the SQLite data file. Must be on persistent local storage.</summary>
    public string? DataFilePath { get; set; }
}

internal sealed class StorageOptionsValidator : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(string? name, StorageOptions options) =>
        string.IsNullOrWhiteSpace(options.DataFilePath)
            ? ValidateOptionsResult.Fail(RequiredSetting.Missing(StorageOptions.Section, nameof(StorageOptions.DataFilePath)))
            : ValidateOptionsResult.Success;
}
