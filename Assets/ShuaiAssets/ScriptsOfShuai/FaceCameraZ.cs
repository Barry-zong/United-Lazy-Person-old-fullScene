using UnityEngine;

public class FaceCameraZ : MonoBehaviour
{
    [Tooltip("��Ҫ�����Ŀ�������")]
    public Transform targetCamera;

    [Tooltip("�Ƿ���Y����������ת�����ִ�ֱ��")]
    public bool lockYRotation = false;

    [Tooltip("�Ƿ�ת��������β�˳����������")]
    public bool invertFacing = true;

    void Start()
    {
        // ���û��ָ���������Ĭ��ʹ���������
        if (targetCamera == null)
        {
            targetCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        if (targetCamera != null)
        {
            Vector3 directionToCamera = targetCamera.position - transform.position;

            // �����Ҫ����Y����ת��ֻ��ˮƽ������ת��
            if (lockYRotation)
            {
                directionToCamera.y = 0;
            }

            // ȷ������������Ϊ��
            if (directionToCamera != Vector3.zero)
            {
                // ��ת����ʹ����β�ˣ�����ǰ�ˣ����������
                if (invertFacing)
                {
                    directionToCamera = -directionToCamera;
                }

                // ����������ת��ʹZ���׼���������
                transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }
}