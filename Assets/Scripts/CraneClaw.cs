using UnityEngine;

public class Crane3DController : MonoBehaviour
{
    [Header("원본 집게")]
    public Transform originalClawPivot; // ClawPivot

    [Header("회전 범위")]
    public float upperStartAngle = 100f;
    public float upperEndAngle = 150f;
    public float lowerStartAngle = 40f;
    public float lowerEndAngle = 75f;

    [Header("집게 설정")]
    public float clawSpeed = 3f;
    public float radiusFromCenter = 0.8f;

    [Header("초기 위치 설정")]
    public Vector3 initialPosition = new Vector3(0, 3, 0);  // 크레인 시작 위치

    [Header("3D 이동 설정")]
    public float moveSpeed = 4f;         // 수평 이동 속도
    public float verticalSpeed = 3f;     // 상하 이동 속도
    public float maxMoveRange = 6f;      // X/Z 축 이동 범위
    public float maxHeight = 5f;         // 최대 높이
    public float minHeight = 0.5f;       // 최소 높이 (바닥)

    [Header("자동 집기 설정")]
    public float descendSpeed = 2f;      // 내려가는 속도
    public float ascendSpeed = 1.5f;     // 올라오는 속도
    public float grabWaitTime = 0.5f;    // 집는 대기 시간

    [Header("충돌 감지 설정")]
    public LayerMask grabbableLayer = -1;    // 잡을 수 있는 오브젝트 레이어
    public float detectionRadius = 0.3f;     // 충돌 감지 반경
    public Transform detectionPoint;         // 감지 포인트 (집게 중심)

    [Header("컨트롤")]
    public KeyCode grabKey = KeyCode.Space;     // 자동 집기
    public KeyCode moveForwardKey = KeyCode.W;  // 앞으로 (Z+)
    public KeyCode moveBackKey = KeyCode.S;     // 뒤로 (Z-)
    public KeyCode moveLeftKey = KeyCode.A;     // 왼쪽 (X-)
    public KeyCode moveRightKey = KeyCode.D;    // 오른쪽 (X+)
    public KeyCode openClawKey = KeyCode.X;     // 집게 벌리기

    // 집게 관련 변수
    private Transform[] clawPivots = new Transform[3];
    private Transform[] lowerPivots = new Transform[3];
    private float clawProgress = 0f;

    // 크레인 위치
    private Vector3 cranePosition = Vector3.zero;

    // 자동 집기 상태
    public enum GrabState
    {
        Idle,       // 대기 상태
        Descending, // 내려가는 중
        Grabbing,   // 집는 중
        Ascending   // 올라오는 중
    }

    private GrabState currentState = GrabState.Idle;
    private float grabTimer = 0f;
    private float originalHeight = 0f;
    private GameObject grabbedObject = null;  // 현재 잡은 오브젝트

    void Start()
    {
        if (originalClawPivot == null)
        {
            Debug.LogError("원본 ClawPivot을 설정해주세요!");
            return;
        }

        // 초기화
        CreateThreeClaws();
        SetClawProgress(clawProgress);

        // 초기 위치 설정 (크레인은 항상 이 높이에서 시작하고 돌아옴)
        cranePosition = initialPosition;
        transform.position = cranePosition;

        // 감지 포인트가 설정되지 않았다면 자동으로 중심점 사용
        if (detectionPoint == null)
        {
            detectionPoint = transform;
        }

        Debug.Log("크레인 게임이 " + initialPosition + " 위치에서 시작됩니다!");
        Debug.Log("WASD로 위치 조정 후 Space로 집기!");
    }

    void Update()
    {
        HandleInput();
        UpdateAutoGrab();
        UpdateManualMovement();
    }

    void CreateThreeClaws()
    {
        for (int i = 0; i < 3; i++)
        {
            Transform clawCopy;

            if (i == 0)
            {
                clawCopy = originalClawPivot;
                clawCopy.name = "Claw1_Pivot";
            }
            else
            {
                clawCopy = Instantiate(originalClawPivot, transform);
                clawCopy.name = $"Claw{i + 1}_Pivot";
            }

            // 120도씩 배치
            float angle = i * 120f;
            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Sin(radians) * radiusFromCenter,
                0,
                Mathf.Cos(radians) * radiusFromCenter
            );

            clawCopy.localPosition = position;
            clawCopy.localRotation = Quaternion.Euler(upperStartAngle, angle, 0);

            clawPivots[i] = clawCopy;

            Transform lowerPivot = clawCopy.Find("ClawLower_Pivot");
            if (lowerPivot != null)
            {
                lowerPivots[i] = lowerPivot;
                lowerPivot.localRotation = Quaternion.Euler(lowerStartAngle, 0, 0);
            }
            else
            {
                Debug.LogWarning($"Claw{i + 1}에서 ClawLower_Pivot을 찾을 수 없습니다!");
            }
        }

        Debug.Log("3개 집게가 생성되었습니다!");
    }

    void HandleInput()
    {
        // 자동 집기 시작
        if (Input.GetKeyDown(grabKey) && currentState == GrabState.Idle)
        {
            Debug.Log("Space 키 눌림! 자동 집기 시작 시도");
            StartAutoGrab();
        }

        // 집게 수동 벌리기
        if (Input.GetKeyDown(openClawKey))
        {
            Debug.Log("X 키 눌림! 집게 벌리기");
            OpenClaw();
        }

        // 디버깅용 - 현재 상태 출력
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"현재 상태: {currentState}");
            Debug.Log($"현재 위치: {cranePosition}");
            Debug.Log($"Transform 위치: {transform.position}");
        }
    }

    void UpdateManualMovement()
    {
        // 자동 집기 중이 아닐 때만 수동 조작 가능 (상하 이동 제외)
        if (currentState != GrabState.Idle) return;

        Vector3 movement = Vector3.zero;

        // 3D 이동 (X, Z축만 - 크레인 게임처럼)
        if (Input.GetKey(moveForwardKey))
        {
            movement.z += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveBackKey))
        {
            movement.z -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveLeftKey))
        {
            movement.x -= moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(moveRightKey))
        {
            movement.x += moveSpeed * Time.deltaTime;
        }

        // 위치 업데이트 및 범위 제한 (Y축은 고정 - initialPosition.y 유지)
        cranePosition += movement;
        cranePosition.x = Mathf.Clamp(cranePosition.x, -maxMoveRange, maxMoveRange);
        cranePosition.z = Mathf.Clamp(cranePosition.z, -maxMoveRange, maxMoveRange);
        cranePosition.y = initialPosition.y; // 항상 초기 높이 유지

        // 실제 위치 적용
        transform.position = cranePosition;
    }

    void UpdateAutoGrab()
    {
        switch (currentState)
        {
            case GrabState.Descending:
                // 내려가기 (일정 거리 내려간 후에만 충돌 감지)
                cranePosition.y -= descendSpeed * Time.deltaTime;
                transform.position = cranePosition;

                // 0.5 이상 내려간 후에만 물체 감지 시작
                if (cranePosition.y <= (originalHeight - 0.5f) && CheckForObstacle())
                {
                    currentState = GrabState.Grabbing;
                    grabTimer = 0f;
                    Debug.Log("물체 감지! 집기 시작");
                }
                else if (cranePosition.y <= minHeight)
                {
                    cranePosition.y = minHeight;
                    transform.position = cranePosition;
                    currentState = GrabState.Grabbing;
                    grabTimer = 0f;
                    Debug.Log("바닥에 도달! 집기 시작");
                }
                break;

            case GrabState.Grabbing:
                // 집기 (잠시 대기하면서 집게 오므리기)
                grabTimer += Time.deltaTime;
                clawProgress = Mathf.Lerp(0f, 1f, grabTimer / grabWaitTime);
                SetClawProgress(clawProgress);

                if (grabTimer >= grabWaitTime)
                {
                    TryGrabObject();
                    currentState = GrabState.Ascending;
                    Debug.Log("집기 완료! 올라가기 시작");
                }
                break;

            case GrabState.Ascending:
                // 올라가기
                cranePosition.y += ascendSpeed * Time.deltaTime;
                transform.position = cranePosition;

                Debug.Log($"올라가는 중: 현재 {cranePosition.y:F1}, 목표 {originalHeight:F1}");

                if (cranePosition.y >= originalHeight)
                {
                    cranePosition.y = originalHeight;
                    transform.position = cranePosition;
                    currentState = GrabState.Idle;
                    Debug.Log("원래 높이 도달! 완료");
                }
                break;
        }
    }

    void StartAutoGrab()
    {
        originalHeight = initialPosition.y; // 항상 초기 높이로 돌아감
        currentState = GrabState.Descending;
        clawProgress = 0f;
        SetClawProgress(clawProgress);
        Debug.Log($"자동 집기 시작! 원래 높이: {originalHeight}");
    }

    void OpenClaw()
    {
        clawProgress = 0f;
        SetClawProgress(clawProgress);
        currentState = GrabState.Idle;

        if (grabbedObject != null)
        {
            ReleaseObject();
        }
    }

    void SetClawProgress(float progress)
    {
        for (int i = 0; i < 3; i++)
        {
            if (clawPivots[i] != null)
            {
                float upperAngle = Mathf.Lerp(upperStartAngle, upperEndAngle, progress);
                float yRotation = i * 120f;
                clawPivots[i].localRotation = Quaternion.Euler(upperAngle, yRotation, 0);
            }

            if (lowerPivots[i] != null)
            {
                float lowerAngle = Mathf.Lerp(lowerStartAngle, lowerEndAngle, progress);
                lowerPivots[i].localRotation = Quaternion.Euler(lowerAngle, 0, 0);
            }
        }
    }

    bool CheckForObstacle()
    {
        // 집게 바로 아래쪽에 물체가 가까이 있는지 감지 (더 엄격하게)
        Vector3 checkPosition = detectionPoint.position + Vector3.down * 0.5f; // 0.5만큼 아래에서 체크
        Collider[] nearbyObjects = Physics.OverlapSphere(checkPosition, detectionRadius, grabbableLayer);

        if (nearbyObjects.Length > 0)
        {
            Debug.Log($"감지된 물체 수: {nearbyObjects.Length}, 체크 위치: {checkPosition}");
        }

        return nearbyObjects.Length > 0;
    }

    void TryGrabObject()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(detectionPoint.position, detectionRadius, grabbableLayer);

        if (nearbyObjects.Length > 0)
        {
            float closestDistance = float.MaxValue;
            Collider closestObject = null;

            foreach (Collider obj in nearbyObjects)
            {
                float distance = Vector3.Distance(detectionPoint.position, obj.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }

            if (closestObject != null)
            {
                GrabObject(closestObject.gameObject);
            }
        }
    }

    void GrabObject(GameObject obj)
    {
        grabbedObject = obj;
        grabbedObject.transform.SetParent(transform);

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Debug.Log("물체를 잡았습니다: " + grabbedObject.name);
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Debug.Log("물체를 놓았습니다: " + grabbedObject.name);
            grabbedObject = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (detectionPoint != null)
        {
            // 감지 범위 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionPoint.position, detectionRadius);

            // 실제 체크 위치 시각화
            Gizmos.color = Color.red;
            Vector3 checkPosition = detectionPoint.position + Vector3.down * 0.5f;
            Gizmos.DrawWireSphere(checkPosition, detectionRadius);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "=== 3D 크레인 컨트롤 ===");
        GUI.Label(new Rect(10, 30, 300, 20), "상태: " + currentState.ToString());
        GUI.Label(new Rect(10, 50, 300, 20), "집게 진행도: " + (clawProgress * 100f).ToString("F1") + "%");
        GUI.Label(new Rect(10, 70, 300, 20), "위치: " + cranePosition.ToString("F1"));
        GUI.Label(new Rect(10, 90, 300, 20), "잡은 물체: " + (grabbedObject != null ? grabbedObject.name : "없음"));

        GUI.Label(new Rect(10, 120, 300, 20), "=== 조작법 ===");
        GUI.Label(new Rect(10, 140, 300, 20), "위치 조정: WASD (앞뒤좌우)");
        GUI.Label(new Rect(10, 160, 300, 20), "자동 집기: Space (내려가서 집고 올라옴)");
        GUI.Label(new Rect(10, 180, 300, 20), "집게 벌리기: X");
        GUI.Label(new Rect(10, 200, 300, 20), "디버그 정보: T키");
    }
}