using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimManager : MonoBehaviour
{
    public Animator animator;
    public WaterPour waterShader;

    public void PlayFlip() {
        animator.SetTrigger("SwitchTrigger");
        waterShader.reverseDrop = 1;
        StartCoroutine(routine:DelayCallFlip());
    }

    public void PlayReverse() {
        animator.SetTrigger("SwitchTrigger");
        waterShader.reverseDrop = 0;
        StartCoroutine(routine:DelayCallReverse());
    }

    IEnumerator DelayCallFlip() {
        yield return new WaitForSeconds(0.85f);
        waterShader.CallDrop();
    }
    IEnumerator DelayCallReverse() {
        yield return new WaitForSeconds(0.10f);
        waterShader.CallDrop();
    }
}
