using System;
using UnityEngine;




public class MutatingParameters: IMutatingParameters {
    public int trainCapacity { get; set; } = 50;
    public int trainFrequency { get; set; } = 100; // one in n chance to spawn
}

public class TrainStation : IFitStation
{
    public int clientProbabilitySampleSize = 500;
    public int stationCapacity = 100;
    public int overcowdedThreshold = 50;

    public GameObject passantPrefab;

    private MutatingParameters _mutatingParameters = new();
    public override IMutatingParameters mutatingParameters {
        get => _mutatingParameters;
        set => _mutatingParameters = (MutatingParameters)value;
    }

    private Fitness fitness;

    private class Fitness {
        public int timeSpentOvercrowded = 0;
        public int timeSpentFull = 0;
        public int numberOfTrainsPassed = 0;

        public float FinalFitness(ConstantParameters p)
        {
            return numberOfTrainsPassed * p.costPerTrain
            + timeSpentOvercrowded * p.costPerOvercrowded
            + timeSpentFull * p.costPerFull;
        }
    }

    private class ConstantParameters {
        public float costPerTrain = 100;
        public float costPerOvercrowded = 1;
        public float costPerFull = 10;
    }

    private int _numberOfPeopleOnStation = 0;

    private ConstantParameters constantParameters;

    // Start is called before the first frame update
    void Awake()
    {
        _mutatingParameters = new();
        constantParameters = new();
        fitness = new();
    }

    // Update is called once per frame
    void Update()
    {
        //showTimeOnUI.numberOfPeopleOnStation = _numberOfPeopleOnStation;
    }

    void FixedUpdate()
    {
        if(UnityEngine.Random.Range(0, clientProbabilitySampleSize) < 1) {
            _numberOfPeopleOnStation += 1;

            var cube = Instantiate(
                passantPrefab,
                this.transform.position + new Vector3(0, _numberOfPeopleOnStation * 1, 0),
                Quaternion.identity,
                this.transform
            );
            cube.transform.localScale = new Vector3(0.05f, 0.5f, 0.025f);
        }

        if(UnityEngine.Random.Range(0, mutatingParameters.trainFrequency) < 1) {
            int toDelete = Math.Min(
                mutatingParameters.trainCapacity, 
                _numberOfPeopleOnStation
            );
            _numberOfPeopleOnStation -= toDelete;

            int totalChildren = transform.childCount;

            for (int i = totalChildren - 1; i >= totalChildren - toDelete; i--)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);
            }
/*
            while(toDelete-->=0) {
                Transform child = transform.GetChild(transform.childCount - 1);
                Destroy(child.gameObject);
            }
*/
            fitness.numberOfTrainsPassed += 1;
        }

        _updateFitness();
    }

    private void _updateFitness()
    {
        if(_numberOfPeopleOnStation > overcowdedThreshold) {
            fitness.timeSpentOvercrowded += 1;
        }

        if(_numberOfPeopleOnStation >= stationCapacity) {
            fitness.timeSpentFull += 1;
        }
    }

    override public float FinalFitness()
    {
        return 
        fitness.FinalFitness(constantParameters);
    }

    override public string DebugFitness()
    {
        return "trains " + fitness.numberOfTrainsPassed
        + "crowded " + fitness.timeSpentOvercrowded
        + "full " + fitness.timeSpentFull;
    }

    override public void Cross(
        IMutatingParameters p1, IMutatingParameters p2
    ) {
        
        mutatingParameters.trainCapacity = 
        (
            p1.trainCapacity 
            + p2.trainCapacity
        ) / 2;
        mutatingParameters.trainFrequency = (
            p1.trainFrequency 
            + p2.trainFrequency
        ) / 2;
    }

    override public void Mutate()
    {
        {
            float nv = mutatingParameters.trainCapacity 
                * (1f + UnityEngine.Random.Range(-0.5f, 1f));
            mutatingParameters.trainCapacity = (int)nv;
        }

        {
            float nv = mutatingParameters.trainFrequency 
                * (1 + UnityEngine.Random.Range(-0.5f, 1f));
            mutatingParameters.trainFrequency = (int)nv;
        }
    }

    void OnDrawGizmos()
    {
        // Set the gizmo color
        GUIStyle style = new GUIStyle();
        //style.normal.textColor = textColor;
        style.fontSize = 12; // Adjust font size as needed
        style.alignment = TextAnchor.MiddleCenter;

        // Draw the text at the cube's position plus the offset
        Vector3 textPosition = transform.position ;//+ textOffset;
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            textPosition, 
            "People waiting " + _numberOfPeopleOnStation + 
        "\nTrain frequency " + mutatingParameters.trainFrequency
        + "\nTrain capacity " + mutatingParameters.trainCapacity
        , style);
#endif
    }

}
