using System;
using UnityEngine;

public abstract class IFitStation: MonoBehaviour {
    public abstract IMutatingParameters mutatingParameters { get; set; }
    public abstract float FinalFitness();
    public abstract string DebugFitness();
    public abstract void Cross(IMutatingParameters mp1, IMutatingParameters mp2);
    public abstract void Mutate();}
