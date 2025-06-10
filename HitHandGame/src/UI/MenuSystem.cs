using System;
using System.Threading.Tasks;
using HitHandGame.Core;
using HitHandGame.Tests.DiagnosticTests;
using HitHandGame.Tests.IntegrationTests;

namespace HitHandGame.UI
{
    /// <summary>
    /// 功能選單系統管理器
    /// </summary>
    public class MenuSystem
    {
        private readonly ConsoleUI _ui;

        public MenuSystem(ConsoleUI ui)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public async Task<bool> HandleMenuOption(string option, SoundManager soundManager)
        {
            try
            {
                switch (option?.ToLower())
                {
                    case "1":
                        soundManager.PlayRandomSound();
                        break;
                    case "2":
                        soundManager.ListSoundFiles();
                        break;
                    case "3":
                        soundManager.StopCurrentSound();
                        _ui.ShowSuccess("已停止播放");
                        break;
                    case "4":
                        soundManager.ReloadSounds();
                        _ui.ShowSuccess("已重新載入音效檔案");
                        break;
                    case "5":
                        _ui.ShowInfo("播放三音檔組合...");
                        await soundManager.PlayThreeSoundCombo();
                        break;
                    case "6":
                        await StartAutoPlay(soundManager);
                        break;
                    case "7":
                        _ui.ShowSuccess("感謝使用！");
                        return false; // Exit program
                    case "8":
                        TestSoundTouch.TestSoundTouchBasics();
                        break;
                    case "9":
                        await TestPlaySounds(soundManager);
                        break;
                    case "10":
                        await SimpleSpeedTest.TestBasicPlayback();
                        break;
                    case "11":
                        await SimpleSpeedTest.TestVarispeedPlayback(1.5f);
                        break;
                    case "12":
                        await SimpleSpeedTest.TestSimpleSpeedChange(1.5f);
                        break;
                    case "13":
                        await AlternativeSpeedSolution.TestSimpleResample(1.5f);
                        break;
                    case "14":
                        await AlternativeSpeedSolution.TestPlaybackSpeedAdjustment(1.5f);
                        break;
                    case "15":
                        await AlternativeSpeedSolution.TestPitchShifting(1.5f);
                        break;
                    case "16":
                        WaveFormatDiagnostics.DiagnoseWaveFormatIssues();
                        break;
                    case "17":
                        await WaveFormatDiagnostics.SimplePlaybackTest();
                        break;
                    case "h":
                    case "help":
                        _ui.ShowMenu();
                        break;
                    default:
                        _ui.ShowError("無效的選項，請重新輸入");
                        break;
                }
            }
            catch (Exception ex)
            {
                _ui.ShowError($"執行功能時發生錯誤: {ex.Message}");
            }

            return true; // Continue program
        }

        private async Task StartAutoPlay(SoundManager soundManager)
        {
            _ui.ShowInfo("進入自動播放模式 (按 ESC 鍵退出)");
            _ui.ShowInfo("每次將播放三個音檔組合：隨機(1-9).mp3 → hit.mp3 → 隨機(1-9).mp3");
            
            string maxInput = _ui.GetUserInput("請輸入最大隨機編號 (1-9, 預設 9): ");
            int maxNumber = 9;
            if (!string.IsNullOrEmpty(maxInput) && int.TryParse(maxInput, out int parsedMax))
            {
                maxNumber = Math.Max(1, Math.Min(9, parsedMax));
            }

            string intervalInput = _ui.GetUserInput("請輸入播放間隔 (秒, 預設 3): ");
            int interval = 3;
            if (!string.IsNullOrEmpty(intervalInput) && int.TryParse(intervalInput, out int parsedInterval))
            {
                interval = Math.Max(1, Math.Min(60, parsedInterval));
            }

            string speedInput = _ui.GetUserInput("請輸入播放速度 (0.5-2.0, 預設 1.5): ");
            float speed = 1.5f;
            if (!string.IsNullOrEmpty(speedInput) && float.TryParse(speedInput, out float parsedSpeed))
            {
                speed = Math.Max(0.5f, Math.Min(2.0f, parsedSpeed));
            }

            _ui.ShowInfo($"自動播放已開始，間隔 {interval} 秒，最大隨機編號 {maxNumber}，播放速度 {speed}");
            _ui.ShowInfo("按 ESC 鍵停止自動播放");

            bool autoPlaying = true;
            try
            {
                while (autoPlaying)
                {
                    // 播放三音檔組合
                    await soundManager.PlayThreeSoundCombo(maxNumber, speed);

                    // 等待指定時間或按下 ESC 鍵
                    DateTime endTime = DateTime.Now.AddSeconds(interval);
                    while (DateTime.Now < endTime)
                    {
                        if (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Escape)
                            {
                                autoPlaying = false;
                                break;
                            }
                        }
                        await Task.Delay(100);
                    }
                }
            }
            finally
            {
                soundManager.StopCurrentSound();
                _ui.ShowInfo("自動播放已停止");
            }
        }

        private async Task TestPlaySounds(SoundManager soundManager)
        {
            _ui.ShowInfo("=== 音檔播放測試 ===");
            string fileName = _ui.GetUserInput("請輸入要測試的音檔名稱 (例如: hit.mp3): ");
            
            if (string.IsNullOrEmpty(fileName))
            {
                _ui.ShowError("未輸入檔案名稱");
                return;
            }

            float[] testSpeeds = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
            
            foreach (float speed in testSpeeds)
            {
                _ui.ShowInfo($"\n--- 測試速度 {speed}x ---");
                await soundManager.TestPlaySingleSound(fileName, speed);
                
                _ui.WaitForKeyPress("按任意鍵繼續下一個速度測試...");
            }
            
            _ui.ShowSuccess("\n所有速度測試完成！");
        }
    }
}
