using UnityEngine;

public class UIfollowPlayCon : MonoBehaviour
{
    public GameObject playerCenterpoint;
    public GameObject uifollowpoint1;
    public GameObject uifollowpoint2;
    public GameObject uifollowpoint3;
    public GameObject needfollowui1;
    public GameObject needfollowui2;
    public GameObject needfollowui3;
    [SerializeField]
    private float hightoffset = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 更新needfollowui1的位置和旋转
        if (needfollowui1 != null && needfollowui1.activeSelf && uifollowpoint1 != null)
        {
            UpdateUIPositionAndRotation(needfollowui1, uifollowpoint1);
        }

        // 更新needfollowui2的位置和旋转
        if (needfollowui2 != null && needfollowui2.activeSelf && uifollowpoint2 != null)
        {
            UpdateUIPositionAndRotation(needfollowui2, uifollowpoint2);
        }

        // 更新needfollowui3的位置和旋转
        if (needfollowui3 != null && needfollowui3.activeSelf && uifollowpoint3 != null)
        {
            UpdateUIPositionAndRotation(needfollowui3, uifollowpoint3);
        }
    }

    private void UpdateUIPositionAndRotation(GameObject uiObject, GameObject followPoint)
    {
        if (playerCenterpoint == null) return;

        // 设置位置
        Vector3 newPosition = new Vector3(
            followPoint.transform.position.x,
            playerCenterpoint.transform.position.y - hightoffset,
            followPoint.transform.position.z
        );
        uiObject.transform.position = newPosition;

        // 设置旋转，使蓝轴指向playerCenterpoint
        Vector3 directionToPlayer = uiObject.transform.position - playerCenterpoint.transform.position;
        uiObject.transform.rotation = Quaternion.LookRotation(-directionToPlayer, Vector3.up);
    }
}
