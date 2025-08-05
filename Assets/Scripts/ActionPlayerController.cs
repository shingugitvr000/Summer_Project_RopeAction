using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class ActionPlayerController : MonoBehaviour
{

    [Header("������ ����")]
    public float moveSpeed = 10f;
    public float jumpeForce = 15f;

    [Header("�� üũ")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;         
    public LayerMask groundMask;                //�� ���̾� ����

    public Rigidbody rb;
    public bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();                     //������ �ٵ� ������Ʈ�� �Ҵ��Ѵ�. 
        rb.freezeRotation = true;                           //ȸ���� ����Ѵ�. 
        
        //Ground Check ������Ʈ�� ������ �ڵ����� ����
        if(groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);                  //������ ������Ʈ�� �θ� ���� ������Ʈ ��ġ�� �д�. (�ڽ����� �����Ѵ�)
            groundCheckObj.transform.localPosition = new Vector3(0, 0, 0);
            groundCheck = groundCheckObj.transform;                         //������ ������Ʈ�� Transform ������ �Ҵ� �Ѵ�. 
        }
    }

    // Update is called once per frame
    void Update()
    {
        //�� üũ
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetButtonDown("Jump") && isGrounded)                          //���� ���� ���� ���� 
        {
            rb.AddForce(Vector3.up * jumpeForce, ForceMode.Impulse);            //�������� ���� ���� �ش�. 
        }
    }

    void FixedUpdate()
    {
        //�̵� �Է� 
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //�̵� ���� ��� (ī�޶� ����)
        Vector3 direction = transform.right  * horizontal + transform.forward * vertical;
        //Horizontal ���� �Է��� ������ ������/���� �������� �̵�
        //vertical ���� �Է��� ������ ��/�� �������� �̵�
        direction.Normalize();          //������ ���̸� 1�� ����� ���� (���⸸ �����ϰ� ũ�⸦ ����)

        //�̵� ����
        if(direction.magnitude > 0)             //�ӷ��� �ִ��� Ȯ�� (magnitude ������ ���� (ũ��))
        {
            rb.AddForce(direction * moveSpeed, ForceMode.Force);        //�ش� �������� ���� �ش�. 
        }

        //���� �ӵ� ����(Y�� �ӵ��� ����)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x , 0 , rb.velocity.z);
        if(horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;             //�ִ� �ӵ��� �Ѿ ��� �ִ� �ӵ��� ����
            rb.velocity = new Vector3(horizontalVelocity.x , rb.velocity.y, horizontalVelocity.z);      //�ش� �ӵ��� ������ �ٵ� �־���
        }
    }
}
