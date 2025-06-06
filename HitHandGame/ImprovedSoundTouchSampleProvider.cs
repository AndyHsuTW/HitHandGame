using System;
using NAudio.Wave;
using SoundTouch;

namespace HitHandGame
{
    /// <summary>
    /// 改進版的 SoundTouchSampleProvider - 修正無聲音問題
    /// </summary>
    public class ImprovedSoundTouchSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly SoundTouchProcessor processor;
        private readonly int channels;
        private readonly int sampleRate;
        private readonly float[] sourceBuffer;
        private readonly float[] processedBuffer;
        private int processedOffset;
        private int processedCount;
        private bool sourceEnded;
        private bool processorFlushed;
        private bool debug;

        public WaveFormat WaveFormat => source.WaveFormat;

        public ImprovedSoundTouchSampleProvider(ISampleProvider source, float tempo = 1.0f, bool enableDebug = false)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.channels = source.WaveFormat.Channels;
            this.sampleRate = source.WaveFormat.SampleRate;
            this.debug = enableDebug;
            
            // 初始化 SoundTouch 處理器
            this.processor = new SoundTouchProcessor();
            processor.SampleRate = sampleRate;
            processor.Channels = channels;
            processor.Tempo = tempo;
            
            // 設定 SoundTouch 參數 - 針對短音效優化
            processor.SetSetting(SoundTouch.SettingId.SequenceDurationMs, 40);
            processor.SetSetting(SoundTouch.SettingId.SeekWindowDurationMs, 15);
            processor.SetSetting(SoundTouch.SettingId.OverlapDurationMs, 8);
            
            // 初始化緩衝區 - 使用較大的緩衝區
            int bufferSize = 8192 * channels;
            this.sourceBuffer = new float[bufferSize];
            this.processedBuffer = new float[bufferSize * 2]; // 處理後可能變大
            
            this.processedOffset = 0;
            this.processedCount = 0;
            this.sourceEnded = false;
            this.processorFlushed = false;
            
            if (debug)
            {
                Console.WriteLine($"[ImprovedSoundTouch] 初始化完成");
                Console.WriteLine($"  速度: {tempo}x");
                Console.WriteLine($"  取樣率: {sampleRate}Hz");
                Console.WriteLine($"  聲道數: {channels}");
                Console.WriteLine($"  緩衝區大小: {bufferSize} 樣本");
            }
        }        public int Read(float[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset >= buffer.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0 || offset + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

            int totalRead = 0;
            int iterations = 0;
            const int maxIterations = 1000; // 防止無限循環

            while (totalRead < count && iterations < maxIterations)
            {
                iterations++;
                
                // 1. 先從處理過的緩衝區複製資料
                if (processedOffset < processedCount)
                {
                    int toCopy = Math.Min(processedCount - processedOffset, count - totalRead);
                    Array.Copy(processedBuffer, processedOffset, buffer, offset + totalRead, toCopy);
                    processedOffset += toCopy;
                    totalRead += toCopy;
                    
                    if (debug && toCopy > 0)
                        Console.WriteLine($"[ImprovedSoundTouch] 從緩衝區複製 {toCopy} 樣本 (總計 {totalRead}/{count})");
                    
                    if (totalRead >= count)
                        break;
                }

                // 2. 檢查是否完全結束
                if (sourceEnded && processorFlushed && processedOffset >= processedCount)
                {
                    if (debug)
                        Console.WriteLine($"[ImprovedSoundTouch] 播放完全結束，最終輸出 {totalRead} 樣本");
                    break;
                }

                // 3. 讀取來源資料並送入處理器
                if (!sourceEnded)
                {
                    int sourceRead = source.Read(sourceBuffer, 0, sourceBuffer.Length);
                    if (sourceRead == 0)
                    {
                        sourceEnded = true;
                        if (debug)
                            Console.WriteLine($"[ImprovedSoundTouch] 來源資料讀取完畢");
                    }
                    else
                    {
                        // 確保樣本數是聲道數的倍數
                        if (sourceRead % channels != 0)
                        {
                            sourceRead = (sourceRead / channels) * channels;
                            if (debug)
                                Console.WriteLine($"[ImprovedSoundTouch] 調整讀取樣本數為聲道倍數: {sourceRead}");
                        }
                        
                        int frames = sourceRead / channels;
                        if (frames > 0)
                        {
                            processor.PutSamples(sourceBuffer, frames);
                            if (debug)
                                Console.WriteLine($"[ImprovedSoundTouch] 送入 {frames} frames ({sourceRead} 樣本，{channels} 聲道)");
                        }
                    }
                }

                // 4. 如果來源結束且還沒 flush，執行 flush
                if (sourceEnded && !processorFlushed)
                {
                    processor.Flush();
                    processorFlushed = true;
                    if (debug)
                        Console.WriteLine($"[ImprovedSoundTouch] 執行 Flush()");
                }

                // 5. 從處理器取出資料
                int maxFrames = processedBuffer.Length / channels;
                int receivedFrames = processor.ReceiveSamples(processedBuffer, maxFrames);
                
                if (receivedFrames > 0)
                {
                    processedOffset = 0;
                    processedCount = receivedFrames * channels;
                    
                    if (debug)
                        Console.WriteLine($"[ImprovedSoundTouch] 從處理器取得 {receivedFrames} frames = {processedCount} 樣本 ({channels} 聲道)");
                }
                else
                {
                    // 如果沒有取得資料且來源已結束
                    if (sourceEnded && processorFlushed)
                    {
                        if (debug)
                            Console.WriteLine($"[ImprovedSoundTouch] 處理器無更多資料，播放結束");
                        break;
                    }
                    
                    if (debug && !sourceEnded)
                        Console.WriteLine($"[ImprovedSoundTouch] 暫時無輸出資料，需要更多輸入");
                    
                    // 如果來源沒結束但暫時沒輸出，繼續循環讀取更多資料
                    if (!sourceEnded)
                        continue;
                }
            }

            if (iterations >= maxIterations)
            {
                Console.WriteLine($"[ImprovedSoundTouch] 警告：達到最大迭代次數 {maxIterations}，可能存在問題");
            }

            if (debug && totalRead > 0)
                Console.WriteLine($"[ImprovedSoundTouch] Read() 完成，返回 {totalRead} 樣本");

            return totalRead;
        }public void Dispose()
        {
            // SoundTouchProcessor 沒有實作 IDisposable，所以不需要呼叫 Dispose
            // processor?.Dispose();
        }
    }
}
