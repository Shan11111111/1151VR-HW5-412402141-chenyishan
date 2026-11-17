
# 1151VR-HW5-412402141-陳怡珊

## 1. 專案截圖

![遊戲畫面](https://github.com/user-attachments/assets/eb1c1c44-c50f-4506-b068-64adcfa6ef02)

## 2. GitHub 連結

https://github.com/Shan11111111/1151VR-HW5-412402141-chenyishan

## 3. YouTube 連結

https://youtu.be/POhNyAFynoM

## 4. 製作流程與相關操作

### (1) 建立 Unity 3D 專案與場景

- 使用 Unity 6.3 LTS 建立 3D 專案，並建立 `TestScene` 作為主要遊戲場景。
- 透過 Unity Terrain 系統建立具有高低起伏的地形，使用 `Raise or Lower Terrain` 工具調整山丘與地勢，製作森林及背景山脈。
- 匯入 `Stylized Grass Texture` 作為 Terrain Layer，將草地紋理套用至地形表面，再匯入 `Free Trees` 樹木模型，利用 Terrain 的樹木繪製工具配置森林環境。
- 最後調整 `Main Camera` 的位置與拍攝角度，使訓練家和寶可夢能同時出現在畫面中。

### (2) 匯入訓練家、寶可夢與精靈球模型

- 匯入 `CityPeople` 角色資源作為訓練家，使用 `Cute Fantasy Monster - Bat FREE` 的蝙蝠模型作為收服目標。
- 精靈球則使用外部匯入的 Pokeball 3D 模型，並調整材質顏色，使其具有紅色上半部、白色下半部及深色分隔區域的外觀。
- 在場景中建立 `Trainer`、`Pokemon` 等物件，調整模型的位置、旋轉角度及縮放比例。
- 另外建立 `ThrowPoint` 和 `CapturePoint`，分別代表精靈球的拋出起點與收服目標位置。

### (3) 實作訓練家投擲動畫

- 使用 Unity 的 Humanoid 動畫系統與 Animator Controller 控制訓練家的動作。
- 由於 CityPeople 模型已提供多組工具使用動畫，因此使用原有的 `swap` 動畫作為待機狀態，並以 `hammerUse` 動畫模擬人物向前揮手投擲精靈球的動作。
- 建立 `TrainerController`，設定 `Idle` 與 `Throw` 動畫狀態，並使用 `Throw` Trigger 控制動畫切換。
- 當玩家執行投擲時，程式先觸發人物揮手動畫，再經過短暫延遲，讓精靈球從 ThrowPoint 飛出，達成人物動作與精靈球飛行的同步效果。

### (4) 實作精靈球拋物線飛行

- 使用 C# 撰寫 `PokeballThrow.cs`，透過 Coroutine 控制投擲動畫及精靈球移動。
- 利用 `Vector3.Lerp()` 計算精靈球起點與終點之間的位置，再結合拋物線公式產生飛行高度：

  `height = 4 × arcHeight × t × (1 - t)`

  其中 `t` 為 0 到 1 的飛行進度，`arcHeight` 控制飛行弧度。

- 飛行過程中也讓精靈球持續旋轉，使拋擲效果更自然。

### (5) 設計力道判定與兩次挑戰機制

- 為了增加遊戲互動性，使用 Unity UI Slider 製作自動移動的力道指標。
- 透過 `Mathf.PingPong()` 讓力道值在 0 到 1 之間反覆變化，玩家必須在適當時機點擊「拋出精靈球」按鈕。

**力道判定規則：**

| 力道範圍 | 判定 | 結果 |
|---|---|---|
| 0%～未滿 68% | 力道不足 | 精靈球飛行距離不足 |
| 68%～82% | 完美投擲 | 成功命中寶可夢 |
| 超過 82%～100% | 力道過強 | 精靈球飛過目標 |

- 每回合最多提供兩次投擲機會。
- 第一次失敗後可以再次嘗試，若第二次仍未命中，則顯示挑戰失敗畫面。

### (6) 製作收服特效與精靈球動畫

- 使用 Unity Particle System 製作紫色收服粒子效果，並將特效儲存為 `CaptureEffect` Prefab。
- 當精靈球成功命中寶可夢時，播放紫色粒子特效，同時透過 `Vector3.Lerp()` 將寶可夢的縮放比例逐漸縮小至零，模擬被精靈球吸入的過程。
- 收服後，精靈球會向地面移動，並使用 Terrain 的高度資訊計算落地位置。
- 接著透過旋轉角度與正弦函式，模擬精靈球左右搖晃三次，最後播放成功特效並顯示收服成功畫面。

### (7) 設計開始、遊戲及結果介面

- 使用 Unity UI（Canvas）與 TextMeshPro 建立三個主要介面：

  - `StartPanel`：顯示遊戲標題、玩法說明、開始遊戲按鈕及音樂開關。
  - `GamePanel`：顯示剩餘投擲次數、力道條、黃色完美區域與投擲按鈕。
  - `ResultPanel`：顯示收服成功或挑戰失敗，以及「再玩一次」、「返回首頁」按鈕。

- 為解決繁體中文顯示問題，匯入中文字型並建立 Dynamic Font Asset，使 TextMeshPro 能夠動態產生所需中文字形。
- 介面採用 Canvas Scaler，參考解析度為 `1920 × 1080`，以支援不同解析度的橫向顯示裝置。

### (8) 加入背景音樂與遊戲音效

- 在 `GameManager` 建立兩個 Audio Source，分別負責背景音樂（BGM）及遊戲音效（SFX）。
- 背景音樂使用 Loop 循環播放，音效則透過 `PlayOneShot()` 在指定事件發生時播放。
- 實作的聲音包含：
  - 背景音樂
  - 按鈕點擊音效
  - 精靈球投擲音效
  - 命中及收服音效
  - 精靈球搖晃音效
  - 收服成功音效
  - 失敗音效
  - 掉落地面音效
- 開始介面也提供音樂開關功能，讓玩家自行決定是否播放背景音樂。

### (9) Android 平台設定與 APK 輸出

- 使用 Unity 6.3 LTS 的 Build Profiles 將專案切換至 Android 平台，並確認安裝 Android Build Support、Android SDK & NDK Tools 及 OpenJDK。
- 在 Player Settings 中設定遊戲使用橫向螢幕，並將 `TestScene` 加入建置場景清單。
- 遊戲操作使用 Unity UI Button，能透過手機觸控執行開始遊戲、精靈球投擲、重新挑戰及返回首頁等功能。
- 完成平台設定後，透過 Build Profiles 輸出 Android APK。

### (10) 遊戲操作方式

**開始遊戲**

- 開啟遊戲後，閱讀開始介面的遊玩說明，點擊「開始遊戲」進入收服挑戰。

**投擲精靈球**

- 觀察畫面下方的力道指標，等待指標進入黃色完美區域，再點擊「拋出精靈球」。
- 電腦測試時也可以按下 `Space` 執行投擲。

**判定與收服**

- 若投擲力道正確，精靈球會命中寶可夢，觸發收服特效。
- 若力道不足或過強，則扣除一次投擲機會。
- 兩次皆未成功即判定挑戰失敗。

**重新遊玩**

- 遊戲結束後，可點擊「再玩一次」重新挑戰，或點擊「返回首頁」回到開始介面。
