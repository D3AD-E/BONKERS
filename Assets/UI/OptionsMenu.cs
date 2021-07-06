using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    TMPro.TMP_Dropdown m_resolutionsDropdown;
    Toggle m_fullScreen;
    Slider m_volume;

    Resolution[] m_resolutions;
    private void Awake()
    {
        m_resolutionsDropdown = transform.Find("Resolution").GetComponent<TMPro.TMP_Dropdown>();
        m_fullScreen = transform.Find("Fullscreen").GetComponent<Toggle>();
        m_volume = transform.Find("Volume").GetComponent<Slider>();

        SetupResolution();
        m_volume.value = AudioListener.volume;
        m_fullScreen.isOn = Screen.fullScreen;

    }

    void SetupResolution()
    {
        m_resolutions = Screen.resolutions;
        m_resolutionsDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResoltionIndex = 0;
        for (int i = 0; i < m_resolutions.Length; i++)
        {
            var option = $"{m_resolutions[i].width} x {m_resolutions[i].height}";
            options.Add(option);
            if (m_resolutions[i].width == Screen.currentResolution.width && m_resolutions[i].height == Screen.currentResolution.height)
            {
                currentResoltionIndex = i;
            }
        }

        m_resolutionsDropdown.AddOptions(options);
        m_resolutionsDropdown.value = currentResoltionIndex;
        m_resolutionsDropdown.RefreshShownValue();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save()
    {
        AudioListener.volume = m_volume.value;
        // Screen.fullScreen = m_fullScreen.isOn;
        var resolution = m_resolutions[m_resolutionsDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, m_fullScreen.isOn);
    }
}
