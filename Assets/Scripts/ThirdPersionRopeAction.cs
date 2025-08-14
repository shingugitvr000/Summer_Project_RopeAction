using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersionRopeAction : MonoBehaviour
{
    public LayerMask grapplingObj = 1;              //�׷� �Ǵ� ������Ʈ ���̾� 
    public float maxGrappleDistance = 100f;         //�ִ� �׷� �Ÿ� 

    public Camera playerCamera;                     //�÷��̾� ī�޶� ����
    public Transform aimPoint;                      //������

    public RaycastHit hit;

    public LineRenderer lineRenderer;
    public bool isGrappling = false;
    public Vector3 grapplePoint;

    //���� ��� �߰�
    public Transform player;                        //�÷��̾� Transform ����
    public float spring = 4.5f;
    public float damper = 7.0f;
    public float massScale = 1.0f;
    public float ropeMinDistance = 0.25f;

    public SpringJoint springJoint;
    public Rigidbody playerRigidbody;

    //���� ��� �߰�
    public float pullForce = 1000.0f;
    public float pullSpeed = 20f;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main;                  //���� ī�޶� �Ҵ� �Ѵ�. 

        lineRenderer = GetComponent<LineRenderer>();                        //���� �������� �����´�.
        if (lineRenderer == null)                                           //���� ���
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();         //������Ʈ�� �����Ѵ�. 
        }

        //LineRenderer �⺻ ����
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))             //���콺 ���� ��ư�� ���� ��� 
        {
            StartGrapple();
        }

        if (Input.GetMouseButton(1) && isGrappling)     //��Ŭ������ ���� 
        {
            PullTowrdsGrapplePoint();
        }

        //�׷��ø� ���� �� ���� ������Ʈ 
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
        lineRenderer.positionCount = 0;             //���� ���� �����

        if (springJoint != null)                    //������ ����Ʈ ���� 
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

        //�÷��̾�� �׷��� ����Ʈ���� ���� ���
        Vector3 directionToGrapple = (grapplePoint - player.position).normalized;

        //���� �ӵ��� ��ǥ ���⳻�� �������� �̹� �� �������� �����̰� �ִ��� Ȯ��
        float currentVelocityInDirection = Vector3.Dot(playerRigidbody.velocity, directionToGrapple);

        //�ִ� �ӵ��� ������ ���� ���
        if (currentVelocityInDirection < pullSpeed)
        {
            Vector3 pullForceVector = directionToGrapple * pullForce * Time.deltaTime;
            playerRigidbody.AddForce(pullForceVector, ForceMode.Force);
        }
    }

    void StartGrapple()
    {
        if (isGrappling) return;                    //�̹� �׷��ø� ���̸� ����

        Ray ray;

        ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out hit, maxGrappleDistance, grapplingObj))
        {
            grapplePoint = hit.point;
            isGrappling = true;

            //���� ���� �׸��� ���� 
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);        //�÷��̾� ��ġ
            lineRenderer.SetPosition(1, grapplePoint);              //�׷��� ����Ʈ 

            //������ ����Ʈ ����
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            //�Ÿ� ���
            float distance = Vector3.Distance(player.position, grapplePoint);

            //������ ����Ʈ ����
            springJoint.maxDistance = distance * 0.8f;
            springJoint.minDistance = distance * ropeMinDistance;
            springJoint.spring = spring;
            springJoint.damper = damper;
            springJoint.massScale = massScale;

        }
    }
}
