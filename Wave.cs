using NAudio.Wave;

namespace ToWhisperNet
{
    public class Wave
    {
        public double[] Data { get; set; }
        public double[] EData { get; set; }
        public WaveFormat Format { get; private set; }

        public Wave(double[] data, WaveFormat format)
        {
            Data = data;
            Format = format;
        }

        public double[] Read(string filename)
        {
            Data = null;

            Format = new WaveFormat(48000, 16, 1);
            string tmpFile = "resampled.wav";
            using (WaveFileReader reader = new WaveFileReader(filename))
            {
                using (var resampler = new MediaFoundationResampler(reader, Format))
                {
                    WaveFileWriter.CreateWaveFile(tmpFile, resampler);
                }
            }
            using (WaveFileReader reader = new WaveFileReader(tmpFile))
            {
                byte[] src = new byte[reader.Length];
                reader.Read(src, 0, src.Length);
                Data = ConvertToDouble(src);
            }

            return Data;
        }

        public void Write(string filename, double[] data)
        {
            using (WaveFileWriter writer = new WaveFileWriter(filename, Format))
            {
                float scale = 5f;
                writer.Write(ConvertToByte(data, scale), 0, data.Length * 2);
            }
        }

        short nonzero = 1;

        private double[] ConvertToDouble(byte[] data)
        {
            double[] result = new double[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                short d = (short)(data[i] | (data[i + 1] << 8));
                if (d == 0)
                {
                    d = nonzero;
                }
                result[i / 2] = d / 32767.0;
            }
            return result;
        }

        private byte[] ConvertToByte(double[] data, float scale)
        {
            byte[] result = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                short d = (short)(data[i] * 32767.0 * scale);
                if (d == nonzero)
                {
                    d = 0;
                }
                result[i * 2] = (byte)(d & 255);
                result[i * 2 + 1] = (byte)((d >> 8) & 255);
            }
            return result;
        }
    }
}
