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

    [ReadOnly]
    private NativeArray<TileMapEnum.TileMapSprite> Grid;
    [ReadOnly]
    private float CellSize;
    [ReadOnly]
    private int Width;


    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        Grid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTileType(); });
        CellSize = Testing.Instance.grid.GetCellSize();
        Width = Testing.Instance.grid.GetWidth();
    }

    //Handles increment and decrement of parameters of HumanComponent   
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;
        var width = Width;
        var cellSize = CellSize;
        var grid = Grid;

        var lockdown = Human.conf.Lockdown;
        var vaccinationPolicy = Human.conf.VaccinationPolicy;
        var totalIntensive = Human.Instance.totalIntensiveCare;
        var lockGym = Human.conf.lockGym;
        var lockSchool = Human.conf.lockSchool;
        var greenPass = Human.conf.greenPass;

        //----------------------------------INCREMENTO BISOGNI----------------------------------------
        JobHandle jobhandle = Entities.ForEach((ref HumanComponent hc, in InfectionComponent ic) =>
        {
          
                //se ha sintomi ed è responsabile, aumenterà solo il bisogno di riposare e quindi stare a casa
                if (ic.symptomatic && hc.socialResposibility > 0.5f)
                {
                    //QUARANTENA
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60);
                }
                else
                {
                    //increment of 1 value per second(that is a minute in game) for each HumanComponent parameters
                    hc.hunger = math.min(hc.hunger + 1f * deltaTime, 7 * 60); //Human eat for 1 hour three times each day, OGNI 7 ORE SI MANGIA

                    if (hc.age != HumanStatusEnum.HumanStatus.Retired)
                        hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60); //Human student and worker rest for 8 hours, OGNI 17 ORE SI DORME
                    else
                        hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 13 * 60); //Human retired rest for 12 hours, need restored after 12 hours from fulfillment

                    if (hc.age == HumanStatusEnum.HumanStatus.Worker)
                        hc.work = math.min(hc.work + 1f * deltaTime, 17 * 60); //Human worker goes to work for 8 hours every day
                    else if (hc.age == HumanStatusEnum.HumanStatus.Student)
                        hc.work = math.min(hc.work + 1f * deltaTime, 20 * 60); //Human student goes to school for 5 hours every day

                    if (ic.currentImmunityLevel > 0.01f)
                        hc.immunityTime = math.min(hc.immunityTime + 1f * deltaTime, 120 * 25 * 60); //contatore di immunità

                //-------BISOGNI INFLUENZATI DA LOCKDOWN-----------
                if (!lockdown)
                {
                    hc.sociality = math.min(hc.sociality + 1f * deltaTime, 22 * 60); //Human does 3 hours of sociality each day, OGNI 22 ORE SI SOCIALIZZA
                    hc.sportivity = math.min(hc.sportivity + 1f * deltaTime, 2 * 23 * 60); //Human does 1.5 hours of sport every two days
                    hc.grocery = math.min(hc.grocery + 1f * deltaTime, 3 * 25 * 60);//Human goes to the supermarket for one hour once every 3 days                    
                }
                else
                {
                    hc.grocery = math.min(hc.grocery + 0.5f * deltaTime, 3 * 25 * 60);
                    hc.sociality = math.min(hc.sociality + (1 - hc.socialResposibility) * 1f * deltaTime, 23 * 60); //PERCHE' 0.1f*deltTime???
                    hc.sportivity = math.min(hc.sportivity + (1 - hc.socialResposibility) * 1f * deltaTime, 2 * 23 * 60);
                }
                    
                    //------PROCEDIMENTO INCREMENTO CICLO VACCINALE--------------
                    if (hc.PROvax && vaccinationPolicy && ic.status != Status.recovered && ic.currentImmunityLevel < 0.4f)
                    {                      
                        if (hc.vaccinations == 0 && ic.status == Status.susceptible)
                            hc.need4vax = math.min(hc.need4vax + 1f * deltaTime, hc.firstDoseTime + 60f);
                        else if (hc.vaccinations > 0 && ic.status == Status.susceptible)
                            hc.need4vax = math.min(hc.need4vax + 1f * deltaTime, 150 * 25 * 60);                       
                    }
                }           
        }).ScheduleParallel(Dependency);
        jobhandle.Complete();

        //-------------------------------ASSEGNAZIONE BISOGNI---------------------------
        //cycle all the entities without a NeedComponent and assign it according to parameters
        JobHandle jobhandle1 = Entities.WithNone<NeedComponent>().ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent hc, ref InfectionComponent ic) =>
        {
            if(ic.symptomatic && ic.infected && ic.intensiveCare && ic.status == Status.infectious){
                             
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
            else if (hc.hunger > 60 * 6) //non mangia da 6 ore
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
            else if (hc.fatigue > 16 * 60 && hc.age != HumanStatusEnum.HumanStatus.Retired) //non dorme da 16 ore
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
            else if (hc.fatigue > 12 * 60 && hc.age == HumanStatusEnum.HumanStatus.Retired) // il pensionato non dorme da 12 ore
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
            else if (hc.sportivity > 2 * 22 * 60) //non si allena da 44 ore (in due giorni quindi)
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
            else if (hc.sociality > 21 * 60) //non socializza da 21 ore
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
            else if (hc.grocery > 3 * 24 * 60) //non va al supermercato da 3 giorni
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
            else if (hc.work > 16 * 60 && hc.age == HumanStatusEnum.HumanStatus.Worker) //non lavora da 16 ore
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
            else if (hc.work > 19 * 60 && hc.age == HumanStatusEnum.HumanStatus.Student) //non va a scuola lavora da 19 ore
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

            if (hc.PROvax && vaccinationPolicy && !ic.intensiveCare && ic.status != Status.recovered)
            {
                  
                if (hc.vaccinations == 0 && hc.need4vax > hc.firstDoseTime && ic.currentImmunityLevel < 0.4f)
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
                    ic.currentImmunityLevel = 0.9f;

                }
                else if (hc.vaccinations > 0 && hc.need4vax > 150 * 24 * 60 && ic.currentImmunityLevel < 0.4f) //5 mesi
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
                    ic.currentImmunityLevel = 0.9f;
                    hc.immunityTime = 0f;
                }
                
                if(hc.immunityTime > 120 * 24* 60 && ic.currentImmunityLevel > 0.4f)
                {
                    ic.currentImmunityLevel = 0.39f; //dati presi da articolo tgcom protezione vaccini
                    hc.immunityTime = 0f;
                }
            }

        }).ScheduleParallel(jobhandle);

        jobhandle1.Complete();



        //-----------------------DECREMENTO BISOGNI--------------------------
        //manage satisfied needs, when value for a parameter decreases under 25% as threshold 
        JobHandle jobhandle2 = Entities.ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent hc, ref InfectionComponent ic, in Translation t, in NeedComponent needComponent) =>
        {
            //retrieve entity position
            GetXY(t.Value, Vector3.zero, cellSize, out int currentX, out int currentY); //TODO fix hardcoded origin

            //decrement based to position:
            //home -> decrement fatigue
            //park -> decrement sociality and sportivity
            //pub -> decrement hunger and sociality
            //road -> decrement sportivity

            //process of decrementation based on Location!
            switch (grid[currentX + currentY * width])
            {
                case TileMapEnum.TileMapSprite.Home:
                    if (hc.homePosition.x == currentX && hc.homePosition.y == currentY)
                    {
                        if (needComponent.currentNeed == NeedType.needToRest)
                            hc.fatigue = Math.Max(0, hc.fatigue - (2f + 1f) * deltaTime); //fatica viene decrementata di 3 minuti(in game) ogni secondo
                        else if (needComponent.currentNeed == NeedType.needForFood)
                            hc.hunger = Math.Max(0, hc.hunger - (7f + 1f) * deltaTime); //la fame viene decrementata di 7 minuti(in game) ogni secondo
                        if(needComponent.currentNeed == NeedType.needToWork)     
                            hc.work = Math.Max(0, hc.work - (2f + 1f) * deltaTime); //Decremento bisogno di smart working                      
                    }
                    else
                        hc.sociality = Math.Max(0, hc.sociality - (5f + 1f) * deltaTime); //la socialità viene decrementata di 5 minuti(in game) ogni secondo
                    break;
                case TileMapEnum.TileMapSprite.Home2:
                    if (hc.homePosition.x == currentX && hc.homePosition.y == currentY)
                    {
                        if (needComponent.currentNeed == NeedType.needToRest)
                            hc.fatigue = Math.Max(0, hc.fatigue - (2f + 1f) * deltaTime); //fatica viene decrementata di 3 minuti(in game) ogni secondo
                        else if (needComponent.currentNeed == NeedType.needForFood)
                            hc.hunger = Math.Max(0, hc.hunger - (7f + 1f) * deltaTime); //la fame viene decrementata di 7 minuti(in game) ogni secondo

                        if (needComponent.currentNeed == NeedType.needToWork)
                        {
                            if ((hc.age == HumanStatusEnum.HumanStatus.Student && lockdown) || (hc.age == HumanStatusEnum.HumanStatus.Student && lockSchool))
                                hc.work = Math.Max(0, hc.work - (2f + 1f) * deltaTime); //se ce lockdown o lockdown solo per scuole allora lavoro da casa
                            if (hc.age == HumanStatusEnum.HumanStatus.Worker && lockdown && hc.jobEssentiality < 0.5f)
                                hc.work = Math.Max(0, hc.work - (2f + 1f) * deltaTime);
                        }
                    }
                    else
                        hc.sociality = Math.Max(0, hc.sociality - (5f + 1f) * deltaTime); //la socialità viene decrementata di 5 minuti(in game) ogni secondo
                    break;

                case TileMapEnum.TileMapSprite.Park:
                    if (needComponent.currentNeed == NeedType.needForSport)//la sport viene decrementato di 30 minuti ogni secondo
                        hc.sportivity = Math.Max(0, hc.sportivity - (30f + 1f) * deltaTime);
                    else if (needComponent.currentNeed == NeedType.needForSociality)
                        hc.sociality = Math.Max(0, hc.sociality - (15f + 1f) * deltaTime); //la socialità viene decrementata di 15 minuti(in game) ogni secondo al parco
                    break;

                case TileMapEnum.TileMapSprite.Pub:
                    if (needComponent.currentNeed == NeedType.needForFood)
                        hc.hunger = Math.Max(0, hc.hunger - (7f + 1f) * deltaTime);//la fame viene decrementata di 7 minuti(in game) ogni secondo
                    else if (needComponent.currentNeed == NeedType.needForSociality)
                        hc.sociality = Math.Max(0, hc.sociality - (15f + 1f) * deltaTime);//la socialità viene decrementata di 15 minuti(in game) ogni secondo al pub
                    break;

                case TileMapEnum.TileMapSprite.Supermarket:
                    hc.grocery = Math.Max(0, hc.grocery - (3 * 24f + 1f) * deltaTime);//andare al supermercato viene decrementato di 3*24 minuti ogni secondo così
                                                                                      //appena passano 60 secondi (1h in game) il bisogno è stato soddisfatto
                    break;

                case TileMapEnum.TileMapSprite.Office:
                    hc.work = Math.Max(0, hc.work - (2f + 1f) * deltaTime);
                    break;

                case TileMapEnum.TileMapSprite.School:
                    hc.work = Math.Max(0, hc.work - (2f + 1f) * deltaTime);
                    break;

                case TileMapEnum.TileMapSprite.Gym:
                    if (needComponent.currentNeed == NeedType.needForSport)//la sport viene decrementato di 30 minuti ogni secondo
                        hc.sportivity = Math.Max(0, hc.sportivity - (30f + 1f) * deltaTime);
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
                            hc.fatigue = Math.Max(0, hc.fatigue - (2f + 1f) * deltaTime); //fatica viene decrementata di 3 minuti(in game) ogni secondo
                        else if (needComponent.currentNeed == NeedType.needForFood)
                            hc.hunger = Math.Max(0, hc.hunger - (7f + 1f) * deltaTime); //la fame viene decrementata di 7 minuti(in game) ogni secondo
                                       
                    }
                    else
                        hc.sociality = Math.Max(0, hc.sociality - (5f + 1f) * deltaTime); //la socialità viene decrementata di 5 minuti(in game) ogni secondo
                    break;

                case TileMapEnum.TileMapSprite.RoadHorizontal:
                case TileMapEnum.TileMapSprite.RoadVertical:
                case TileMapEnum.TileMapSprite.RoadCrossing:

                    break;
            }
            //IMPLEMENTARE LOGICA % DEI THRESHOLD
            //As soon as its current need drops below a minimum threshold (25% che si può parametrizzare) the entity is free to satisfy its next need.
            if(needComponent.currentNeed == NeedType.needToHeal && !ic.intensiveCare && !ic.infected)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
              
           else if (needComponent.currentNeed == NeedType.needForFood && hc.hunger < 25f * 7 * 0.6) //25% di 7*60
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 25f * 17 * 0.6 && hc.age != HumanStatusEnum.HumanStatus.Retired)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 25f * 13 * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Retired)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSport && hc.sportivity < 25f * 23 * 0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSociality && hc.sociality < 25f * 11 * 0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForGrocery && hc.grocery < 25f * 25 * 3 * 0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToWork && hc.work < 25f * 17 * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Worker)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToWork && hc.work < 25f * 20 * 0.6 && hc.age == HumanStatusEnum.HumanStatus.Student)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

            if (hc.PROvax && vaccinationPolicy)
            {                  
                if (hc.vaccinations == 1 && needComponent.currentNeed == NeedType.needForVax && hc.need4vax < 25f * hc.firstDoseTime * 0.01f)
                {
                    ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

                }
                else if (hc.vaccinations > 1 && needComponent.currentNeed == NeedType.needForVax && hc.need4vax < 25f * 150 * 24 * 0.6)
                {
                    ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);

                }               
            }

            if(ic.currentImmunityLevel < 0.4f)
            {
                ic.currentImmunityLevel = Math.Max(0.01f , ic.currentImmunityLevel - (1f - hc.socialResposibility) * deltaTime); //perdere l'immunità nel tempo
            }


        }).ScheduleParallel(jobhandle1);

        jobhandle2.Complete();

        ecbSystem.AddJobHandleForProducer(jobhandle2);

       
    }

    
    protected override void OnStopRunning()
    {
        Grid.Dispose();
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}