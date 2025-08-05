using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("���콺 ����")]
    public float mouseSensitivity = 100f;

    [Header("ī�޶� ����")]
    public Transform playerBody;
    public float maxLockAngle = 80f;                        //�� �� �ִ� ���� ���� 

    public float xRotation = 0f;                            //x�� ȸ���� Ȯ���ϴ� ���� �� 


    // Start is called before the first frame update
    void Start()
    {
        //���콺 Ŀ�� ��ױ�
        Cursor.lockState = CursorLockMode.Locked;               //���콺 Ŀ���� �߾ӿ� ����
    }

    // Update is called once per frame
    void Update()
    {
        //���콺 �Է� �ޱ�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //���� ȸ��(ī�޶�)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLockAngle, maxLockAngle);        //�ִ� �ּ� �����̴� ���� ����
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up, mouseX);                                  //�÷��̾� �¿� ȸ��
    }
}
