using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace HitHandGame
{
    /// <summary>
    /// 順序播放多個音檔的 SampleProvider，確保音檔無縫銜接
    /// </summary>
    public class SequentialSampleProvider : ISampleProvider
    {
        private readonly List<ISampleProvider> sources;
        private int currentSourceIndex;
        private readonly WaveFormat waveFormat;
        private bool debug;

        public WaveFormat WaveFormat => waveFormat;

        public SequentialSampleProvider(IEnumerable<ISampleProvider> sources, bool enableDebug = false)
        {
            this.sources = new List<ISampleProvider>(sources);
            this.debug = enableDebug;
            
            if (this.sources.Count == 0)
                throw new ArgumentException("至少需要一個來源", nameof(sources));
            
            // 使用第一個來源的 WaveFormat
            this.waveFormat = this.sources[0].WaveFormat;
            this.currentSourceIndex = 0;
            
            // 檢查所有來源是否有相同的 WaveFormat
            foreach (var source in this.sources)
            {
                if (!source.WaveFormat.Equals(this.waveFormat))
                    throw new ArgumentException("所有來源必須有相同的 WaveFormat");
            }
            
            if (debug)
            {
                Console.WriteLine($"[SequentialSampleProvider] 初始化完成，共 {this.sources.Count} 個來源");
                Console.WriteLine($"[SequentialSampleProvider] WaveFormat: {this.waveFormat}");
            }
        }        public int Read(float[] buffer, int offset, int count)
        {
            int totalRead = 0;
            int attempts = 0;
            const int maxAttempts = 1000; // 防止無限循環
            
            while (totalRead < count && currentSourceIndex < sources.Count && attempts < maxAttempts)
            {
                attempts++;
                var currentSource = sources[currentSourceIndex];
                int samplesNeeded = count - totalRead;
                int samplesRead = currentSource.Read(buffer, offset + totalRead, samplesNeeded);
                
                if (debug && samplesRead > 0)
                {
                    Console.WriteLine($"[SequentialSampleProvider] 從來源 {currentSourceIndex} 讀取 {samplesRead} 樣本 (需要: {samplesNeeded})");
                }
                
                totalRead += samplesRead;
                
                // 如果當前來源沒有更多資料，切換到下一個
                if (samplesRead == 0)
                {
                    if (debug)
                    {
                        Console.WriteLine($"[SequentialSampleProvider] 來源 {currentSourceIndex} 結束，切換到下一個 (來源總數: {sources.Count})");
                    }
                    currentSourceIndex++;
                }
                // 即使讀取到資料，也要檢查是否已讀取完畢
                else if (samplesRead < samplesNeeded)
                {
                    // 當前來源可能已經沒有更多資料，但還沒完全結束
                    // 嘗試再讀取一次確認
                    int testRead = currentSource.Read(new float[1], 0, 1);
                    if (testRead == 0)
                    {
                        if (debug)
                        {
                            Console.WriteLine($"[SequentialSampleProvider] 來源 {currentSourceIndex} 確認結束，切換到下一個");
                        }
                        currentSourceIndex++;
                    }
                    else
                    {
                        // 還有資料，把測試讀取的樣本放回（無法實現，所以跳過這個測試）
                        if (debug)
                        {
                            Console.WriteLine($"[SequentialSampleProvider] 來源 {currentSourceIndex} 還有資料可讀取");
                        }
                    }
                }
            }
            
            if (debug && totalRead > 0)
            {
                Console.WriteLine($"[SequentialSampleProvider] 總共讀取 {totalRead} 樣本，當前來源索引: {currentSourceIndex}/{sources.Count}，嘗試次數: {attempts}");
            }
            
            if (attempts >= maxAttempts)
            {
                Console.WriteLine($"[SequentialSampleProvider] 警告：達到最大嘗試次數 {maxAttempts}");
            }
            
            return totalRead;
        }
    }
}
