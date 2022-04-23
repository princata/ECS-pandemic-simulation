using System.IO;
using UnityEngine;


public class Configuration 
{
    public bool appendLog; 
    public bool heatmap;
    public int numberOfHumans;
    public int numberOfInfects;
    public float timeScale;
    public float sectionSize;
    public string map;
    public float minDaysInfectious;
    public float maxDaysInfectious;
    public float minDaysRecovered;
    public float maxDaysRecovered;
    public float minDaysExposed;
    public float maxDaysExposed;
    public bool lockdown;
    public bool vaccinationPolicy;
    public bool lockGym;
    public bool lockSchool;
    public bool lockPubs;
    public float malePercentage;
    public int[] inputAge;
    public bool randomSocResp;
    public float socialResponsibility;
    public float noVaxPercentage;
    public int icu4100k;
    public float symptomsStudent;
    public float ifrStudent;
    public float symptomsWorker;
    public float ifrWorker;
    public float symptomsRetired;
    public float ifrRetired;
    public int minDaysFDTstudent;
    public int maxDaysFDTstudent;
    public int minDaysFDTworker;
    public int maxDaysFDTworker;
    public int minDaysFDTretired;
    public int maxDaysFDTretired;
    public float exposureTime;
    public float contagionDistance;
    public float protectionRecovered;
    public float protectionVaccinated;
    public float householdTRs;
    public float householdTRa;
    public float workplaceTRs;
    public float workplaceTRa;
    public float retirehouseTRs;
    public float retirehouseTRa;
    public float outdoorTRs;
    public float outdoorTRa;
    public float indoorTRs;
    public float indoorTRa;
    public float schoolTRs;
    public float schoolTRa;
    public float eatingOutProb;
    public float visitFriendProbNL;
    public float visitFriendProbL;
    public float remoteWorkerPercent;
    public int maxDoses;
    public int daysBTWdoses;
    public float protectionAfterImmunity;
    public int daysOfImmunity;
    public float hungerOnset;
    public float hungerDuration;
    public float restDuration;
    public float sociabilityOnset;
    public float sociabilityDuration;
    public float sportmanshipOnset;
    public float sportmanshipDuration;
    public float groceryOnset;
    public float groceryDuration;
    public float workDuration;
    public int[] familyTemplate;
    public float[] familyDistrib;

    // TextAsset text = new TextAsset(File.ReadAllText("./Assets/Conf/conf.txt"));


    public static Configuration CreateFromJSON()
    {
        string text = File.ReadAllText("./Assets/Conf/conf.txt");
        // Debug.Log(text);
        return JsonUtility.FromJson<Configuration>(text);
    }


}
