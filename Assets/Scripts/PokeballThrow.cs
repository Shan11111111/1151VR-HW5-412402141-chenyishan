
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokeballThrow : MonoBehaviour
{
    [Header("遊戲物件")]
    public GameObject pokeballPrefab;
    public GameObject pokemon;
    public Transform throwPoint;
    public Transform capturePoint;

    [Header("收服特效")]
    public GameObject captureEffectPrefab;

    [Header("訓練家動畫")]
    public Animator trainerAnimator;
    public float releaseDelay = 0.4f;

    [Header("遊戲介面")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject resultPanel;

    [Header("UI 按鈕")]
    public Button startButton;
    public Button throwButton;
    public Button retryButton;
    public Button homeButton;
    public Button musicButton;

    [Header("文字與力道條")]
    public TMP_Text attemptsText;
    public TMP_Text resultText;
    public Slider powerSlider;

    [Header("力道設定")]
    public float powerSpeed = 1.2f;
    public float perfectMin = 0.68f;
    public float perfectMax = 0.82f;
    public int maxAttempts = 2;

    [Header("精靈球飛行")]
    public float throwDuration = 1.5f;
    public float arcHeight = 2f;

    [Header("收服動畫")]
    public float captureDuration = 0.6f;
    public float fallDuration = 0.5f;
    public float shakeDuration = 0.3f;

    [Header("音樂播放器")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("背景音樂與音效")]
    public AudioClip backgroundMusic;
    public AudioClip clickSound;
    public AudioClip throwSound;
    public AudioClip captureSound;
    public AudioClip shakeSound;
    public AudioClip successSound;
    public AudioClip missSound;
    public AudioClip landSound;

    private int attemptsLeft;

    private bool isPlaying = false;
    private bool isThrowing = false;
    private bool isCaptured = false;
    private bool powerMoving = false;
    private bool musicEnabled = true;

    private float powerTimer = 0f;

    private Vector3 originalPokemonScale;
    private Vector3 originalPokemonPosition;
    private Quaternion originalPokemonRotation;

    private GameObject currentBall;

    void Start()
    {
        // 保存寶可夢的原始狀態
        if (pokemon != null)
        {
            originalPokemonScale = pokemon.transform.localScale;
            originalPokemonPosition = pokemon.transform.position;
            originalPokemonRotation = pokemon.transform.rotation;
        }

        // 綁定 UI 按鈕
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (throwButton != null)
            throwButton.onClick.AddListener(ThrowPokeball);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (homeButton != null)
            homeButton.onClick.AddListener(BackToHome);

        if (musicButton != null)
            musicButton.onClick.AddListener(ToggleMusic);

        // 設定力道條
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.wholeNumbers = false;
            powerSlider.interactable = false;
            powerSlider.value = 0f;
        }

        // 設定背景音樂
        if (bgmSource != null)
        {
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            if (backgroundMusic != null)
                bgmSource.clip = backgroundMusic;

            bgmSource.mute = !musicEnabled;

            if (bgmSource.clip != null)
                bgmSource.Play();
        }

        // 音效不需要循環
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        ResetGame();
        ShowPanels(true, false, false);
        UpdateMusicButton();
    }

    void Update()
    {
        // 力道指標左右移動
        if (powerMoving && isPlaying && !isThrowing)
        {
            powerTimer += Time.deltaTime;

            if (powerSlider != null)
            {
                powerSlider.value = Mathf.PingPong(
                    powerTimer * powerSpeed,
                    1f
                );
            }
        }

        // 電腦版保留 Space 測試
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowPokeball();
        }
    }

    // 播放短音效
    void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 開關背景音樂
    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        if (bgmSource != null)
        {
            bgmSource.mute = !musicEnabled;
        }

        UpdateMusicButton();
        PlaySound(clickSound);
    }

    // 更新音樂按鈕文字
    void UpdateMusicButton()
    {
        if (musicButton == null)
            return;

        TMP_Text buttonText =
            musicButton.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            buttonText.text = musicEnabled
                ? "關閉音樂"
                : "開啟音樂";
        }
    }

    // 控制三個畫面
    void ShowPanels(bool start, bool game, bool result)
    {
        if (startPanel != null)
            startPanel.SetActive(start);

        if (gamePanel != null)
            gamePanel.SetActive(game);

        if (resultPanel != null)
            resultPanel.SetActive(result);
    }

    // 更新剩餘機會
    void UpdateAttempts()
    {
        if (attemptsText != null)
        {
            attemptsText.text =
                "剩餘機會：" + attemptsLeft +
                " / " + maxAttempts;
        }
    }

    // 重置整場遊戲
    void ResetGame()
    {
        StopAllCoroutines();

        if (currentBall != null)
        {
            Destroy(currentBall);
            currentBall = null;
        }

        if (pokemon != null)
        {
            pokemon.SetActive(false);

            pokemon.transform.position =
                originalPokemonPosition;

            pokemon.transform.rotation =
                originalPokemonRotation;

            pokemon.transform.localScale =
                originalPokemonScale;

            pokemon.SetActive(true);
        }

        if (trainerAnimator != null)
        {
            trainerAnimator.ResetTrigger("Throw");
            trainerAnimator.Play("Idle", 0, 0f);
        }

        attemptsLeft = maxAttempts;

        isPlaying = false;
        isThrowing = false;
        isCaptured = false;
        powerMoving = false;
        powerTimer = 0f;

        if (powerSlider != null)
            powerSlider.value = 0f;

        if (throwButton != null)
            throwButton.interactable = true;

        UpdateAttempts();
    }

    // 點擊開始遊戲
    public void StartGame()
    {
        PlaySound(clickSound);

        ResetGame();

        isPlaying = true;
        powerMoving = true;

        ShowPanels(false, true, false);

        Debug.Log("遊戲開始！");
    }

    // 玩家按下拋球按鈕
    public void ThrowPokeball()
    {
        if (!isPlaying || isThrowing || isCaptured)
            return;

        if (powerSlider == null)
        {
            Debug.LogError("尚未指定 PowerSlider！");
            return;
        }

        if (pokeballPrefab == null ||
            pokemon == null ||
            throwPoint == null ||
            capturePoint == null)
        {
            Debug.LogError("遊戲物件參照不完整！");
            return;
        }

        powerMoving = false;
        isThrowing = true;

        if (throwButton != null)
            throwButton.interactable = false;

        float power = powerSlider.value;

        attemptsLeft--;
        UpdateAttempts();

        bool perfect =
            power >= perfectMin &&
            power <= perfectMax;

        Debug.Log("本次力道：" + power.ToString("F2"));

        StartCoroutine(ThrowSequence(power, perfect));
    }

    // 投擲動作與精靈球飛行
    IEnumerator ThrowSequence(float power, bool perfect)
    {
        // 人物先揮手
        if (trainerAnimator != null)
            trainerAnimator.SetTrigger("Throw");

        yield return new WaitForSeconds(
            Mathf.Max(0f, releaseDelay)
        );

        // 球離手時播放音效
        PlaySound(throwSound);

        Vector3 start = throwPoint.position;
        Vector3 target = capturePoint.position;

        // 根據力道決定終點
        if (!perfect)
        {
            Vector3 direction = target - start;

            if (power < perfectMin)
            {
                target = start + direction * 0.55f;
                Debug.Log("力道不足！");
            }
            else
            {
                target = start + direction * 1.4f;
                target.y += 3f;

                Debug.Log("力道過強！");
            }
        }
        else
        {
            Debug.Log("PERFECT！");
        }

        currentBall = Instantiate(
            pokeballPrefab,
            start,
            pokeballPrefab.transform.rotation
        );

        float elapsed = 0f;

        // 精靈球拋物線
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, throwDuration)
            );

            Vector3 position =
                Vector3.Lerp(start, target, t);

            position.y +=
                4f * arcHeight * t * (1f - t);

            currentBall.transform.position = position;

            currentBall.transform.Rotate(
                Vector3.right,
                360f * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        currentBall.transform.position = target;

        if (perfect)
        {
            yield return StartCoroutine(
                CaptureSuccess(target)
            );
        }
        else
        {
            yield return StartCoroutine(
                CaptureMiss()
            );
        }
    }

    // 播放紫色粒子
    void PlayCaptureEffect(Vector3 position)
    {
        if (captureEffectPrefab == null)
            return;

        GameObject effect = Instantiate(
            captureEffectPrefab,
            position,
            Quaternion.identity
        );

        ParticleSystem particles =
            effect.GetComponentInChildren<ParticleSystem>();

        if (particles != null)
            particles.Play();

        Destroy(effect, 3f);
    }

    // 收服成功動畫
    IEnumerator CaptureSuccess(Vector3 position)
    {
        // 命中聲與紫色粒子
        PlaySound(captureSound);
        PlayCaptureEffect(position);

        Vector3 startScale =
            pokemon.transform.localScale;

        float elapsed = 0f;

        // 寶可夢縮小
        while (elapsed < captureDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, captureDuration)
            );

            pokemon.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    t
                );

            yield return null;
        }

        pokemon.SetActive(false);

        // 取得地面高度
        Vector3 landingPoint =
            currentBall.transform.position;

        Terrain terrain = Terrain.activeTerrain;

        float groundY = landingPoint.y - 1f;

        if (terrain != null)
        {
            Vector3 terrainPosition =
                terrain.transform.position;

            Vector3 terrainSize =
                terrain.terrainData.size;

            bool insideTerrain =
                landingPoint.x >= terrainPosition.x &&
                landingPoint.x <= terrainPosition.x + terrainSize.x &&
                landingPoint.z >= terrainPosition.z &&
                landingPoint.z <= terrainPosition.z + terrainSize.z;

            if (insideTerrain)
            {
                groundY =
                    terrain.SampleHeight(landingPoint) +
                    terrainPosition.y;
            }
        }

        Renderer ballRenderer =
            currentBall.GetComponentInChildren<Renderer>();

        float radius = 0.15f;

        if (ballRenderer != null)
            radius = ballRenderer.bounds.extents.y;

        landingPoint.y = groundY + radius;

        Vector3 fallStart =
            currentBall.transform.position;

        elapsed = 0f;

        // 精靈球落地
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fallDuration)
            );

            currentBall.transform.position =
                Vector3.Lerp(fallStart, landingPoint, t);

            yield return null;
        }

        currentBall.transform.position = landingPoint;

        // 落地音效（沒有素材也可以留空）
        PlaySound(landSound);

        // 精靈球搖晃三次
        Quaternion originalRotation =
            currentBall.transform.rotation;

        for (int i = 0; i < 3; i++)
        {
            PlaySound(shakeSound);

            elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(
                    elapsed / Mathf.Max(0.01f, shakeDuration)
                );

                float angle =
                    Mathf.Sin(t * Mathf.PI * 2f) * 20f;

                currentBall.transform.rotation =
                    originalRotation *
                    Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }

            currentBall.transform.rotation =
                originalRotation;

            yield return new WaitForSeconds(0.2f);
        }

        // 最後的成功音效與粒子
        PlaySound(successSound);

        PlayCaptureEffect(
            currentBall.transform.position
        );

        isCaptured = true;
        isPlaying = false;
        isThrowing = false;

        Debug.Log("收服成功！");

        yield return new WaitForSeconds(0.8f);

        if (resultText != null)
            resultText.text = "收服成功！";

        ShowPanels(false, false, true);
    }

    // 投擲失敗
    IEnumerator CaptureMiss()
    {
        PlaySound(missSound);

        yield return new WaitForSeconds(0.6f);

        if (currentBall != null)
        {
            Destroy(currentBall);
            currentBall = null;
        }

        isThrowing = false;

        if (attemptsLeft > 0)
        {
            // 還有一次機會
            powerTimer = 0f;
            powerMoving = true;

            if (powerSlider != null)
                powerSlider.value = 0f;

            if (throwButton != null)
                throwButton.interactable = true;

            Debug.Log(
                "投擲失敗！剩餘 " + attemptsLeft + " 次機會"
            );
        }
        else
        {
            isPlaying = false;
            powerMoving = false;

            if (resultText != null)
            {
                resultText.text =
                    "挑戰失敗！\n兩次機會已用完";
            }

            ShowPanels(false, false, true);

            Debug.Log("挑戰失敗！");
        }
    }

    // 再玩一次
    public void RetryGame()
    {
        PlaySound(clickSound);
        StartGameWithoutClick();
    }

    void StartGameWithoutClick()
    {
        ResetGame();

        isPlaying = true;
        powerMoving = true;

        ShowPanels(false, true, false);
    }

    // 返回首頁
    public void BackToHome()
    {
        PlaySound(clickSound);
        ResetGame();
        ShowPanels(true, false, false);
    }
}
