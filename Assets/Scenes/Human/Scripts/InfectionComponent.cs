using System;
using Unity.Entities;

public enum Status
{
    susceptible,
    exposed,
    infectious,
    recovered,
    removed
}
public struct InfectionComponent : IComponentData
{
    public Boolean infected;
    public Boolean symptomatic;
    public Boolean intensiveCare;
    public Boolean criticalDisease;
    public int doses;

    public Status status;
    public Status oldstatus;

    public float contagionCounter;
    public float infectiousCounter; // counter to track infection exposure, if > threshold human become infected
    public float exposedCounter;
    public float recoveredCounter;

    public float firstHumanSymptomsProbability;
    public float firstHumanDeathProbability;

    public float currentHumanSymptomsProbability;
    public float currentHumanDeathProbability;
    public float currentImmunityLevel; //indicatore di capacità di respingere il virus ( anticorpi neutralizzanti )
    public float myRndValue;

    public float infectiousThreshold;
    public float exposedThreshold;
    public float recoveredThreshold;
}
