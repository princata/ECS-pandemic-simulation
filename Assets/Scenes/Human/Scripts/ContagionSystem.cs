using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using UnityEngine;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;

[UpdateAfter(typeof(QuadrantSystem))]
//[UpdateAfter(typeof(PathFollowSystem))]
[BurstCompile]
public class ContagionSystem : SystemBase
{
    //copy of the grid, used to know where is each entity
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;

    private const float contagionThreshold = 15f; //15 minutes of close contact
    [ReadOnly]
    public long startIntensive;

    public static long currentTotIntensive;

    Configuration conf;

    public const float quadrantCellSize = 10f;

    protected override void OnCreate()
    {
        conf = Configuration.CreateFromJSON();
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }
    protected override void OnStartRunning()
    {
        startIntensive = Human.Instance.totalIntensiveCare;
        currentTotIntensive = startIntensive;      
    }

    protected override void OnUpdate()
    {
        var quadrantMultiHashMap = quadrantMultiHashMap2;
        var vaccinationPolicy = Human.Instance.vaccinationPolicy;
        var startIntensives = startIntensive;        
        var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
        var curTotIntensive = currentTotIntensive;
        float deltaTime = Time.DeltaTime;

        NativeArray<long> localIntensiveCounter = new NativeArray<long>(1, Allocator.TempJob);
        localIntensiveCounter[0] = 0;

        //job -> each element, if not infected, check if there are infected in its same quadrant
        var jobHandle = Entities.WithNativeDisableParallelForRestriction(randomArray).
            ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic, in TileComponent tc) =>
        {
            
            
            //for non infected entities, a check in the direct neighbours is done for checking the presence of infected 
            if (ic.status == Status.susceptible)
            {
                //not infected-> look for infected in same cell
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
                {
                    //la multihashmap ha iscritte solo le entità infette
                    //cycle through all entries on hashmap with the same Key
                    do
                    {
                        //Debug.Log(quadrantData.position);                       
                        if (math.distance(t.Value, quadrantData.position) < 2f)
                        {
                            float3 myHome = new float3(humanComponent.homePosition.x * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.homePosition.y * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.homePosition.z);
                            float3 myOffice = new float3(humanComponent.officePosition.x * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.officePosition.y * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.officePosition.z);
                            //TODO consider also social resp other human involved
                            //increment infectionCounter if one/more infected are in close positions (infected at quadrantData.position) 
                            //infection counter is determined by time and human responsibility

                            if (math.distance(t.Value, myHome) < 2f) //quindi siamo nelle vicinanze di casa con i condimini o familiari  
                            {
                                if (quadrantData.familyKey == humanComponent.familyKey && tc.currentFloor == quadrantData.currentFloor) //HOUSEHOLD CONTACT e ovviamente nello stesso piano z , CASO PLUS: visita agli amici oltre ad household
                                {
                                    Debug.Log($"Household transmission at {tc.currentFloor}th floor");
                                    //se sono a casa e un infetto della mia famiglia è sintomatico e vicino a me allora mi contagio 18% + velocemente ACCORDING TO LUXEMBURG STUDY
                                    if (quadrantData.symptomatic)
                                    {
                                        ic.contagionCounter += (2f * 0.18f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //PARAMETRO IMMUNITY -> (0.01f - 0.99f)

                                    }

                                    else
                                    {
                                        ic.contagionCounter += (2f * 0.05f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //AIC HOUSEHOLD TRANSMISSION 5%                                    
                                    }
                                }
                                else if (quadrantData.familyKey != humanComponent.familyKey && tc.currentFloor == quadrantData.currentFloor)//NO HOUSEHOLD CONTACT
                                { // se invece è un condomino che incontro in "ascensore" o scale o vicino di casa che viene a casa mia

                                    if (quadrantData.symptomatic)
                                    {
                                        ic.contagionCounter += (2f * 0.12f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //SIC NORMAL CONTACT TRANSMISSION -> (0.8% - 15.4%) SELECTED VALUE: 12%
                                    }
                                    else
                                    {
                                        ic.contagionCounter += (2f * 0.02f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //AIC NORMAL CONTACT TRANSMISSION -> (0% - 2.2%) SELECTED VALUE: 1.2%                                        
                                    }

                                }
                            }

                            else if (math.distance(t.Value, myOffice) < 2f) //quindi siamo nel mio ufficio con qualcuno infetto nel mio stesso piano  
                            {
                                if (tc.currentFloor == quadrantData.currentFloor) //WORKPLACE CONTACT e ovviamente nello stesso piano z 
                                {
                                    Debug.Log($"workplace transmission at {tc.currentFloor}th floor");
                                    if (quadrantData.symptomatic)
                                    {
                                        ic.contagionCounter += (2f * 0.15f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //PARAMETRO IMMUNITY -> (0.01f - 0.99f)

                                    }

                                    else
                                    {
                                        ic.contagionCounter += (2f * 0.02f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //AIC HOUSEHOLD TRANSMISSION 5%                                    
                                    }
                                }

                            }
                            //SE NON SONO A CASA e siamo sullo stesso piano
                            else if (quadrantData.symptomatic && tc.currentFloor == quadrantData.currentFloor) //caso sintomatico
                            {
                                ic.contagionCounter += (2f * 0.1f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //SIC NORMAL CONTACT TRANSMISSION -> (0.8% - 15.4%) SELECTED VALUE: 15.4%
                            }
                            else if (!quadrantData.symptomatic && tc.currentFloor == quadrantData.currentFloor)
                            {
                                ic.contagionCounter += (2f * 0.012f) * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //AIC NORMAL CONTACT TRANSMISSION -> (0% - 2.2%) SELECTED VALUE: 2.2%
                            }
                        }
                        //TODO we could add a cap here to speed up the check
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }

                else
                {
                    //no infected in same cell -> human is safe
                    ic.contagionCounter = 0f;
                }
            }
            //infection happened
            if (ic.contagionCounter >= (contagionThreshold) && ic.status == Status.susceptible)
            {
                //human become infected
                qe.typeEnum = QuadrantEntity.TypeEnum.exposed;
                ic.status = Status.exposed;
                ic.oldstatus = Status.susceptible;
                ic.exposedCounter = 0;
            }

            //Infectious status -> symptoms vs non-symptoms
            if (ic.exposedCounter > ic.exposedThreshold && ic.status == Status.exposed)
            {
                qe.typeEnum = QuadrantEntity.TypeEnum.infectious;
                ic.status = Status.infectious;
                ic.oldstatus = Status.exposed;
                ic.infected = true;

                //var random = randomArray[nativeThreadIndex];

                if (ic.myRndValue < 0)//questo per avere un random value ogni volta che si diventa susceptible
                {
                    var random = randomArray[nativeThreadIndex];
                    ic.myRndValue = random.NextFloat(0f, 100f); //random number for symptoms and dead
                    randomArray[nativeThreadIndex] = random;
                }

                //randomArray[nativeThreadIndex] = random;
               // Debug.Log($"random symptoms{ic.myRndValue} of {entity.Index}");
                if ( ic.myRndValue > (100f - (ic.currentHumanSymptomsProbability - (ic.currentImmunityLevel*ic.currentHumanSymptomsProbability)))) //VARIABILE RND PER AGGIUNGERE UNA CASUALITA' ALLA PROBABILITA' SVILUPPO SINTOMI MA COMUNQUE RELATIVA ALL'ETA'
                {
                    //Debug.Log($"sintomatico index {entity.Index}");
                    ic.symptomatic = true;
                    ic.infectiousCounter = 0;
                    if (ic.myRndValue > (100f - (ic.currentHumanDeathProbability - (ic.currentImmunityLevel * ic.currentHumanDeathProbability))))//IMPORTANTE! I PARAMETRI DI PERCENTUALE MORTE SONO SIMILI A QUELLI DI CRITICAL DISEASE DELL'ARTICOLO
                    {
                        
                        if (curTotIntensive <= startIntensives && curTotIntensive > 0 )
                        {
                            ic.intensiveCare = true;
                            unsafe
                            {
                                Interlocked.Decrement(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                            }
                            
                        }
                        else
                            ic.criticalDisease = true;
                    }
                        
                }
                else
                {
                    //asymptomatic
                    ic.symptomatic = false;
                    ic.infectiousCounter = 0;
                }
                
            }

            if (ic.infectiousCounter > ic.infectiousThreshold && ic.status == Status.infectious)
            {
               
                Debug.Log($"random dead{ic.myRndValue} with current computed: {ic.currentHumanDeathProbability - (ic.currentImmunityLevel * ic.currentHumanDeathProbability)}");
                if (ic.myRndValue > (100f - (ic.currentHumanDeathProbability - (ic.currentImmunityLevel * ic.currentHumanDeathProbability))) && ic.symptomatic)
                {
                    //remove entity
                    ic.status = Status.removed;
                    ic.oldstatus = Status.infectious;
                    ic.infected = false;
                    ic.criticalDisease = false;
                    //ic.intensiveCare = false;
                    qe.typeEnum = QuadrantEntity.TypeEnum.removed;
                    if (ic.intensiveCare)
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                        }
                    }
                }
                else
                {
                    ic.status = Status.recovered;
                    ic.oldstatus = Status.infectious;
                    ic.infected = false;
                    ic.criticalDisease = false;
                    // ic.intensiveCare = false;
                    qe.typeEnum = QuadrantEntity.TypeEnum.recovered;
                    ic.recoveredCounter = 0;
                    ic.currentImmunityLevel = 0.8f; //ACCORDING TO DENMARK STUDY
                    humanComponent.immunityTime = 0f;
                    if(humanComponent.PROvax && vaccinationPolicy)
                    {
                        humanComponent.need4vax = 0f;
                    if (humanComponent.vaccinations < 1) //CASO PARTICOLARE: SE UN PROVAX VIENE CONTAGIATO PRIMA DI FARE LA PRIMA DOSE, IL FIRST DOSE TIME VIENE SETTATO DOPO 5 MESI DAL RECUPERO
                        humanComponent.firstDoseTime = 150 * 25 * 60;
                    }

                    if (ic.intensiveCare)
                    {
                        unsafe
                        {
                            Interlocked.Increment(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]);
                        }
                    }

                }

            }
            //recoveredThreshold deve essere + basse per i giovani che stanno meno a recuperare
            if (ic.recoveredCounter > ic.recoveredThreshold && ic.status == Status.recovered)
            {
                ic.status = Status.susceptible;
                ic.oldstatus = Status.recovered;
                qe.typeEnum = QuadrantEntity.TypeEnum.susceptible;
               
            }

            if (ic.status == Status.exposed)
            {
                ic.exposedCounter += 1f * deltaTime;

                //DA PROVARE CODICE COMMENTATO SOTTO, CHIEDERE AL PROF
                //if (ic.currentHumanSymptomsProbability > ic.firstHumanSymptomsProbability)
                //    ic.currentHumanSymptomsProbability -= ic.currentImmunityLevel * ic.currentHumanSymptomsProbability; //immunità rallenta il contagio e previene dall'essere sintomatico
                //else
                //    ic.currentHumanSymptomsProbability = ic.firstHumanSymptomsProbability + 0.01f;
            }
            if (ic.status == Status.infectious)
            {
                ic.infectiousCounter += 1f * deltaTime;
                if (ic.intensiveCare)
                {
                    //SE SEI IN TERAPIA INTENSIVA LE TUE PROBABILITA' DI MORIRE DIMINUISCONO, SEMPRE IN BASE ALL'ETA'

                    if (ic.currentHumanDeathProbability > ic.firstHumanDeathProbability)
                        ic.currentHumanDeathProbability -= (0.1f / (100f * (int)humanComponent.age)) * deltaTime;
                    else
                        ic.currentHumanDeathProbability = ic.firstHumanDeathProbability + 0.01f;
                }
                else if (ic.criticalDisease)
                {
                    if(ic.symptomatic && ic.currentHumanDeathProbability < 99.9f) //SE NON SI è IN TERAPIA INTENSIVA E SI HANNO SINTOMI, LE PROBABILITà DI MORIRE AUMENTANO LEGGERMENTE
                        ic.currentHumanDeathProbability += (0.1f* (int)humanComponent.age / 100f ) * deltaTime;
                }
            }
            if (ic.status == Status.recovered)
            {
                 ic.recoveredCounter += 1f * deltaTime;              
            }

           

        }).WithReadOnly(quadrantMultiHashMap).WithName("jobHandle").ScheduleParallel(Dependency);

        // m_EndSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
         this.Dependency = jobHandle;

        jobHandle.Complete();

        unsafe
        {
            Interlocked.Add(ref currentTotIntensive, Interlocked.Read(ref ((long*)localIntensiveCounter.GetUnsafePtr())[0]));
        }

        localIntensiveCounter.Dispose();
    }

}


