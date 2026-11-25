using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SpeechApiTests
{
    private static HttpClient CreateClient(ISpeechSynthesizer synthesizer) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.ConfigureTestServices(s =>
                s.AddSingleton(synthesizer)))
            .CreateClient();

    [Fact]
    public async Task Speech_ValidText_ReturnsWav()
    {
        var client = CreateClient(new FakeSynthesizer());

        var response = await client.PostAsJsonAsync("/speech", new { text = "Retire seu cartão." });
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("audio/wav", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("RIFF", Encoding.ASCII.GetString(bytes, 0, 4));
        Assert.Equal("WAVE", Encoding.ASCII.GetString(bytes, 8, 4));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Speech_EmptyText_Returns400(string text)
    {
        var client = CreateClient(new FakeSynthesizer());

        var response = await client.PostAsJsonAsync("/speech", new { text });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Speech_TextTooLong_Returns400()
    {
        var client = CreateClient(new FakeSynthesizer());

        var response = await client.PostAsJsonAsync("/speech", new { text = new string('a', 501) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Speech_SynthesizerFails_Returns502()
    {
        var client = CreateClient(new FailingSynthesizer());

        var response = await client.PostAsJsonAsync("/speech", new { text = "Olá" });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }
}

/// <summary>Returns a valid, silent WAV (16 kHz, 16 bit, mono, 0.1 s). No Azure involved.</summary>
class FakeSynthesizer : ISpeechSynthesizer
{
    public Task<byte[]> SynthesizeAsync(string text, CancellationToken cancellationToken)
    {
        const int sampleRate = 16000;
        var data = new byte[sampleRate / 10 * 2];
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write(Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + data.Length);
        w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16); w.Write((short)1); w.Write((short)1);
        w.Write(sampleRate); w.Write(sampleRate * 2); w.Write((short)2); w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data"));
        w.Write(data.Length); w.Write(data);
        return Task.FromResult(ms.ToArray());
    }
}

class FailingSynthesizer : ISpeechSynthesizer
{
    public Task<byte[]> SynthesizeAsync(string text, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("boom");
}
