using System.IO;
using System.Threading;
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
    public static long recoveredCounter;
    public static long recoveredVAXCounter;
    public static long totalRecoveredCounter;
    public static long deathCounter;
    public static long deathVAXCounter;
    public static long populationCounter;
    public static long firstDosesCounter;
    public static long secondDosesCounter;
    public static long thirdDosesCounter;
    public static long fourthDosesCounter;
    public static long totalIntensiveCounter;
    public static long intensiveVAXCounter;
    public static long intensiveNOVAXCounter;
    private StreamWriter writer;
    public static string logPath = "statistics/log.txt";

    protected override void OnCreate()
    {
        conf = Configuration.CreateFromJSON();
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
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
        writer = new StreamWriter(logPath, false); // false is for overwrite existing file
        writer.WriteLine("Population\tExposed\tTotalExposed\tSymptomatic\tAsymptomatic\tDeath\tRecovered\tTotalRecovered\tMinutesPassed");
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

        NativeArray<long> localDeathCounter = new NativeArray<long>(1, Allocator.TempJob);
        localDeathCounter[0] = 0;

        NativeArray<long> localDeathVAXCounter = new NativeArray<long>(1, Allocator.TempJob);
        localDeathVAXCounter[0] = 0;

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
            //Interlocked.Add(ref totalIntensiveCounter, -Interlocked.Read(ref ((long*)localIntensiveVAXCounter.GetUnsafePtr())[0]));
            //Interlocked.Add(ref totalIntensiveCounter, -Interlocked.Read(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref totalIntensiveCounter, Interlocked.Read(ref ContagionSystem.currentTotIntensive));
        }


        //Human.Instance.totalIntensiveCare = totalIntensiveCounter;

        //Write some text to the test.txt file
        writer.WriteLine(Interlocked.Read(ref populationCounter) + "\t"
                            + Interlocked.Read(ref infectedCounter) + "\t"
                            + Interlocked.Read(ref totalInfectedCounter) + "\t"
                            + Interlocked.Read(ref symptomaticCounter) + "\t"
                            + Interlocked.Read(ref asymptomaticCounter) + "\t"
                            + Interlocked.Read(ref deathCounter) + "\t"
                            + Interlocked.Read(ref recoveredCounter) + "\t"
                            + Interlocked.Read(ref totalRecoveredCounter) + "\t"
                            + (int)Datetime.total_minutes);

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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer.Close();
    }
}

