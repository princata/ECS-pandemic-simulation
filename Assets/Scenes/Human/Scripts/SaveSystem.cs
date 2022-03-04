using UnityEngine;
using Unity.Entities;
using Unity.Entities.Serialization;
using System.IO;
using System.Threading;

public class SaveSystem : MonoBehaviour
{
    //ReferencedUnityObjects g;
    Data dataHold;
    object[] sc;

    private void Start()
    {
        dataHold = new Data();
       // g = ScriptableObject.CreateInstance<ReferencedUnityObjects>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Save();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
    }
    public void Save()
    {
        SaveCounters counters = SaveValues();
        string json = JsonUtility.ToJson(counters);
        File.WriteAllText(Application.dataPath + "/counterSave.txt", json);
        
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        using (var writer = new StreamBinaryWriter(Application.dataPath + "/save"))
        {
            SerializeUtility.SerializeWorld(entityManager, writer, out sc);
            //SerializeUtilityHybrid.Serialize(entityManager, writer, out g);
            Debug.Log("save");
           // Debug.Log(g);
        }
        dataHold.Array = (Object[])sc;
        var data = JsonUtility.ToJson(dataHold);
        PlayerPrefs.SetString("Data", data);
        CounterSystem.startAppend = false; //questo parametro serve per scrivere nel file di statistiche, quando salvo vuol dire che voglio successivamente caricare quindi non scrivo le statistiche dal momento del salvataggio
    }
    public void Load()
    {
        if (File.Exists(Application.dataPath + "/counterSave.txt"))
        {
            string json = File.ReadAllText(Application.dataPath + "/counterSave.txt");
            SaveCounters counters = JsonUtility.FromJson<SaveCounters>(json);
            LoadValues(counters);
        }


        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        var data = PlayerPrefs.GetString("Data");
        dataHold = JsonUtility.FromJson<Data>(data);
        sc = dataHold.Array;
        EntityManager main = World.DefaultGameObjectInjectionWorld.EntityManager;
        World world = new World("svet");
        EntityManager entityManager = world.EntityManager;
        var transaction = world.EntityManager.BeginExclusiveEntityTransaction();
        using (var reader = new StreamBinaryReader(Application.dataPath + "/save"))
        {
            SerializeUtility.DeserializeWorld(transaction, reader, sc);
            //SerializeUtilityHybrid.Deserialize(entityManager, reader, g);
            Debug.Log("load");
        }
        world.EntityManager.EndExclusiveEntityTransaction();
        main.MoveEntitiesFrom(entityManager);
        CounterSystem.startAppend = true; //rimetto a true così inizio a scrivere statistiche con le nuove disposizioni
        world.Dispose();

    }

    public SaveCounters SaveValues()
    {
        SaveCounters obj = new SaveCounters()
        {
            infectedCounter = Interlocked.Read(ref CounterSystem.infectedCounter),
            infectedVAXCounter = Interlocked.Read(ref CounterSystem.infectedVAXCounter),
            totalInfectedCounter = Interlocked.Read(ref CounterSystem.totalInfectedCounter),
            symptomaticCounter = Interlocked.Read(ref CounterSystem.symptomaticCounter),
            symptomaticVAXCounter = Interlocked.Read(ref CounterSystem.symptomaticVAXCounter),
            asymptomaticCounter = Interlocked.Read(ref CounterSystem.asymptomaticCounter),
            asymptomaticVAXCounter = Interlocked.Read(ref CounterSystem.asymptomaticVAXCounter),
            recoveredCounter = Interlocked.Read(ref CounterSystem.recoveredCounter),
            recoveredVAXCounter = Interlocked.Read(ref CounterSystem.recoveredVAXCounter),
            totalRecoveredCounter = Interlocked.Read(ref CounterSystem.totalRecoveredCounter),
            deathCounter = Interlocked.Read(ref CounterSystem.deathCounter),
            deathVAXCounter = Interlocked.Read(ref CounterSystem.deathVAXCounter),
            populationCounter = Interlocked.Read(ref CounterSystem.populationCounter),
            firstDosesCounter = Interlocked.Read(ref CounterSystem.firstDosesCounter),
            secondDosesCounter = Interlocked.Read(ref CounterSystem.secondDosesCounter),
            thirdDosesCounter = Interlocked.Read(ref CounterSystem.thirdDosesCounter),
            fourthDosesCounter = Interlocked.Read(ref CounterSystem.fourthDosesCounter),
            totalIntensiveCounter = Interlocked.Read(ref CounterSystem.totalIntensiveCounter),
            intensiveNOVAXCounter = Interlocked.Read(ref CounterSystem.intensiveNOVAXCounter),
            intensiveVAXCounter = Interlocked.Read(ref CounterSystem.intensiveVAXCounter),
            totIntensiveStudent = Interlocked.Read(ref CounterSystem.totIntensiveStudent),
            totIntensiveWorker = Interlocked.Read(ref CounterSystem.totIntensiveWorker),
            totIntensiveRetired = Interlocked.Read(ref CounterSystem.totIntensiveRetired),
            currentIntensive = Interlocked.Read(ref ContagionSystem.currentTotIntensive),
            totDeathStudent = Interlocked.Read(ref CounterSystem.totDeathStudent),
            totDeathWorker = Interlocked.Read(ref CounterSystem.totDeathWorker),
            totDeathRetired = Interlocked.Read(ref CounterSystem.totDeathRetired),

            totInfectedStudent = Interlocked.Read(ref CounterSystem.totInfectedStudent),
            totInfectedWorker = Interlocked.Read(ref CounterSystem.totInfectedWorker),
            totInfectedRetired = Interlocked.Read(ref CounterSystem.totInfectedRetired),

            totalMinutes = Datetime.total_minutes            
        };


        return obj;
    }

    public void LoadValues(SaveCounters obj)
    {
        unsafe
        {
           
            Interlocked.Exchange(ref CounterSystem.infectedCounter, obj.infectedCounter);
            Interlocked.Exchange(ref CounterSystem.infectedVAXCounter, obj.infectedVAXCounter);
            Interlocked.Exchange(ref CounterSystem.totalInfectedCounter, obj.totalInfectedCounter);
            Interlocked.Exchange(ref CounterSystem.symptomaticCounter, obj.symptomaticCounter);
            Interlocked.Exchange(ref CounterSystem.asymptomaticCounter, obj.asymptomaticCounter);
            Interlocked.Exchange(ref CounterSystem.symptomaticVAXCounter, obj.symptomaticVAXCounter);
            Interlocked.Exchange(ref CounterSystem.asymptomaticVAXCounter, obj.asymptomaticVAXCounter);
            Interlocked.Exchange(ref CounterSystem.recoveredCounter, obj.recoveredCounter);
            Interlocked.Exchange(ref CounterSystem.recoveredVAXCounter, obj.recoveredVAXCounter);
            Interlocked.Exchange(ref CounterSystem.totalRecoveredCounter, obj.totalRecoveredCounter);
            Interlocked.Exchange(ref CounterSystem.deathCounter, obj.deathCounter);
            Interlocked.Exchange(ref CounterSystem.deathVAXCounter, obj.deathVAXCounter);
            Interlocked.Exchange(ref CounterSystem.populationCounter, obj.populationCounter);
            Interlocked.Exchange(ref CounterSystem.firstDosesCounter, obj.firstDosesCounter);
            Interlocked.Exchange(ref CounterSystem.secondDosesCounter, obj.secondDosesCounter);
            Interlocked.Exchange(ref CounterSystem.thirdDosesCounter, obj.thirdDosesCounter);
            Interlocked.Exchange(ref CounterSystem.fourthDosesCounter, obj.fourthDosesCounter);
            Interlocked.Exchange(ref CounterSystem.intensiveVAXCounter, obj.intensiveVAXCounter);
            Interlocked.Exchange(ref CounterSystem.intensiveNOVAXCounter, obj.intensiveNOVAXCounter);
            Interlocked.Exchange(ref CounterSystem.totalIntensiveCounter, obj.totalIntensiveCounter);
            Interlocked.Exchange(ref ContagionSystem.currentTotIntensive, obj.currentIntensive);

            Interlocked.Exchange(ref CounterSystem.totInfectedRetired, obj.totInfectedRetired);
            Interlocked.Exchange(ref CounterSystem.totInfectedWorker, obj.totInfectedWorker);
            Interlocked.Exchange(ref CounterSystem.totInfectedStudent, obj.totInfectedStudent);
            Interlocked.Exchange(ref CounterSystem.totDeathRetired, obj.totDeathRetired);
            Interlocked.Exchange(ref CounterSystem.totDeathWorker, obj.totDeathWorker);
            Interlocked.Exchange(ref CounterSystem.totDeathStudent, obj.totDeathStudent);
            Interlocked.Exchange(ref CounterSystem.totIntensiveRetired, obj.totIntensiveRetired);
            Interlocked.Exchange(ref CounterSystem.totIntensiveWorker, obj.totIntensiveWorker);
            Interlocked.Exchange(ref CounterSystem.totIntensiveStudent, obj.totIntensiveStudent);
        }
        Datetime.total_minutes = obj.totalMinutes;
    }

}

public class Data
{
    public Object[] Array;
}

public class SaveCounters
{
    public long infectedCounter;
    public long infectedVAXCounter;
    public long totalInfectedCounter;
    public long symptomaticCounter;
    public long asymptomaticCounter;
    public long symptomaticVAXCounter;
    public long asymptomaticVAXCounter;
    public long recoveredCounter;
    public long recoveredVAXCounter;
    public long totalRecoveredCounter;
    public long deathCounter;
    public long deathVAXCounter;
    public long populationCounter;
    public long firstDosesCounter;
    public long secondDosesCounter;
    public long thirdDosesCounter;
    public long fourthDosesCounter;
    public long totalIntensiveCounter;
    public long intensiveVAXCounter;
    public long intensiveNOVAXCounter;
    public long currentIntensive;
    public long totInfectedRetired;
    public long totInfectedWorker;
    public long totInfectedStudent;
    public long totDeathRetired;
    public long totDeathWorker;
    public long totDeathStudent;
    public long totIntensiveRetired;
    public long totIntensiveWorker;
    public long totIntensiveStudent;

    public float totalMinutes;
    public bool startAppend;
}