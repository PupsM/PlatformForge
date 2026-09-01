using PlatformAudio.Interfaces;
using PlatformAudio.Enums;

namespace PlatformEngine.Resources;

/// <summary>
/// Загрузчик звуков
/// </summary>
public static class SoundLoader
{
    public static ISoundBuffer Load(IAudio audio, string path)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var data = File.ReadAllBytes(path);
        var buffer = audio.CreateBuffer();

        var ext = Path.GetExtension(path).ToLowerInvariant();
        var format = ext switch
        {
            ".wav" => AudioFormat.Stereo16,
            ".ogg" => AudioFormat.Stereo16,
            ".mp3" => AudioFormat.Stereo16,
            _ => AudioFormat.Stereo16
        };

        buffer.SetData(data, format, 44100);
        return buffer;
    }
}