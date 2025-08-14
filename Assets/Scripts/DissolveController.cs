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
        float elapsed = 0f;                                         //��� �ð�
        float startValue = -1f;                                     //���� ��
        float endValue = 1;                                         //�� ��

        while (elapsed < duration)                                  //duation ���� �ݺ�
        {
            elapsed += Time.deltaTime;                              //�� ������ ��� �ð� �߰�
            float t = Mathf.Clamp01(elapsed / duration);            //0~1���� ������ ��ȯ
            float value = Mathf.Lerp(startValue, endValue, t);      //���� �������� �� ��� (-1 -> 1)
            dissovleMaterial.SetFloat("_Amount", value);            //��Ƽ������ Amount �Ӽ��� �� ����
            yield return null;                                      //���� ������ ���� ���
        }

        dissovleMaterial.SetFloat("_Amount", endValue);              //���������� �� ���� Ȯ���ϰ� ����
    }
}
