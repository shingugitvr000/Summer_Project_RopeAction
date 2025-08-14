using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public float turnSmoothTime = 0.1f;             //ȸ�� ������ �ð�

    [Header("�� üũ")]
    public Transform groundCheck;
    public float GroundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("ī�޶� ����")]
    public Transform cameraTransform;               //3��Ī ī�޶� ������

    public Rigidbody rb;
    public bool isGrounded;

    private float turnSmoothVelocity;               //ĳ���� ȸ�� �ӵ� 


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

        //ī�޶� ���� �ڵ� ����
        if(cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //�� üũ
        isGrounded = Physics.CheckSphere(groundCheck.position, GroundDistance, groundMask);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        //�̵� �Է�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //3��Ī ������ ���� �̵� ���� ���(ī�޶� ����)
        Vector3 direction = Vector3.zero;

        //ī�޶��� forward�� right ���͸� �������� �̵� ���� ���
        Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1).normalized);
        Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1).normalized);

        direction = cameraForward * vertical + cameraRight * horizontal;

        direction.Normalize();

        //�̵� ����
        if(direction.magnitude > 0)
        {
            //�÷��̾� ȸ�� (�̵� �������� �ε巴�� ȸ��)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //�̵� �� ����
            rb.AddForce(direction * moveSpeed, ForceMode.Force);
        }

        //���� �ӵ� ����(Y�� �ӵ��� ����)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;             //�ִ� �ӵ��� �Ѿ ��� �ִ� �ӵ��� ����
            rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);      //�ش� �ӵ��� ������ �ٵ� �־���
        }

    }
}
