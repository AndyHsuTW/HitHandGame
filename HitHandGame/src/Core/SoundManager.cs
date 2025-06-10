using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SoundTouch;
using HitHandGame.Audio.Providers;
using HitHandGame.Audio.Effects;

namespace HitHandGame.Core
{
    /// <summary>
    /// 音效管理器 - 負責載入、管理和隨機播放音效檔案
    /// </summary>
    public class SoundManager : IDisposable
    {
        private readonly List<string> _soundFiles;
        private readonly Random _random;
        private IWavePlayer? _wavePlayer;
        private AudioFileReader? _audioFileReader;
        private readonly string _soundsDirectory;
        private bool _isPlaying;

        public SoundManager(string soundsDirectory = "Sounds")
        {
            _soundsDirectory = soundsDirectory;
            _soundFiles = new List<string>();
            _random = new Random();
            _isPlaying = false;
            LoadSoundFiles();
        }

        /// <summary>
        /// 載入指定目錄中的所有音效檔案
        /// </summary>
        private void LoadSoundFiles()
        {
            if (!Directory.Exists(_soundsDirectory))
            {
                Console.WriteLine($"音效目錄不存在: {_soundsDirectory}");
                return;
            }

            // 支援的音效格式
            string[] supportedExtensions = { "*.wav", "*.mp3", "*.m4a", "*.aac" };
            
            foreach (string extension in supportedExtensions)
            {
                var files = Directory.GetFiles(_soundsDirectory, extension, SearchOption.AllDirectories);
                _soundFiles.AddRange(files);
            }

            Console.WriteLine($"已載入 {_soundFiles.Count} 個音效檔案");
            
            if (_soundFiles.Count == 0)
            {
                Console.WriteLine("請將音效檔案放入 Sounds 資料夾中");
                Console.WriteLine("支援格式: WAV, MP3, M4A, AAC");
            }
        }

        /// <summary>
        /// 播放隨機音效
        /// </summary>
        public void PlayRandomSound()
        {
            if (_soundFiles.Count == 0)
            {
                Console.WriteLine("沒有可播放的音效檔案");
                return;
            }

            // 檢查是否正在播放音效
            if (_isPlaying)
            {
                Console.WriteLine("音效正在播放中，請等待播放完成");
                return;
            }

            // 隨機選擇一個音效檔案
            int randomIndex = _random.Next(_soundFiles.Count);
            string selectedFile = _soundFiles[randomIndex];
            
            Console.WriteLine($"播放: {Path.GetFileName(selectedFile)}");
            
            try
            {
                PlaySound(selectedFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放音效時發生錯誤: {ex.Message}");
                _isPlaying = false; // 發生錯誤時重設播放狀態
            }
        }

        /// <summary>
        /// 播放指定的音效檔案
        /// </summary>
        /// <param name="filePath">音效檔案路徑</param>
        private void PlaySound(string filePath)
        {
            // 停止當前播放的音效
            StopCurrentSound();
            
            _audioFileReader = new AudioFileReader(filePath);
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(_audioFileReader);
            
            // 設定播放狀態
            _isPlaying = true;
            
            // 當音效播放完成時觸發的事件
            _wavePlayer.PlaybackStopped += (sender, e) =>
            {
                Console.WriteLine("音效播放完成");
                _isPlaying = false; // 播放完成後重設狀態
            };
            
            _wavePlayer.Play();
        }

        /// <summary>
        /// 停止當前播放的音效
        /// </summary>
        public void StopCurrentSound()
        {
            _wavePlayer?.Stop();
            _wavePlayer?.Dispose();
            _audioFileReader?.Dispose();
            _wavePlayer = null;
            _audioFileReader = null;
            _isPlaying = false; // 停止播放時重設狀態
        }

        /// <summary>
        /// 列出所有可用的音效檔案
        /// </summary>
        public void ListSoundFiles()
        {
            Console.WriteLine("\n可用的音效檔案:");
            if (_soundFiles.Count == 0)
            {
                Console.WriteLine("  (無)");
                return;
            }

            for (int i = 0; i < _soundFiles.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(_soundFiles[i])}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 重新載入音效檔案
        /// </summary>
        public void ReloadSounds()
        {
            StopCurrentSound();
            _soundFiles.Clear();
            LoadSoundFiles();
        }

        /// <summary>
        /// 取得音效檔案數量
        /// </summary>
        public int SoundCount => _soundFiles.Count;

        /// <summary>
        /// 檢查是否正在播放音效
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 播放三個音檔的組合：隨機1-maxNumber.mp3 + hit.mp3 + 隨機1-maxNumber.mp3（不重複）
        /// </summary>
        public async Task PlayThreeSoundCombo(int maxNumber, float speed = 1.0f)
        {
            if (_soundFiles.Count == 0)
            {
                Console.WriteLine("沒有可播放的音效檔案");
                return;
            }

            // 檢查是否正在播放音效
            if (_isPlaying)
            {
                Console.WriteLine("音效正在播放中，請等待播放完成");
                return;
            }

            // 尋找 hit.mp3 檔案
            string? hitFile = _soundFiles.FirstOrDefault(f => Path.GetFileName(f).Equals("hit.mp3", StringComparison.OrdinalIgnoreCase));
            if (hitFile == null)
            {
                Console.WriteLine("找不到 hit.mp3 檔案");
                return;
            }

            // 尋找 1-maxNumber.mp3 檔案
            var numberedFiles = _soundFiles.Where(f => {
                string fileName = Path.GetFileNameWithoutExtension(f);
                return int.TryParse(fileName, out int num) && num >= 1 && num <= maxNumber;
            }).ToList();

            if (numberedFiles.Count < 2)
            {
                Console.WriteLine($"需要至少2個編號檔案 (1-{maxNumber}.mp3) 才能播放組合");
                return;
            }

            // 隨機選擇第1個和第3個音檔（不重複）
            int firstIndex = _random.Next(numberedFiles.Count);
            string firstFile = numberedFiles[firstIndex];
            
            // 移除已選擇的檔案，確保第3個不重複
            var remainingFiles = numberedFiles.Where((f, index) => index != firstIndex).ToList();
            int thirdIndex = _random.Next(remainingFiles.Count);
            string thirdFile = remainingFiles[thirdIndex];

            Console.WriteLine($"播放組合: {Path.GetFileName(firstFile)} → {Path.GetFileName(hitFile)} → {Path.GetFileName(thirdFile)} (速度: {speed}x)");            try
            {
                // 創建音檔讀取器 - 不使用 using，因為需要在播放過程中保持活躍
                var firstReader = new AudioFileReader(firstFile);
                var hitReader = new AudioFileReader(hitFile);
                var thirdReader = new AudioFileReader(thirdFile);
                
                Console.WriteLine($"[DEBUG] 音檔長度 - 第1個: {firstReader.TotalTime}, hit: {hitReader.TotalTime}, 第3個: {thirdReader.TotalTime}");
                
                // 使用自定義的順序播放提供者
                var sequentialProvider = new SequentialSampleProvider(new ISampleProvider[] { firstReader, hitReader, thirdReader }, true);
                ISampleProvider provider = sequentialProvider;
                  // 如果需要調整速度，包裝在 SpeedController 中
                if (speed != 1.0f)
                {
                    Console.WriteLine($"[DEBUG] 使用 SpeedController 調整速度: {speed}x");
                    provider = SpeedController.ApplySpeedChange(provider, speed, true);
                }

                StopCurrentSound();
                _wavePlayer = new WaveOutEvent();
                _wavePlayer.Init(provider);
                _isPlaying = true;
                
                var playbackComplete = new TaskCompletionSource<bool>();
                _wavePlayer.PlaybackStopped += (s, e) =>
                {
                    Console.WriteLine("三音檔組合播放完成");
                    
                    // 清理資源
                    _wavePlayer?.Dispose();
                    _wavePlayer = null;
                    firstReader?.Dispose();
                    hitReader?.Dispose();
                    thirdReader?.Dispose();
                    _isPlaying = false;
                    
                    playbackComplete.SetResult(true);
                };

                Console.WriteLine($"[DEBUG] 開始播放合併音檔 (速度: {speed}x)");
                _wavePlayer.Play();
                await playbackComplete.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放組合時發生錯誤: {ex.Message}");
                _isPlaying = false;
            }
        }

        /// <summary>
        /// 無參數版本，預設maxNumber=9
        /// </summary>
        public async Task PlayThreeSoundCombo()
        {
            await PlayThreeSoundCombo(9, 1.0f);
        }        /// <summary>
        /// 播放音檔並等待播放完成
        /// </summary>
        /// <param name="filePath">音效檔案路徑</param>
        private async Task PlaySoundAndWait(string filePath, float speed = 1.0f)
        {
            StopCurrentSound();
            _audioFileReader = new AudioFileReader(filePath);
            ISampleProvider sampleProvider = _audioFileReader;
            
            Console.WriteLine($"[DEBUG] 播放: {Path.GetFileName(filePath)}, 速度: {speed}x");
            Console.WriteLine($"[DEBUG] WaveFormat: {_audioFileReader.WaveFormat.Encoding}, {_audioFileReader.WaveFormat.BitsPerSample}bit, {_audioFileReader.WaveFormat.SampleRate}Hz, {_audioFileReader.WaveFormat.Channels}ch");
              if (speed != 1.0f)
            {
                Console.WriteLine($"[DEBUG] 使用 SpeedController 調整速度: {speed}x");
                sampleProvider = SpeedController.ApplySpeedChange(_audioFileReader, speed, true);
            }
            
            _wavePlayer = new WaveOutEvent();
            _wavePlayer.Init(sampleProvider);
            _isPlaying = true;
            
            var playbackComplete = new TaskCompletionSource<bool>();
            _wavePlayer.PlaybackStopped += (sender, e) =>
            {
                Console.WriteLine($"音效播放完成: {Path.GetFileName(filePath)}");
                _wavePlayer?.Dispose();
                _wavePlayer = null;
                _isPlaying = false;
                playbackComplete.SetResult(true);
            };
            
            _wavePlayer.Play();
            await playbackComplete.Task;
        }/// <summary>
        /// 測試播放單一音檔的方法（供偵錯使用）
        /// </summary>
        public async Task TestPlaySingleSound(string fileName, float speed = 1.5f)
        {
            var soundFile = _soundFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (soundFile == null)
            {
                Console.WriteLine($"找不到音檔: {fileName}");
                return;
            }

            Console.WriteLine($"測試播放: {fileName} (速度: {speed}x)");
            await PlaySoundAndWait(soundFile, speed);
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {        StopCurrentSound();
        }
    }
}
