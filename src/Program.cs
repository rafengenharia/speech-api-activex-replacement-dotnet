var builder = WebApplication.CreateBuilder(args);

// The real synthesizer is only created when the first request needs it, so the app
// (and the tests, which replace it) starts without any Azure key.
builder.Services.AddSingleton<ISpeechSynthesizer>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return new AzureSpeechSynthesizer(
        cfg["Speech:Key"] ?? throw new InvalidOperationException("Speech:Key is not configured"),
        cfg["Speech:Region"] ?? throw new InvalidOperationException("Speech:Region is not configured"),
        cfg["Speech:Voice"] ?? "pt-BR-FranciscaNeural");
});

var app = builder.Build();

app.MapPost("/speech", async (SpeechRequest request, ISpeechSynthesizer synthesizer,
    ILogger<Program> log, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 500)
        return Results.BadRequest("text is required and must have at most 500 characters");

    try
    {
        var wav = await synthesizer.SynthesizeAsync(request.Text, ct);
        return Results.File(wav, "audio/wav");
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        // Log the error type only: the phrase may contain user data.
        log.LogError("Speech synthesis failed: {ErrorType}", ex.GetType().Name);
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }
});

app.Run();

public partial class Program { }

public record SpeechRequest(string? Text);
