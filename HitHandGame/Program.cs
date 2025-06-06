using System;
using System.Threading;
using System.Threading.Tasks;

namespace HitHandGame
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== HitHandGame 隨機音效播放器 ===");
            Console.WriteLine("歡迎使用！請將音效檔案放入 Sounds 資料夾中");
            Console.WriteLine();

            using var soundManager = new SoundManager();
            
            if (soundManager.SoundCount == 0)
            {
                Console.WriteLine("請先將音效檔案放入 Sounds 資料夾，然後重新啟動程式");
                Console.WriteLine("按任意鍵結束...");
                Console.ReadKey();
                return;
            }

            ShowMenu();
            
            bool running = true;
            while (running)
            {
                Console.Write("請選擇功能 (輸入數字): ");
                string? input = Console.ReadLine();
                
                switch (input)
                {
                    case "1":
                        soundManager.PlayRandomSound();
                        break;
                    case "2":
                        soundManager.ListSoundFiles();
                        break;
                    case "3":
                        soundManager.StopCurrentSound();
                        Console.WriteLine("已停止播放");
                        break;
                    case "4":
                        soundManager.ReloadSounds();
                        Console.WriteLine("已重新載入音效檔案");
                        break;
                    case "5":
                        Console.WriteLine("播放三音檔組合...");
                        await soundManager.PlayThreeSoundCombo();
                        break;
                    case "6":
                        StartAutoPlay(soundManager);
                        break;
                    case "7":
                        running = false;
                        Console.WriteLine("感謝使用！");
                        break;
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
                        break;                    case "12":
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
                    case "h":
                    case "help":
                        ShowMenu();
                        break;
                    default:
                        Console.WriteLine("無效的選項，請重新輸入 (輸入 h 查看說明)");
                        break;
                }
                
                if (running)
                {
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// 顯示功能選單
        /// </summary>
        static void ShowMenu()
        {
            Console.WriteLine("功能選單:");
            Console.WriteLine("  1. 播放隨機音效");
            Console.WriteLine("  2. 列出所有音效檔案");
            Console.WriteLine("  3. 停止播放");
            Console.WriteLine("  4. 重新載入音效檔案");
            Console.WriteLine("  5. 播放三音檔組合 (隨機→hit→隨機)");
            Console.WriteLine("  6. 自動隨機播放模式");
            Console.WriteLine("  7. 結束程式");
            Console.WriteLine("  8. 測試 SoundTouch 功能");
            Console.WriteLine("  9. 測試播放音檔 (不同速度)");            Console.WriteLine(" 10. 測試基本播放功能");
            Console.WriteLine(" 11. 測試 NAudio VariSpeed 播放");
            Console.WriteLine(" 12. 測試簡單速度調整");
            Console.WriteLine(" 13. 測試重取樣速度調整");
            Console.WriteLine(" 14. 測試播放速度調整");
            Console.WriteLine(" 15. 測試 Pitch Shifting");
            Console.WriteLine("  h. 顯示說明");
            Console.WriteLine();
        }

        /// <summary>
        /// 自動播放模式
        /// </summary>
        /// <param name="soundManager">音效管理器</param>
        static async void StartAutoPlay(SoundManager soundManager)
        {
            Console.WriteLine("進入自動播放模式 (按 ESC 鍵退出)");
            Console.WriteLine("每次將播放三個音檔組合：隨機(1-9).mp3 → hit.mp3 → 隨機(1-9).mp3");
            Console.Write("請輸入最大隨機編號 (1-9, 預設 9): ");
            string? maxInput = Console.ReadLine();
            int maxNumber = 9;
            if (!string.IsNullOrEmpty(maxInput) && int.TryParse(maxInput, out int parsedMax))
            {
                if (parsedMax >= 1 && parsedMax <= 9)
                    maxNumber = parsedMax;
                else
                    Console.WriteLine("輸入無效，將使用預設值 9");
            }

            Console.Write("請輸入播放間隔 (秒數, 預設 3 秒): ");
            string? intervalInput = Console.ReadLine();
            double interval = 3;
            if (!string.IsNullOrEmpty(intervalInput) && double.TryParse(intervalInput, out double parsedInterval))
            {
                interval = Math.Max(0.1, parsedInterval); // 最少 0.1 秒
            }

            Console.Write("請輸入播放速度 (0.5~2.0, 預設 1.0): ");
            string? speedInput = Console.ReadLine();
            float speed = 1.0f;
            if (!string.IsNullOrEmpty(speedInput) && float.TryParse(speedInput, out float parsedSpeed))
            {
                if (parsedSpeed >= 0.5f && parsedSpeed <= 2.0f)
                    speed = parsedSpeed;
                else
                    Console.WriteLine("輸入無效，將使用預設值 1.0");
            }

            Console.WriteLine($"自動播放已開始，間隔 {interval} 秒，最大隨機編號 {maxNumber}，播放速度 {speed}");
            Console.WriteLine("按 ESC 鍵停止自動播放");
            Console.WriteLine();

            bool autoPlaying = true;
            while (autoPlaying)
            {
                // 播放三音檔組合，傳入最大編號與速度
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
                    Thread.Sleep(100);
                }
            }            soundManager.StopCurrentSound();
            Console.WriteLine("自動播放已停止");
        }

        /// <summary>
        /// 測試播放音檔（不同速度）
        /// </summary>
        static async Task TestPlaySounds(SoundManager soundManager)
        {
            Console.WriteLine("=== 音檔播放測試 ===");
            Console.Write("請輸入要測試的音檔名稱 (例如: hit.mp3): ");
            string? fileName = Console.ReadLine();
            
            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("未輸入檔案名稱");
                return;
            }

            float[] testSpeeds = { 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f };
            
            foreach (float speed in testSpeeds)
            {
                Console.WriteLine($"\n--- 測試速度 {speed}x ---");
                await soundManager.TestPlaySingleSound(fileName, speed);
                
                Console.WriteLine("按任意鍵繼續下一個速度測試...");
                Console.ReadKey(true);
            }
            
            Console.WriteLine("\n所有速度測試完成！");
        }
    }
}
