using System;
using NAudio.Wave;
using HitHandGame.Audio.Providers;

namespace HitHandGame.Audio.Effects
{
    /// <summary>
    /// 音效速度控制器 - 負責管理音效播放速度調整
    /// </summary>
    public class SpeedController
    {
        /// <summary>
        /// 應用速度變更到音效來源
        /// </summary>
        /// <param name="source">原始音效來源</param>
        /// <param name="speed">播放速度 (1.0 = 正常速度)</param>
        /// <param name="enableDebug">是否啟用除錯模式</param>
        /// <returns>套用速度變更的音效來源</returns>
        public static ISampleProvider ApplySpeedChange(ISampleProvider source, float speed, bool enableDebug = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // 如果速度為 1.0，直接返回原始來源
            if (Math.Abs(speed - 1.0f) < 0.001f)
            {
                if (enableDebug)
                    Console.WriteLine("[SpeedController] 速度為 1.0x，不需要處理");
                return source;
            }

            // 使用改進版的 SoundTouch 處理器
            if (enableDebug)
                Console.WriteLine($"[SpeedController] 套用速度變更: {speed}x");

            return new ImprovedSoundTouchSampleProvider(source, speed, enableDebug);
        }

        /// <summary>
        /// 檢查是否需要速度變更
        /// </summary>
        /// <param name="speed">目標速度</param>
        /// <returns>是否需要套用速度變更</returns>
        public static bool IsSpeedChangeRequired(float speed)
        {
            return Math.Abs(speed - 1.0f) >= 0.001f;
        }

        /// <summary>
        /// 驗證速度值是否有效
        /// </summary>
        /// <param name="speed">要驗證的速度值</param>
        /// <returns>是否為有效的速度值</returns>
        public static bool IsValidSpeed(float speed)
        {
            return speed >= 0.1f && speed <= 5.0f;
        }

        /// <summary>
        /// 將速度值限制在有效範圍內
        /// </summary>
        /// <param name="speed">輸入速度值</param>
        /// <returns>限制後的速度值</returns>
        public static float ClampSpeed(float speed)
        {
            return Math.Max(0.1f, Math.Min(5.0f, speed));
        }

        /// <summary>
        /// 取得建議的速度預設值
        /// </summary>
        /// <returns>建議的速度值陣列</returns>
        public static float[] GetRecommendedSpeeds()
        {
            return new float[] { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
        }

        /// <summary>
        /// 格式化速度值顯示
        /// </summary>
        /// <param name="speed">速度值</param>
        /// <returns>格式化的速度字串</returns>
        public static string FormatSpeed(float speed)
        {
            return $"{speed:F1}x";
        }
    }
}
