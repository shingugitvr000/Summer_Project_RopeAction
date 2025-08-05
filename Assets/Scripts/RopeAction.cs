using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    public LayerMask grapplingObj = 1;              //그랩 되는 오브젝트 레이어 
    public float maxGrappleDistance = 100f;         //최대 그랩 거리 

    public Camera playerCamera;                     //플레이어 카메라 설정

    public RaycastHit hit;

    void Start()
    {
        playerCamera = Camera.main;                  //메인 카메라를 할당 한다. 
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))             //마우스 왼쪽 버튼을 누를 경우 
        {
            StartGrapple();
        }
    }
    void StartGrapple()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if(Physics.Raycast(ray,out hit, maxGrappleDistance, grapplingObj))
        {
            Debug.Log("그래플 가능한 오브젝트");
        }
    }
}
