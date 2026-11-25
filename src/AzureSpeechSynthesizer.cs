using Microsoft.CognitiveServices.Speech;

/// <summary>
/// Real implementation with the Azure Speech SDK (Microsoft.CognitiveServices.Speech).
/// The CI only compiles this class. It does not call Azure.
/// </summary>
public sealed class AzureSpeechSynthesizer : ISpeechSynthesizer
{
    private readonly SpeechConfig _config;

    public AzureSpeechSynthesizer(string key, string region, string voice)
    {
        _config = SpeechConfig.FromSubscription(key, region);
        _config.SpeechSynthesisVoiceName = voice;
        // Ask for a WAV (RIFF) container so the caller gets a playable file.
        _config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
    }

    public async Task<byte[]> SynthesizeAsync(string text, CancellationToken cancellationToken)
    {
        // A null AudioConfig keeps the audio in memory instead of playing it on the server.
        using var synthesizer = new SpeechSynthesizer(_config, null);
        using var result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            return result.AudioData;

        var details = SpeechSynthesisCancellationDetails.FromResult(result);
        throw new InvalidOperationException($"Speech synthesis failed: {details.Reason}");
    }
}
