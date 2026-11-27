namespace TodoApi.AcceptanceTests.Infrastructure;

/// <summary>Token settings that exist only in the test host.</summary>
public static class TestAuth
{
    public const string Issuer = "https://vibranium.test/";
    public const string Audience = "todo-api-tests";

    public const string SigningKey = "dGVzdC1vbmx5LXNpZ25pbmcta2V5LWZvci10b2RvLWFwaS10ZXN0cw==";
}
