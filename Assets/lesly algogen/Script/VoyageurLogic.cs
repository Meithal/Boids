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
        if(RandomPointOnNavMesh(transform.localPosition, 10, out randomPointInTrain, station.areaName)) {
            agent.SetDestination(randomPointInTrain);
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
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.GetAreaFromName(navmeshAreaName)))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }


}
