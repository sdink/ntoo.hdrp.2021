using System.Collections.Generic;
using UnityEngine;

public class randomanim_idle : StateMachineBehaviour
{
    [SerializeField]
    private string targetParameter = "idleanim1";

    [SerializeField]
    [Tooltip("List of relative probabilities for each index. Probability of a single index is it's value divided by the sum of all probabilities")]
    private int[] probabilities = { 1,1,1,1,1 };

    private int[] randomLookup;

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (probabilities.Length <= 0)
        {
            animator.SetInteger(targetParameter, 0);
        }
        else
        {
            if (randomLookup == null)
            {
                // Build a lookup list with duplicate indices based on relative probability
                List<int> indicesLookup = new List<int>();
                for(int i = 0; i < probabilities.Length; i++)
                {
                    for (int j = 0; j < probabilities[i]; j++)
                    {
                        indicesLookup.Add(i);
                    }
                }
                randomLookup = indicesLookup.ToArray();
            }

            int index = Random.Range(0, randomLookup.Length);
            animator.SetInteger(targetParameter, randomLookup[index]);
        }
    }
}
