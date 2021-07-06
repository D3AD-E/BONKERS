using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PausePopupController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_pauseText;
    public float PauseDuration =3f;
    float m_pauseTime;
    float m_currentPauseTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_currentPauseTime == 0f)
        {
            Destroy(gameObject);
            return;
        }
        else if(Time.realtimeSinceStartup - m_pauseTime>=1f)
        {
            m_pauseTime = Time.realtimeSinceStartup;
            m_currentPauseTime--;
            m_pauseText.text = "Resuming in: " + m_currentPauseTime;
        }
    }

    public void StartPause()
    {
        m_pauseTime = Time.realtimeSinceStartup;
        m_currentPauseTime = PauseDuration;
        m_pauseText.text = "Resuming in: " + m_currentPauseTime;
    }
}
