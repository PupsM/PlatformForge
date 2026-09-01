using PlatformNative.Core;
using PlatformNative.Core.Library;
using System.Runtime.InteropServices;

namespace PlatformNative.Native;

/// <summary>
/// Нативная обёртка для OpenAL Soft 1.25.2
/// </summary>
public static class OpenAL
{
    #region ---- Хост ----

    private sealed class OpenALHost : Host<OpenALHost, OpenALLibrary>
    {
        protected override string LibraryKey => "OpenAL";
        protected override Func<string, IntPtr> Resolver => Manager.ResolveOpenAL;
        protected override Func<bool> Loader => Manager.LoadOpenAL;

        private IntPtr _device = IntPtr.Zero;
        private IntPtr _context = IntPtr.Zero;

        protected override bool InitializeLibrary()
        {
            if (!TryGetFunction<alcOpenDeviceDelegate>("alcOpenDevice", out var openDevice) || openDevice is null)
            {
                Diagnostics.Warning("OpenAL: alcOpenDevice не найден");
                return false;
            }

            if (!TryGetFunction<alcCreateContextDelegate>("alcCreateContext", out var createContext) || createContext is null)
            {
                Diagnostics.Warning("OpenAL: alcCreateContext не найден");
                return false;
            }

            if (!TryGetFunction<alcMakeContextCurrentDelegate>("alcMakeContextCurrent", out var makeCurrent) || makeCurrent is null)
            {
                Diagnostics.Warning("OpenAL: alcMakeContextCurrent не найден");
                return false;
            }

            _device = openDevice(null);
            if (_device == IntPtr.Zero)
            {
                Diagnostics.Warning("OpenAL: не удалось открыть устройство");
                return false;
            }

            _context = createContext(_device, IntPtr.Zero);
            if (_context == IntPtr.Zero)
            {
                Diagnostics.Warning("OpenAL: не удалось создать контекст");
                return false;
            }

            if (makeCurrent(_context) == 0)
            {
                Diagnostics.Warning("OpenAL: не удалось активировать контекст");
                return false;
            }

            if (!TryGetFunction<alcGetProcAddressDelegate>("alcGetProcAddress", out var getProc) || getProc is null)
            {
                Diagnostics.Warning("OpenAL: alcGetProcAddress не найден");
                return false;
            }

            LoadALFunctions(_device, getProc);

            Diagnostics.Info($"OpenAL инициализирован (устройство: {GetDeviceName(_device)})");
            return true;
        }

        protected override void ShutdownLibrary()
        {
            if (TryGetFunction<alcMakeContextCurrentDelegate>("alcMakeContextCurrent", out var makeCurrent) && makeCurrent is not null)
            {
                makeCurrent(IntPtr.Zero);
            }

            if (_context != IntPtr.Zero)
            {
                if (TryGetFunction<alcDestroyContextDelegate>("alcDestroyContext", out var destroyContext) && destroyContext is not null)
                {
                    destroyContext(_context);
                }
                _context = IntPtr.Zero;
            }

            if (_device != IntPtr.Zero)
            {
                if (TryGetFunction<alcCloseDeviceDelegate>("alcCloseDevice", out var closeDevice) && closeDevice is not null)
                {
                    closeDevice(_device);
                }
                _device = IntPtr.Zero;
            }

            Diagnostics.Info("OpenAL завершён");
        }

        private static string GetDeviceName(IntPtr device)
        {
            if (TryGetFunction<alcGetStringDelegate>("alcGetString", out var getString) && getString is not null)
            {
                IntPtr namePtr = getString(device, ALC_DEFAULT_DEVICE_SPECIFIER);
                if (namePtr != IntPtr.Zero)
                {
                    return Marshal.PtrToStringAnsi(namePtr) ?? "unknown";
                }
            }
            return "unknown";
        }

        private static void LoadALFunctions(IntPtr device, alcGetProcAddressDelegate getProc)
        {
            // Буферы
            ALGenBuffers = LoadFunction<alGenBuffersDelegate>(device, getProc, "alGenBuffers");
            ALDeleteBuffers = LoadFunction<alDeleteBuffersDelegate>(device, getProc, "alDeleteBuffers");
            ALBufferData = LoadFunction<alBufferDataDelegate>(device, getProc, "alBufferData");
            ALBufferi = LoadFunction<alBufferiDelegate>(device, getProc, "alBufferi");
            ALBuffer3i = LoadFunction<alBuffer3iDelegate>(device, getProc, "alBuffer3i");
            ALBufferiv = LoadFunction<alBufferivDelegate>(device, getProc, "alBufferiv");

            // Источники
            ALGenSources = LoadFunction<alGenSourcesDelegate>(device, getProc, "alGenSources");
            ALDeleteSources = LoadFunction<alDeleteSourcesDelegate>(device, getProc, "alDeleteSources");
            ALSourcei = LoadFunction<alSourceiDelegate>(device, getProc, "alSourcei");
            ALSource3i = LoadFunction<alSource3iDelegate>(device, getProc, "alSource3i");
            ALSourcef = LoadFunction<alSourcefDelegate>(device, getProc, "alSourcef");
            ALSource3f = LoadFunction<alSource3fDelegate>(device, getProc, "alSource3f");
            ALSourcefv = LoadFunction<alSourcefvDelegate>(device, getProc, "alSourcefv");
            ALGetSourcei = LoadFunction<alGetSourceiDelegate>(device, getProc, "alGetSourcei");
            ALGetSourcef = LoadFunction<alGetSourcefDelegate>(device, getProc, "alGetSourcef");
            ALGetSource3f = LoadFunction<alGetSource3fDelegate>(device, getProc, "alGetSource3f");
            ALSourcePlay = LoadFunction<alSourcePlayDelegate>(device, getProc, "alSourcePlay");
            ALSourcePause = LoadFunction<alSourcePauseDelegate>(device, getProc, "alSourcePause");
            ALSourceStop = LoadFunction<alSourceStopDelegate>(device, getProc, "alSourceStop");
            ALSourceRewind = LoadFunction<alSourceRewindDelegate>(device, getProc, "alSourceRewind");
            ALSourceQueueBuffers = LoadFunction<alSourceQueueBuffersDelegate>(device, getProc, "alSourceQueueBuffers");
            ALSourceUnqueueBuffers = LoadFunction<alSourceUnqueueBuffersDelegate>(device, getProc, "alSourceUnqueueBuffers");

            // Слушатель
            ALListenerf = LoadFunction<alListenerfDelegate>(device, getProc, "alListenerf");
            ALListener3f = LoadFunction<alListener3fDelegate>(device, getProc, "alListener3f");
            ALListenerfv = LoadFunction<alListenerfvDelegate>(device, getProc, "alListenerfv");
            ALGetListenerf = LoadFunction<alGetListenerfDelegate>(device, getProc, "alGetListenerf");
            ALGetListener3f = LoadFunction<alGetListener3fDelegate>(device, getProc, "alGetListener3f");

            // Настройки
            ALDopplerFactor = LoadFunction<alDopplerFactorDelegate>(device, getProc, "alDopplerFactor");
            ALSpeedOfSound = LoadFunction<alSpeedOfSoundDelegate>(device, getProc, "alSpeedOfSound");
            ALDistanceModel = LoadFunction<alDistanceModelDelegate>(device, getProc, "alDistanceModel");

            // Ошибки и информация
            ALGetError = LoadFunction<alGetErrorDelegate>(device, getProc, "alGetError");
            ALGetString = LoadFunction<alGetStringDelegate>(device, getProc, "alGetString");

            Diagnostics.Debug("OpenAL: AL функции загружены");
        }

        private static T? LoadFunction<T>(IntPtr device, alcGetProcAddressDelegate getProc, string name) where T : Delegate
        {
            IntPtr ptr = getProc(device, name);
            if (ptr == IntPtr.Zero)
            {
                Diagnostics.Debug($"OpenAL: {name} не найден");
                return null;
            }

            try
            {
                return Marshal.GetDelegateForFunctionPointer<T>(ptr);
            }
            catch (Exception ex)
            {
                Diagnostics.Warning($"OpenAL: ошибка загрузки {name}: {ex.Message}");
                return null;
            }
        }
    }

    private sealed class OpenALLibrary : Base
    {
        protected override Func<string, IntPtr> Resolver => Manager.ResolveOpenAL;
    }

    #endregion

    #region ---- Публичные методы ----
    
    public static bool IsInitialized => OpenALHost.IsInitializedStatic;

    public static bool Initialize() => OpenALHost.InitializeStatic();
    public static void Shutdown() => OpenALHost.ShutdownStatic();

    public static T LoadFunction<T>(string name) where T : Delegate
        => OpenALHost.LoadFunction<T>(name);

    public static bool TryGetFunction<T>(string name, out T? del) where T : Delegate
        => OpenALHost.TryGetFunction(name, out del);

    public static void ClearCache()
        => OpenALHost.ClearCache();

    public static void Cleanup()
        => OpenALHost.Cleanup();

    #endregion

    #region ---- Публичные методы ALC ----

    public static IntPtr AlcOpenDevice(string? deviceName)
        => ALCOpenDevice?.Invoke(deviceName) ?? IntPtr.Zero;

    public static int AlcCloseDevice(IntPtr device)
        => ALCCloseDevice?.Invoke(device) ?? 0;

    public static IntPtr AlcCreateContext(IntPtr device, IntPtr attrList)
        => ALCCreateContext?.Invoke(device, attrList) ?? IntPtr.Zero;

    public static int AlcMakeContextCurrent(IntPtr context)
        => ALCMakeContextCurrent?.Invoke(context) ?? 0;

    public static void AlcDestroyContext(IntPtr context)
        => ALCDestroyContext?.Invoke(context);

    public static IntPtr AlcGetCurrentContext()
        => ALCGetCurrentContext?.Invoke() ?? IntPtr.Zero;

    public static IntPtr AlcGetContextsDevice(IntPtr context)
        => ALCGetContextsDevice?.Invoke(context) ?? IntPtr.Zero;

    public static IntPtr AlcGetString(IntPtr device, int param)
        => ALCGetString?.Invoke(device, param) ?? IntPtr.Zero;

    public static int AlcGetError(IntPtr device)
        => ALCGetError?.Invoke(device) ?? 0;

    public static IntPtr AlcGetProcAddress(IntPtr device, string funcName)
        => ALCGetProcAddress?.Invoke(device, funcName) ?? IntPtr.Zero;

    #endregion

    #region ---- Публичные методы AL ----

    public static void AlGenBuffers(int n, out uint buffers)
    {
        buffers = 0;
        ALGenBuffers?.Invoke(n, out buffers);
    }

    public static void AlDeleteBuffers(int n, ref uint buffers)
        => ALDeleteBuffers?.Invoke(n, ref buffers);

    public static void AlBufferData(uint buffer, int format, IntPtr data, int size, int freq)
        => ALBufferData?.Invoke(buffer, format, data, size, freq);

    public static void AlBufferi(uint buffer, int param, int value)
        => ALBufferi?.Invoke(buffer, param, value);

    public static void AlBuffer3i(uint buffer, int param, int value1, int value2, int value3)
        => ALBuffer3i?.Invoke(buffer, param, value1, value2, value3);

    public static void AlBufferiv(uint buffer, int param, IntPtr values)
        => ALBufferiv?.Invoke(buffer, param, values);

    public static void AlGenSources(int n, out uint sources)
    {
        sources = 0;
        ALGenSources?.Invoke(n, out sources);
    }

    public static void AlDeleteSources(int n, ref uint sources)
        => ALDeleteSources?.Invoke(n, ref sources);

    public static void AlSourcei(uint source, int param, int value)
        => ALSourcei?.Invoke(source, param, value);

    public static void AlSource3i(uint source, int param, int value1, int value2, int value3)
        => ALSource3i?.Invoke(source, param, value1, value2, value3);

    public static void AlSourcef(uint source, int param, float value)
        => ALSourcef?.Invoke(source, param, value);

    public static void AlSource3f(uint source, int param, float value1, float value2, float value3)
        => ALSource3f?.Invoke(source, param, value1, value2, value3);

    public static void AlSourcefv(uint source, int param, IntPtr values)
        => ALSourcefv?.Invoke(source, param, values);

    public static void AlGetSourcei(uint source, int param, out int value)
    {
        value = 0;
        ALGetSourcei?.Invoke(source, param, out value);
    }

    public static void AlGetSourcef(uint source, int param, out float value)
    {
        value = 0;
        ALGetSourcef?.Invoke(source, param, out value);
    }

    public static void AlGetSource3f(uint source, int param, out float value1, out float value2, out float value3)
    {
        value1 = 0;
        value2 = 0;
        value3 = 0;
        ALGetSource3f?.Invoke(source, param, out value1, out value2, out value3);
    }

    public static void AlSourcePlay(uint source)
        => ALSourcePlay?.Invoke(source);

    public static void AlSourcePause(uint source)
        => ALSourcePause?.Invoke(source);

    public static void AlSourceStop(uint source)
        => ALSourceStop?.Invoke(source);

    public static void AlSourceRewind(uint source)
        => ALSourceRewind?.Invoke(source);

    public static void AlSourceQueueBuffers(uint source, int n, IntPtr buffers)
        => ALSourceQueueBuffers?.Invoke(source, n, buffers);

    public static void AlSourceUnqueueBuffers(uint source, int n, IntPtr buffers)
        => ALSourceUnqueueBuffers?.Invoke(source, n, buffers);

    public static void AlListenerf(int param, float value)
        => ALListenerf?.Invoke(param, value);

    public static void AlListener3f(int param, float value1, float value2, float value3)
        => ALListener3f?.Invoke(param, value1, value2, value3);

    public static void AlListenerfv(int param, IntPtr values)
        => ALListenerfv?.Invoke(param, values);

    public static void AlGetListenerf(int param, out float value)
    {
        value = 0;
        ALGetListenerf?.Invoke(param, out value);
    }

    public static void AlGetListener3f(int param, out float value1, out float value2, out float value3)
    {
        value1 = 0;
        value2 = 0;
        value3 = 0;
        ALGetListener3f?.Invoke(param, out value1, out value2, out value3);
    }

    public static void AlDopplerFactor(float value)
        => ALDopplerFactor?.Invoke(value);

    public static void AlSpeedOfSound(float value)
        => ALSpeedOfSound?.Invoke(value);

    public static void AlDistanceModel(int model)
        => ALDistanceModel?.Invoke(model);

    public static int AlGetError()
        => ALGetError?.Invoke() ?? 0;

    public static IntPtr AlGetString(int param)
        => ALGetString?.Invoke(param) ?? IntPtr.Zero;

    #endregion

    #region ---- Делегаты ALC ----

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcOpenDeviceDelegate(string? devicename);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int alcCloseDeviceDelegate(IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcCreateContextDelegate(IntPtr device, IntPtr attrlist);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int alcMakeContextCurrentDelegate(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alcDestroyContextDelegate(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcGetStringDelegate(IntPtr device, int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int alcGetIntegervDelegate(IntPtr device, int param, int size, IntPtr values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcGetProcAddressDelegate(IntPtr device, string funcname);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int alcGetErrorDelegate(IntPtr device);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcGetCurrentContextDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alcGetContextsDeviceDelegate(IntPtr context);

    #endregion

    #region ---- Делегаты AL ----

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr alGetStringDelegate(int param);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int alGetErrorDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGenBuffersDelegate(int n, out uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alDeleteBuffersDelegate(int n, ref uint buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alBufferDataDelegate(uint buffer, int format, IntPtr data, int size, int freq);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alBufferiDelegate(uint buffer, int param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alBuffer3iDelegate(uint buffer, int param, int value1, int value2, int value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alBufferivDelegate(uint buffer, int param, IntPtr values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGenSourcesDelegate(int n, out uint sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alDeleteSourcesDelegate(int n, ref uint sources);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourceiDelegate(uint source, int param, int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSource3iDelegate(uint source, int param, int value1, int value2, int value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourcefDelegate(uint source, int param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSource3fDelegate(uint source, int param, float value1, float value2, float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourcefvDelegate(uint source, int param, IntPtr values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGetSourceiDelegate(uint source, int param, out int value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGetSourcefDelegate(uint source, int param, out float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGetSource3fDelegate(uint source, int param, out float value1, out float value2, out float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourcePlayDelegate(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourcePauseDelegate(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourceStopDelegate(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourceRewindDelegate(uint source);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourceQueueBuffersDelegate(uint source, int n, IntPtr buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSourceUnqueueBuffersDelegate(uint source, int n, IntPtr buffers);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGetListenerfDelegate(int param, out float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alGetListener3fDelegate(int param, out float value1, out float value2, out float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alListenerfDelegate(int param, float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alListener3fDelegate(int param, float value1, float value2, float value3);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alListenerfvDelegate(int param, IntPtr values);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alDopplerFactorDelegate(float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alDistanceModelDelegate(int model);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void alSpeedOfSoundDelegate(float value);

    #endregion

    #region ---- Поля для AL функций ----

    // ALC
    private static readonly alcOpenDeviceDelegate? ALCOpenDevice;
    private static readonly alcCloseDeviceDelegate? ALCCloseDevice;
    private static readonly alcCreateContextDelegate? ALCCreateContext;
    private static readonly alcMakeContextCurrentDelegate? ALCMakeContextCurrent;
    private static readonly alcDestroyContextDelegate? ALCDestroyContext;
    private static readonly alcGetStringDelegate? ALCGetString;
    private static readonly alcGetProcAddressDelegate? ALCGetProcAddress;
    private static readonly alcGetErrorDelegate? ALCGetError;
    private static readonly alcGetCurrentContextDelegate? ALCGetCurrentContext;
    private static readonly alcGetContextsDeviceDelegate? ALCGetContextsDevice;

    // AL
    private static alGetStringDelegate? ALGetString;
    private static alGetErrorDelegate? ALGetError;
    private static alGenBuffersDelegate? ALGenBuffers;
    private static alDeleteBuffersDelegate? ALDeleteBuffers;
    private static alBufferDataDelegate? ALBufferData;
    private static alBufferiDelegate? ALBufferi;
    private static alBuffer3iDelegate? ALBuffer3i;
    private static alBufferivDelegate? ALBufferiv;
    private static alGenSourcesDelegate? ALGenSources;
    private static alDeleteSourcesDelegate? ALDeleteSources;
    private static alSourceiDelegate? ALSourcei;
    private static alSource3iDelegate? ALSource3i;
    private static alSourcefDelegate? ALSourcef;
    private static alSource3fDelegate? ALSource3f;
    private static alSourcefvDelegate? ALSourcefv;
    private static alGetSourceiDelegate? ALGetSourcei;
    private static alGetSourcefDelegate? ALGetSourcef;
    private static alGetSource3fDelegate? ALGetSource3f;
    private static alSourcePlayDelegate? ALSourcePlay;
    private static alSourcePauseDelegate? ALSourcePause;
    private static alSourceStopDelegate? ALSourceStop;
    private static alSourceRewindDelegate? ALSourceRewind;
    private static alSourceQueueBuffersDelegate? ALSourceQueueBuffers;
    private static alSourceUnqueueBuffersDelegate? ALSourceUnqueueBuffers;
    private static alGetListenerfDelegate? ALGetListenerf;
    private static alGetListener3fDelegate? ALGetListener3f;
    private static alListenerfDelegate? ALListenerf;
    private static alListener3fDelegate? ALListener3f;
    private static alListenerfvDelegate? ALListenerfv;
    private static alDopplerFactorDelegate? ALDopplerFactor;
    private static alDistanceModelDelegate? ALDistanceModel;
    private static alSpeedOfSoundDelegate? ALSpeedOfSound;

    #endregion

    #region ---- Константы ----

    public const int ALC_DEVICE_SPECIFIER = 0x1005;
    public const int ALC_DEFAULT_DEVICE_SPECIFIER = 0x1004;
    public const int ALC_EXTENSIONS = 0x1006;
    public const int ALC_MAJOR_VERSION = 0x1007;
    public const int ALC_MINOR_VERSION = 0x1008;

    public const int AL_NO_ERROR = 0;
    public const int AL_INVALID_NAME = 0xA001;
    public const int AL_INVALID_ENUM = 0xA002;
    public const int AL_INVALID_VALUE = 0xA003;
    public const int AL_INVALID_OPERATION = 0xA004;
    public const int AL_OUT_OF_MEMORY = 0xA005;

    public const int AL_FORMAT_MONO8 = 0x1100;
    public const int AL_FORMAT_MONO16 = 0x1101;
    public const int AL_FORMAT_STEREO8 = 0x1102;
    public const int AL_FORMAT_STEREO16 = 0x1103;

    public const int AL_BUFFER = 0x1009;
    public const int AL_GAIN = 0x100A;
    public const int AL_PITCH = 0x1003;
    public const int AL_POSITION = 0x1004;
    public const int AL_VELOCITY = 0x1005;
    public const int AL_LOOPING = 0x1007;
    public const int AL_REFERENCE_DISTANCE = 0x1020;
    public const int AL_MAX_DISTANCE = 0x1021;
    public const int AL_ROLLOFF_FACTOR = 0x1022;
    public const int AL_CONE_OUTER_GAIN = 0x1023;
    public const int AL_CONE_OUTER_ANGLE = 0x1024;
    public const int AL_CONE_INNER_ANGLE = 0x1025;
    public const int AL_SOURCE_STATE = 0x1010;

    public const int AL_INITIAL = 0x1011;
    public const int AL_PLAYING = 0x1012;
    public const int AL_PAUSED = 0x1013;
    public const int AL_STOPPED = 0x1014;

    public const int AL_ORIENTATION = 0x100F;

    public const int AL_NONE = 0;
    public const int AL_INVERSE_DISTANCE = 0x1101;
    public const int AL_INVERSE_DISTANCE_CLAMPED = 0x1102;
    public const int AL_LINEAR_DISTANCE = 0x1103;
    public const int AL_LINEAR_DISTANCE_CLAMPED = 0x1104;
    public const int AL_EXPONENT_DISTANCE = 0x1105;
    public const int AL_EXPONENT_DISTANCE_CLAMPED = 0x1106;

    public const int AL_FREQUENCY = 0x2001;
    public const int AL_BITS = 0x2002;
    public const int AL_CHANNELS = 0x2003;
    public const int AL_SIZE = 0x2004;

    #endregion
}