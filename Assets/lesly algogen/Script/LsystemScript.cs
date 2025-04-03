using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using DG.Tweening;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class LsystemScript : MonoBehaviour
{
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject stairPrefab;
    [SerializeField] private GameObject intersectionPrefab;
    [SerializeField] private GameObject metroPrefab;
    [SerializeField] private GameObject pietonPrefab;
    [SerializeField] private float segmentLength = 10f;
    [SerializeField] private float stairHeight = 3f;
    [SerializeField] private float intersectionOffset = 5f;
    [SerializeField] private int maxDepth = 2;

    /// <summary>
    /// Le nombre de secondes où le train reste dans la station
    /// Et où aucun autre train ne peut spawn
    /// </summary>
    [SerializeField] private float secondsTrainStayinStation = 15;

    private const string axiom = "F";
    private Dictionary<char, string> rules;
    private string currentString;
    public List<StationParams> platforms = new();
    public GameObject pietonsGroup;

    private DefaultDictionary<StationParams, HashSet<WeakReference<GameObject>>> pedestrianTargets = new ();

    public struct TransformInfo
    {
        public Vector3 position;
        public Quaternion rotation;

        public TransformInfo(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
        }
    }

    public class StationParams {
        public TransformInfo transformInfo;
        public int trainSpawnProbability;
        public float lastTrainEntry;
        public int pietonSpawnProbability;
        public string areaName;

        public StationParams(TransformInfo transformInfo, int trainSpawnProbability, int pietonSpawnProbability, int areaNumber)
        {
            if(areaNumber > 18) {
                throw new Exception("Not enough areas for pahfinding, lower the number of stations");
            }
            this.transformInfo = transformInfo;
            this.trainSpawnProbability = trainSpawnProbability;
            this.pietonSpawnProbability = pietonSpawnProbability;
            this.lastTrainEntry = 0;
            this.areaName = "Train " + areaNumber;
        }
    }

    // async await
    private CancellationTokenSource cancellationTokenSource;

    // navmesh
    public NavMeshSurface navMeshSurface;


    void Start()
    {
        rules = new Dictionary<char, string>
        {
            /* 
             'F' = couloirs 
             'I' = intersections (ou entrées)
             'S' = escaliers
             'P' = platforme
            */
            { 'F', "FIF" },
            { 'I', "[S]F" }
        };

        currentString = axiom; 
        GenerateLSystem(maxDepth);
        BuildStation();
    }

    void ScheduleAction(System.Action action, float delay)
    {
        StartCoroutine(DelayedCallback(action, delay));
    }
    
    IEnumerator DelayedCallback(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    void FixedUpdate()
    {
        foreach(var station in platforms) {

            // fait spawn un train
            if(UnityEngine.Random.Range(0, station.trainSpawnProbability) < 1) {
                
                /// Si un train est toujours présent sur la station, on continue
                if(Time.fixedTime < station.lastTrainEntry + secondsTrainStayinStation + 3) {
                    continue;
                }

                _spawnTrain(station);
            }

            // fait spawn un pieton
            if(UnityEngine.Random.Range(0, station.pietonSpawnProbability) < 1) {
                _spawnPietonAndSendHimToQuai(station);
            }
        }
    }


    void GenerateLSystem(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            string nextString = "";
            foreach (char c in currentString)
            {
                nextString += rules.ContainsKey(c) ? rules[c] : c.ToString();
            }
            currentString = nextString;
        }
    }

    void BuildStation()
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        int currentStation = 1;

        foreach (char c in currentString)
        {
            switch (c)
            {
                case 'F':
                    CreateSegment(corridorPrefab);
                    break;
                case 'I':
                    transformStack.Push(
                        new TransformInfo(
                            transform.position, 
                            transform.rotation
                        )
                    );
                    CreateIntersection();
                    transformStack.Pop();
                    break;
                case 'P':
                    CreateSegment(platformPrefab);
                    break;
                case 'S':
                    CreateStair(currentStation);
                    currentStation += 2;
                    break;
                case '[':
                    transformStack.Push(
                        new TransformInfo(transform.position, transform.rotation)
                    );
                    transform.Rotate(Vector3.up, -90);
                    break;
                case ']':
                    if (transformStack.Count > 0)
                    {
                        TransformInfo ti = transformStack.Pop();
                        transform.position = ti.position;
                        transform.rotation = ti.rotation;
                        transform.Rotate(Vector3.up, 90);
                    }
                    break;
            }
        }

        navMeshSurface = gameObject.AddComponent<NavMeshSurface>();

        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            NavMeshBuildSettings navMeshSettings = NavMesh.GetSettingsByID(i);
            Debug.Log($"Agent Type ID: {navMeshSettings.agentTypeID}, Name: {NavMesh.GetSettingsNameFromID(i)}");
        }
        
        //navMeshSurface.agentTypeID = NavMesh.GetSettingsByID(-1).agentTypeID;
        navMeshSurface.BuildNavMesh();
    }

    void CreateSegment(GameObject prefab)
    {
        Vector3 startPosition = transform.position;
        Instantiate(prefab, startPosition, transform.rotation);
        transform.Translate(Vector3.forward * segmentLength);
    }

    void CreateStair(int platformNumber)
    {
        Vector3 startPosition = transform.position;
        Instantiate(stairPrefab, startPosition, transform.rotation);

        CreatePlatform(startPosition, platformNumber);
    }

    // On crée la plateforme
    void CreatePlatform(Vector3 startPosition, int platformNumber)
    {
        // Créer la plateforme sous l'escalier
        Vector3 platformPosition = startPosition;
        platformPosition.y -= stairHeight; // Descendre au sol
        platformPosition += transform.forward * (segmentLength / 150); // Centrer sous l'escalier
        platformPosition += transform.right * (segmentLength * 7); // Avancer la plateforme
        Quaternion platformRotation = Quaternion.Euler(
            0, transform.rotation.eulerAngles.y + 90, 0
        ); // Rotation de la plateforme 90º
        Instantiate(platformPrefab, platformPosition, platformRotation);

        // Monter et avancer
        //transform.Translate(Vector3.up * stairHeight);
        //transform.Translate(Vector3.forward * (segmentLength / 2));

        // ajout une plateforme vers chaque direction
        Quaternion rotation90X = Quaternion.Euler(0, 180f, 0f);
        Quaternion aRotation90X = Quaternion.Euler(0, 0f, 0f);

        platforms.Add(new StationParams(new TransformInfo(platformPosition + transform.forward * 3, platformRotation * rotation90X), 500, 100, platformNumber));
        platforms.Add(new StationParams(new TransformInfo(platformPosition - transform.forward * 3, platformRotation * aRotation90X), 500, 100, platformNumber + 1));
    }

    void CreateIntersection()
    {
        transform.GetPositionAndRotation(
            out Vector3 originalPosition, 
            out Quaternion originalRotation
        );

        Instantiate(intersectionPrefab, originalPosition, originalRotation);

        transform.Translate(Vector3.forward * intersectionOffset);
    }


    private void _spawnPietonAndSendHimToQuai(StationParams station)
    {
        var pedestrian = Instantiate(
            pietonPrefab,
            transform.position,
            Quaternion.identity,
            pietonsGroup.transform
        );
        //cube.transform.localScale = new Vector3(0.05f, 0.5f, 0.025f);
 
        // Check if the pedestrian is close enough to the NavMesh
        if (!IsOnNavMesh(pedestrian.transform.position))
        {
            // If not, find the closest point on the NavMesh
            Vector3 closestPoint = FindClosestNavMeshPoint(pedestrian.transform.position, 2);

            pedestrian.transform.position = closestPoint; // Move the pedestrian to the closest point
        }

        //NavMeshAgent navMeshAgent = pedestrian.AddComponent<NavMeshAgent>();
        NavMeshAgent navMeshAgent = pedestrian.GetComponent<NavMeshAgent>();

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent is not on the NavMesh!");
        }

        Vector3 target = station.transformInfo.position;

        Vector3 closest = FindClosestNavMeshPoint(
            target, 10);

        bool found = navMeshAgent.SetDestination(closest);

        if(!found) {
            Debug.Log("cant find target destination");
        } else {
            pedestrianTargets[station].Add(new WeakReference<GameObject>(pedestrian));
        }
    }

    bool IsOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 1f, NavMesh.AllAreas);
    }

    Vector3 FindClosestNavMeshPoint(Vector3 position, float jumpDistance)
    {
        NavMeshHit hit;
        // Sample the position on the NavMesh
        if (NavMesh.SamplePosition(position, out hit, jumpDistance, NavMesh.AllAreas))
        {
            return hit.position; // Return the closest NavMesh point
        }
        return position; // Return the original position if no point is found
    }



    private void _spawnTrain(StationParams station)
    {
        var trainEaseInDuration = 1.0f;

        var goTrain = Instantiate(metroPrefab, station.transformInfo.position, station.transformInfo.rotation);

        Vector3 targetPosition = goTrain.transform.position + goTrain.transform.forward * 1.1f;

        goTrain.transform.DOMove(targetPosition, trainEaseInDuration).SetEase(Ease.OutBack);

        station.lastTrainEntry = Time.fixedTime;

        ScheduleAction(() => {

            Vector3 targetPosition = goTrain.transform.position + goTrain.transform.forward * 1.1f;


            goTrain.transform.DOMove(targetPosition, 1f).SetEase(Ease.InBack).OnComplete(() => {
                Destroy(goTrain);
                /*if(trainNavMeshSurface != null) {
                    trainNavMeshSurface.RemoveData(); // Remove the old NavMesh data safely
                    Destroy(trainNavMeshSurface.gameObject);
                    trainNavMeshSurface = null;
                }*/
                
                //navMeshSurface.BuildNavMesh();
                //navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
                    //NavMesh.RemoveAllNavMeshData(); // Clear all NavMesh data
                    //navMeshSurface.BuildNavMesh(); // Rebuild the NavMesh

                }
            );

        }, secondsTrainStayinStation);

        ScheduleAction(() => {
            var trainNavMeshSurface = goTrain.GetComponent<NavMeshSurface>();
            var modifier = goTrain.AddComponent<NavMeshModifier>();
            modifier.overrideArea = true;
            modifier.generateLinks = true;
            modifier.area = NavMesh.GetAreaFromName(station.areaName);

            trainNavMeshSurface.BuildNavMesh();

            foreach(var weakpeople in pedestrianTargets[station]) {
                if(weakpeople.TryGetTarget(out GameObject pedestrian)) {
                    AgentAreaScatter agent = pedestrian.GetComponent<AgentAreaScatter>();
                    if(!agent.onPlatform) {
                        continue;
                    }
                    ExecuteEvents.Execute<ITrainMessageTarget>(pedestrian, null, (x,y)=>x.TrainArrive(goTrain, station));
                    pedestrian.transform.parent = goTrain.transform;
                }
            }

        }, trainEaseInDuration);
    }
}


public interface ITrainMessageTarget : IEventSystemHandler
{
    // functions that can be called via the messaging system
    void TrainArrive(GameObject train, LsystemScript.StationParams station);
    void ArriveSurPlateforme();
}
