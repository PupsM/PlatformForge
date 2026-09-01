using PlatformAudio.Enums;
using PlatformAudio.Interfaces;
using PlatformNative.Native;
using System.Runtime.InteropServices;

namespace PlatformAudio;

/// <summary>
/// Реализация источника звука через OpenAL
/// </summary>
public sealed class OpenALSoundSource : ISoundSource
{
    private uint ALHandle;
    private float ALGain = 1.0f;
    private float ALPitch = 1.0f;
    private bool ALLooping;
    private (float X, float Y, float Z) ALPosition;
    private (float X, float Y, float Z) ALVelocity;
    private float ALReferenceDistance = 1.0f;
    private float ALMaxDistance = 100.0f;
    private float ALRolloffFactor = 1.0f;
    private float ALConeInnerAngle = 360.0f;
    private float ALConeOuterAngle = 360.0f;
    private float ALConeOuterGain = 0.0f;
    private bool Disposed;
    private bool PlaybackEndedFired;

    public event Action<ISoundSource>? PlaybackEnded;

    public IntPtr Handle => (IntPtr)ALHandle;
    public SourceState State => GetState();

    public float Gain
    {
        get => ALGain;
        set
        {
            ALGain = Math.Clamp(value, 0f, 1f);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_GAIN, ALGain);
        }
    }

    public float Pitch
    {
        get => ALPitch;
        set
        {
            ALPitch = Math.Clamp(value, 0.5f, 2.0f);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_PITCH, ALPitch);
        }
    }

    public bool Looping
    {
        get => ALLooping;
        set
        {
            ALLooping = value;
            OpenAL.AlSourcei(ALHandle, OpenAL.AL_LOOPING, value ? 1 : 0);
        }
    }

    public (float X, float Y, float Z) Position
    {
        get => ALPosition;
        set
        {
            ALPosition = value;
            OpenAL.AlSource3f(ALHandle, OpenAL.AL_POSITION, value.X, value.Y, value.Z);
        }
    }

    public (float X, float Y, float Z) Velocity
    {
        get => ALVelocity;
        set
        {
            ALVelocity = value;
            OpenAL.AlSource3f(ALHandle, OpenAL.AL_VELOCITY, value.X, value.Y, value.Z);
        }
    }

    public float ReferenceDistance
    {
        get => ALReferenceDistance;
        set
        {
            ALReferenceDistance = Math.Max(0.01f, value);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_REFERENCE_DISTANCE, ALReferenceDistance);
        }
    }

    public float MaxDistance
    {
        get => ALMaxDistance;
        set
        {
            ALMaxDistance = Math.Max(ALReferenceDistance, value);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_MAX_DISTANCE, ALMaxDistance);
        }
    }

    public float RolloffFactor
    {
        get => ALRolloffFactor;
        set
        {
            ALRolloffFactor = Math.Max(0f, value);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_ROLLOFF_FACTOR, ALRolloffFactor);
        }
    }

    public float ConeInnerAngle
    {
        get => ALConeInnerAngle;
        set
        {
            ALConeInnerAngle = Math.Clamp(value, 0f, 360f);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_CONE_INNER_ANGLE, ALConeInnerAngle);
        }
    }

    public float ConeOuterAngle
    {
        get => ALConeOuterAngle;
        set
        {
            ALConeOuterAngle = Math.Clamp(value, 0f, 360f);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_CONE_OUTER_ANGLE, ALConeOuterAngle);
        }
    }

    public float ConeOuterGain
    {
        get => ALConeOuterGain;
        set
        {
            ALConeOuterGain = Math.Clamp(value, 0f, 1f);
            OpenAL.AlSourcef(ALHandle, OpenAL.AL_CONE_OUTER_GAIN, ALConeOuterGain);
        }
    }

    public OpenALSoundSource()
    {
        OpenAL.AlGenSources(1, out ALHandle);
        if (ALHandle == 0)
            throw new InvalidOperationException("Не удалось создать источник OpenAL");

        // Устанавливаем начальные значения
        Gain = 1.0f;
        Pitch = 1.0f;
        Position = (0, 0, 0);
        Velocity = (0, 0, 0);
        ReferenceDistance = 1.0f;
        MaxDistance = 100.0f;
        RolloffFactor = 1.0f;
        ConeInnerAngle = 360.0f;
        ConeOuterAngle = 360.0f;
        ConeOuterGain = 0.0f;
    }

    private SourceState GetState()
    {
        OpenAL.AlGetSourcei(ALHandle, OpenAL.AL_SOURCE_STATE, out int state);
        return state switch
        {
            OpenAL.AL_PLAYING => SourceState.Playing,
            OpenAL.AL_PAUSED => SourceState.Paused,
            OpenAL.AL_STOPPED => SourceState.Stopped,
            _ => SourceState.Initial
        };
    }

    public void Play()
    {
        if (Disposed) return;
        PlaybackEndedFired = false;  // Сбрасываем флаг при воспроизведении
        OpenAL.AlSourcePlay(ALHandle);
    }

    public void Pause()
    {
        if (Disposed) return;
        OpenAL.AlSourcePause(ALHandle);
        PlaybackEndedFired = false;
    }

    public void Stop()
    {
        if (Disposed) return;
        OpenAL.AlSourceStop(ALHandle);
        PlaybackEndedFired = false;
    }

    public void Rewind()
    {
        if (Disposed) return;
        OpenAL.AlSourceRewind(ALHandle);
        PlaybackEndedFired = false;
    }

    public void BindBuffer(ISoundBuffer buffer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentNullException.ThrowIfNull(buffer);

        OpenAL.AlSourcei(ALHandle, OpenAL.AL_BUFFER, (int)buffer.Handle);
        PlaybackEndedFired = false;
    }

    public void UnbindBuffer()
    {
        if (Disposed) return;
        OpenAL.AlSourcei(ALHandle, OpenAL.AL_BUFFER, 0);
        PlaybackEndedFired = false;
    }

    public void QueueBuffer(ISoundBuffer buffer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        ArgumentNullException.ThrowIfNull(buffer);

        uint handle = (uint)buffer.Handle;
        GCHandle gcHandle = GCHandle.Alloc(handle, GCHandleType.Pinned);
        try
        {
            OpenAL.AlSourceQueueBuffers(ALHandle, 1, gcHandle.AddrOfPinnedObject());
        }
        finally
        {
            gcHandle.Free();
        }
        PlaybackEndedFired = false;
    }

    public void UnqueueBuffer(ISoundBuffer buffer)
    {
        if (Disposed) return;
        ArgumentNullException.ThrowIfNull(buffer);

        uint handle = (uint)buffer.Handle;
        GCHandle gcHandle = GCHandle.Alloc(handle, GCHandleType.Pinned);
        try
        {
            OpenAL.AlSourceUnqueueBuffers(ALHandle, 1, gcHandle.AddrOfPinnedObject());
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public void ClearQueue()
    {
        if (Disposed) return;
        OpenAL.AlSourcei(ALHandle, OpenAL.AL_BUFFER, 0);
        PlaybackEndedFired = false;
    }

    internal void CheckPlaybackEnded()
    {
        if (Disposed || ALHandle == 0) return;

        if (State == SourceState.Stopped && !PlaybackEndedFired)
        {
            // Проверяем, что источник действительно остановлен (не на паузе)
            OpenAL.AlGetSourcei(ALHandle, OpenAL.AL_SOURCE_STATE, out int state);
            if (state == OpenAL.AL_STOPPED)
            {
                PlaybackEndedFired = true;
                PlaybackEnded?.Invoke(this);
            }
        }
    }

    public void Dispose()
    {
        if (Disposed) return;

        Stop();
        ClearQueue();

        if (ALHandle != 0)
        {
            uint handle = ALHandle;
            OpenAL.AlDeleteSources(1, ref handle);
            ALHandle = 0;
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }
}