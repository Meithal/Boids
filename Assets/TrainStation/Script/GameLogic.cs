using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;


public class GameLogic : MonoBehaviour
{
    public DateTime dateTime;
    public ShowTimeOnUI showTimeOnUI;

    public TrainStation trainStationPrefab;

    [Range(0f, 100f)]
    public float speed = 1.0f;

    public int numberOfStations = 40;
    public List<TrainStation> stations = new();

    private float elapsedTime = 0f; // Time elapsed in seconds
    private float thirtyMinutes = 10f * 60f; // 30 minutes in seconds


    public float horizontalSpaceBetweenStations = 15;
    public float verticalSpaceBetweenStations = 25;

    // Start is called before the first frame update
    void Start()
    {
        dateTime = DateTime.Parse("2025-01-15 00:00:00");
        showTimeOnUI.startTime = dateTime;

        int columns = 10;

        for (int i = 0; i < numberOfStations; i++)
        {
            int row = i / columns;
            int column = i % columns;
            stations.Add(Instantiate<TrainStation>(
                trainStationPrefab, 
                new Vector3(
                    row * horizontalSpaceBetweenStations, 
                    0, 
                    column * verticalSpaceBetweenStations), 
                Quaternion.identity, 
                this.gameObject.transform));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = speed;
        //Time.fixedDeltaTime = speed * Time.timeScale;

        elapsedTime += Time.deltaTime; // Increment elapsed time by frame time

        if (elapsedTime >= thirtyMinutes)
        {
            Debug.Log("30 minutes have passed!");
            elapsedTime = 0f; // Reset timer or handle as needed
            _spawnNewGeneration();
        }
    }

    private TrainStation pop(List<TrainStation> s, int w=0)
    {
        TrainStation ts = s[w];
        s.RemoveAt(w);

        return ts;
    }

    private void _spawnNewGeneration()
    {
        stations.Sort(
            (s1, s2) => s1.FinalFitness().CompareTo(s2.FinalFitness())
        );
        
        List<TrainStation> newStations = new();
        int j = 0;
        int columns = 10;

        for(int i = 0; i < stations.Count / 4; i++) {
            var ts1 = stations[i*2];
            var ts2 = stations[i*2+1];

            for(int k = 0; k < 4; k++) {
                int row = j / columns;
                int column = j % columns;

                TrainStation nw = Instantiate<TrainStation>(
                    trainStationPrefab, 
                    new Vector3(
                        row * horizontalSpaceBetweenStations, 
                        0, 
                        column * verticalSpaceBetweenStations), 
                    Quaternion.identity, 
                    this.gameObject.transform);


                newStations.Add(nw);

                nw.Cross(ts1.mutatingParameters, ts2.mutatingParameters);
                nw.Mutate();
                j += 1;
            }
        }
        for (int i = 0; i < stations.Count; i++)
        {
            Destroy(stations[i].gameObject);
        }

        stations = newStations;
    }
}
