using UnityEngine;

public class CursorVisibilityControl : MonoBehaviour
{
  private void OnEnable()
  {
    Cursor.visible = true;
  }

  private void OnDisable()
  {
    Cursor.visible = false;
  }
}
