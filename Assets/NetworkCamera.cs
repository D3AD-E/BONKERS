using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCamera : NetworkBehaviour
{
    [SerializeField] Camera m_cam;
    // Start is called before the first frame update
    void Awake()
    {
        if (IsLocalPlayer)
        {
            m_cam = GameObject.FindObjectOfType<Camera>();
            m_cam.GetComponent<CameraManager>().SetCamera(transform);
            return;
        }

        m_cam = GameObject.FindObjectOfType<Camera>();
        m_cam.GetComponent<CameraManager>().SetCamera(transform);
        return;
    }
}
