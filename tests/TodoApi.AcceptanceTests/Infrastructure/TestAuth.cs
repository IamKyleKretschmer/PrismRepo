namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>Token settings that exist only in the test host.</summary>
public static class TestAuth
{
    public const string Issuer = "https://vibranium.test/";
    public const string Audience = "todo-api-tests";

    // Test-only symmetric key (256 bits). Never used outside the test host.
    public const string SigningKey = "dGVzdC1vbmx5LXNpZ25pbmcta2V5LWZvci10b2RvLWFwaS10ZXN0cw==";
}
