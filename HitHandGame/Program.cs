using System;
using System.Threading.Tasks;
using HitHandGame.Core;
using HitHandGame.UI;

namespace HitHandGame
{
    /// <summary>
    /// 主程式進入點
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            // 初始化 UI 和選單系統
            var ui = new ConsoleUI();
            var menuSystem = new MenuSystem(ui);

            // 顯示歡迎訊息
            ui.ShowWelcomeMessage();

            // 初始化音效管理器
            using var soundManager = new SoundManager("assets/Sounds");
            
            // 檢查是否有音效檔案
            if (soundManager.SoundCount == 0)
            {
                ui.ShowError("請先將音效檔案放入 assets/Sounds 資料夾，然後重新啟動程式");
                ui.WaitForKeyPress();
                return;
            }

            // 顯示主選單
            ui.ShowMenu();
            
            // 主程式迴圈
            bool running = true;
            while (running)
            {
                string input = ui.GetUserInput();
                running = await menuSystem.HandleMenuOption(input, soundManager);
            }
        }
    }
}
