namespace Shared.Mathf;

public static class Noise
{
    private static NoiseDotNet.NoiseSettings settings = new()
    {
        XFrequency = .1f,
        YFrequency = .1f,
        ZFrequency = .1f,
        Amplitude = 1f,
        Amplitude2 = 1f,
        Accumulate = false,
        Seed = 100
    };

    public static void SetFrequency(float freq)
    {
        if (freq == 1)
            freq += 0.01f;

        settings = new()
        {
            XFrequency = freq,
            YFrequency = freq,
            ZFrequency = freq,
            Amplitude = 1f,
            Amplitude2 = 1f,
            Accumulate = false,
            Seed = 100
        };
    }

    public static float Gradient(float x)
    {
        Span<float> xInput = [x];
        Span<float> yInput = [0f];
        Span<float> output = stackalloc float[1];

        NoiseDotNet.Noise.GradientNoise2D(
            xInput,
            yInput,
            output,
            in settings);

        return output[0];
    }

    public static float Gradient(float x, float y)
    {
        Span<float> xInput = [x];
        Span<float> yInput = [y];
        Span<float> output = stackalloc float[1];

        NoiseDotNet.Noise.GradientNoise2D(
            xInput,
            yInput,
            output,
            in settings);

        return output[0];
    }

    public static float Gradient(float x, float y, float z)
    {
        Span<float> xInput = [x];
        Span<float> yInput = [y];
        Span<float> zInput = [z];
        Span<float> output = stackalloc float[1];

        NoiseDotNet.Noise.GradientNoise3D(
            xInput,
            yInput,
            zInput,
            output,
            in settings);

        return output[0];
    }
}
