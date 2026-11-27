using TodoApi.Api;
using TodoApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddTodoObservability();
builder.Services.AddTodoPersistence(builder.Configuration);
builder.Services.AddTodoApi(builder.Configuration);

var app = builder.Build();

app.UseTodoApi();
app.MapHealthChecks("/health").AllowAnonymous();

try
{
    await app.RunAsync();
}
catch (Exception ex) when (StartupFailure.TryDescribe(ex, out var message))
{
    await Console.Error.WriteLineAsync(message);
    throw;
}

public partial class Program;
