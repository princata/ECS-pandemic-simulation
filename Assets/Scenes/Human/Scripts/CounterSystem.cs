using System.IO;
using System.Threading;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//[UpdateAfter(typeof(HumanSystem))]
[UpdateAfter(typeof(ContagionSystem))]
[BurstCompile]
public class CounterSystem : SystemBase
{
    Configuration conf;
    EndInitializationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    public static long infectedCounter;
    public static long infectedVAXCounter;
    public static long totalInfectedCounter;

    public static long symptomaticCounter;
    public static long asymptomaticCounter;
    public static long symptomaticVAXCounter;
    public static long asymptomaticVAXCounter;


    public static long totInfectedRetired;
    public static long totInfectedWorker;
    public static long totInfectedStudent;

    public static long recoveredCounter;
    public static long recoveredVAXCounter;
    public static long totalRecoveredCounter;

    public static long deathCounter;
    public static long deathVAXCounter;
    public static long totDeathRetired;
    public static long totDeathWorker;
    public static long totDeathStudent;

    public static long populationCounter;
    public static long firstDosesCounter;
    public static long secondDosesCounter;
    public static long thirdDosesCounter;
    public static long fourthDosesCounter;
    public static long totalIntensiveCounter;
    public static long intensiveVAXCounter;
    public static long intensiveNOVAXCounter;

    public static long totIntensiveRetired;
    public static long totIntensiveWorker;
    public static long totIntensiveStudent;

    public static bool startAppend;
    public static long appendLog;
    private StreamWriter writer;
    public static string logPath = "statistics/log.txt";

    protected override void OnCreate()
    {
        conf = Configuration.CreateFromJSON();
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        appendLog = Convert.ToInt32(conf.appendLog);
        infectedCounter = conf.NumberOfInfects;
        infectedVAXCounter = 0;
        symptomaticCounter = 0;
        asymptomaticCounter = 0;
        symptomaticVAXCounter = 0;
        asymptomaticVAXCounter = 0;
        recoveredCounter = 0;
        recoveredVAXCounter = 0;
        deathCounter = 0;
        deathVAXCounter = 0;
        populationCounter = conf.NumberOfHumans;
        firstDosesCounter = 0;
        intensiveNOVAXCounter = 0;
        intensiveVAXCounter = 0;
        totalIntensiveCounter = 0;
      
        totInfectedRetired = 0;
        totInfectedWorker = 0;
        totInfectedStudent = 0;
        totIntensiveRetired = 0;
        totIntensiveWorker = 0;
        totIntensiveStudent = 0;
        totDeathRetired = 0;
        totDeathWorker = 0;
        totDeathStudent = 0;

        writer = new StreamWriter(logPath, conf.appendLog); // false is for overwrite existing file, true to append
        if (!conf.appendLog)
        {
            startAppend = true;
            writer.WriteLine("ChangePolicies\tPopulation\tExposed\tExposedVAX\tTotalExposed\tSymptomatic\tSymptomaticVAX\tAsymptomatic\tAsymptomaticVAX\ttotInfectedRetired\ttotInfectedWorker\ttotInfectedStudent\tDeath\tDeathVAX\ttotDeathRetired\ttotDeathWorker\ttotDeathStudent\tRecovered\tRecoveredVAX\tTotalRecovered\t" +
                "IntensiveCare\tIntensiveCareVAX\tTotalIntensive\ttotIntensiveRetired\ttotIntensiveRetired\ttotIntensiveRetired\tFirstDoses\tSecondDoses\tThirdDoses\tFourthDoses\tMinutesPassed");
        }
        else
            startAppend = false; //che diventa true appena faccio load
        
    }

    protected override void OnStartRunning()
    {
        //totalIntensiveCounter = Human.Instance.totalIntensiveCare;
    }
    protected override void OnUpdate()
    {
        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
        var vaccinationPolicy = Human.Instance.vaccinationPolicy;
        //if(totalIntensiveCounter > Human.Instance.totalIntensiveCare)
         //   totalIntensiveCounter = Human.Instance.totalIntensiveCare;

        NativeArray<long> localInfectedCounter = new NativeArray<long>(1, Allocator.TempJob);
        localInfectedCounter[0] = 0;

        NativeArray<long> localInfectedVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localInfectedVAXCounter[0] = 0;

        NativeArray<long> localTotalInfectedCounter = new NativeArray<long>(1, Allocator.TempJob);
        localTotalInfectedCounter[0] = 0;

        NativeArray<long> localSymptomaticCounter = new NativeArray<long>(1, Allocator.TempJob);
        localSymptomaticCounter[0] = 0;


        NativeArray<long> localAsymptomaticCounter = new NativeArray<long>(1, Allocator.TempJob);
        localAsymptomaticCounter[0] = 0;

        NativeArray<long> localSymptomaticVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localSymptomaticVAXCounter[0] = 0;


        NativeArray<long> localAsymptomaticVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localAsymptomaticVAXCounter[0] = 0;

        NativeArray<long> localTotInfRetired = new NativeArray<long>(1, Allocator.TempJob);
        localTotInfRetired[0] = 0;

        NativeArray<long> localTotInfWorker = new NativeArray<long>(1, Allocator.TempJob);
        localTotInfWorker[0] = 0;

        NativeArray<long> localTotInfStudent = new NativeArray<long>(1, Allocator.TempJob);
        localTotInfStudent[0] = 0;

        NativeArray<long> localDeathCounter = new NativeArray<long>(1, Allocator.TempJob);
        localDeathCounter[0] = 0;

        NativeArray<long> localDeathVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localDeathVAXCounter[0] = 0;

        NativeArray<long> localTotDeathRetired = new NativeArray<long>(1, Allocator.TempJob);
        localTotDeathRetired[0] = 0;

        NativeArray<long> localTotDeathWorker = new NativeArray<long>(1, Allocator.TempJob);
        localTotDeathWorker[0] = 0;

        NativeArray<long> localTotDeathStudent = new NativeArray<long>(1, Allocator.TempJob);
        localTotDeathStudent[0] = 0;

        NativeArray<long> localRecoveredCounter = new NativeArray<long>(1, Allocator.TempJob);
        localRecoveredCounter[0] = 0;

        NativeArray<long> localRecoveredVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localRecoveredVAXCounter[0] = 0;

        NativeArray<long> localTotalRecoveredCounter = new NativeArray<long>(1, Allocator.TempJob);
        localTotalRecoveredCounter[0] = 0;

        NativeArray<long> localFirstDosesCounter = new NativeArray<long>(1, Allocator.TempJob);
        localFirstDosesCounter[0] = 0;

        NativeArray<long> localSecondDosesCounter = new NativeArray<long>(1, Allocator.TempJob);
        localSecondDosesCounter[0] = 0;

        NativeArray<long> localThirdDosesCounter = new NativeArray<long>(1, Allocator.TempJob);
        localThirdDosesCounter[0] = 0;

        NativeArray<long> localFourthDosesCounter = new NativeArray<long>(1, Allocator.TempJob);
        localFourthDosesCounter[0] = 0;

        NativeArray<long> localIntensiveVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localIntensiveVAXCounter[0] = 0;

        NativeArray<long> localIntensiveCounter = new NativeArray<long>(1, Allocator.TempJob);
        localIntensiveCounter[0] = 0;

        NativeArray<long> localTotalIntensiveCounter = new NativeArray<long>(1, Allocator.TempJob);
        localTotalIntensiveCounter[0] = 0;

        NativeArray<long> localTotIntRetired = new NativeArray<long>(1, Allocator.TempJob);
        localTotIntRetired[0] = 0;  
                                    
        NativeArray<long> localTotIntWorker = new NativeArray<long>(1, Allocator.TempJob);
        localTotIntWorker[0] = 0;   
                                    
        NativeArray<long> localTotIntStudent = new NativeArray<long>(1, Allocator.TempJob);
        localTotIntStudent[0] = 0;

        var JobHandle1 = Entities.ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent humanComponent, ref InfectionComponent ic) =>
        {
          
            if (ic.status == Status.exposed && ic.oldstatus == Status.susceptible)
            {
                if(humanComponent.PROvax && humanComponent.vaccinations > 0)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localInfectedVAXCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localTotalInfectedCounter.GetUnsafePtr())[0]);
                    }
                }
                else
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localTotalInfectedCounter.GetUnsafePtr())[0]);
                    }
                }
                
                ic.oldstatus = Status.exposed;
            }

            if (ic.status == Status.infectious && ic.infected && ic.oldstatus == Status.exposed)
            {
                if (ic.symptomatic)
                {
                    if (humanComponent.PROvax && humanComponent.vaccinations > 0) //PROVAX  IN QUESTO CASO SI NUMERANO I SINTOMATICI CHE HANNO ALMENO UNA DOSE
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localSymptomaticVAXCounter.GetUnsafePtr())[0]);
                        }
                        if (ic.intensiveCare)
                        {
                            unsafe
                            {
                                Interlocked.Increment(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]);
                                Interlocked.Increment(ref ((long*)localTotalIntensiveCounter.GetUnsafePtr())[0]);
                            }
                            //CONTATORI PER ETA'
                            switch (humanComponent.age)
                            {
                                case HumanStatusEnum.HumanStatus.Retired:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntRetired.GetUnsafePtr())[0]);
                                    }
                                    break;
                                case HumanStatusEnum.HumanStatus.Worker:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntWorker.GetUnsafePtr())[0]);
                                    }
                                    break;
                                case HumanStatusEnum.HumanStatus.Student:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntStudent.GetUnsafePtr())[0]);
                                    }
                                    break;

                            }
                        }                     

                    }
                    else
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);
                        }
                        if(ic.intensiveCare)
                        {
                            unsafe
                            {
                                Interlocked.Increment(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                                Interlocked.Increment(ref ((long*)localTotalIntensiveCounter.GetUnsafePtr())[0]);
                            }
                            //CONTATORI PER ETA'
                            switch (humanComponent.age)
                            {
                                case HumanStatusEnum.HumanStatus.Retired:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntRetired.GetUnsafePtr())[0]);
                                    }
                                    break;
                                case HumanStatusEnum.HumanStatus.Worker:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntWorker.GetUnsafePtr())[0]);
                                    }
                                    break;
                                case HumanStatusEnum.HumanStatus.Student:
                                    unsafe
                                    {
                                        Interlocked.Increment(ref ((long*)localTotIntStudent.GetUnsafePtr())[0]);
                                    }
                                    break;

                            }

                        }
                    }

                }
                else 
                {
                    if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localAsymptomaticVAXCounter.GetUnsafePtr())[0]);
                        }
                    }
                    else
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]);
                        }
                    }
                }

                //DECREMENTO COUNTER ENTITA' IN INCUBAZIONE SE SONO INFETTI
                if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                {
                    unsafe
                    {
                        Interlocked.Decrement(ref ((long*)localInfectedVAXCounter.GetUnsafePtr())[0]);
                    }
                }
                else
                {
                    unsafe
                    {
                        Interlocked.Decrement(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]);
                    }
                }
                //CONTATORI PER ETA'
                switch (humanComponent.age)
                {
                    case HumanStatusEnum.HumanStatus.Retired:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotInfRetired.GetUnsafePtr())[0]);
                        }
                        break;
                    case HumanStatusEnum.HumanStatus.Worker:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotInfWorker.GetUnsafePtr())[0]);
                        }
                        break;
                    case HumanStatusEnum.HumanStatus.Student:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotInfStudent.GetUnsafePtr())[0]);
                        }
                        break;

                }

                ic.oldstatus = Status.infectious;
            }

            if (ic.status == Status.removed && !ic.infected && ic.oldstatus == Status.infectious) //SOLO I SINTOMATICI POSSONO MORIRE
            {
                if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localDeathVAXCounter.GetUnsafePtr())[0]);
                        Interlocked.Decrement(ref ((long*)localSymptomaticVAXCounter.GetUnsafePtr())[0]);
                        //Interlocked.Decrement(ref ((long*)localInfectedVAXCounter.GetUnsafePtr())[0]);
                    }
                    if (ic.intensiveCare)
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]);
                        }
                        ic.intensiveCare = false;
                       // Debug.Log($"morto vax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} in intensive care");
                    }
                    /*else
                    {
                        if(ic.criticalDisease)
                            Debug.Log($"morto vax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} but critical disease with {totalIntensiveCounter} available");
                        else
                            Debug.Log($"morto vax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} without reason");
                    }*/
                }
                else
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localDeathCounter.GetUnsafePtr())[0]);
                        Interlocked.Decrement(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);
                        //Interlocked.Decrement(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]);
                    }
                    if (ic.intensiveCare)
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                        }
                        ic.intensiveCare = false;
                       // Debug.Log($"morto novax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} in intensive care");
                    }
                   /* else
                    {
                        if (ic.criticalDisease)
                            Debug.Log($"morto novax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} but critical disease with {totalIntensiveCounter} available");
                        else
                            Debug.Log($"morto novax con rnd : {ic.myRndValue} e hdp : {ic.currentHumanDeathProbability} without reason");
                    }*/
                }
                //remove entity
                /*
                unsafe
                {                   
                    Interlocked.Decrement(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]);
                }
                */
                //CONTATORI PER ETA'
                switch (humanComponent.age)
                {
                    case HumanStatusEnum.HumanStatus.Retired:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotDeathRetired.GetUnsafePtr())[0]);
                        }
                        break;
                    case HumanStatusEnum.HumanStatus.Worker:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotDeathWorker.GetUnsafePtr())[0]);
                        }
                        break;
                    case HumanStatusEnum.HumanStatus.Student:
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localTotDeathStudent.GetUnsafePtr())[0]);
                        }
                        break;

                }
                ic.oldstatus = Status.removed;
                ecb.DestroyEntity(nativeThreadIndex, entity);
            }

            else if (ic.status == Status.recovered && !ic.infected && ic.oldstatus == Status.infectious)
            {

                if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                {
                    unsafe
                    {
                       // Interlocked.Decrement(ref ((long*)localInfectedVAXCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localRecoveredVAXCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localTotalRecoveredCounter.GetUnsafePtr())[0]);
                    }
                }
                else
                {
                    unsafe
                    {
                       // Interlocked.Decrement(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localTotalRecoveredCounter.GetUnsafePtr())[0]);
                    }
                }

                if (ic.symptomatic)
                {
                    if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localSymptomaticVAXCounter.GetUnsafePtr())[0]);
                        }
                        if (ic.intensiveCare)
                        {
                            unsafe
                            {
                                Interlocked.Decrement(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]);
                            }
                            ic.intensiveCare = false;
                        }

                    }
                    else
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);
                        }
                        if (ic.intensiveCare)
                        {
                            unsafe
                            {
                                Interlocked.Decrement(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                            }
                            ic.intensiveCare = false;
                        }
                    }

                    ic.symptomatic = false;
                }
                else
                {
                    if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localAsymptomaticVAXCounter.GetUnsafePtr())[0]);
                        }
                    }
                    else
                    {
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]);
                        }
                    }
                }
                ic.oldstatus = Status.recovered;
            }

            if (ic.status == Status.susceptible && ic.oldstatus == Status.recovered)
            {
                if (humanComponent.PROvax && humanComponent.vaccinations > 0)
                {
                    unsafe
                    {
                        Interlocked.Decrement(ref ((long*)localRecoveredVAXCounter.GetUnsafePtr())[0]);
                    }
                }
                else
                {
                    unsafe
                    {
                        Interlocked.Decrement(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]);
                    }

                }
                ic.myRndValue = -1;
                ic.oldstatus = Status.susceptible;
            }

            //---------INSERIMENTO COUNTERS VACCINI------------
            if (vaccinationPolicy && humanComponent.PROvax && ic.doses < 2)
            {
                if (humanComponent.vaccinations == 1 && ic.doses == 0)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localFirstDosesCounter.GetUnsafePtr())[0]);
                    }
                    ic.doses = 1;
                }
                else if (humanComponent.vaccinations == 2 && ic.doses == 1)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localSecondDosesCounter.GetUnsafePtr())[0]);
                    }

                    ic.doses = 2;
                }
                else if (humanComponent.vaccinations == 3 && ic.doses == 2)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localThirdDosesCounter.GetUnsafePtr())[0]);
                    }

                    ic.doses = 3;
                }
                else if (humanComponent.vaccinations == 4 && ic.doses == 3)
                {
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localFourthDosesCounter.GetUnsafePtr())[0]);
                    }

                    ic.doses = 4;
                }


            }


        }).ScheduleParallel(this.Dependency);

        m_EndSimulationEcbSystem.AddJobHandleForProducer(JobHandle1);
        this.Dependency = JobHandle1;
        JobHandle1.Complete();

        if (infectedCounter < 0)
            infectedCounter = 0;

        if (infectedVAXCounter < 0)
            infectedVAXCounter = 0;

        if (intensiveNOVAXCounter < 0)
            intensiveNOVAXCounter = 0;
        if (intensiveVAXCounter < 0)
            intensiveVAXCounter = 0;

        unsafe
        {
            Interlocked.Add(ref infectedCounter, Interlocked.Read(ref ((long*)localInfectedCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref infectedVAXCounter, Interlocked.Read(ref ((long*)localInfectedVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref totalInfectedCounter, Interlocked.Read(ref ((long*)localTotalInfectedCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref symptomaticCounter, Interlocked.Read(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref asymptomaticCounter, Interlocked.Read(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref symptomaticVAXCounter, Interlocked.Read(ref ((long*)localSymptomaticVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref asymptomaticVAXCounter, Interlocked.Read(ref ((long*)localAsymptomaticVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref recoveredCounter, Interlocked.Read(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref recoveredVAXCounter, Interlocked.Read(ref ((long*)localRecoveredVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref totalRecoveredCounter, Interlocked.Read(ref ((long*)localTotalRecoveredCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref deathCounter, Interlocked.Read(ref ((long*)localDeathCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref deathVAXCounter, Interlocked.Read(ref ((long*)localDeathVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref populationCounter, -Interlocked.Read(ref ((long*)localDeathCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref populationCounter, -Interlocked.Read(ref ((long*)localDeathVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref firstDosesCounter, Interlocked.Read(ref ((long*)localFirstDosesCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref secondDosesCounter, Interlocked.Read(ref ((long*)localSecondDosesCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref thirdDosesCounter, Interlocked.Read(ref ((long*)localThirdDosesCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref fourthDosesCounter, Interlocked.Read(ref ((long*)localFourthDosesCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref intensiveVAXCounter, Interlocked.Read(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref intensiveNOVAXCounter, Interlocked.Read(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref totalIntensiveCounter, Interlocked.Read(ref ((long*)localTotalIntensiveCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref totInfectedRetired, Interlocked.Read(ref ((long*)localTotInfRetired.GetUnsafePtr())[0]));
            Interlocked.Add(ref totInfectedWorker, Interlocked.Read(ref ((long*)localTotInfWorker.GetUnsafePtr())[0]));
            Interlocked.Add(ref totInfectedStudent, Interlocked.Read(ref ((long*)localTotInfStudent.GetUnsafePtr())[0]));
            Interlocked.Add(ref totDeathRetired, Interlocked.Read(ref ((long*)localTotDeathRetired.GetUnsafePtr())[0]));
            Interlocked.Add(ref totDeathWorker, Interlocked.Read(ref ((long*)localTotDeathWorker.GetUnsafePtr())[0]));
            Interlocked.Add(ref totDeathStudent, Interlocked.Read(ref ((long*)localTotDeathStudent.GetUnsafePtr())[0]));
            Interlocked.Add(ref totIntensiveRetired, Interlocked.Read(ref ((long*)localTotIntRetired.GetUnsafePtr())[0]));
            Interlocked.Add(ref totIntensiveWorker, Interlocked.Read(ref ((long*)localTotIntWorker.GetUnsafePtr())[0]));
            Interlocked.Add(ref totIntensiveStudent, Interlocked.Read(ref ((long*)localTotIntStudent.GetUnsafePtr())[0]));

            //Interlocked.Add(ref totalIntensiveCounter, -Interlocked.Read(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]));
            //Interlocked.Add(ref totalIntensiveCounter, -Interlocked.Read(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]));
        }

        //Human.Instance.totalIntensiveCare = totalIntensiveCounter;

        if (startAppend)
        {
            //Write some text to the test.txt file
            writer.WriteLine(Interlocked.Read(ref appendLog) + "\t"
                                + Interlocked.Read(ref populationCounter) + "\t"
                                + Interlocked.Read(ref infectedCounter) + "\t"
                                + Interlocked.Read(ref infectedVAXCounter) + "\t"
                                + Interlocked.Read(ref totalInfectedCounter) + "\t"
                                + Interlocked.Read(ref symptomaticCounter) + "\t"
                                + Interlocked.Read(ref symptomaticVAXCounter) + "\t"
                                + Interlocked.Read(ref asymptomaticCounter) + "\t"
                                + Interlocked.Read(ref asymptomaticVAXCounter) + "\t"
                                + Interlocked.Read(ref totInfectedRetired) + "\t"
                                + Interlocked.Read(ref totInfectedWorker) + "\t"
                                + Interlocked.Read(ref totInfectedStudent) + "\t"
                                + Interlocked.Read(ref deathCounter) + "\t"
                                + Interlocked.Read(ref deathVAXCounter) + "\t"
                                + Interlocked.Read(ref totDeathRetired) + "\t"
                                + Interlocked.Read(ref totDeathWorker) + "\t"
                                + Interlocked.Read(ref totDeathStudent) + "\t"
                                + Interlocked.Read(ref recoveredCounter) + "\t"
                                + Interlocked.Read(ref recoveredVAXCounter) + "\t"
                                + Interlocked.Read(ref totalRecoveredCounter) + "\t"
                                + Interlocked.Read(ref intensiveNOVAXCounter) + "\t"
                                + Interlocked.Read(ref intensiveVAXCounter) + "\t"
                                + Interlocked.Read(ref totIntensiveRetired) + "\t"
                                + Interlocked.Read(ref totIntensiveWorker) + "\t"
                                + Interlocked.Read(ref totIntensiveStudent) + "\t"
                                + Interlocked.Read(ref totalIntensiveCounter) + "\t"
                                + Interlocked.Read(ref firstDosesCounter) + "\t"
                                + Interlocked.Read(ref secondDosesCounter) + "\t"
                                + Interlocked.Read(ref thirdDosesCounter) + "\t"
                                + Interlocked.Read(ref fourthDosesCounter) + "\t"
                                + (int)Datetime.total_minutes);
        }

        
        
        
            
        localInfectedCounter.Dispose();
        localInfectedVAXCounter.Dispose();
        localTotalInfectedCounter.Dispose();
        localAsymptomaticCounter.Dispose();
        localSymptomaticCounter.Dispose();
        localRecoveredCounter.Dispose();
        localRecoveredVAXCounter.Dispose();
        localTotalRecoveredCounter.Dispose();
        localDeathCounter.Dispose();
        localFirstDosesCounter.Dispose();
        localSecondDosesCounter.Dispose();
        localThirdDosesCounter.Dispose();
        localFourthDosesCounter.Dispose();
        localDeathVAXCounter.Dispose();
        localAsymptomaticVAXCounter.Dispose();
        localSymptomaticVAXCounter.Dispose();
        localIntensiveVAXCounter.Dispose();
        localIntensiveCounter.Dispose();
        localTotalIntensiveCounter.Dispose();
        localTotInfRetired.Dispose();
        localTotInfWorker.Dispose();
        localTotInfStudent.Dispose();
        localTotIntRetired.Dispose();
        localTotIntWorker.Dispose();
        localTotIntStudent.Dispose();
        localTotDeathRetired.Dispose();
        localTotDeathWorker.Dispose();
        localTotDeathStudent.Dispose();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer.Close();
    }

}

