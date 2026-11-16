
using UnityEngine;

public class TrainerAnimationTest : MonoBehaviour
{
    public Animator trainerAnimator;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (trainerAnimator == null)
            {
                Debug.LogError("沒有設定 Trainer Animator！");
                return;
            }

            trainerAnimator.SetTrigger("Throw");

            Debug.Log("已發送 Throw Trigger");
        }
    }
}
