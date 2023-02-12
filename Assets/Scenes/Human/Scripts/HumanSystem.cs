using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(ContagionSystem))]
//HUMAN SYSTEM DA PARAMETRIZZARE TUTTO 

public class HumanSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    // [ReadOnly]
    //private NativeArray<TileMapEnum.TileMapSprite> Grid;
    [ReadOnly]
    private float CellSize;
    [ReadOnly]
    private int Width;
    [ReadOnly]
    public float protVax;
    [ReadOnly]
    public float protImmun;

    public int daysOfImmunity;
    public int daysBTWdoses;
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

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        // Grid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTileType()[0]; });
        CellSize = Testing.Instance.grid.GetCellSize();
        Width = Testing.Instance.grid.GetWidth();
        protVax = Human.conf.protectionVaccinated / 100f;
        protImmun = Human.conf.protectionAfterImmunity / 100f;
        daysBTWdoses = Human.conf.daysBTWdoses;
        if (daysBTWdoses <= 0)
            daysBTWdoses = 1;
        daysOfImmunity = Human.conf.daysOfImmunity;
        if (daysOfImmunity > Human.conf.daysBTWdoses)
            daysOfImmunity = daysBTWdoses - 1;
        hungerOnset = Human.conf.hungerOnset;
        hungerDuration = Human.conf.hungerDuration;
        restDuration = Human.conf.restDuration;
        sociabilityOnset = Human.conf.sociabilityOnset;
        sociabilityDuration = Human.conf.sociabilityDuration;
        sportmanshipOnset = Human.conf.sportmanshipOnset;
        sportmanshipDuration = Human.conf.sportmanshipDuration;
        groceryOnset = Human.conf.groceryOnset;
        groceryDuration = Human.conf.groceryDuration;
        workDuration = Human.conf.workDuration;
    }

    //Handles increment and decrement of parameters of HumanComponent   
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;
        var width = Width;
        var cellSize = CellSize;
        //var grid = Grid;

        var lockdown = Human.conf.lockdown;
        var vaccinationPolicy = Human.conf.vaccinationPolicy;
        var totalIntensive = Human.Instance.totalIntensiveCare;
        var lockGym = Human.conf.lockGym;
        var lockSchool = Human.conf.lockSchool;
        var protectionVaccinated = protVax;
        var daysBTWdose = daysBTWdoses;
        var protectionImmun = protImmun;
        var immunityTime = daysOfImmunity;
        var hungerOn = hungerOnset;
        var hungerDur = hungerDuration;
        var restDur = restDuration;
        var sociabilityOn = sociabilityOnset;
        var sociabilityDur = sociabilityDuration;
        var sportmanshipOn = sportmanshipOnset;
        var sportmanshipDur = sportmanshipDuration;
        var groceryOn = groceryOnset;
        var groceryDur = groceryDuration;
        var workDur = workDuration;

        //----------------------------------INCREASING NEEDS----------------------------------------
        JobHandle jobhandle = Entities.ForEach((ref HumanComponent hc, in InfectionComponent ic) =>
        {


            if (ic.symptomatic && hc.socialResposibility > 0.5f)
            {
                //QUARANTENA
                hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, (25f - restDur) * 60);
            }
            else
            {
                //increment of 1 value per second(that is a minute in game) for each HumanComponent parameters
                hc.hunger = math.min(hc.hunger + 1f * deltaTime, hungerOn * 60f);

                if (hc.age != HumanStatusEnum.HumanStatus.Retired)
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, (25f - restDur) * 60f);
                else
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, (25f - restDur - 3f) * 60);

                if (hc.age == HumanStatusEnum.HumanStatus.Worker)
                    hc.work = math.min(hc.work + 1f * deltaTime, (25f - workDur) * 60);
                else if (hc.age == HumanStatusEnum.HumanStatus.Student)
                    hc.work = math.min(hc.work + 1f * deltaTime, (25f - workDur + 3f) * 60);

                if (ic.currentImmunityLevel > 0.01f)
                    hc.immunityTime = math.min(hc.immunityTime + 1f * deltaTime, immunityTime * 25 * 60);

                //-------NEEDS INFLUENCED BY LOCKDOWN-----------
                if (!lockdown)
                {
                    hc.sociality = math.min(hc.sociality + 1f * deltaTime, sociabilityOn * 60);
                    hc.sportivity = math.min(hc.sportivity + 1f * deltaTime, sportmanshipOn * 60);
                    hc.grocery = math.min(hc.grocery + 1f * deltaTime, groceryOn * 60);
                }
                else//needs influenced by lockdown
                {
                    hc.grocery = math.min(hc.grocery + (1 - hc.socialResposibility) * 0.3f * deltaTime, groceryOn * 60);
                    hc.sociality = math.min(hc.sociality + (1 - hc.socialResposibility) * 0.3f * deltaTime, sociabilityOn * 60);
                    hc.sportivity = math.min(hc.sportivity + (1 - hc.socialResposibility) * 0.3f * deltaTime, sportmanshipOn * 60);
                }

                //------INCREASING VAX NEED--------------
                if (hc.PROvax && vaccinationPolicy)
                {
                    if (hc.vaccinations == 0 && ic.status == Status.susceptible && ic.currentImmunityLevel < protectionImmun + 1f)
                        hc.need4vax = math.min(hc.need4vax + 1f * deltaTime, hc.firstDoseTime + 60f);
                    else if (hc.vaccinations > 0 && ic.status == Status.susceptible && ic.currentImmunityLevel < protectionImmun + 1f)
                        hc.need4vax = math.min(hc.need4vax + 1f * deltaTime, (daysBTWdose * 25 * 60) - hc.immunityTime); //next doses after "daysBTWdoses" considering the immunity time                     
                }
            }
        }).ScheduleParallel(Dependency);
        jobhandle.Complete();

        //-------------------------------NEEDS ASSIGNMENT---------------------------
        //cycle all the entities without a NeedComponent and assign it according to parameters
        JobHandle jobhandle1 = Entities.WithNone<NeedComponent>().ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent hc, ref InfectionComponent ic) =>
        {

            if (hc.PROvax && vaccinationPolicy && !ic.intensiveCare && ic.status != Status.recovered)
            {

                if (hc.vaccinations == 0 && hc.need4vax > hc.firstDoseTime - hc.immunityTime && ic.currentImmunityLevel < protectionImmun + 1f)
                {
                    ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                    {
                        currentNeed = NeedType.needForVax
                    });
                    ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                    {
                        searchRadius = 2
                    });
                    hc.vaccinations++;
                    ic.currentImmunityLevel = protectionVaccinated;
                    hc.immunityTime = 0f;
                }
                else if (hc.vaccinations > 0 && hc.need4vax > (daysBTWdose * 23 * 60) - hc.immunityTime && ic.currentImmunityLevel < protectionImmun + 1f)
                {
                    ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                    {
                        currentNeed = NeedType.needForVax
                    });
                    ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                    {
                        searchRadius = 2
                    });
                    hc.vaccinations++;
                    ic.currentImmunityLevel = protectionVaccinated;
                    hc.immunityTime = 0f;
                }

            }
            if (ic.symptomatic && ic.infected && ic.intensiveCare && ic.status == Status.infectious)
            {

                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToHeal
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            //set searchRadius for retrieving areas in the map included in that radius if the need is over a certain threshold
            else if (hc.hunger > 60 * (hungerOn - 1f))
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needForFood
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.fatigue > (24f - restDur) * 60 && hc.age != HumanStatusEnum.HumanStatus.Retired)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToRest
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.fatigue > (24f - (restDur + 3f)) * 60 && hc.age == HumanStatusEnum.HumanStatus.Retired)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToRest
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.work > (24f - workDur) * 60 && hc.age == HumanStatusEnum.HumanStatus.Worker)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToWork
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.work > (24f - (workDur - 3f)) * 60 && hc.age == HumanStatusEnum.HumanStatus.Student)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToWork
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.sportivity > (sportmanshipOn - 1f) * 60)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needForSport
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.sociality > (sociabilityOn - 1f) * 60)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needForSociality
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.grocery > (groceryOn - 1f) * 60)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needForGrocery
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }


            if (hc.immunityTime > immunityTime * 24 * 60 && ic.currentImmunityLevel > protectionImmun)
            {
                ic.currentImmunityLevel = protectionImmun; //protection decades after immunity

            }

        }).ScheduleParallel(jobhandle);

        jobhandle1.Complete();



        //-----------------------DECREASING NEEDS--------------------------
        //manage satisfied needs, when value for a parameter decreases under 25% as threshold 
        JobHandle jobhandle2 = Entities.ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent hc, ref InfectionComponent ic, in Translation t, in NeedComponent needComponent, in TileComponent tileComponent, in PathFollow pathFollow) =>
        {
            //retrieve entity position
            GetXY(t.Value, Vector3.zero, cellSize, out int currentX, out int currentY); //TODO fix hardcoded origin

            //decrement based to position:
            //home -> decrement fatigue
            //park -> decrement sociality and sportivity
            //pub -> decrement hunger and sociality
            //road -> decrement sportivity
            if (pathFollow.pathIndex < 0)
            {

                //process of decrementation based on Location!
                switch (tileComponent.currentTile)
                {
                    case TileMapEnum.TileMapSprite.Home:
                        if (hc.homePosition.x == currentX && hc.homePosition.y == currentY)
                        {
                            if (needComponent.currentNeed == NeedType.needToRest && hc.age != HumanStatusEnum.HumanStatus.Retired)
                                hc.fatigue = Math.Max(0, hc.fatigue - ((24f / restDur) + 1f) * deltaTime);
                            else if (needComponent.currentNeed == NeedType.needToRest && hc.age == HumanStatusEnum.HumanStatus.Retired)
                                hc.fatigue = Math.Max(0, hc.fatigue - ((21f / restDur) + 1f) * deltaTime);//retired people sleep 3h more
                            else if (needComponent.currentNeed == NeedType.needForFood)
                                hc.hunger = Math.Max(0, hc.hunger - ((hungerOn / hungerDur) + 1f) * deltaTime);
                            if (needComponent.currentNeed == NeedType.needToWork)
                            {
                                if ((hc.age == HumanStatusEnum.HumanStatus.Student && lockdown) || (hc.age == HumanStatusEnum.HumanStatus.Student && lockSchool))
                                    hc.work = Math.Max(0, hc.work - ((27f / workDur) + 1f) * deltaTime); //students work 3 hour less
                                if (hc.age == HumanStatusEnum.HumanStatus.Worker && lockdown && hc.jobEssentiality < 0.5f)
                                    hc.work = Math.Max(0, hc.work - ((24f / workDur) + 1f) * deltaTime);
                            }
                        }
                        else
                            hc.sociality = Math.Max(0, hc.sociality - ((sociabilityOn / sociabilityDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Park:
                        if (needComponent.currentNeed == NeedType.needForSport)
                            hc.sportivity = Math.Max(0, hc.sportivity - ((sportmanshipOn / sportmanshipDur) + 1f) * deltaTime);
                        else if (needComponent.currentNeed == NeedType.needForSociality)
                            hc.sociality = Math.Max(0, hc.sociality - ((sociabilityOn / sociabilityDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Pub:
                        if (needComponent.currentNeed == NeedType.needForFood)
                            hc.hunger = Math.Max(0, hc.hunger - ((hungerOn / hungerDur) + 1f) * deltaTime);
                        else if (needComponent.currentNeed == NeedType.needForSociality)
                            hc.sociality = Math.Max(0, hc.sociality - ((sociabilityOn / sociabilityDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Supermarket:
                        hc.grocery = Math.Max(0, hc.grocery - ((groceryOn / groceryDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Office:
                        hc.work = Math.Max(0, hc.work - ((24f / workDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.School:
                        hc.work = Math.Max(0, hc.work - ((27f / workDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Gym:
                        if (needComponent.currentNeed == NeedType.needForSport)
                            hc.sportivity = Math.Max(0, hc.sportivity - ((sportmanshipOn / sportmanshipDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.Hospital:
                        if (needComponent.currentNeed == NeedType.needForVax)
                        {
                            hc.need4vax = Math.Max(0.1f, hc.need4vax - (10 * 24f + 1f) * deltaTime);
                        }
                        break;

                    case TileMapEnum.TileMapSprite.OAhome:
                        if (hc.homePosition.x == currentX && hc.homePosition.y == currentY)
                        {
                            if (needComponent.currentNeed == NeedType.needToRest)
                                hc.fatigue = Math.Max(0, hc.fatigue - ((21f / restDur) + 1f) * deltaTime);
                            else if (needComponent.currentNeed == NeedType.needForFood)
                                hc.hunger = Math.Max(0, hc.hunger - ((hungerOn / hungerDur) + 1f) * deltaTime);

                        }
                        else
                            hc.sociality = Math.Max(0, hc.sociality - ((sociabilityOn / sociabilityDur) + 1f) * deltaTime);
                        break;

                    case TileMapEnum.TileMapSprite.RoadHorizontal:
                    case TileMapEnum.TileMapSprite.RoadVertical:
                    case TileMapEnum.TileMapSprite.RoadCrossing:

                        break;
                }
            }

            //As soon as its current need drops below a minimum threshold the entity is free to satisfy its next need.
            if (needComponent.currentNeed == NeedType.needToHeal && !ic.intensiveCare && !ic.infected)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity); //entity will stay in intensive care until it is not infected anymore

            }

            else if (needComponent.currentNeed == NeedType.needForFood && hc.hunger < 10f * hungerOn * 0.6)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 10f * (25f - restDur) * 0.6 && hc.age != HumanStatusEnum.HumanStatus.Retired)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 10f * (21 - restDur) * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Retired)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needForSport && hc.sportivity < 10f * sportmanshipOn / 2f * 0.6)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needForSociality && hc.sociality < 10f * sociabilityOn / 2f * 0.6)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needForGrocery && hc.grocery < 10f * groceryOn * 0.6)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needToWork && hc.work < 10f * (24f - workDur) * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Worker)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }
            else if (needComponent.currentNeed == NeedType.needToWork && hc.work < 10f * (27f - workDur) * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Student)
            {
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            }

            if (hc.PROvax && vaccinationPolicy)
            {
                if (hc.vaccinations == 1 && needComponent.currentNeed == NeedType.needForVax && hc.need4vax < 10f * hc.firstDoseTime * 0.01f)
                {
                    ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

                }
                else if (hc.vaccinations > 1 && needComponent.currentNeed == NeedType.needForVax && hc.need4vax < 10f * daysBTWdose * 24 * 0.6)
                {
                    ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

                }
            }

            if (ic.currentImmunityLevel < protectionImmun + 1f)
            {
                ic.currentImmunityLevel = Math.Max(0.01f, ic.currentImmunityLevel - (1f - hc.socialResposibility) * deltaTime); //protection is reduced in time
            }


        }).ScheduleParallel(jobhandle1);

        jobhandle2.Complete();

        ecbSystem.AddJobHandleForProducer(jobhandle2);


    }


    protected override void OnStopRunning()
    {
        // Grid.Dispose();
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}