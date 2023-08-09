using UnityEngine.UI;
using UnityEngine;

public class MicSelectorBehaviour : MonoBehaviour
{
    Dropdown m_Dropdown;

    [SerializeField]
    MicManager micManager;

    void Start()   
    {
        m_Dropdown = GetComponent<Dropdown>();
        m_Dropdown.ClearOptions();
        foreach (string device in Microphone.devices)
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData();
            optionData.text = device;
            m_Dropdown.options.Add(optionData);
            if (device == micManager.SelectedMic)
            {
              m_Dropdown.SetValueWithoutNotify(m_Dropdown.options.Count - 1);
            }
        }
    }
}
