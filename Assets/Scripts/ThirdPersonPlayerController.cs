using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("이동 조작")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float turnSmoothTime = 0.1f;             //회전 스무딩 시간

    [Header("땅 체크")]
    public Transform groundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("카메라 참조")]
    public Transform cameraTransform;               //3인칭 카메라 참ㅈ조

    public Rigidbody rb;
    public bool isGrounded;

    private float turnSmoothVelocity;               //캐릭터 회전 속도 


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if(groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, 0, 0);
            groundCheck = groundCheckObj.transform;
        }

        //카메라 참조 자동 설정
        if(cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //땅 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, GroundDistance, groundMask);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        //이동 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //3인칭 시점을 위한 이동 방향 계산(카메라 기준)
        Vector3 direction = Vector3.zero;

        //카메라의 forward와 right 벡터를 기준으로 이동 방향 계산
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1).normalized);
        Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1).normalized);

        direction = cameraForward * vertical + cameraRight * horizontal;

        direction.Normalize();

        //이동 적용
        if(direction.magnitude > 0)
        {
            //플레이어 회전 (이동 방향으로 부드럽게 회전)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //이동 힘 적용
            rb.AddForce(direction * moveSpeed, ForceMode.Force);
        }

        //수평 속도 제한(Y축 속도는 유지)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;             //최대 속도를 넘어갈 경우 최대 속도로 변경
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);      //해당 속도를 리지드 바디에 넣어줌
        }

    }
}
