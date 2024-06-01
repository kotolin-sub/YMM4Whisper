using System;
using YukkuriMovieMaker.Player.Audio;
using YukkuriMovieMaker.Player.Audio.Effects;
using ToWhisperNet;
using NAudio.Wave;

namespace WhisperAudioEffect
{
    internal class WhisperAudioEffectProcessor : AudioEffectProcessorBase
    {
        readonly WhisperAudioEffect item;
        readonly TimeSpan duration;
        private Whisper whisper;
        public override int Hz => Input?.Hz ?? 0;

        public override long Duration => (long)(duration.TotalSeconds * Input?.Hz ?? 0) * 2;

        public WhisperAudioEffectProcessor(WhisperAudioEffect item, TimeSpan duration)
        {
            this.item = item;
            this.duration = duration;
            
            this.whisper = new Whisper();
        }

        protected override void seek(long position)
        {
            Input?.Seek(position);
        }

        protected override int read(float[] destBuffer, int offset, int count)
        {
            int samplesRead = Input?.Read(destBuffer, offset, count) ?? 0;

            if (samplesRead > 0)
            {
                double[] inputData = new double[samplesRead];
                for (int i = 0; i < samplesRead; i++)
                {
                    inputData[i] = destBuffer[offset + i];
                }

                Wave wave = new Wave(inputData, new WaveFormat(Hz, 16, 1));

                var hpf = item.Hpf;
                var order = item.Order;

                whisper.Convert(wave);

                for (int i = 0; i < samplesRead; i++)
                {
                    destBuffer[offset + i] = (float)wave.Data[i];
                }
            }

            return samplesRead;
        }
    }
}
