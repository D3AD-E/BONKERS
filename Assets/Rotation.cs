using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : NetworkBehaviour
{
    public int m_rotationOffset = 90;
    [SerializeField] Camera m_camera;
    // Start is called before the first frame update

    private void Awake()
    {
        m_camera = transform.parent.Find("Camera").GetChild(0).GetComponent<Camera>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount > 0)
        {
            
            transform.GetChild(0).transform.localPosition = Vector3.zero;
            transform.GetChild(0).transform.localRotation = Quaternion.identity;
        }

        Vector3 diff = m_camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        diff.Normalize();

        float rotZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
    }
}
