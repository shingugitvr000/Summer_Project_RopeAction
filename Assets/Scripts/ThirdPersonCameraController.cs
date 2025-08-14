using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("타켓")]
    public Transform player;
    public Transform cameraTarget;                      //카메라의 타겟

    [Header("카메라 설정")]
    public float distance = 5f;                         //플레이어와 카메라의 거리
    public float height = 2.0f;                         //플레이어를 보는 카메라의 높이
    public float sensitivity = 2.0f;                    //컨트롤 민감도
    public float shoulderOffset = 0.8f;                 //오른쪽 어깨 오프셋 

    [Header("카메라 제한")]
    public float minY = -40f;
    public float maxY = 70f;

    [Header("총돌 설정")]
    public LayerMask collisionMask = -1;
    public float minDistance = 1f;

    [Header("부드러움")]
    public float positionSmoothTime = 0.15f;             //위치 부드러움
    public float rotationSmoothTime = 0.05f;             //회전 부드러움
    public float heightSmoothTime = 0.2f;                //Y축 높이 전용 부드러움 

    //내부 변수
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 positionVelocity;
    private Vector3 currentRotationVelocity;
    private float smoothedTargetY = 0;                      //부드러운 Y 위치
    private float targetYVelocity = 0;                      //y축 속도

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;       
      
        GameObject cameraRoot = new GameObject("PlayerCameraRoot");     //타겟을 미리 설정한다. 
        cameraRoot.transform.SetParent(player);
        cameraRoot.transform.localPosition = new Vector3(0f, 1.375f, 0f);
        cameraTarget = cameraRoot.transform;

        currentX = player.eulerAngles.y;
        currentY = 0;
        smoothedTargetY = cameraTarget.position.y;          //초기 Y 값 설정

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        HandleInput();
        PositionCamera();
    }
    void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        currentX += mouseX;
        currentY -= mouseY;
        currentY = Mathf.Clamp(currentY, minY, maxY);

        //마우스 휠로 거리 조절
        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //if (Mathf.Abs(scroll) > 0.1f)
        //{
        //    distance = Mathf.Clamp(distance - scroll * 2.0f, 2.0f, 10.0f);
        //}
    }

    void PositionCamera()
    {
        //카메라 타겟 위치 
        Vector3 targetPos = cameraTarget.position;

        //y축을 별도로 부드럽게 처리 
        smoothedTargetY = Mathf.SmoothDamp(smoothedTargetY, targetPos.y, ref targetYVelocity, heightSmoothTime);
        Vector3 smoothedTargetPos = new Vector3(targetPos.x, smoothedTargetY, targetPos.z);

        //목표 회전 계산
        Vector3 targetEuler = new Vector3(currentY, currentX, 0f);

        //회전 부드럽게 적용
        Vector3 currentEuler = new Vector3(
            Mathf.SmoothDampAngle(transform.eulerAngles.x , targetEuler.x, ref currentRotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(transform.eulerAngles.y, targetEuler.y, ref currentRotationVelocity.y, rotationSmoothTime),
            0f
        );

        Quaternion smoothRotation = Quaternion.Euler(currentEuler);

        //어깨 너머 오프셋 계산 (부드러운 타겟 위치 사용)
        Vector3 offset = new Vector3(shoulderOffset, height, -distance);
        Vector3 desiredPosition = smoothedTargetPos + smoothRotation * offset;

        //벽 충돌 체크 (원래 타겟 위치 기준)
        Vector3 direction = (desiredPosition - targetPos).normalized;
        float distanceToTarget = Vector3.Distance(targetPos, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction, out hit, distanceToTarget, collisionMask))
        {
            //충돌 시 위치 조정
            desiredPosition = hit.point - direction * 0.2f;

            //최소 거리 보장
            if (Vector3.Distance(targetPos, desiredPosition) < minDistance)
            {
                desiredPosition = targetPos + direction * minDistance;
            }
        }

        //카메라 위치 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        //카메라 회전 적용
        transform.rotation = smoothRotation;
    }

    //화면 중앙 월드 좌표 반환(그래플링용)
    public Vector3 GetCrosshairWorldPosition(float maxDistance = 100f)
    {
        Ray ray = new Ray(transform.position, transform.forward);

        RaycastHit hit;
        if (Physics.Raycast(ray,out hit, maxDistance))
        {
            return hit.point;
        }

        return ray.origin + ray.direction * maxDistance;
    }
}
