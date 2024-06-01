using System;

namespace ToWhisperNet
{
    public class Whisper
    {
        public int Order { get; set; } = 0;
        public double Rate { get; set; } = 0.02;
        public double Hpf { get; set; } = 0.97;
        public double Lpf { get; set; } = 0.2;
        public double FrameT { get; set; } = 20.0;
        int frame = 0;

        Random random = new Random();

        public void Convert(Wave wave)
        {
            AdjustSize(wave);
            WhisperFilter(wave);
        }

        private void WhisperFilter(Wave wave)
        {
            Func<int, int, double> windowFunction = Hamming;
            int length = wave.Data.Length;
            double[] x = new double[frame];
            double[] y = new double[frame];
            double[] E = new double[frame];
            double[] a = new double[Order + 1];

            double[] v = wave.Data;
            double[] v1 = new double[v.Length];
            double[] v2 = new double[v.Length];

            for (int i = 0; i < length / (frame / 2) - 1; i++)
            {
                double max = 0.0;
                for (int j = 0; j < frame; j++)
                {
                    x[j] = v[j + i * (frame / 2)] * windowFunction(j, frame);
                }

                LevinsonDurbin(x, frame, a, Order);

                for (int j = 0; j < frame; j++)
                {
                    double e = 0.0;
                    for (int n = 0; n <= Order; n++)
                    {
                        if (j >= n)
                        {
                            e += a[n] * x[j - n];
                        }
                    }
                    E[j] = e;
                    max += e * e;
                }

                for (int j = 0; j < frame; j++)
                {
                    v2[j + i * (frame / 2)] += E[j] * 10.0;
                }

                for (int j = 0; j < frame; j++)
                {
                    y[j] = GenerateWhiteNoise();
                }

                for (int j = 1; j < frame; j++)
                {
                    E[j] = ((1.0 - Rate) * E[j - 1] + E[j]) / (2.0 - Rate);
                }

                max = Math.Sqrt(3.0 * max / frame);
                for (int j = 0; j < frame; j++)
                {
                    y[j] = Rate * E[j] + (1.0 - Rate) * max * y[j];
                }

                for (int j = 0; j < frame; j++)
                {
                    for (int n = 1; n <= Order; n++)
                    {
                        if (j >= n) y[j] -= a[n] * y[j - n];
                    }
                }

                for (int j = 0; j < frame; j++)
                {
                    v1[j + i * (frame / 2)] += y[j];
                }
            }


            for (int i = 1; i < length; i++) v1[i] = Lpf * v1[i - 1] + v1[i];
            for (int i = 1; i < length; i++) v2[i] = 0.97 * v2[i - 1] + v2[i];

            wave.Data = v1;
            wave.EData = v2;
        }

        private void AdjustSize(Wave wave)
        {

            if (Order == 0)
            {
                Order = wave.Format.SampleRate * 40 / 44100;
            }

            frame = (int)(wave.Format.SampleRate * FrameT / 1000);
            if (frame % 2 != 0) frame++;

            int last = wave.Data.Length;
            int length = wave.Data.Length - (wave.Data.Length % frame) + frame;

            double[] v1 = wave.Data;
            double[] v = new double[length];

            for (int i = 1; i < length; i++)
            {
                if (i < last)
                {
                    v[i] = v1[i] - Hpf * v1[i - 1];
                }
                else
                {
                    v[i] = 0.0;
                }
            }

            wave.Data = v;
        }

        double Hamming(int i, int frame)
        {
            return 0.54 - 0.46 * Math.Cos(2.0 * Math.PI / frame * i);
        }

        double AutoCorrelation(double[] x, int l, int N)
        {
            double res = 0.0;
            double r = 0.0, t = 0.0;
            for (int i = 0; i < N - l; i++)
            {
                t = res + (x[i] * x[i + l] + r);
                r = (x[i] * x[i + l] + r) - (t - res);
                res = t;
            }
            return res;
        }

        void LevinsonDurbin(double[] x, int length, double[] a, int lpcOrder)
        {
            double lambda = 0.0, E = 0.0;
            double[] r = new double[lpcOrder + 1];
            double[] V = new double[lpcOrder + 1];
            double[] U = new double[lpcOrder + 1];
            for (int i = 0; i < lpcOrder + 1; i++)
            {
                r[i] = AutoCorrelation(x, i, length);
            }
            for (int i = 0; i <= lpcOrder; i++)
            {
                a[i] = 0.0;
            }
            a[0] = 1.0;
            a[1] = -r[1] / r[0];
            E = r[0] + r[1] * a[1];
            for (int k = 1; k < lpcOrder; k++)
            {
                lambda = 0.0;
                for (int j = 0; j <= k; j++)
                {
                    lambda += a[j] * r[k + 1 - j];
                }
                lambda /= -E;
                for (int j = 0; j <= k + 1; j++)
                {
                    U[j] = a[j];
                    V[j] = a[k + 1 - j];
                }
                for (int j = 0; j <= k + 1; j++)
                {
                    a[j] = U[j] + lambda * V[j];
                }
                E = (1.0 - lambda * lambda) * E;
            }
            return;
        }

        double GenerateWhiteNoise()
        {
            return random.NextDouble() * 2.0 - 1.0;
        }
    }
}
