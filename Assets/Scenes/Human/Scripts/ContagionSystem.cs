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
    [ReadOnly]
    public float contagionThreshold;
    [ReadOnly]
    public float contagionDistance;
    [ReadOnly]
    public float protectionRecovered;
    [ReadOnly]
    public long startIntensive;
    [ReadOnly]
    public float householdTRs;
    [ReadOnly]
    public float householdTRa;
    [ReadOnly]
    public float workplaceTRs;
    [ReadOnly]
    public float workplaceTRa;
    [ReadOnly]
    public float retirehouseTRs;
    [ReadOnly]
    public float retirehouseTRa;
    [ReadOnly]
    public float outdoorTRs;
    [ReadOnly]   
    public float outdoorTRa;
    [ReadOnly]
    public float indoorTRs;
    [ReadOnly]   
    public float indoorTRa;
    [ReadOnly]
    public float schoolTRs;
    [ReadOnly]   
    public float schoolTRa;
    [ReadOnly]
    public int daysBTWdoses;

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
        householdTRs = Human.conf.householdTRs / 100f;
        householdTRa = Human.conf.householdTRa / 100f;
        workplaceTRs = Human.conf.workplaceTRs / 100f;
        workplaceTRa = Human.conf.workplaceTRa / 100f;
        retirehouseTRs = Human.conf.retirehouseTRs / 100f;
        retirehouseTRa = Human.conf.retirehouseTRa / 100f;
        outdoorTRs = Human.conf.outdoorTRs / 100f;
        outdoorTRa = Human.conf.outdoorTRa / 100f;
        indoorTRa = Human.conf.indoorTRa / 100f;
        indoorTRs = Human.conf.indoorTRs / 100f;
        schoolTRs = Human.conf.schoolTRs / 100f;
        schoolTRa = Human.conf.schoolTRa/100f;
        daysBTWdoses = Human.conf.daysBTWdoses;
        protectionRecovered = Human.conf.protectionRecovered/100f;
        contagionThreshold = Human.conf.exposureTime;
        contagionDistance = Human.conf.contagionDistance;
        startIntensive = Human.Instance.totalIntensiveCare;
        if(!Human.conf.appendLog)
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
        var exposureTime = contagionThreshold;
        var distance = contagionDistance;
        var protectRecov = protectionRecovered;
        //Transmission Rate for each location and type of infected
        var householdTRS = householdTRs;
        var householdTRA = householdTRa;
        var workplaceTRS = workplaceTRs;
        var workplaceTRA = workplaceTRa;
        var retirehouseTRS = retirehouseTRs;
        var retirehouseTRA = retirehouseTRa;
        var outdoorTRS = outdoorTRs;
        var outdoorTRA = outdoorTRa;
        var indoorTRS = indoorTRs;
        var indoorTRA = indoorTRa;
        var schoolTRS = schoolTRs;
        var schoolTRA = schoolTRa;
        var daysBTWdose = daysBTWdoses;

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
                   
                    //cycle through all entries on hashmap with the same Key
                    do
                    {
                        //Debug.Log(quadrantData.position);                       
                        if (math.distance(t.Value, quadrantData.position) < distance)
                        {
                            float3 myHome = new float3(humanComponent.homePosition.x * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.homePosition.y * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.homePosition.z);
                            float3 myOffice = new float3(humanComponent.officePosition.x * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.officePosition.y * quadrantCellSize + quadrantCellSize * 0.5f, humanComponent.officePosition.z);
                            //TODO consider also social resp other human involved
                            //increment infectionCounter if one/more infected are in close positions (infected at quadrantData.position) 
                            //infection counter is determined by time and human responsibility
                            var random = randomArray[nativeThreadIndex];
                            
                            ic.myRndValue = random.NextFloat(0f, 100f); 
                            randomArray[nativeThreadIndex] = random;

                            if (math.distance(t.Value, myHome) < 2f) //quindi siamo nelle vicinanze di casa con i condimini o familiari  
                            {

                                if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile == TileMapEnum.TileMapSprite.Home) 
                                {
                                  
                                    if (quadrantData.symptomatic)
                                    {

                                        ic.contagionCounter += householdTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //IMMUNITY -> (0.01f - 0.99f) 15% SYMPTOMATIC TRANSMISSION RATE

                                    }

                                    else
                                    {
                                        ic.contagionCounter += householdTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); //5%  ASYMPTOMATIC TRANSMISSION RATE                                  
                                    }
                                }
                                else if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile == TileMapEnum.TileMapSprite.OAhome ) 
                                {
                                    
                                    if (quadrantData.symptomatic)
                                    {

                                        ic.contagionCounter += retirehouseTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); 

                                    }

                                    else
                                    {
                                        ic.contagionCounter += retirehouseTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);                                 
                                    }
                                }

                            }

                            else if (math.distance(t.Value, myOffice) < 2f) 
                            {
                                if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile == TileMapEnum.TileMapSprite.Office) //same office at the same floor
                                {
                                    //Debug.Log($"workplace transmission with a probability of {ic.myRndValue}");
                                    if (quadrantData.symptomatic)
                                    {
                                        ic.contagionCounter += workplaceTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); 

                                    }

                                    else
                                    {
                                        ic.contagionCounter += workplaceTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);                                   
                                    }
                                }
                                else if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile == TileMapEnum.TileMapSprite.School) 
                                {
                                    //Debug.Log($"workplace transmission with a probability of {ic.myRndValue}");
                                    if (quadrantData.symptomatic)
                                    {
                                        ic.contagionCounter += schoolTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);

                                    }

                                    else
                                    {
                                        ic.contagionCounter += schoolTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);                                   
                                    }
                                }

                            }
                            //SE NON SONO A CASA e siamo sullo stesso piano
                            else if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile == TileMapEnum.TileMapSprite.Park ) 
                            {
                                if (quadrantData.symptomatic)
                                {
                                    ic.contagionCounter += outdoorTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); 

                                }

                                else
                                {
                                    ic.contagionCounter += outdoorTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);                                   
                                }
                            }
                            else if (tc.currentFloor == quadrantData.currentFloor && tc.currentTile != TileMapEnum.TileMapSprite.Park ) 
                            {
                                if (quadrantData.symptomatic)
                                {
                                    ic.contagionCounter += indoorTRS * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel); 

                                }

                                else
                                {
                                    ic.contagionCounter += indoorTRA * deltaTime * (1 - humanComponent.socialResposibility) * (1f - ic.currentImmunityLevel);                              
                                }
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
            if (ic.contagionCounter >= (exposureTime) && ic.status == Status.susceptible)
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

                             
                var random = randomArray[nativeThreadIndex];
                ic.myRndValue = random.NextFloat(0f, 100f); 
                randomArray[nativeThreadIndex] = random;
            

                //randomArray[nativeThreadIndex] = random;
               
                if ( ic.myRndValue > (100f - (ic.currentHumanSymptomsProbability - (ic.currentImmunityLevel*ic.currentHumanSymptomsProbability)))) //probability computation through random value
                {
                   
                    ic.symptomatic = true;
                    ic.infectiousCounter = 0;
                    if (ic.myRndValue > (100f - (ic.currentHumanDeathProbability - (ic.currentImmunityLevel * ic.currentHumanDeathProbability))))//IMPORTANT! death values are similar to critical disease values
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
                    ic.currentImmunityLevel = protectRecov; 
                    humanComponent.immunityTime = 0f;
                    if(humanComponent.PROvax && vaccinationPolicy)
                    {
                        humanComponent.need4vax = 0f;
                    if (humanComponent.vaccinations < 1) //particular case: SE UN PROVAX VIENE CONTAGIATO PRIMA DI FARE LA PRIMA DOSE, IL FIRST DOSE TIME VIENE SETTATO DOPO 5 MESI DAL RECUPERO
                        humanComponent.firstDoseTime = daysBTWdose * 25 * 60;
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


