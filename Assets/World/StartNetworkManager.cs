using MLAPI;
using MLAPI.Transports.UNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += FinishedLoading;
    }

    private void FinishedLoading(Scene arg0, LoadSceneMode arg1)
    {
        if (GlobalData.IsHost)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ServerListenPort = int.Parse(GlobalData.Port);
            NetworkManager.Singleton.StartHost();
        }
        else if (GlobalData.IsClient)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = GlobalData.IP;
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectPort = int.Parse(GlobalData.Port);
            NetworkManager.Singleton.StartClient();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
