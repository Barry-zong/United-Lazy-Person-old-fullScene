using UnityEngine;
using Unity.Netcode;

public class AutoHostStarter : MonoBehaviour
{
    void Start()
    {
        // 游戏启动后自动启动 host
        NetworkManager.Singleton.StartHost();

        // 可选：添加日志以确认启动状态
        Debug.Log("正在启动 Host...");
    }
}