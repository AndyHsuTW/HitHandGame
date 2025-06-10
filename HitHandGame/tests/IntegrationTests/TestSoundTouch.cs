using System;
using NAudio.Wave;
using SoundTouch;

namespace HitHandGame.Tests.IntegrationTests
{
    /// <summary>
    /// 簡化的 SoundTouch 測試類別，用於診斷播放問題
    /// </summary>
    public class TestSoundTouch
    {
        public static void TestSoundTouchBasics()
        {
            Console.WriteLine("=== SoundTouch 基本測試 ===");
              // 測試 SoundTouchProcessor 基本功能
            var processor = new SoundTouchProcessor();
            Console.WriteLine($"SoundTouch 版本: {SoundTouchProcessor.VersionString}");
            
            // 設定基本參數
            processor.SampleRate = 44100;
            processor.Channels = 2;
            processor.Tempo = 1.5f; // 1.5倍速
            
            Console.WriteLine($"取樣率: {processor.SampleRate}");
            Console.WriteLine($"聲道數: {processor.Channels}");
            Console.WriteLine($"速度: {processor.Tempo}");
            
            // 設定 SoundTouch 參數
            processor.SetSetting(SoundTouch.SettingId.SequenceDurationMs, 40);
            processor.SetSetting(SoundTouch.SettingId.SeekWindowDurationMs, 15); 
            processor.SetSetting(SoundTouch.SettingId.OverlapDurationMs, 8);
            
            Console.WriteLine("SoundTouch 參數設定完成");
            
            // 創建測試資料（1秒的正弦波）
            int sampleRate = 44100;
            int channels = 2;
            int samples = sampleRate * channels; // 1秒
            float[] testData = new float[samples];
            
            for (int i = 0; i < samples; i += channels)
            {
                float time = (float)(i / channels) / sampleRate;
                float sineWave = (float)Math.Sin(2 * Math.PI * 440 * time) * 0.5f; // 440Hz A音
                testData[i] = sineWave;     // Left channel
                testData[i + 1] = sineWave; // Right channel
            }
            
            Console.WriteLine($"生成測試音訊: {samples} 樣本 ({samples/channels} frames)");
            
            // 將資料送入 SoundTouch
            int frames = samples / channels;
            processor.PutSamples(testData, frames);
            Console.WriteLine($"送入 {frames} frames 到 SoundTouch");
            
            // Flush 確保所有資料都被處理
            processor.Flush();
            Console.WriteLine("已執行 Flush()");
            
            // 取出處理後的資料
            float[] outputBuffer = new float[samples * 2]; // 預留更多空間
            int maxFrames = outputBuffer.Length / channels;
            int receivedFrames = processor.ReceiveSamples(outputBuffer, maxFrames);
            
            Console.WriteLine($"從 SoundTouch 取得 {receivedFrames} frames ({receivedFrames * channels} 樣本)");
            
            if (receivedFrames > 0)
            {
                Console.WriteLine("✓ SoundTouch 處理成功，有輸出資料");
                
                // 檢查輸出資料的前10個樣本
                Console.WriteLine("前10個輸出樣本:");
                for (int i = 0; i < Math.Min(10, receivedFrames * channels); i++)
                {
                    Console.WriteLine($"  [{i}] = {outputBuffer[i]:F6}");
                }
            }
            else
            {
                Console.WriteLine("✗ SoundTouch 沒有輸出資料！");
            }
            
            Console.WriteLine("=== 測試完成 ===\n");
        }
    }
}
