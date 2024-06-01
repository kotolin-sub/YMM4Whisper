using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Audio.Effects;
using YukkuriMovieMaker.Plugin.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using YukkuriMovieMaker.Player.Video;

namespace WhisperAudioEffect
{
    [AudioEffect("囁き声化", new[] { "フィルター" }, new string[] { })]
    public class WhisperAudioEffect : AudioEffectBase
    {
        /// <summary>
        /// エフェクトの名前
        /// </summary>
        public override string Label => "囁き声化エフェクト";

        /// <summary>
        /// アイテム編集エリアに表示するエフェクトの設定項目。
        /// [Display]と[AnimationSlider]等のアイテム編集コントロール属性の2つを設定する必要があります。
        /// [AnimationSlider]以外のアイテム編集コントロール属性の一覧はSamplePropertyEditorsプロジェクトを参照してください。
        /// </summary>

        [Display(GroupName = "Whisper", Name = "lpfの強度(未実装)", Description = "")]
        [AnimationSlider("F0", "", 0.0, 1.0)]
        public Animation Hpf { get; } = new Animation(0.97, 0.0, 1.0);

        [Display(GroupName = "Whisper", Name = "声帯音源の割合(未実装)", Description = "")]
        [AnimationSlider("F0", "%", 0, 100)]
        public Animation Order { get; } = new Animation(0, 0, 100);

        /// <summary>
        /// 音声エフェクトを作成する
        /// </summary>
        /// <param name="duration">音声エフェクトの長さ</param>
        /// <returns>音声エフェクト</returns>
        public override IAudioEffectProcessor CreateAudioEffect(TimeSpan duration)
        {
            return new WhisperAudioEffectProcessor(this, duration);
        }



        /// <summary>
        /// ExoFilterを作成する
        /// </summary>
        /// <param name="keyFrameIndex">キーフレーム番号</param>
        /// <param name="exoOutputDescription">exo出力に必要な各種項目</param>
        /// <returns>exoフィルタ</returns>
        public override IEnumerable<string> CreateExoAudioFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            var fps = exoOutputDescription.VideoInfo.FPS;
            return new[]
            {
                $"hoge=foo\r\n"
            };
        }

        /// <summary>
        /// IAnimatableを実装するプロパティを返す
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<IAnimatable> GetAnimatables() => new[] { Order,Hpf };
    }
}