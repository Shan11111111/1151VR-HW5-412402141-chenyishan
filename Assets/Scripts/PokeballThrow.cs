
using System.Collections;
using UnityEngine;

public class PokeballThrow : MonoBehaviour
{
    public GameObject pokeballPrefab;
    public GameObject pokemon;
    public Transform throwPoint;
    public Transform capturePoint;

    // 收服特效
    public GameObject captureEffectPrefab;

    public float throwDuration = 1.5f;
    public float arcHeight = 2f;
    public float captureDuration = 0.6f;

    private bool isThrowing = false;
    private bool isCaptured = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            !isThrowing && !isCaptured)
        {
            StartCoroutine(ThrowBall());
        }
    }

    IEnumerator ThrowBall()
    {
        isThrowing = true;

        GameObject ball = Instantiate(
            pokeballPrefab,
            throwPoint.position,
            pokeballPrefab.transform.rotation
        );

        Vector3 start = throwPoint.position;
        Vector3 end = capturePoint.position;

        float elapsed = 0f;

        // 第一階段：精靈球拋物線飛行
        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / throwDuration);

            Vector3 position = Vector3.Lerp(start, end, t);

            position.y += 4f * arcHeight * t * (1f - t);

            ball.transform.position = position;

            ball.transform.Rotate(
                Vector3.right,
                360f * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        ball.transform.position = end;

        // 第二階段：播放紫色收服粒子
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

        // 第三階段：蝙蝠逐漸縮小
        Vector3 originalScale = pokemon.transform.localScale;

        elapsed = 0f;

        while (elapsed < captureDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / captureDuration
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

        // 暫時保留精靈球半秒
        yield return new WaitForSeconds(0.5f);

        Destroy(ball);

        isCaptured = true;
        isThrowing = false;

        Debug.Log("收服成功！");
    }
}
