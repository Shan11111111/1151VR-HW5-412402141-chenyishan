
using System.Collections;
using UnityEngine;

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

    [Header("精靈球飛行設定")]
    public float throwDuration = 1.5f;
    public float arcHeight = 2f;

    [Header("收服動畫設定")]
    public float captureDuration = 0.6f;

    private bool isThrowing = false;
    private bool isCaptured = false;

    void Update()
    {
        // 按下 Space，開始人物投擲動畫
        if (Input.GetKeyDown(KeyCode.Space) &&
            !isThrowing &&
            !isCaptured)
        {
            StartCoroutine(PlayThrowAnimation());
        }
    }

    // 第一階段：播放人物揮手動畫
    IEnumerator PlayThrowAnimation()
    {
        // 防止重複拋球
        isThrowing = true;

        // 播放 hammerUse 投擲動畫
        if (trainerAnimator != null)
        {
            trainerAnimator.SetTrigger("Throw");
        }
        else
        {
            Debug.LogWarning("尚未指定 Trainer Animator！");
        }

        // 等待人物揮手到出手位置
        yield return new WaitForSeconds(
            Mathf.Max(0f, releaseDelay)
        );

        // 精靈球正式飛出
        yield return StartCoroutine(ThrowBall());
    }

    // 第二階段：精靈球沿拋物線飛行
    IEnumerator ThrowBall()
    {
        if (pokeballPrefab == null ||
            pokemon == null ||
            throwPoint == null ||
            capturePoint == null)
        {
            Debug.LogError("精靈球或寶可夢參照尚未設定！");
            isThrowing = false;
            yield break;
        }

        GameObject ball = Instantiate(
            pokeballPrefab,
            throwPoint.position,
            pokeballPrefab.transform.rotation
        );

        Vector3 start = throwPoint.position;
        Vector3 end = capturePoint.position;

        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, throwDuration)
            );

            // 計算精靈球飛行位置
            Vector3 position = Vector3.Lerp(
                start,
                end,
                t
            );

            // 增加拋物線高度
            position.y +=
                4f * arcHeight * t * (1f - t);

            ball.transform.position = position;

            // 精靈球在空中旋轉
            ball.transform.Rotate(
                Vector3.right,
                360f * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        ball.transform.position = end;

        // 第三階段：播放紫色收服特效
        if (captureEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                captureEffectPrefab,
                end,
                Quaternion.identity
            );

            ParticleSystem particles =
                effect.GetComponent<ParticleSystem>();

            if (particles != null)
            {
                particles.Play();
            }

            Destroy(effect, 3f);
        }

        // 第四階段：蝙蝠逐漸縮小
        Vector3 originalScale =
            pokemon.transform.localScale;

        elapsed = 0f;

        while (elapsed < captureDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, captureDuration)
            );

            pokemon.transform.localScale =
                Vector3.Lerp(
                    originalScale,
                    Vector3.zero,
                    t
                );

            yield return null;
        }

        pokemon.SetActive(false);

        // 精靈球暫時停留半秒
        yield return new WaitForSeconds(0.5f);

        Destroy(ball);

        isCaptured = true;
        isThrowing = false;

        Debug.Log("收服成功！");
    }
}
