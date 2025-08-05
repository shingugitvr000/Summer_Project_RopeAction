using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class ActionPlayerController : MonoBehaviour
{

    [Header("움직임 셋팅")]
    public float moveSpeed = 10f;
    public float jumpeForce = 15f;

    [Header("땅 체크")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;         
    public LayerMask groundMask;                //땅 레이어 설정

    public Rigidbody rb;
    public bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();                     //리지드 바디 컴포넌트를 할당한다. 
        rb.freezeRotation = true;                           //회전을 잠금한다. 
        
        //Ground Check 오브젝트가 없으면 자동으로 생성
        if(groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);                  //생성된 오브젝트의 부모를 지금 오브젝트 위치로 둔다. (자식으로 생성한다)
            groundCheckObj.transform.localPosition = new Vector3(0, 0, 0);
            groundCheck = groundCheckObj.transform;                         //생성한 오브젝트를 Transform 변수에 할당 한다. 
        }
    }

    // Update is called once per frame
    void Update()
    {
        //땅 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (Input.GetButtonDown("Jump") && isGrounded)                          //땅에 있을 때만 점프 
        {
            rb.AddForce(Vector3.up * jumpeForce, ForceMode.Impulse);            //위쪽으로 점프 힘을 준다. 
        }
    }

    void FixedUpdate()
    {
        //이동 입력 
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //이동 방향 계산 (카메라 기존)
        Vector3 direction = transform.right  * horizontal + transform.forward * vertical;
        //Horizontal 수평 입력이 있으면 오론쪽/왼쪽 방향으로 이동
        //vertical 수직 입력이 있으면 앞/뒤 방향으로 이동
        direction.Normalize();          //벡터의 길이를 1로 만들기 위해 (방향만 유지하고 크기를 제거)

        //이동 적용
        if(direction.magnitude > 0)             //임력이 있는지 확인 (magnitude 벡터의 길이 (크기))
        {
            rb.AddForce(direction * moveSpeed, ForceMode.Force);        //해당 방향으로 힘을 준다. 
        }

        //수평 속도 제한(Y축 속도는 유지)
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x , 0 , rb.velocity.z);
        if(horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;             //최대 속도를 넘어갈 경우 최대 속도로 변경
            rb.velocity = new Vector3(horizontalVelocity.x , rb.velocity.y, horizontalVelocity.z);      //해당 속도를 리지드 바디에 넣어줌
        }
    }
}
