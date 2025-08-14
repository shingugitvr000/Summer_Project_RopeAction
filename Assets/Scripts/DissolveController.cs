using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public Material dissovleMaterial;
    public float duration = 2f;

    void Start()
    {
        dissovleMaterial = GetComponent<Renderer>().material;     
        StartCoroutine(ChangeAmountOverTime());
    }

    IEnumerator ChangeAmountOverTime()
    {
        float elapsed = 0f;                                         //경과 시간
        float startValue = -1f;                                     //시작 값
        float endValue = 1;                                         //끝 값

        while (elapsed < duration)                                  //duation 동안 반복
        {
            elapsed += Time.deltaTime;                              //매 프레임 경과 시간 추가
            float t = Mathf.Clamp01(elapsed / duration);            //0~1사이 배율로 변환
            float value = Mathf.Lerp(startValue, endValue, t);      //선형 보간으로 값 계산 (-1 -> 1)
            dissovleMaterial.SetFloat("_Amount", value);            //머티리얼의 Amount 속성에 값 적용
            yield return null;                                      //다음 프레임 까지 대기
        }

        dissovleMaterial.SetFloat("_Amount", endValue);              //마지막으로 끝 값을 확실하게 적용
    }
}
