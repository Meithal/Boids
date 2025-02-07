using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShowTimeOnUI : MonoBehaviour
{
    public UIDocument uIDocument;
    private Label timeLabel;
    private Label peopleOnStationLabel;
    public DateTime startTime;

    public int numberOfPeopleOnStation = 0;

    // Start is called before the first frame update
    void Start()
    {
        var doc = uIDocument.rootVisualElement;
        
        timeLabel = doc.Q<Label>("time_spent");
        peopleOnStationLabel = doc.Q<Label>("people_on_station");

    }

    // Update is called once per frame
    void Update()
    {
        if (startTime == null)
            return;
        timeLabel.text = startTime.AddSeconds(Time.time).ToLongTimeString();
        peopleOnStationLabel.text = "People on station : " + numberOfPeopleOnStation;
    }
}
