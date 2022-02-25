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
        long population = Interlocked.Read(ref CounterSystem.populationCounter);
        long sym = Interlocked.Read(ref CounterSystem.symptomaticCounter);
        long symVAX = Interlocked.Read(ref CounterSystem.symptomaticVAXCounter);
        long exp = Interlocked.Read(ref CounterSystem.infectedCounter);
        long expVAX = Interlocked.Read(ref CounterSystem.infectedVAXCounter);
        long death = Interlocked.Read(ref CounterSystem.deathCounter);
        long deathVAX = Interlocked.Read(ref CounterSystem.deathVAXCounter);
        long asy = Interlocked.Read(ref CounterSystem.asymptomaticCounter);
        long asyVAX = Interlocked.Read(ref CounterSystem.asymptomaticVAXCounter);
        long rec = Interlocked.Read(ref CounterSystem.recoveredCounter);
        long recVAX = Interlocked.Read(ref CounterSystem.recoveredVAXCounter);
        long first = Interlocked.Read(ref CounterSystem.firstDosesCounter);
        long sec = Interlocked.Read(ref CounterSystem.secondDosesCounter);
        long third = Interlocked.Read(ref CounterSystem.thirdDosesCounter);
        long fourth = Interlocked.Read(ref CounterSystem.fourthDosesCounter);

        SymptomaticCounterText.text = "Symptomatic: " + sym + "        " + string.Format("{0:0.00}", Percentage(population,sym)) + "%" ;
        SymptomaticVAXCounterText.text = "Symptomatic: " + symVAX + "       " + string.Format("{0:0.00}", Percentage(population, symVAX)) + "%";
        ExposedCounterText.text = "Exposed: " + exp + "        " + string.Format("{0:0.00}", Percentage(population, exp)) + "%";
        ExposedCounterVAXText.text = "Exposed: " + expVAX + "        " + string.Format("{0:0.00}", Percentage(population, expVAX)) + "%";
        DeathCounterText.text = "Deaths: " + death + "        " + string.Format("{0:0.00}", Percentage(population, death)) + "%";
        DeathVAXCounterText.text = "Deaths: " +  deathVAX + "        " + string.Format("{0:0.00}", Percentage(population, deathVAX)) + "%";
        PopulationCounterText.text = "Population: " + population;
        AsymptomaticCounterText.text = "Asynthomatic: " + asy + "        " + string.Format("{0:0.00}", Percentage(population, asy)) + "%";
        AsymptomaticVAXCounterText.text = "Asynthomatic: " + asyVAX + "        " + string.Format("{0:0.00}", Percentage(population, asyVAX)) + "%";
        RecoveredCounterText.text = "Recovered: " + rec + "        " + string.Format("{0:0.00}", Percentage(population, rec)) + "%";
        RecoveredVAXCounterText.text = "Recovered: " + recVAX + "        " + string.Format("{0:0.00}", Percentage(population, recVAX)) + "%";
        FirstDosesCounterText.text = "1st Doses: " + first + "    " + string.Format("{0:0.00}", Percentage(population, first)) + "%";
        SecondDosesCounterText.text = "2nd Doses: " + sec + "    " + string.Format("{0:0.00}", Percentage(population, sec)) + "%";
        ThirdDosesCounterText.text = "3rd Doses: " + third + "    " + string.Format("{0:0.00}", Percentage(population, third)) + "%";
        FourthDosesCounterText.text = "4th Doses: " + fourth + "    " + string.Format("{0:0.00}", Percentage(population, fourth)) + "%";
        TotalIntensiveCareCounterText.text = "Intensive Care available: " + Interlocked.Read(ref ContagionSystem.currentTotIntensive); ;
        IntensiveVAXCareCounterText.text = "in Intensive Care: " + Interlocked.Read(ref CounterSystem.intensiveVAXCounter); ;
        IntensiveNOVAXCareCounterText.text = "in Intensive Care: " + Interlocked.Read(ref CounterSystem.intensiveNOVAXCounter); ;

    }

    public float Percentage(long population, long counter)
    {
        
        return ((float) counter / population)*100f;
    }
}

