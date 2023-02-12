using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(ContagionSystem))]
[UpdateAfter(typeof(PathFollowSystem))]
public class GetNeedPathSystem : SystemBase
{

    private float CellSize;
    private int Width;
    private int Height;
    [ReadOnly]
    public float eatingOutProb;
    [ReadOnly]
    public float visitFriendProbNL;
    [ReadOnly]
    public float visitFriendProbL;
    //  private NativeArray<int> Grid;

    //  private NativeArray<int2> directions;
    // private NativeArray<int2> start_offset;
    //private Unity.Mathematics.Random rnd;

    private EndSimulationEntityCommandBufferSystem ecbSystem;

    public NativeMultiHashMap<int, Vector3Int> places;
    public NativeArray<Vector3Int> housesToVisit;
    public float sectionSize;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }

    protected override void OnStartRunning()
    {
        eatingOutProb = Human.conf.eatingOutProb / 100f;
        visitFriendProbNL = Human.conf.visitFriendProbNL / 100f; //in case of NO lockdown
        visitFriendProbL = Human.conf.visitFriendProbL / 100f; //in case of NO lockdown
        CellSize = Testing.Instance.grid.GetCellSize();
        Width = Testing.Instance.grid.GetWidth();
        Height = Testing.Instance.grid.GetHeight();
        //Grid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTiles(); });
        //  directions = new NativeArray<int2>(4, Allocator.Persistent);
        //  directions.CopyFrom(new int2[] { new int2(1, 0), new int2(0, -1), new int2(-1, 0), new int2(0, 1) });
        // start_offset = new NativeArray<int2>(4, Allocator.Persistent);
        // start_offset.CopyFrom(new int2[] { new int2(-1, 1), new int2(1, 1), new int2(1, -1), new int2(-1, -1) });
        //rnd = new Unity.Mathematics.Random((uint)random.NextInt(1, 420));
        housesToVisit = Human.housesToVisit;
        places = Human.places;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        var cellSize = this.CellSize;
        var height = this.Height;
        var width = this.Width;

        // var rnd = this.rnd;
        //  var start_offset = this.start_offset;
        // var directions = this.directions;
        var placesToSatisfy = places;
        var houses = housesToVisit;
        var lockdown = Human.conf.lockdown;
        var lockGym = Human.conf.lockGym;
        var lockSchool = Human.conf.lockSchool;
        var lockPubs = Human.conf.lockPubs;
        var vaccinationPolicy = Human.conf.vaccinationPolicy;
        var sectionsize = sectionSize;
        var eatingOutP = eatingOutProb;
        var visitFriendPNL = visitFriendProbNL;
        var visitFriendPL = visitFriendProbL;
        //  var large = Human.Instance.large;
        var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

        //Note the use of the keywords ref and in on the parameters of the ForEach lambda function.
        //Use ref for components that you write to, and in for components that you only read.
        //Marking components as read - only helps the job scheduler execute your jobs more efficiently.

        JobHandle jobHandle = Entities.WithNativeDisableParallelForRestriction(randomArray).WithNativeDisableContainerSafetyRestriction(placesToSatisfy).WithNativeDisableContainerSafetyRestriction(houses).
            ForEach((Entity entity, int nativeThreadIndex, ref NeedPathParams needPathParams, ref HumanComponent humanComponent, ref TileComponent tileComponent, in Translation translation, in NeedComponent needComponent) =>
            {

                GetXY(translation.Value, Vector3.zero, cellSize, out int startX, out int startY);

                var random = randomArray[nativeThreadIndex];
                // var rnd = random.NextFloat(0f, 100f);


                //FIXME validation removed!

                //int pos = FindTarget(startX, startY, hc.status, range, grid, width, height);

                int endX = -1, endY = -1;

                int i = 0;
                Vector3Int pos = new Vector3Int();
                NativeArray<Vector3Int> result;
                NativeMultiHashMap<int, Vector3Int>.Enumerator e;
                switch (needComponent.currentNeed)
                {
                    case NeedType.needForFood:
                        //NextDouble() returns a double-precision floating point number which is greater than or equal to 0.0, and less than 1.0.
                        if (random.NextDouble() < eatingOutP && !lockdown && !lockPubs)//probability to go to the pub for eating 
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(0), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(0);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Pub;
                            tileComponent.currentFloor = pos.z;

                        }
                        else
                        {
                            // Go home to eat most of the times

                            endX = humanComponent.homePosition.x;
                            endY = humanComponent.homePosition.y;

                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                            tileComponent.currentFloor = humanComponent.homePosition.z;

                        }
                        break;

                    case NeedType.needForGrocery:
                        result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(2), Allocator.Temp);
                        i = 0;
                        e = placesToSatisfy.GetValuesForKey(2);
                        while (e.MoveNext())
                        {
                            result[i++] = e.Current;
                        }
                        e.Dispose();

                        pos = result[random.NextInt(0, result.Length)];
                        endX = pos.x;
                        endY = pos.y;
                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Supermarket;
                        tileComponent.currentFloor = pos.z;
                        break;

                    case NeedType.needForSociality:
                        if ((random.NextDouble() < visitFriendPNL && !lockdown) || (random.NextDouble() < visitFriendPL * (1 - humanComponent.socialResposibility) && lockdown))
                        {
                            // result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);


                            Vector3Int friendHouse = new Vector3Int();
                            friendHouse = houses[random.NextInt(0, houses.Length)];
                            //} while (houses[friendIndex].x == humanComponent.homePosition.x && houses[friendIndex].y == humanComponent.homePosition.y && houses[friendIndex].z == humanComponent.homePosition.z); //cerco una casa di una persona fin quando non è vicino casa mia
                            endX = friendHouse.x;
                            endY = friendHouse.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                            tileComponent.currentFloor = friendHouse.z;

                        }
                        else if (!lockdown && !lockPubs)
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(0), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(0);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Pub;
                            tileComponent.currentFloor = pos.z;

                        }
                        else
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(1), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(1);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;

                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Park;
                            tileComponent.currentFloor = pos.z;

                        }

                        break;

                    case NeedType.needForSport:

                        if (!lockdown && !lockGym)
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(4), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(4);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Gym;
                            tileComponent.currentFloor = pos.z;
                        }
                        else
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(1), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(1);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;

                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Park;
                            tileComponent.currentFloor = pos.z;
                        }

                        break;

                    case NeedType.needToRest:

                        endX = humanComponent.homePosition.x;
                        endY = humanComponent.homePosition.y;
                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                        tileComponent.currentFloor = humanComponent.homePosition.z;
                        break;

                    case NeedType.needToWork:

                        if ((humanComponent.age == HumanStatusEnum.HumanStatus.Worker && humanComponent.jobEssentiality >= 0.5f) ||
                        (humanComponent.age == HumanStatusEnum.HumanStatus.Student && !lockSchool && !lockdown))
                        {

                            endX = humanComponent.officePosition.x;
                            endY = humanComponent.officePosition.y;
                            if (humanComponent.age == HumanStatusEnum.HumanStatus.Worker)
                            {
                                tileComponent.currentTile = TileMapEnum.TileMapSprite.Office;
                                tileComponent.currentFloor = humanComponent.officePosition.z;
                            }
                            else if (humanComponent.age == HumanStatusEnum.HumanStatus.Student)
                            {

                                tileComponent.currentTile = TileMapEnum.TileMapSprite.School;
                                tileComponent.currentFloor = humanComponent.officePosition.z; ;
                            }

                        }
                        else //distance learning for students and smart working for workers
                        {

                            endX = humanComponent.homePosition.x;
                            endY = humanComponent.homePosition.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                            tileComponent.currentFloor = humanComponent.homePosition.z;
                        }
                        break;

                    case NeedType.needForVax:
                        if (vaccinationPolicy)
                        {
                            result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(3), Allocator.Temp);

                            e = placesToSatisfy.GetValuesForKey(3);
                            while (e.MoveNext())
                            {
                                result[i++] = e.Current;
                            }
                            e.Dispose();

                            pos = result[random.NextInt(0, result.Length)];
                            endX = pos.x;
                            endY = pos.y;
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Hospital;
                            tileComponent.currentFloor = pos.z;
                        }
                        break;

                    case NeedType.needToHeal:
                        result = new NativeArray<Vector3Int>(placesToSatisfy.CountValuesForKey(3), Allocator.Temp);

                        e = placesToSatisfy.GetValuesForKey(3);
                        while (e.MoveNext())
                        {
                            result[i++] = e.Current;
                        }
                        e.Dispose();

                        pos = result[random.NextInt(0, result.Length)];
                        endX = pos.x;
                        endY = pos.y;
                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Hospital;
                        tileComponent.currentFloor = pos.z;

                        break;

                }

                /*for (int l = 0; l < result.Length && !found; l++)
                {
                    string temp = grid[startX + startY * width].ToString("X"); //cerco nei piani superiori
                    for(int k = 0; k < temp.Length && !found; k++)
                    {
                        TileMapEnum.TileMapSprite tile = (TileMapEnum.TileMapSprite)int.Parse(temp[k].ToString(), System.Globalization.NumberStyles.HexNumber);
                        if (result[l] == tile)
                        {
                            endX = startX;
                            endY = startY;
                            found = true;
                            tileComponent.currentTile = tile;
                            tileComponent.currentFloor = k;
                        }

                    }
                }*/

                //TODO this could esplode... keep an eye on this
                /* for (int range = 1; !found; range++)
                 {
                     //random number selection
                     int starting_edge = random.NextInt(0, 4);
                     int pos_step = random.NextInt(0, range * 2 + 1);
                     i = startX + start_offset[starting_edge].x * range + directions[starting_edge].x * pos_step;
                     j = startY + start_offset[starting_edge].y * range + directions[starting_edge].y * pos_step;
                     for (int turns = 0, tot_step = 0; turns < 5 && tot_step < 8 * range; turns++)
                     {
                         //TODO random starting step?
                         var edge = (turns + starting_edge) % 4;
                         for (int steps = pos_step; steps < range * 2 + 1 - pos_step && !found; steps++)
                         {
                             i += directions[edge].x;
                             j += directions[edge].y;
                             if (i >= 0 && i < width && j >= 0 && j < height)
                                 for (int l = 0; l < result.Length && !found; l++)
                                 {
                                     string temp = grid[i + j * width].ToString("X"); //cerco nei piani superiori
                                     for (int k = 0; k < temp.Length && !found; k++)
                                     {
                                         TileMapEnum.TileMapSprite tile = (TileMapEnum.TileMapSprite)int.Parse(temp[k].ToString(), System.Globalization.NumberStyles.HexNumber);
                                         if (result[l] == tile)
                                         {
                                             endX = i;
                                             endY = j;
                                             found = true;
                                             tileComponent.currentTile = tile;
                                             tileComponent.currentFloor = k;
                                         }

                                     }

                                 }
                         }
                         pos_step = 0;
                     }
                 }*/


                ecb.RemoveComponent<NeedPathParams>(nativeThreadIndex, entity);

                if (startX != endX && startY != endY)
                {

                    ecb.AddComponent<PathfindingParams>(nativeThreadIndex, entity, new PathfindingParams
                    {
                        startPosition = new int2(startX, startY),
                        endPosition = new int2(endX, endY)
                    });
                }

            }).WithReadOnly(houses).ScheduleParallel(Dependency);

        jobHandle.Complete();

        ecbSystem.AddJobHandleForProducer(jobHandle);
    }

    protected override void OnStopRunning()
    {
        //Grid.Dispose();
        //directions.Dispose();
        //start_offset.Dispose();

    }
    public static int GetPositionHashMapKey(int x, int y, float size)
    {
        return (int)(math.floor(x / size) + (1000 * math.floor(y / size)));
    }
    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}