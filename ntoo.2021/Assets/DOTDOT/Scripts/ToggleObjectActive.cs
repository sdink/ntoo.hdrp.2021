using UnityEngine;

public class ToggleObjectActive : MonoBehaviour
{
    [SerializeField]
    GameObject target;
  
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            target.SetActive(!target.activeSelf);
        }
    }
}
