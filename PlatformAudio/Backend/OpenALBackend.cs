using PlatformAudio.Enums;
using PlatformAudio.Interfaces;
using PlatformNative.Core;
using PlatformNative.Native;

namespace PlatformAudio.Backend;

/// <summary>
/// Бэкенд аудио через OpenAL Soft
/// </summary>
public sealed class OpenALBackend : IAudio
{
    private bool Initialized;
    private bool Disposed;
    private readonly List<ISoundSource> Sources = [];
    private readonly List<ISoundBuffer> Buffers = [];
    private readonly Lock Lock = new();

    public string Name => "OpenAL";
    public bool IsInitialized => Initialized;

    public void Initialize()
    {
        if (Initialized) return;

        // ✅ Просто инициализируем OpenAL через Host
        // Он сам создаст устройство и контекст
        if (!OpenAL.IsInitialized)
        {
            if (!OpenAL.Initialize())
            {
                throw new InvalidOperationException("Не удалось инициализировать OpenAL");
            }
        }

        // ✅ Проверяем, что OpenAL действительно инициализирован
        // Для этого проверяем наличие AL функций
        int error = OpenAL.AlGetError();
        if (error != 0)
        {
            Diagnostics.Warning($"OpenAL: ошибка при проверке инициализации: {error}");
        }

        Initialized = true;
        Diagnostics.Info("OpenAL бэкенд инициализирован");
    }

    public void Update()
    {
        // Проверяем окончание воспроизведения для всех источников
        lock (Lock)
        {
            foreach (var source in Sources)
            {
                if (source is OpenALSoundSource openALSound)
                {
                    openALSound.CheckPlaybackEnded();
                }
            }
        }
    }

    public ISoundSource CreateSource()
    {
        EnsureInitialized();
        var source = new OpenALSoundSource();
        lock (Lock) Sources.Add(source);
        return source;
    }

    public void DestroySource(ISoundSource source)
    {
        if (source is null) return;

        lock (Lock)
        {
            // ✅ Удаляем напрямую, без лишнего Contains
            if (Sources.Remove(source))
            {
                source.Dispose();
            }
        }
    }

    public ISoundBuffer CreateBuffer()
    {
        EnsureInitialized();
        var buffer = new OpenALSoundBuffer();
        lock (Lock) Buffers.Add(buffer);
        return buffer;
    }

    public void DestroyBuffer(ISoundBuffer buffer)
    {
        if (buffer is null) return;

        lock (Lock)
        {
            // ✅ Удаляем напрямую, без лишнего Contains
            if (Buffers.Remove(buffer))
            {
                buffer.Dispose();
            }
        }
    }

    public void SetListenerPosition(float x, float y, float z)
    {
        EnsureInitialized();
        OpenAL.AlListener3f(OpenAL.AL_POSITION, x, y, z);
    }

    public void SetListenerOrientation(float atX, float atY, float atZ, float upX, float upY, float upZ)
    {
        EnsureInitialized();
        float[] orientation = [atX, atY, atZ, upX, upY, upZ];
        var handle = System.Runtime.InteropServices.GCHandle.Alloc(orientation, System.Runtime.InteropServices.GCHandleType.Pinned);
        try
        {
            OpenAL.AlListenerfv(OpenAL.AL_ORIENTATION, handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    public void SetListenerVelocity(float x, float y, float z)
    {
        EnsureInitialized();
        OpenAL.AlListener3f(OpenAL.AL_VELOCITY, x, y, z);
    }

    public void SetDistanceModel(DistanceModel model)
    {
        EnsureInitialized();
        int alModel = model switch
        {
            DistanceModel.None => OpenAL.AL_NONE,
            DistanceModel.InverseDistance => OpenAL.AL_INVERSE_DISTANCE,
            DistanceModel.InverseDistanceClamped => OpenAL.AL_INVERSE_DISTANCE_CLAMPED,
            DistanceModel.LinearDistance => OpenAL.AL_LINEAR_DISTANCE,
            DistanceModel.LinearDistanceClamped => OpenAL.AL_LINEAR_DISTANCE_CLAMPED,
            DistanceModel.ExponentDistance => OpenAL.AL_EXPONENT_DISTANCE,
            DistanceModel.ExponentDistanceClamped => OpenAL.AL_EXPONENT_DISTANCE_CLAMPED,
            _ => OpenAL.AL_NONE
        };
        OpenAL.AlDistanceModel(alModel);
    }

    public void SetDopplerFactor(float factor)
    {
        EnsureInitialized();
        OpenAL.AlDopplerFactor(factor);
    }

    public void SetSpeedOfSound(float speed)
    {
        EnsureInitialized();
        OpenAL.AlSpeedOfSound(speed);
    }

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (!Initialized)
            throw new InvalidOperationException("OpenALBackend не инициализирован");
    }

    public void Dispose()
    {
        if (Disposed) return;

        lock (Lock)
        {
            foreach (var source in Sources)
                source.Dispose();
            Sources.Clear();

            foreach (var buffer in Buffers)
                buffer.Dispose();
            Buffers.Clear();
        }

        // ✅ Не закрываем устройство и контекст здесь!
        // Это делает OpenAL.Shutdown() при завершении хоста

        Initialized = false;
        Disposed = true;
        Diagnostics.Info("OpenAL бэкенд освобождён");
        GC.SuppressFinalize(this);
    }
}