using System.Numerics;

namespace RayLib3dTest;

public static class MrPerlin
{
    private static Random _random = new Random();
    private static int[] _permutation;

    private static Vector2[] _gradients;

    static MrPerlin()
    {
        CalculatePermutation(out _permutation);
        CalculateGradients(out _gradients);
    }

    private static void CalculatePermutation(out int[] p)
    {
        p = Enumerable.Range(0, 256).ToArray();

        /// shuffle the array
        for (var i = 0; i < p.Length; i++)
        {
            var source = _random.Next(p.Length);

            (p[i], p[source]) = (p[source], p[i]);
        }
    }

    /// <summary>
    /// generate a new permutation.
    /// </summary>
    public static void Reseed()
    {
        CalculatePermutation(out _permutation);
    }

    private static void CalculateGradients(out Vector2[] grad)
    {
        grad = new Vector2[256];

        for (var i = 0; i < grad.Length; i++)
        {
            Vector2 gradient;

            do
            {
                gradient = new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1));
            }
            while (gradient.LengthSquared() >= 1);

            gradient = Vector2.Normalize(gradient);

            grad[i] = gradient;
        }

    }

    private static float Drop(float t)
    {
        t = Math.Abs(t);
        return 1f - t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Q(float u, float v)
    {
        return Drop(u) * Drop(v);
    }

    public static float Noise(float x, float y)
    {
        var cell = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));

        var total = 0f;

        Span<Vector2> corners = stackalloc Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

        foreach (var n in corners)
        {
            var ij = cell + n;
            var uv = new Vector2(x - ij.X, y - ij.Y);

            var index = _permutation[(int)ij.X % _permutation.Length];
            index = _permutation[(index + (int)ij.Y) % _permutation.Length];

            var grad = _gradients[index % _gradients.Length];

            total += Q(uv.X, uv.Y) * Vector2.Dot(grad, uv);
        }

        return Math.Max(Math.Min(total, 1f), -1f);
    }

    public static float[] GenerateNoiseMap(int width, int height, int octaves, float frequency = 0.5f, float amplitude = 1f)
    {
        var data = new float[width * height];

        var min = float.MaxValue;
        var max = float.MinValue;
        
        Reseed();

        for (var octave = 0; octave < octaves; octave++)
        {
            Parallel.For(0
                ,width * height
                ,offset =>
                {
                    var i = offset % width;
                    var j = offset / width;
                    var noise = Noise(i * frequency *1f / width, j * frequency * 1f / height);
                    noise = data[j * width + i] += noise * amplitude;

                    min = Math.Min(min, noise);
                    max = Math.Max(max, noise);
                }
            );

            frequency *= 2;
            amplitude /= 2;
        }
        
        return data.Select(x => (x - min) / (max - min)).ToArray();
    }
}