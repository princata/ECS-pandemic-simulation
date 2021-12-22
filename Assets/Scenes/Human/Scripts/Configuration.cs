using System.IO;
using UnityEngine;


public class Configuration 
{
    public bool appendLog; //volontà di appendere le statistiche quindi intenzione di fare una load
    public int numberOfHumans;
    public int numberOfInfects;
    public float timeScale;
    public float probabilityOfSymptomatic;
    public float probabilityOfDeath;
    public string map;
    public float minDaysInfectious;
    public float maxDaysInfectious;
    public float minDaysRecovered;
    public float maxDaysRecovered;
    public float minDaysExposed;
    public float maxDaysExposed;
    public bool lockdown;
    public bool vaccinationPolicy;
    public bool greenPass;
    public bool lockGym;
    public bool lockSchool;
    public bool lockPubs;

    // TextAsset text = new TextAsset(File.ReadAllText("./Assets/Conf/conf.txt"));

    public int NumberOfHumans { get => numberOfHumans; set => numberOfHumans = value; }
    public int NumberOfInfects { get => numberOfInfects; set => numberOfInfects = value; }
    public float TimeScale { get => timeScale; set => timeScale = value; }
    public float ProbabilityOfSymptomatic { get => probabilityOfSymptomatic; set => probabilityOfSymptomatic = value; }
    public float ProbabilityOfDeath { get => probabilityOfDeath; set => probabilityOfDeath = value; }
    public string Map { get => map; set => map = value; }
    public float MinDaysInfectious { get => minDaysInfectious; set => minDaysInfectious = value; }
    public float MaxDaysInfectious { get => maxDaysInfectious; set => maxDaysInfectious = value; }
    public float MinDaysRecovered { get => minDaysRecovered; set => minDaysRecovered = value; }
    public float MaxDaysRecovered { get => maxDaysRecovered; set => maxDaysRecovered = value; }
    public float MinDaysExposed { get => minDaysExposed; set => minDaysExposed = value; }
    public float MaxDaysExposed { get => maxDaysExposed; set => maxDaysExposed = value; }
    public bool Lockdown { get => lockdown; set => lockdown = value; }
    public bool VaccinationPolicy { get => vaccinationPolicy; set => vaccinationPolicy = value; }
    public bool GreenPass { get => greenPass; set => greenPass = value; }
    public bool LockGym { get => lockGym; set => lockGym = value; }
    public bool LockSchool { get => lockSchool; set => lockSchool = value; }
    public bool AppendLog { get => appendLog; set => appendLog = value; }

    public static Configuration CreateFromJSON()
    {
        string text = File.ReadAllText("./Assets/Conf/conf.txt");
        // Debug.Log(text);
        return JsonUtility.FromJson<Configuration>(text);
    }


}
