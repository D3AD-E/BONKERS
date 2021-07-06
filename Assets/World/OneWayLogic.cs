using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayLogic : MonoBehaviour
{
    private PlatformEffector2D m_effector;
    [SerializeField] float m_delay = 0.5f;

    float m_time =0f;
    // Start is called before the first frame update
    private void Awake()
    {
        m_effector = GetComponent<PlatformEffector2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        m_time += Time.deltaTime;
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.Space))
        {
            if (m_effector.rotationalOffset != 180)
            {
                m_time = 0f;
                m_effector.rotationalOffset = 180f;
            } 
        }
        else if(m_effector.rotationalOffset!=0 && m_time>m_delay)
        {
            m_effector.rotationalOffset = 0;
        }

        //may be bad
        //else if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        //{
        //    m_effector.rotationalOffset = 0f;
        //}
    }
}
