using UnityEngine;

public class ToggleObjectActive : MonoBehaviour
{
    [SerializeField]
    GameObject target;

    [SerializeField]
    [Tooltip("If the target should be enabled or disabled when this script initializes")]
    private bool activeOnLoad = false;

    private void Start()
    {
        target.SetActive(activeOnLoad);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            target.SetActive(!target.activeSelf);
        }
    }
}
