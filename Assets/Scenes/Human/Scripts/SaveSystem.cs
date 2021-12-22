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
            save();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            load();
        }
    }
    public void save()
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

    }
    public void load()
    {
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
            intensiveVAXCounter = Interlocked.Read(ref CounterSystem.intensiveVAXCounter)
        };


        return obj;
    }

    public void LoadValues(SaveCounters obj)
    {

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


}