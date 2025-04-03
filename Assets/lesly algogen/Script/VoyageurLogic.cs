using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class VoyageurLogic : MonoBehaviour, ITrainMessageTarget
{

    private bool onPlatform = false;
    public void TrainArrive(GameObject train, LsystemScript.StationParams station)
    {
        //Debug.Log("messagwe recu" + mess);
        if(!onPlatform)
            return;

        var mod = train.GetComponent<NavMeshModifier>();
        var agent = GetComponent<NavMeshAgent>();


        Vector3 randomPointInTrain;
        if(RandomPointOnNavMesh(train.transform.position, 10, out randomPointInTrain, station.areaName)) {
            if(!agent.SetDestination(randomPointInTrain)) {
                Debug.LogError("can't find way on the train");
            }
        } else {
            Debug.LogError("can't find random point on " + station.areaName);
        }
    }

    public void ArriveSurPlateforme()
    {
        onPlatform = true;
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public static bool RandomPointOnNavMesh(Vector3 center, float range, out Vector3 result, string navmeshAreaName)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }


}
