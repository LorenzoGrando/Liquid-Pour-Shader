using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WaterPour : MonoBehaviour
{
    [HideInInspector]
    public float tPourValue;
    public float dropDurationInSeconds;
    [Range(0,1)]
    public float fluidAnimSpeed;
    [Range(0.2f, 2.5f)]
    public float wobbleSpeed;
    [Range(0,10)]
    public float fresnelIntensity;
    [Range(0,10)]
    public float fresnelRamp;
    [Range(0,10)]
    public float invertedFresnelIntensity;
    [Range(0,10)]
    public float invertedFresnelRamp;
    public MeshRenderer meshRenderer;
    Coroutine timeRoutine = null;
    [Range(0,1)]
    public int reverseDrop;

    void Start()
    {
        //Reset to no flow - AKA invisible mesh - at start
        tPourValue = 1;
        meshRenderer.material.SetFloat("_FlowCutoff", tPourValue);
    }

    //Routine for initiaing and processing the initial liquid drop/ stop liquid drop animation. tPourValue refers to Y cutoff
    //on mesh used to hide mesh in frag shader;
    IEnumerator DropEffect() {
        //Begins animation lerp and defines if it's a drop/undrop animation
        if(timeRoutine == null) {
            timeRoutine = StartCoroutine(routine: LerpOverTimeSeconds(0, 1, dropDurationInSeconds));
            meshRenderer.material.SetFloat("_ReverseDrop", (float)reverseDrop);
        }
        //Continuously feeds Y cutoff value every update frame to frag shader until animation is over
        while(tPourValue < 1.1) {
            meshRenderer.material.SetFloat("_FlowCutoff", tPourValue);
            yield return null;
        }
    }

    //Method for lerping the liquid drop/undrop animation over time
    IEnumerator LerpOverTimeSeconds(float a, float b, float seconds) {
        for(float i = 0; i < seconds; i += Time.deltaTime) {
            tPourValue = Mathf.Lerp(a, b, i / seconds);
            //Guarantees that tPourValue will reach final state (so animation dosent abruptly end with remaining cutoff)
            //Theres prolly a better way to do this?
            if(i + Time.deltaTime > seconds - 0.01f) {
                tPourValue = b;
                yield break;
            } 
            yield return null;
        }
    }

    //Public method for calling the animation from outside the script. Sets the properties of the particular flow once before call
    //Only values consistently fed to shader (in previous methods) will be tPourValue

    public void CallDrop() {
        timeRoutine = null;
        meshRenderer.material.SetFloat("_WobbleSpeed", wobbleSpeed); //Refers to velocity of the liquid moving forwards/backwards 
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity); //How strong the outline color is
        meshRenderer.material.SetFloat("_FresnelRamp", fresnelRamp); //How constrained the outside fresnel is
        meshRenderer.material.SetFloat("_InvertedFresnelIntensity", invertedFresnelIntensity); //How bright the liquid color is
        meshRenderer.material.SetFloat("_InvertedFresnelRamp", invertedFresnelRamp); //How constrained the color is
        meshRenderer.material.SetFloat("_UVScaler", fluidAnimSpeed); //Speed of the wave flow texture animation
        StartCoroutine(routine: DropEffect());
    }
}
