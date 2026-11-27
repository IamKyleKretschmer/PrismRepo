using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>The real service, hosted in-process against its own data file and the test signing key.</summary>
public sealed class TodoApiFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _settings;
    private readonly bool _ownsDataFile;

    /// <param name="dataFilePath">Reuse an existing data file (e.g. to restart); a fresh one is created when null.</param>
    /// <param name="settings">Overrides; a null value removes the setting.</param>
    public TodoApiFactory(string? dataFilePath = null, IReadOnlyDictionary<string, string?>? settings = null)
    {
        _ownsDataFile = dataFilePath is null;
        DataFilePath = dataFilePath ?? TestDataFile.NewPath();

        _settings = new Dictionary<string, string?>
        {
            ["Storage:DataFilePath"] = DataFilePath,
            ["Authentication:Issuer"] = TestAuth.Issuer,
            ["Authentication:Audience"] = TestAuth.Audience,
            ["Authentication:SigningKey"] = TestAuth.SigningKey,
        };

        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            _settings[key] = value;
        }
    }

    public string DataFilePath { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        foreach (var (key, value) in _settings)
        {
            builder.UseSetting(key, value);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_ownsDataFile)
        {
            TestDataFile.Delete(DataFilePath);
        }
    }
}
