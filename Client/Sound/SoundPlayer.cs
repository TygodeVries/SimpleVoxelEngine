using Client.Rendering;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using Shared.Mathf;

namespace Client.Sound;

public class SoundPlayer
{
    private static readonly Dictionary<string, int> audioClips = new();

    private static readonly List<int> activeSources = new();

    public static void Reset()
    {
        foreach (int sourceId in activeSources)
        {
            try
            {
                AL.SourceStop(sourceId);
                AL.DeleteSource(sourceId);
            }
            catch
            {

            }
        }

        activeSources.Clear();

        foreach (int bufferId in audioClips.Values)
        {
            try
            {
                AL.DeleteBuffer(bufferId);
            }
            catch
            {

            }
        }

        audioClips.Clear();
    }

    public static void AddAudioResource(List<string>? names, byte[] data)
    {
        if (names == null)
            throw new NullReferenceException("Audio Name is null!");

        SoundPlayer.LoadAudio(names[0], data);
    }


    public static void Start()
    {
        var device = ALC.OpenDevice(null);
        var contex = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(contex);

        AL.DistanceModel(DistanceModel.LinearDistanceClamped);
    }

    public static void LoadAudio(string name, byte[] data)
    {
        int bufferId = AL.GenBuffer();

        byte[] rawPcmData = LoadWavData(data, out Format format, out int sampleRate);

        AL.BufferData(bufferId, format, rawPcmData.AsSpan(), rawPcmData.Length, sampleRate);

        audioClips[name] = bufferId;

        Console.WriteLine($"Loaded audio clip {name}...");
    }

    public static void PlayAudioGlobal(string name)
    {
        if (!audioClips.TryGetValue(name, out int bufferId))
        {
            Console.WriteLine($"[SoundPlayer Error] Audio clip '{name}' is not loaded.");
            return;
        }

        Update();

        int sourceId = AL.GenSource();

        AL.Sourcei(sourceId, (SourcePNameI)SourcePNameB.SourceRelative, 1);
        AL.Source3f(sourceId, SourcePName3F.Position, 0.0f, 0.0f, 0.0f);

        AL.Sourcei(sourceId, SourcePNameI.Buffer, bufferId);

        AL.SourcePlay(sourceId);

        activeSources.Add(sourceId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">The name of the audio clip</param>
    /// <param name="position">The location the audio is played at</param>
    /// <param name="volume">Gain</param>
    /// <param name="referenceDistance">The distance where the sound STARTS to fade.</param>
    /// <param name="maxDistance">The distance at what the sound can no longer be heard.</param>
    /// <param name="rolloffFactor">How fast the roll off is</param>
    public static void PlayAudioAtPosition(string name, Vector3 position, float volume, float referenceDistance = 1.0f, float maxDistance = 50.0f, float rolloffFactor = 1.0f)
    {
        if (!audioClips.TryGetValue(name, out int bufferId))
        {
            Console.WriteLine($"[SoundPlayer Error] Audio clip '{name}' is not loaded.");
            return;
        }

        Update();

        AL.Listener3f(ListenerPName3F.Position, Camera.Position.X, Camera.Position.Y, Camera.Position.Z);

        int sourceId = AL.GenSource();

        AL.Sourcei(sourceId, (SourcePNameI)SourcePNameB.SourceRelative, 0);
        AL.Source3f(sourceId, SourcePName3F.Position, position.X, position.Y, position.Z);
        AL.Sourcef(sourceId, SourcePNameF.Gain, volume);

        AL.Sourcef(sourceId, SourcePNameF.ReferenceDistance, referenceDistance);

        AL.Sourcef(sourceId, SourcePNameF.MaxDistance, maxDistance);

        AL.Sourcef(sourceId, SourcePNameF.RolloffFactor, rolloffFactor);

        AL.Sourcei(sourceId, SourcePNameI.Buffer, bufferId);
        AL.SourcePlay(sourceId);

        activeSources.Add(sourceId);
    }

    public static void Update()
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            int sourceId = activeSources[i];
            AL.GetSourcei(sourceId, SourceGetPNameI.SourceState, out int state);

            if ((SourceState)state == SourceState.Stopped)
            {
                AL.DeleteSource(sourceId);
                activeSources.RemoveAt(i);
            }
        }

        UpdateListenerState();
    }

    public static void UpdateListenerState()
    {
        AL.Listener3f(ListenerPName3F.Position, Camera.Position.X, Camera.Position.Y, Camera.Position.Z);

        float[] listenerOrientation = new float[]
        {
        Camera.Direction.X, Camera.Direction.Y, Camera.Direction.Z,
        0.0f, 1.0f, 0.0f
        };
        AL.Listenerfv(ListenerPNameFV.Orientation, listenerOrientation);
    }


    private static byte[] LoadWavData(byte[] data, out Format format, out int sampleRate)
    {
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);

        string riff = new(reader.ReadChars(4));

        if (riff != "RIFF")
            throw new NotSupportedException("Invalid RIFF descriptor.");

        int riffSize = reader.ReadInt32();

        string wave = new(reader.ReadChars(4));

        if (wave != "WAVE")
            throw new NotSupportedException("Invalid WAVE audio format.");

        int audioFormat = 0;
        int numChannels = 0;
        int bitsPerSample = 0;
        sampleRate = 0;

        bool foundFmt = false;
        bool foundData = false;

        byte[] rawAudioBytes = Array.Empty<byte>();

        while (stream.Position + 8 <= stream.Length)
        {
            long chunkPosition = stream.Position;

            string chunkId = new(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            Console.WriteLine(
                $"WAV chunk: '{chunkId}' @ {chunkPosition}, size={chunkSize}");

            if (chunkId == "data")
            {
                if (chunkSize == -1)
                    chunkSize = checked((int)(stream.Length - stream.Position));

                if (chunkSize < 0)
                {
                    throw new InvalidDataException(
                        $"Invalid WAV data chunk size: {chunkSize}.");
                }

                if (chunkSize > stream.Length - stream.Position)
                {
                    throw new InvalidDataException(
                        $"WAV data chunk extends past end of WAV. " +
                        $"Size={chunkSize}, " +
                        $"Remaining={stream.Length - stream.Position}, " +
                        $"Position={stream.Position}.");
                }

                rawAudioBytes = reader.ReadBytes(chunkSize);

                if (rawAudioBytes.Length != chunkSize)
                {
                    throw new InvalidDataException(
                        $"Could not read complete WAV data chunk. " +
                        $"Expected={chunkSize}, " +
                        $"Read={rawAudioBytes.Length}.");
                }

                foundData = true;
                break;
            }

            if (chunkId == "fmt ")
            {
                if (chunkSize == -1)
                {
                    throw new InvalidDataException(
                        "The fmt chunk cannot have an unknown size.");
                }

                if (chunkSize < 16)
                {
                    throw new InvalidDataException(
                        $"Invalid fmt chunk size: {chunkSize}.");
                }

                if (chunkSize > stream.Length - stream.Position)
                {
                    throw new InvalidDataException(
                        $"fmt chunk extends past end of WAV. " +
                        $"Size={chunkSize}, " +
                        $"Remaining={stream.Length - stream.Position}.");
                }

                long chunkEnd = stream.Position + chunkSize;

                audioFormat = reader.ReadInt16();
                numChannels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();

                reader.ReadInt32();
                reader.ReadInt16();
                bitsPerSample = reader.ReadInt16();

                stream.Position = chunkEnd;

                foundFmt = true;
            }
            else
            {
                if (chunkSize == -1)
                {
                    throw new InvalidDataException(
                        $"Unknown-size non-data chunk '{chunkId}'.");
                }

                if (chunkSize > stream.Length - stream.Position)
                {
                    throw new InvalidDataException(
                        $"WAV chunk '{chunkId}' extends past end of WAV. " +
                        $"Size={chunkSize}, " +
                        $"Remaining={stream.Length - stream.Position}, " +
                        $"Position={stream.Position}.");
                }

                stream.Seek(chunkSize, SeekOrigin.Current);
            }

            if ((chunkSize & 1) != 0)
            {
                if (stream.Position >= stream.Length)
                    break;

                stream.Seek(1, SeekOrigin.Current);
            }
        }

        if (!foundFmt)
            throw new InvalidDataException("WAV fmt chunk not found.");

        if (!foundData)
            throw new InvalidDataException("WAV data chunk not found.");

        if (audioFormat != 1)
        {
            throw new NotSupportedException(
                $"Unsupported WAV audio format: {audioFormat}. " +
                "Only PCM is supported.");
        }

        if (numChannels != 1 && numChannels != 2)
        {
            throw new NotSupportedException(
                $"Unsupported channel count: {numChannels}.");
        }

        if (bitsPerSample != 8 && bitsPerSample != 16)
        {
            throw new NotSupportedException(
                $"Unsupported bit depth: {bitsPerSample}.");
        }

        if (numChannels == 2)
        {
            if (bitsPerSample == 16)
            {
                if (rawAudioBytes.Length % 4 != 0)
                {
                    throw new InvalidDataException(
                        "Stereo 16-bit WAV data is not aligned to a complete frame.");
                }

                int totalFrames = rawAudioBytes.Length / 4;
                byte[] monoBytes = new byte[totalFrames * 2];

                for (int i = 0; i < totalFrames; i++)
                {
                    int offset = i * 4;

                    short left = (short)(
                        rawAudioBytes[offset] |
                        (rawAudioBytes[offset + 1] << 8));

                    short right = (short)(
                        rawAudioBytes[offset + 2] |
                        (rawAudioBytes[offset + 3] << 8));

                    short monoSample = (short)(
                        (left + right) / 2);

                    monoBytes[i * 2] = (byte)(monoSample & 0xFF);
                    monoBytes[(i * 2) + 1] = (byte)((monoSample >> 8) & 0xFF);
                }

                rawAudioBytes = monoBytes;
            }
            else
            {
                if (rawAudioBytes.Length % 2 != 0)
                {
                    throw new InvalidDataException(
                        "Stereo 8-bit WAV data is not aligned to a complete frame.");
                }

                int totalFrames = rawAudioBytes.Length / 2;
                byte[] monoBytes = new byte[totalFrames];

                for (int i = 0; i < totalFrames; i++)
                {
                    int offset = i * 2;

                    int left = rawAudioBytes[offset];
                    int right = rawAudioBytes[offset + 1];

                    monoBytes[i] = (byte)((left + right) / 2);
                }

                rawAudioBytes = monoBytes;
            }

            numChannels = 1;
        }

        format = (numChannels, bitsPerSample) switch
        {
            (1, 8) => Format.Mono8,
            (1, 16) => Format.Mono16,
            (2, 8) => Format.Stereo8,
            (2, 16) => Format.Stereo16,

            _ => throw new NotSupportedException(
                $"Format combination not supported: " +
                $"{numChannels} Channels, {bitsPerSample}-bit.")
        };

        Console.WriteLine(
            $"Loaded WAV: {sampleRate} Hz, " +
            $"{numChannels} channel(s), " +
            $"{bitsPerSample}-bit, " +
            $"{rawAudioBytes.Length} audio bytes.");

        return rawAudioBytes;


    }
}