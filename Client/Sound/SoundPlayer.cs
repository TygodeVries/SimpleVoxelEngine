using Client.Rendering;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;
using Shared.Mathf;

namespace Client.Sound;

public class SoundPlayer
{
    private static readonly Dictionary<string, int> audioClips = new();

    private static readonly List<int> activeSources = new();

    public static void Start()
    {
        var device = ALC.OpenDevice(null);
        var contex = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(contex);

        SoundPlayer.LoadAudio("test.wav");

        AL.DistanceModel(DistanceModel.LinearDistanceClamped);
    }

    public static void LoadAudio(string path)
    {
        string shortName = Path.GetFileNameWithoutExtension(path);
        string fullPath = Path.Combine("sounds", path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Audio file not found at: {fullPath}");
        }

        int bufferId = AL.GenBuffer();

        byte[] rawPcmData = LoadWavData(fullPath, out Format format, out int sampleRate);

        AL.BufferData(bufferId, format, rawPcmData.AsSpan(), rawPcmData.Length, sampleRate);

        audioClips[shortName] = bufferId;
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

    private static byte[] LoadWavData(string filename, out Format format, out int sampleRate)
    {
        using var stream = File.OpenRead(filename);
        using var reader = new BinaryReader(stream);

        if (new string(reader.ReadChars(4)) != "RIFF")
            throw new NotSupportedException("Invalid RIFF descriptor.");

        reader.ReadInt32();

        if (new string(reader.ReadChars(4)) != "WAVE")
            throw new NotSupportedException("Invalid WAVE audio format.");

        if (new string(reader.ReadChars(4)) != "fmt ")
            throw new NotSupportedException("Invalid layout section header.");

        int subChunk1Size = reader.ReadInt32();
        int audioFormat = reader.ReadInt16();
        int numChannels = reader.ReadInt16();
        sampleRate = reader.ReadInt32();
        reader.ReadInt32();
        reader.ReadInt16();
        int bitsPerSample = reader.ReadInt16();

        if (subChunk1Size > 16)
            stream.Seek(subChunk1Size - 16, SeekOrigin.Current);

        string chunkHeader = new string(reader.ReadChars(4));
        while (chunkHeader != "data")
        {
            int remainingBytes = reader.ReadInt32();
            stream.Seek(remainingBytes, SeekOrigin.Current);
            chunkHeader = new string(reader.ReadChars(4));
        }

        int dataSize = reader.ReadInt32();
        byte[] rawAudioBytes = reader.ReadBytes(dataSize);

        // Force the sound to be mono, as if its not, falloff and 3d sound will not work.
        if (numChannels == 2)
        {
            if (bitsPerSample == 16)
            {
                int totalFrames = rawAudioBytes.Length / 4;
                byte[] monoBytes = new byte[totalFrames * 2];

                for (int i = 0; i < totalFrames; i++)
                {
                    short left = (short)((rawAudioBytes[(i * 4) + 1] << 8) | rawAudioBytes[i * 4]);
                    short right = (short)((rawAudioBytes[(i * 4) + 3] << 8) | rawAudioBytes[(i * 4) + 2]);

                    short monoSample = (short)((left + right) / 2);

                    monoBytes[i * 2] = (byte)(monoSample & 0xFF);
                    monoBytes[(i * 2) + 1] = (byte)((monoSample >> 8) & 0xFF);
                }
                rawAudioBytes = monoBytes;
            }
            else if (bitsPerSample == 8)
            {
                int totalFrames = rawAudioBytes.Length / 2;
                byte[] monoBytes = new byte[totalFrames];

                for (int i = 0; i < totalFrames; i++)
                {
                    int left = rawAudioBytes[i * 2];
                    int right = rawAudioBytes[(i * 2) + 1];
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
            _ => throw new NotSupportedException($"Format combination not supported: {numChannels} Channels, {bitsPerSample}-bit.")
        };

        return rawAudioBytes;
    }
}