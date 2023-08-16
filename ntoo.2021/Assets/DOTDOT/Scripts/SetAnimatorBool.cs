using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SetAnimatorBool : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string parameterName;

    public void SetBool(bool value)
    {
        if (animator == null) animator = GetComponent<Animator>();
        animator.SetBool(parameterName, value);
    }
}
