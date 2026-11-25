# Speech API: text in, WAV out
Sample code for an article about replacing a legacy ActiveX voice component with a small .NET API.
- `POST /speech` with `{ "text": "..." }` returns `audio/wav`.
- `ISpeechSynthesizer` hides the engine. `AzureSpeechSynthesizer` uses the Azure Speech SDK (Microsoft.CognitiveServices.Speech).
- The CI tests the API with a fake synthesizer. It compiles the Azure class but never calls Azure, so no key is needed.
Run for real: set `Speech__Key` and `Speech__Region`, then `dotnet run --project src`.
Tests: `dotnet test tests/Tests.csproj`.

License: MIT, see [LICENSE](LICENSE). Copyright (c) 2026 Ricardo Ferreira.
