# HitHandGame 隨機音效播放器

這是一個使用 C# .NET 8 開發的隨機音效播放程式，可以載入指定資料夾中的音效檔案並隨機播放，支援播放速度調整功能。

## 功能特色

- 🎵 支援多種音效格式 (WAV, MP3, M4A, AAC)
- 🎲 隨機播放音效檔案
- 📂 自動掃描 Sounds 資料夾中的音效檔案
- ⏯️ 播放控制 (播放、停止)
- 🔄 重新載入音效檔案
- ⏰ 自動播放模式 (可設定時間間隔)
- 📋 列出所有可用音效檔案
- 🎛️ **播放速度調整** (0.5x ~ 2.0x，使用 SoundTouch.Net)
- 🎶 **三音檔組合播放** (隨機→hit→隨機，不重複)

## 系統需求

- .NET 8.0 或更高版本
- Windows 作業系統 (使用 NAudio 套件)

## 使用方法

### 1. 準備音效檔案
在程式目錄下建立 `Sounds` 資料夾，並將音效檔案放入其中：
```
HitHandGame/
├── Sounds/
│   ├── hit.mp3
│   ├── 1.mp3
│   ├── 2.mp3
│   └── ...
└── HitHandGame.exe
```

### 2. 執行程式
```bash
dotnet run
```

### 3. 使用介面
程式啟動後會顯示功能選單：
- **1. 播放隨機音效** - 從載入的音效檔案中隨機選擇一個播放
- **2. 列出所有音效檔案** - 顯示 Sounds 資料夾中的所有音效檔案
- **3. 停止播放** - 停止當前播放的音效
- **4. 重新載入音效檔案** - 重新掃描 Sounds 資料夾
- **5. 播放三音檔組合** - 播放隨機→hit→隨機的組合 (隨機音檔不重複)
- **6. 自動隨機播放模式** - 設定時間間隔、速度自動播放音效組合
- **7. 結束程式** - 退出應用程式
- **8. 測試 SoundTouch 功能** - 測試速度調整功能
- **9. 測試播放音檔** - 測試不同速度播放單一音檔

### 4. 速度調整功能
- 支援 0.5x 到 2.0x 的播放速度
- 在自動播放模式中可指定播放速度
- 使用 SoundTouch.Net 進行高品質音訊處理

## 編譯和部署

### 開發環境編譯
```bash
dotnet build
```

### 發布為獨立執行檔
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained

# Windows x86
dotnet publish -c Release -r win-x86 --self-contained
```

## 支援的音效格式

- **WAV** - 未壓縮音效檔案
- **MP3** - MPEG Audio Layer 3
- **M4A** - MPEG-4 Audio
- **AAC** - Advanced Audio Coding

## 專案結構

```
HitHandGame/
├── Program.cs          # 主程式進入點
├── SoundManager.cs     # 音效管理類別
├── Sounds/            # 音效檔案目錄
├── HitHandGame.csproj # 專案檔案
└── README.md          # 說明文件
```

## 開發說明

### 主要類別

#### SoundManager
負責音效檔案的管理和播放：
- `LoadSoundFiles()` - 載入音效檔案
- `PlayRandomSound()` - 播放隨機音效
- `StopCurrentSound()` - 停止播放
- `ListSoundFiles()` - 列出檔案
- `ReloadSounds()` - 重新載入

#### Program
主程式類別，包含使用者介面和互動邏輯。

### 使用的套件

- **NAudio** (2.2.1) - .NET 音效處理套件

## 注意事項

1. 首次執行時請確保 Sounds 資料夾存在且包含音效檔案
2. 音效檔案路徑不能包含特殊字元
3. 大型音效檔案可能需要較長的載入時間
4. 自動播放模式按 ESC 鍵可隨時退出

## 授權

本專案採用 MIT 授權條款。
