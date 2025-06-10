using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace HitHandGame.Utilities
{
    /// <summary>
    /// 音效檔案處理輔助工具
    /// </summary>
    public static class AudioHelper
    {
        private static readonly string[] SupportedExtensions = { ".wav", ".mp3", ".m4a", ".aac" };

        /// <summary>
        /// 檢查檔案是否為有效的音效檔案
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        /// <returns>是否為有效的音效檔案</returns>
        public static bool IsValidAudioFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return SupportedExtensions.Contains(extension);
        }

        /// <summary>
        /// 取得音效檔案的 WaveFormat
        /// </summary>
        /// <param name="filePath">音效檔案路徑</param>
        /// <returns>WaveFormat，如果檔案無效則返回 null</returns>
        public static WaveFormat? GetWaveFormat(string filePath)
        {
            if (!IsValidAudioFile(filePath))
                return null;

            try
            {
                using var reader = new AudioFileReader(filePath);
                return reader.WaveFormat;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 取得音效檔案的長度
        /// </summary>
        /// <param name="filePath">音效檔案路徑</param>
        /// <returns>音效長度，如果檔案無效則返回 TimeSpan.Zero</returns>
        public static TimeSpan GetAudioDuration(string filePath)
        {
            if (!IsValidAudioFile(filePath))
                return TimeSpan.Zero;

            try
            {
                using var reader = new AudioFileReader(filePath);
                return reader.TotalTime;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// 格式化音效檔案大小
        /// </summary>
        /// <param name="bytes">檔案大小（位元組）</param>
        /// <returns>格式化的檔案大小字串</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int order = 0;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 格式化時間長度
        /// </summary>
        /// <param name="duration">時間長度</param>
        /// <returns>格式化的時間字串</returns>
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return duration.ToString(@"h\:mm\:ss");
            else
                return duration.ToString(@"m\:ss");
        }
    }
}
