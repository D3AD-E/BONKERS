using MLAPI;
using MLAPI.Transports.UNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public string joinIp = string.Empty;
    public string joinPort = string.Empty;
    public string nickName = string.Empty;
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
            GUILayout.Label("IP");
            joinIp = GUILayout.TextField(joinIp, 30);
            GUILayout.Label("Port host/join");
            joinPort = GUILayout.TextField(joinPort, 30);
            GUILayout.Label("NickName");
            nickName = GUILayout.TextField(nickName, 30);
            HandleNotLocalPlay();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    void HandleNotLocalPlay()
    {
        if (GUILayout.Button("Host NOT LOCAL"))
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ServerListenPort = int.Parse(joinPort);
        }
        if (GUILayout.Button("Join IP"))
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = joinIp;
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(joinPort);
        }
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}
