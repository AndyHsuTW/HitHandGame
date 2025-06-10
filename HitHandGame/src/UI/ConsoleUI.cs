using System;

namespace HitHandGame.UI
{
    /// <summary>
    /// 主控台使用者介面管理器
    /// </summary>
    public class ConsoleUI
    {
        public void ShowWelcomeMessage()
        {
            Console.WriteLine("=== HitHandGame 隨機音效播放器 ===");
            Console.WriteLine("歡迎使用！請將音效檔案放入 assets/Sounds 資料夾中");
            Console.WriteLine();
        }

        public void ShowMenu()
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
            Console.WriteLine("  9. 測試播放音檔 (不同速度)");
            Console.WriteLine(" 10. 測試基本播放功能");
            Console.WriteLine("  h. 顯示說明");
            Console.WriteLine();
        }

        public string GetUserInput(string prompt = "請選擇功能 (輸入數字): ")
        {
            Console.Write(prompt);
            return Console.ReadLine() ?? string.Empty;
        }

        public void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"錯誤: {message}");
            Console.ResetColor();
        }

        public void ShowSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void ShowInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void ShowWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"警告: {message}");
            Console.ResetColor();
        }

        public void WaitForKeyPress(string message = "按任意鍵繼續...")
        {
            Console.WriteLine(message);
            Console.ReadKey(true);
        }

        public bool ConfirmAction(string message)
        {
            Console.Write($"{message} (y/n): ");
            var key = Console.ReadKey();
            Console.WriteLine();
            return key.KeyChar == 'y' || key.KeyChar == 'Y';
        }

        public void ClearScreen()
        {
            Console.Clear();
        }
    }
}
