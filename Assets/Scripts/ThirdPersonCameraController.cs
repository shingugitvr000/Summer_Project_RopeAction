using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Ÿ��")]
    public Transform player;
    public Transform cameraTarget;                      //ī�޶��� Ÿ��

    [Header("ī�޶� ����")]
    public float distance = 5f;                         //�÷��̾�� ī�޶��� �Ÿ�
    public float height = 2.0f;                         //�÷��̾ ���� ī�޶��� ����
    public float sensitivity = 2.0f;                    //��Ʈ�� �ΰ���
    public float shoulderOffset = 0.8f;                 //������ ��� ������ 

    [Header("ī�޶� ����")]
    public float minY = -40f;
    public float maxY = 70f;

    [Header("�ѵ� ����")]
    public LayerMask collisionMask = -1;
    public float minDistance = 1f;

    [Header("�ε巯��")]
    public float positionSmoothTime = 0.15f;             //��ġ �ε巯��
    public float rotationSmoothTime = 0.05f;             //ȸ�� �ε巯��
    public float heightSmoothTime = 0.2f;                //Y�� ���� ���� �ε巯�� 

    //���� ����
    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 positionVelocity;
    private Vector3 currentRotationVelocity;
    private float smoothedTargetY = 0;                      //�ε巯�� Y ��ġ
    private float targetYVelocity = 0;                      //y�� �ӵ�

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;       
      
        GameObject cameraRoot = new GameObject("PlayerCameraRoot");     //Ÿ���� �̸� �����Ѵ�. 
        cameraRoot.transform.SetParent(player);
        cameraRoot.transform.localPosition = new Vector3(0f, 1.375f, 0f);
        cameraTarget = cameraRoot.transform;

        currentX = player.eulerAngles.y;
        currentY = 0;
        smoothedTargetY = cameraTarget.position.y;          //�ʱ� Y �� ����

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

        //���콺 �ٷ� �Ÿ� ����
        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //if (Mathf.Abs(scroll) > 0.1f)
        //{
        //    distance = Mathf.Clamp(distance - scroll * 2.0f, 2.0f, 10.0f);
        //}
    }

    void PositionCamera()
    {
        //ī�޶� Ÿ�� ��ġ 
        Vector3 targetPos = cameraTarget.position;

        //y���� ������ �ε巴�� ó�� 
        smoothedTargetY = Mathf.SmoothDamp(smoothedTargetY, targetPos.y, ref targetYVelocity, heightSmoothTime);
        Vector3 smoothedTargetPos = new Vector3(targetPos.x, smoothedTargetY, targetPos.z);

        //��ǥ ȸ�� ���
        Vector3 targetEuler = new Vector3(currentY, currentX, 0f);

        //ȸ�� �ε巴�� ����
        Vector3 currentEuler = new Vector3(
            Mathf.SmoothDampAngle(transform.eulerAngles.x , targetEuler.x, ref currentRotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(transform.eulerAngles.y, targetEuler.y, ref currentRotationVelocity.y, rotationSmoothTime),
            0f
        );

        Quaternion smoothRotation = Quaternion.Euler(currentEuler);

        //��� �ʸ� ������ ��� (�ε巯�� Ÿ�� ��ġ ���)
        Vector3 offset = new Vector3(shoulderOffset, height, -distance);
        Vector3 desiredPosition = smoothedTargetPos + smoothRotation * offset;

        //�� �浹 üũ (���� Ÿ�� ��ġ ����)
        Vector3 direction = (desiredPosition - targetPos).normalized;
        float distanceToTarget = Vector3.Distance(targetPos, desiredPosition);

        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction, out hit, distanceToTarget, collisionMask))
        {
            //�浹 �� ��ġ ����
            desiredPosition = hit.point - direction * 0.2f;

            //�ּ� �Ÿ� ����
            if (Vector3.Distance(targetPos, desiredPosition) < minDistance)
            {
                desiredPosition = targetPos + direction * minDistance;
            }
        }

        //ī�޶� ��ġ �ε巴�� �̵�
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        //ī�޶� ȸ�� ����
        transform.rotation = smoothRotation;
    }

    //ȭ�� �߾� ���� ��ǥ ��ȯ(�׷��ø���)
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
