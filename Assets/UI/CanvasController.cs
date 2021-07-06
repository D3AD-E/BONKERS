using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasController : MonoBehaviour
{
    PlayerHealthController m_health;

    Slider m_slider;
    TextMeshProUGUI m_healthText;

    private void Awake()
    {
        m_slider = transform.Find("Slider").GetComponent<Slider>();
        m_healthText = transform.Find("HealthValue").GetComponent<TextMeshProUGUI>();
        var canvas = GetComponent<CanvasScaler>();
        canvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.referenceResolution = new Vector2(800, 600);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_health)
            return;
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach(var p in players)
        {
            if(p.GetComponent<PlayerController>().IsLocalPlayer)
            {
                m_health = p.GetComponent<PlayerHealthController>();
                m_health.m_currentHealth.OnValueChanged += UpdateHealth;
                return;
            }
        }
    }

    public void UpdateHealth(float previousValue, float newValue)
    {
        m_slider.value = newValue;//Mathf.Clamp(newValue / 100, 0, 1f);
        m_healthText.text = newValue.ToString();
    }
}
