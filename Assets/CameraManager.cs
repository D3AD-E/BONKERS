using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    Transform player;
    [SerializeField] Vector3 offset;

    void Start()
    {
        offset.z = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
            transform.position = Vector3.Lerp(transform.position, new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z), Time.deltaTime * 1.5f); // new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z);
    }

    public void SetCamera(Transform lookAt)
    {
        player = lookAt;
    }
}
