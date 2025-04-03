using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class AgentAreaScatter : MonoBehaviour
{
    public NavMeshAgent agent;
    public int targetAreaIndex; // Change this to match your special area's index
    public float areaSearchRadius = 10f; // Adjust as needed

    public bool onPlatform;

    void Awake()
    {
        targetAreaIndex = NavMesh.GetAreaFromName("Loiter");
    }

    void Update()
    {
        if(!onPlatform) {
        NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                if (hit.mask == (1 << targetAreaIndex)) // Check if the agent is in the special area
                {
                    MoveToRandomPointInArea();
                    onPlatform = true;
                    ExecuteEvents.Execute<ITrainMessageTarget>(gameObject, null, (x,y)=>x.ArriveSurPlateforme());

                }
            }
        }
    }

    void MoveToRandomPointInArea()
    {
        Vector3 randomPoint;
        if (FindRandomPointInArea(out randomPoint))
        {
            agent.SetDestination(randomPoint);
        }
    }

    bool FindRandomPointInArea(out Vector3 result)
    {
        for (int i = 0; i < 10; i++) // Try 10 times to find a valid point
        {
            Vector3 randomDirection = Random.insideUnitSphere * areaSearchRadius;
            randomDirection += agent.transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 2.0f, 1 << targetAreaIndex)) // Ensure it's within the right area
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
}