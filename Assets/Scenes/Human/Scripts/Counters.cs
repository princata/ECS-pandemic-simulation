using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Counters : MonoBehaviour
{

    public Text ExposedText;
    public Text ExposedVAXText;
    public Text SymptomaticText;
    public Text AsymptomaticText;
    public Text SymptomaticVAXText;
    public Text AsymptomaticVAXText;
    public Text DeathText;
    public Text DeathVAXText;
    public Text PopulationText;
    public Text RecoveredText;
    public Text RecoveredVAXText;
    public Text FirstDosesText;
    public Text SecondDosesText;
    public Text ThirdDosesText;
    public Text FourthDosesText;
    public Text TotalIntensiveCareText;
    public Text IntensiveNOVAXCareText;
    public Text IntensiveVAXCareText;

    public static Text ExposedCounterText;
    public static Text ExposedCounterVAXText;
    public static Text SymptomaticCounterText;
    public static Text AsymptomaticCounterText;
    public static Text SymptomaticVAXCounterText;
    public static Text AsymptomaticVAXCounterText;
    public static Text DeathCounterText;
    public static Text DeathVAXCounterText;
    public static Text PopulationCounterText;
    public static Text RecoveredCounterText;
    public static Text RecoveredVAXCounterText;
    public static Text FirstDosesCounterText;
    public static Text SecondDosesCounterText;
    public static Text ThirdDosesCounterText;
    public static Text FourthDosesCounterText;
    public static Text TotalIntensiveCareCounterText;
    public static Text IntensiveNOVAXCareCounterText;
    public static Text IntensiveVAXCareCounterText;

    // Start is called before the first frame update
    void Start()
    {
        SymptomaticCounterText = SymptomaticText;
        AsymptomaticCounterText = AsymptomaticText;
        SymptomaticVAXCounterText = SymptomaticVAXText;
        AsymptomaticVAXCounterText = AsymptomaticVAXText;
        ExposedCounterText = ExposedText;
        ExposedCounterVAXText = ExposedVAXText;
        DeathCounterText = DeathText;
        DeathVAXCounterText = DeathVAXText;
        PopulationCounterText = PopulationText;
        RecoveredCounterText = RecoveredText;
        RecoveredVAXCounterText = RecoveredVAXText;
        FirstDosesCounterText = FirstDosesText;
        SecondDosesCounterText = SecondDosesText;
        ThirdDosesCounterText = ThirdDosesText;
        FourthDosesCounterText = FourthDosesText;
        TotalIntensiveCareCounterText = TotalIntensiveCareText;
        IntensiveNOVAXCareCounterText = IntensiveNOVAXCareText;
        IntensiveVAXCareCounterText = IntensiveVAXCareText;
    }

    // Update is called once per frame
    void Update()
    {
        SymptomaticCounterText.text = "Symptomatic: " + Interlocked.Read(ref CounterSystem.symptomaticCounter); ;
        SymptomaticVAXCounterText.text = "Symptomatic: " + Interlocked.Read(ref CounterSystem.symptomaticVAXCounter); ;
        ExposedCounterText.text = "Exposed: " + Interlocked.Read(ref CounterSystem.infectedCounter); ;
        ExposedCounterVAXText.text = "Exposed: " + Interlocked.Read(ref CounterSystem.infectedVAXCounter); ;
        DeathCounterText.text = "Deaths: " + Interlocked.Read(ref CounterSystem.deathCounter); ;
        DeathVAXCounterText.text = "Deaths: " + Interlocked.Read(ref CounterSystem.deathVAXCounter); ;
        PopulationCounterText.text = "Population: " + Interlocked.Read(ref CounterSystem.populationCounter);
        AsymptomaticCounterText.text = "Asynthomatic: " + Interlocked.Read(ref CounterSystem.asymptomaticCounter); ;
        AsymptomaticVAXCounterText.text = "Asynthomatic: " + Interlocked.Read(ref CounterSystem.asymptomaticVAXCounter); ;
        RecoveredCounterText.text = "Recovered: " + Interlocked.Read(ref CounterSystem.recoveredCounter); ;
        RecoveredVAXCounterText.text = "Recovered: " + Interlocked.Read(ref CounterSystem.recoveredVAXCounter); ;
        FirstDosesCounterText.text = "1st Doses: " + Interlocked.Read(ref CounterSystem.firstDosesCounter); ;
        SecondDosesCounterText.text = "2nd Doses: " + Interlocked.Read(ref CounterSystem.secondDosesCounter); ;
        ThirdDosesCounterText.text = "3rd Doses: " + Interlocked.Read(ref CounterSystem.thirdDosesCounter); ;
        FourthDosesCounterText.text = "4th Doses: " + Interlocked.Read(ref CounterSystem.fourthDosesCounter); ;
        TotalIntensiveCareCounterText.text = "Intensive Care available: " + Interlocked.Read(ref ContagionSystem.currentTotIntensive); ;
        IntensiveVAXCareCounterText.text = "in Intensive Care: " + Interlocked.Read(ref CounterSystem.intensiveVAXCounter); ;
        IntensiveNOVAXCareCounterText.text = "in Intensive Care: " + Interlocked.Read(ref CounterSystem.intensiveNOVAXCounter); ;

    }
}
