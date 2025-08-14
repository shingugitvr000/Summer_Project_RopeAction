using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersionRopeAction : MonoBehaviour
{
    public LayerMask grapplingObj = 1;              //그랩 되는 오브젝트 레이어 
    public float maxGrappleDistance = 100f;         //최대 그랩 거리 

    public Camera playerCamera;                     //플레이어 카메라 설정
    public Transform aimPoint;                      //조준점

    public RaycastHit hit;

    public LineRenderer lineRenderer;
    public bool isGrappling = false;
    public Vector3 grapplePoint;

    //로프 기능 추가
    public Transform player;                        //플레이어 Transform 참조
    public float spring = 4.5f;
    public float damper = 7.0f;
    public float massScale = 1.0f;
    public float ropeMinDistance = 0.25f;

    public SpringJoint springJoint;
    public Rigidbody playerRigidbody;

    //당기기 기능 추가
    public float pullForce = 1000.0f;
    public float pullSpeed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main;                  //메인 카메라를 할당 한다. 

        lineRenderer = GetComponent<LineRenderer>();                        //라인 랜더러를 가져온다.
        if (lineRenderer == null)                                           //없을 경우
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();         //컴포넌트를 생성한다. 
        }

        //LineRenderer 기본 설정
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))             //마우스 왼쪽 버튼을 누를 경우 
        {
            StartGrapple();
        }

        if (Input.GetMouseButton(1) && isGrappling)     //우클릭으로 당기기 
        {
            PullTowrdsGrapplePoint();
        }

        //그래플링 중일 때 로프 업데이트 
        if (isGrappling)
        {
            DrawRope();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        lineRenderer.positionCount = 0;             //로프 라인 숨기기

        if (springJoint != null)                    //스프링 조인트 제거 
        {
            Destroy(springJoint);
            springJoint = null;
        }
    }

    void DrawRope()
    {
        if (lineRenderer.positionCount == 2)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void PullTowrdsGrapplePoint()
    {
        if (!isGrappling || playerRigidbody == null) return;

        //플레이어에서 그래플 포인트로의 방향 계산
        Vector3 directionToGrapple = (grapplePoint - player.position).normalized;

        //현재 속도와 목표 방향내의 내적으로 이미 그 방향으로 움직이고 있는지 확인
        float currentVelocityInDirection = Vector3.Dot(playerRigidbody.velocity, directionToGrapple);

        //최대 속도를 제한을 위한 계산
        if (currentVelocityInDirection < pullSpeed)
        {
            Vector3 pullForceVector = directionToGrapple * pullForce * Time.deltaTime;
            playerRigidbody.AddForce(pullForceVector, ForceMode.Force);
        }
    }

    void StartGrapple()
    {
        if (isGrappling) return;                    //이미 그래플링 중이면 리턴

        Ray ray;

        ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grapplingObj))
        {
            grapplePoint = hit.point;
            isGrappling = true;

            //로프 라인 그리기 시작 
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);        //플레이어 위치
            lineRenderer.SetPosition(1, grapplePoint);              //그래플 포인트 

            //스프링 조인트 생성
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            //거리 계산
            float distance = Vector3.Distance(player.position, grapplePoint);

            //스프링 조인트 설정
            springJoint.maxDistance = distance * 0.8f;
            springJoint.minDistance = distance * ropeMinDistance;
            springJoint.spring = spring;
            springJoint.damper = damper;
            springJoint.massScale = massScale;

        }
    }
}
