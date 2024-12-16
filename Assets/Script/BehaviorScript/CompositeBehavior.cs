using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SetBehaviour
{
    public FlockBehavior Behavior;
    public float Weight;
}
[CreateAssetMenu(menuName = "Flock/Behavior/Composite")]
public class CompositeBehavior : FlockBehavior
{
    [SerializeField] private SetBehaviour[] behaviors;
    
    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //Set up move
        Vector2 move = Vector2.zero;
        
        foreach (var behavior in behaviors)
        {
            Vector2 partialMove = behavior.Behavior.CalculateMove(agent, context, flock) * behavior.Weight;
                
            if (partialMove.magnitude == 0) continue;
                
            if (partialMove.sqrMagnitude > behavior.Weight * behavior.Weight)
            {
                partialMove.Normalize();
                partialMove *= behavior.Weight;
            }

            move += partialMove;
        }

        return move;
    }
}
