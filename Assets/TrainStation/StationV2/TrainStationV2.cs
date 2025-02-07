using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.AI.Navigation;


public class MutatingParametersV2: IMutatingParameters {
    public int trainCapacity { get; set; } = 50;
    public int trainFrequency { get; set; } = 100; // one in n chance to spawn
}


public class TrainStationV2 : IFitStation
{
    public GameObject pieton;

    public void Awake() {
        if(pieton == null)
            Debug.LogError("pieton is not assigned");
    }


#region Genetic things
    private MutatingParametersV2 mutatingParametersV2 = new();
    public override IMutatingParameters mutatingParameters { 
        get => mutatingParametersV2; 
        set => mutatingParametersV2 = (MutatingParametersV2)value; 
    }

    public override void Cross(IMutatingParameters mp1, IMutatingParameters mp2)
    {
        mutatingParameters.trainCapacity = 
        (
            mp1.trainCapacity 
            + mp2.trainCapacity
        ) / 2;
        mutatingParameters.trainFrequency = (
            mp1.trainFrequency 
            + mp2.trainFrequency
        ) / 2;
    }

    public override string DebugFitness()
    {
        return "toto";
    }

    public override float FinalFitness()
    {
        return 1.0f;
    }

    public override void Mutate()
    {
        return;
    }

#endregion

#region Station logic

    public int clientProbabilitySampleSize = 500;
    public int stationCapacity = 100;
    public int overcowdedThreshold = 50;

    public Transform spawnPoint;
    public Transform direction1;
    public Transform direction2;
    public GameObject pietonsGroup;
    public NavMeshSurface navMeshSurface;

    private int _numberOfPeopleOnStation = 0;


    void Start() 
    {
        //var navMeshSurface = GetComponent<NavMeshSurface>();
        //if(navMeshSurface == null)
        //    Debug.LogError("no navmesh on the station prefab");

        navMeshSurface.BuildNavMesh();

    }

    void FixedUpdate()
    {
        if(UnityEngine.Random.Range(0, clientProbabilitySampleSize) < 1) {
            _numberOfPeopleOnStation += 1;

            _spawnPietonAndSendHimToQuai();
        }

        if(UnityEngine.Random.Range(0, mutatingParameters.trainFrequency) < 1) {
            int toDelete = Math.Min(
                mutatingParameters.trainCapacity, 
                _numberOfPeopleOnStation
            );
            _numberOfPeopleOnStation -= toDelete;

            int totalChildren = pietonsGroup.transform.childCount;

            for (int i = totalChildren - 1; i >= totalChildren - toDelete; i--)
            {
                Transform child = pietonsGroup.transform.GetChild(i);
                //Destroy(child.gameObject);
            }

            //fitness.numberOfTrainsPassed += 1;
        }

        _updateFitness();
    }

    private void _updateFitness()
    {
        if(_numberOfPeopleOnStation > overcowdedThreshold) {
            //fitness.timeSpentOvercrowded += 1;
        }

        if(_numberOfPeopleOnStation >= stationCapacity) {
            //fitness.timeSpentFull += 1;
        }
    }

    private void _spawnPietonAndSendHimToQuai()
    {
        var cube = Instantiate(
            pieton,
            spawnPoint.position,
            Quaternion.identity,
            pietonsGroup.transform
        );
        //cube.transform.localScale = new Vector3(0.05f, 0.5f, 0.025f);

        NavMeshAgent navMeshAgent = cube.GetComponent<NavMeshAgent>();

        navMeshAgent.SetDestination((UnityEngine.Random.Range(0, 2) == 0) ? direction1.position : direction2.position);
    }

#endregion

}