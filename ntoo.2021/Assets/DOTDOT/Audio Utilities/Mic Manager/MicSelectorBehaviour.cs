using UnityEngine.UI;
using UnityEngine;

public class MicSelectorBehaviour : MonoBehaviour
{
    Dropdown m_Dropdown;

    void Start()   
    {
        m_Dropdown = GetComponent<Dropdown>();
        m_Dropdown.ClearOptions();
        foreach (string device in Microphone.devices)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData();
            optionData.text = device;
            m_Dropdown.options.Add(optionData);
        }
        int selectedMic = PlayerPrefs.GetInt("selectedMic", 0);
        if (selectedMic > 0 && selectedMic < Microphone.devices.Length)
        {
            m_Dropdown.value = selectedMic;
        }
    }
}
