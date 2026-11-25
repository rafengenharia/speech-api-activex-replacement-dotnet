/// <summary>Turns text into a WAV file. The API only knows this interface.</summary>
public interface ISpeechSynthesizer
{
    Task<byte[]> SynthesizeAsync(string text, CancellationToken cancellationToken);
}
