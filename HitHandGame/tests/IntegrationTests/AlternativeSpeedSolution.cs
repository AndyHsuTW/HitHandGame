using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace HitHandGame.Tests.IntegrationTests
{
    /// <summary>
    /// 不使用 SoundTouch 的替代速度調整方案
    /// </summary>
    public static class AlternativeSpeedSolution
    {
        /// <summary>
        /// 使用簡單的重取樣方法調整播放速度（會改變音調）
        /// </summary>
        public static async Task TestSimpleResample(float speed = 1.5f)
        {
            Console.WriteLine($"=== 簡單重取樣測試 (速度: {speed}x) ===");
            
            string soundsDir = "Sounds";
            string testFile = Path.Combine(soundsDir, "hit.mp3");
            
            if (!File.Exists(testFile))
            {
                Console.WriteLine($"測試檔案不存在: {testFile}");
                return;
            }
            
            try
            {
                using var audioFile = new AudioFileReader(testFile);
                
                // 調整取樣率來改變播放速度
                var newSampleRate = (int)(audioFile.WaveFormat.SampleRate * speed);
                var resampler = new WdlResamplingSampleProvider(audioFile, newSampleRate);
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"原始取樣率: {audioFile.WaveFormat.SampleRate}Hz");
                Console.WriteLine($"新的取樣率: {newSampleRate}Hz");
                Console.WriteLine($"播放速度: {speed}x (音調會改變)");
                
                waveOut.Init(resampler);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"重取樣播放完成 ({speed}x)");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重取樣播放錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用播放速度調整（僅改變播放時間，保持音調）
        /// </summary>
        public static async Task TestPlaybackSpeedAdjustment(float speed = 1.5f)
        {
            Console.WriteLine($"=== 播放速度調整測試 (速度: {speed}x) ===");
            
            string soundsDir = "Sounds";
            string testFile = Path.Combine(soundsDir, "hit.mp3");
            
            if (!File.Exists(testFile))
            {
                Console.WriteLine($"測試檔案不存在: {testFile}");
                return;
            }
            
            try
            {
                using var audioFile = new AudioFileReader(testFile);
                
                // 使用自定義的播放速度調整
                var speedProvider = new PlaybackSpeedSampleProvider(audioFile, speed);
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"檔案長度: {audioFile.TotalTime}");
                Console.WriteLine($"調整後長度: {TimeSpan.FromSeconds(audioFile.TotalTime.TotalSeconds / speed)}");
                Console.WriteLine($"播放速度: {speed}x (音調保持不變)");
                
                waveOut.Init(speedProvider);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"速度調整播放完成 ({speed}x)");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"速度調整播放錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用 NAudio 內建的 SmbPitchShiftingSampleProvider
        /// </summary>
        public static async Task TestPitchShifting(float speed = 1.5f)
        {
            Console.WriteLine($"=== Pitch Shifting 測試 (速度: {speed}x) ===");
            
            string soundsDir = "Sounds";
            string testFile = Path.Combine(soundsDir, "hit.mp3");
            
            if (!File.Exists(testFile))
            {
                Console.WriteLine($"測試檔案不存在: {testFile}");
                return;
            }
            
            try
            {
                using var audioFile = new AudioFileReader(testFile);
                
                // 先調整播放速度，再用 pitch shifting 修正音調
                var speedProvider = new PlaybackSpeedSampleProvider(audioFile, speed);
                var pitchShifter = new SmbPitchShiftingSampleProvider(speedProvider);
                pitchShifter.PitchFactor = 1.0f / speed; // 修正音調
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"播放速度: {speed}x");
                Console.WriteLine($"音調修正: {1.0f / speed}x");
                
                waveOut.Init(pitchShifter);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"Pitch Shifting 播放完成 ({speed}x)");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pitch Shifting 播放錯誤: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 簡單的播放速度調整 Sample Provider
    /// </summary>
    public class PlaybackSpeedSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float speed;
        private float currentPosition = 0;

        public WaveFormat WaveFormat => source.WaveFormat;

        public PlaybackSpeedSampleProvider(ISampleProvider source, float speed)
        {
            this.source = source;
            this.speed = speed;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var tempBuffer = new float[count * 2]; // 較大的暫存緩衝區
            int samplesRead = 0;

            for (int i = 0; i < count; i++)
            {
                int sourceIndex = (int)currentPosition;
                
                if (sourceIndex >= tempBuffer.Length - 1)
                {
                    // 需要更多來源數據
                    int actualRead = source.Read(tempBuffer, 0, tempBuffer.Length);
                    if (actualRead == 0) break;
                    sourceIndex = (int)currentPosition % actualRead;
                }
                else if (samplesRead == 0)
                {
                    // 第一次讀取
                    int actualRead = source.Read(tempBuffer, 0, tempBuffer.Length);
                    if (actualRead == 0) break;
                }

                if (sourceIndex < tempBuffer.Length)
                {
                    buffer[offset + samplesRead] = tempBuffer[sourceIndex];
                    samplesRead++;
                }

                currentPosition += speed;
                
                // 重設位置以避免溢出
                if (currentPosition >= tempBuffer.Length)
                {
                    currentPosition = 0;
                }
            }

            return samplesRead;
        }
    }
}
