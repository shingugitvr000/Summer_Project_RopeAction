using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    public LayerMask grapplingObj = 1;              //�׷� �Ǵ� ������Ʈ ���̾� 
    public float maxGrappleDistance = 100f;         //�ִ� �׷� �Ÿ� 

    public Camera playerCamera;                     //�÷��̾� ī�޶� ����

    public RaycastHit hit;

    void Start()
    {
        playerCamera = Camera.main;                  //���� ī�޶� �Ҵ� �Ѵ�. 
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))             //���콺 ���� ��ư�� ���� ��� 
        {
            StartGrapple();
        }
    }
    void StartGrapple()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if(Physics.Raycast(ray,out hit, maxGrappleDistance, grapplingObj))
        {
            Debug.Log("�׷��� ������ ������Ʈ");
        }
    }
}
