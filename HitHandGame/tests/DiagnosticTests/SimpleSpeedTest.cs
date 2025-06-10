using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using HitHandGame.Audio.Providers;

namespace HitHandGame.Tests.DiagnosticTests
{
    /// <summary>
    /// 簡單的速度測試類別，用於診斷音效播放問題
    /// </summary>
    public static class SimpleSpeedTest
    {
        /// <summary>
        /// 測試不使用 SoundTouch 的基本播放功能
        /// </summary>
        public static async Task TestBasicPlayback()
        {
            Console.WriteLine("=== 基本播放測試 ===");
            
            string soundsDir = "Sounds";
            string testFile = Path.Combine(soundsDir, "hit.mp3");
            
            if (!File.Exists(testFile))
            {
                Console.WriteLine($"測試檔案不存在: {testFile}");
                return;
            }
            
            Console.WriteLine($"測試播放: {testFile}");
            
            try
            {
                using var audioFile = new AudioFileReader(testFile);
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"檔案資訊: {audioFile.WaveFormat}");
                Console.WriteLine($"檔案長度: {audioFile.TotalTime}");
                
                waveOut.Init(audioFile);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine("基本播放完成");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"基本播放錯誤: {ex.Message}");
            }
        }
          /// <summary>
        /// 測試使用替代方案的變速功能
        /// </summary>
        public static async Task TestVarispeedPlayback(float speed = 1.5f)
        {
            Console.WriteLine($"=== 替代變速播放測試 (速度: {speed}x) ===");
            
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
                
                // 使用我們的 ImprovedSoundTouchSampleProvider
                var speedProvider = new ImprovedSoundTouchSampleProvider(audioFile, speed, true);
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"使用 ImprovedSoundTouchSampleProvider，速度: {speed}x");
                
                waveOut.Init(speedProvider);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"變速播放完成 ({speed}x)");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始變速播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"變速播放錯誤: {ex.Message}");
                Console.WriteLine($"錯誤詳細: {ex}");
            }
        }
        
        /// <summary>
        /// 測試簡單的調整取樣率方式（會改變音調）
        /// </summary>
        public static async Task TestSimpleSpeedChange(float speed = 1.5f)
        {
            Console.WriteLine($"=== 簡單速度調整測試 (速度: {speed}x) ===");
            
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
                
                // 建立自訂的簡單速度調整提供者
                var speedProvider = new SimpleSpeedSampleProvider(audioFile, speed);
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine($"使用 SimpleSpeedSampleProvider，速度: {speed}x");
                
                waveOut.Init(speedProvider);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"簡單速度調整播放完成 ({speed}x)");
                    playbackComplete.SetResult(true);
                };
                
                waveOut.Play();
                Console.WriteLine("開始播放...");
                
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"簡單速度調整錯誤: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 簡單的速度調整 Sample Provider（透過跳過樣本來實現）
    /// </summary>
    public class SimpleSpeedSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly float speed;
        private float position;
        
        public WaveFormat WaveFormat => source.WaveFormat;
        
        public SimpleSpeedSampleProvider(ISampleProvider source, float speed)
        {
            this.source = source;
            this.speed = speed;
            this.position = 0;
        }
        
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = 0;
            float[] tempBuffer = new float[count * 2]; // 較大的暫存緩衝區
            
            for (int i = 0; i < count; i++)
            {
                int sourcePosition = (int)(position * speed);
                
                if (sourcePosition < tempBuffer.Length / 2)
                {
                    int actualRead = source.Read(tempBuffer, 0, Math.Min(tempBuffer.Length, sourcePosition + 2));
                    if (actualRead == 0) break;
                    
                    if (sourcePosition < actualRead)
                    {
                        buffer[offset + samplesRead] = tempBuffer[sourcePosition];
                        samplesRead++;
                    }
                }
                
                position += 1.0f;
            }
            
            return samplesRead;
        }
    }
}
