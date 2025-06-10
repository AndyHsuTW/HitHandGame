using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using HitHandGame.Audio.Providers;

namespace HitHandGame.Tests.DiagnosticTests
{
    /// <summary>
    /// WaveFormat 診斷工具 - 專門檢查變速播放時的 WaveFormat 問題
    /// </summary>
    public static class WaveFormatDiagnostics
    {        /// <summary>
        /// 比較正常播放和變速播放的 WaveFormat
        /// </summary>
        public static void DiagnoseWaveFormatIssues()
        {
            Console.WriteLine("=== WaveFormat 診斷測試 ===");
            
            string soundsDir = "Sounds";
            string testFile = Path.Combine(soundsDir, "hit.mp3");
            
            if (!File.Exists(testFile))
            {
                Console.WriteLine($"測試檔案不存在: {testFile}");
                return;
            }
            
            try
            {
                // 1. 檢查原始檔案的 WaveFormat
                using var originalFile = new AudioFileReader(testFile);
                Console.WriteLine($"原始檔案 WaveFormat:");
                Console.WriteLine($"  編碼: {originalFile.WaveFormat.Encoding}");
                Console.WriteLine($"  取樣率: {originalFile.WaveFormat.SampleRate}Hz");
                Console.WriteLine($"  位元深度: {originalFile.WaveFormat.BitsPerSample}bit");
                Console.WriteLine($"  聲道數: {originalFile.WaveFormat.Channels}");
                Console.WriteLine($"  位元組率: {originalFile.WaveFormat.AverageBytesPerSecond}");
                Console.WriteLine($"  完整格式: {originalFile.WaveFormat}");
                Console.WriteLine();
                
                // 2. 檢查 ImprovedSoundTouchSampleProvider 的 WaveFormat
                using var testFile2 = new AudioFileReader(testFile);
                var soundTouchProvider = new ImprovedSoundTouchSampleProvider(testFile2, 1.5f, true);
                Console.WriteLine($"SoundTouch 處理後 WaveFormat:");
                Console.WriteLine($"  編碼: {soundTouchProvider.WaveFormat.Encoding}");
                Console.WriteLine($"  取樣率: {soundTouchProvider.WaveFormat.SampleRate}Hz");
                Console.WriteLine($"  位元深度: {soundTouchProvider.WaveFormat.BitsPerSample}bit");
                Console.WriteLine($"  聲道數: {soundTouchProvider.WaveFormat.Channels}");
                Console.WriteLine($"  位元組率: {soundTouchProvider.WaveFormat.AverageBytesPerSecond}");
                Console.WriteLine($"  完整格式: {soundTouchProvider.WaveFormat}");
                Console.WriteLine();
                
                // 3. 檢查格式是否匹配
                bool formatsMatch = originalFile.WaveFormat.Equals(soundTouchProvider.WaveFormat);
                Console.WriteLine($"WaveFormat 是否匹配: {(formatsMatch ? "✅ 是" : "❌ 否")}");
                
                if (!formatsMatch)
                {
                    Console.WriteLine("❌ WaveFormat 不匹配可能是無聲音的原因！");
                }
                  // 4. 測試實際的樣本資料
                Console.WriteLine("\n--- 測試樣本資料 ---");
                TestSampleData(soundTouchProvider);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"診斷過程發生錯誤: {ex.Message}");
                Console.WriteLine($"詳細錯誤: {ex}");
            }
        }
          /// <summary>
        /// 測試 SoundTouch 輸出的實際樣本資料
        /// </summary>
        private static void TestSampleData(ISampleProvider provider)
        {
            float[] buffer = new float[4096]; // 測試緩衝區
            int totalSamples = 0;
            int readCycles = 0;
            int maxNonZeroSamples = 0;
            
            Console.WriteLine("開始讀取樣本資料...");
            
            for (int cycle = 0; cycle < 10; cycle++) // 最多讀取10次
            {
                int samplesRead = provider.Read(buffer, 0, buffer.Length);
                readCycles++;
                totalSamples += samplesRead;
                
                if (samplesRead == 0)
                {
                    Console.WriteLine($"第 {readCycles} 次讀取: 0 樣本 (可能已結束)");
                    break;
                }
                
                // 檢查樣本是否為零
                int nonZeroSamples = 0;
                float maxAbsValue = 0;
                float minValue = float.MaxValue;
                float maxValue = float.MinValue;
                
                for (int i = 0; i < samplesRead; i++)
                {
                    if (Math.Abs(buffer[i]) > 0.00001f) // 不是接近零的值
                    {
                        nonZeroSamples++;
                        maxAbsValue = Math.Max(maxAbsValue, Math.Abs(buffer[i]));
                        minValue = Math.Min(minValue, buffer[i]);
                        maxValue = Math.Max(maxValue, buffer[i]);
                    }
                }
                
                maxNonZeroSamples = Math.Max(maxNonZeroSamples, nonZeroSamples);
                
                Console.WriteLine($"第 {readCycles} 次讀取: {samplesRead} 樣本");
                Console.WriteLine($"  非零樣本: {nonZeroSamples}");
                Console.WriteLine($"  最大絕對值: {maxAbsValue:F6}");
                Console.WriteLine($"  值範圍: {minValue:F6} ~ {maxValue:F6}");
                
                // 顯示前10個樣本
                Console.Write($"  前10個樣本: ");
                for (int i = 0; i < Math.Min(10, samplesRead); i++)
                {
                    Console.Write($"{buffer[i]:F4} ");
                }
                Console.WriteLine();
                
                if (samplesRead < buffer.Length)
                {
                    Console.WriteLine("  (讀取的樣本數少於緩衝區大小，可能已接近結束)");
                    break;
                }
            }
            
            Console.WriteLine($"\n=== 樣本資料摘要 ===");
            Console.WriteLine($"總讀取週期: {readCycles}");
            Console.WriteLine($"總樣本數: {totalSamples}");
            Console.WriteLine($"最大非零樣本數: {maxNonZeroSamples}");
            
            if (totalSamples == 0)
            {
                Console.WriteLine("❌ 完全沒有樣本資料！");
            }
            else if (maxNonZeroSamples == 0)
            {
                Console.WriteLine("❌ 所有樣本都是零，這就是無聲音的原因！");
            }
            else
            {
                Console.WriteLine("✅ 有有效的樣本資料");
            }
        }
        
        /// <summary>
        /// 簡單的 SoundTouch 播放測試
        /// </summary>
        public static async Task SimplePlaybackTest()
        {
            Console.WriteLine("\n=== 簡單 SoundTouch 播放測試 ===");
            
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
                var soundTouchProvider = new ImprovedSoundTouchSampleProvider(audioFile, 1.5f, true);
                
                using var waveOut = new WaveOutEvent();
                
                Console.WriteLine("初始化 WaveOutEvent...");
                waveOut.Init(soundTouchProvider);
                
                var playbackComplete = new TaskCompletionSource<bool>();
                
                waveOut.PlaybackStopped += (sender, e) =>
                {
                    Console.WriteLine($"播放停止事件觸發");
                    if (e.Exception != null)
                    {
                        Console.WriteLine($"播放異常: {e.Exception}");
                    }
                    playbackComplete.SetResult(true);
                };
                
                Console.WriteLine("開始播放...");
                waveOut.Play();
                
                // 監控播放狀態
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10)); // 10秒超時
                var completedTask = await Task.WhenAny(playbackComplete.Task, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("❌ 播放超時（10秒）");
                    waveOut.Stop();
                }
                else
                {
                    Console.WriteLine("✅ 播放完成");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放測試錯誤: {ex.Message}");
                Console.WriteLine($"詳細錯誤: {ex}");
            }
        }
    }
}
