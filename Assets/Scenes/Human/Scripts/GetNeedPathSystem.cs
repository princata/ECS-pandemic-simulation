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
    private NativeArray<int> Grid;

    private NativeArray<int2> directions;
    private NativeArray<int2> start_offset;
    //private Unity.Mathematics.Random rnd;

    private EndSimulationEntityCommandBufferSystem ecbSystem;
    private NativeArray<Vector3Int> myHouses;
    public NativeMultiHashMap<int, TileInfo> placesHashMap;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
       
    }

    protected override void OnStartRunning()
    {
        CellSize = Testing.Instance.grid.GetCellSize();
        Width = Testing.Instance.grid.GetWidth();
        Height = Testing.Instance.grid.GetHeight();
        Grid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTiles(); });
        directions = new NativeArray<int2>(4, Allocator.Persistent);
        directions.CopyFrom(new int2[] { new int2(1, 0), new int2(0, -1), new int2(-1, 0), new int2(0, 1) });
        start_offset = new NativeArray<int2>(4, Allocator.Persistent);
        start_offset.CopyFrom(new int2[] { new int2(-1, 1), new int2(1, 1), new int2(1, -1), new int2(-1, -1) });
       // rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 420));
        myHouses = Human.Instance.houses;
        placesHashMap = Human.places;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        var cellSize = this.CellSize;
        var height = this.Height;
        var width = this.Width;
        var grid = this.Grid;
       // var rnd = this.rnd;
        var start_offset = this.start_offset;
        var directions = this.directions;
        
        var placesHM = placesHashMap;
        var houses = myHouses;
        var lockdown = Human.conf.Lockdown;
        var lockGym = Human.conf.lockGym;
        var lockSchool = Human.conf.lockSchool;
        var lockPubs = Human.conf.lockPubs;
        var vaccinationPolicy = Human.conf.VaccinationPolicy;

        var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

        //Note the use of the keywords ref and in on the parameters of the ForEach lambda function.
        //Use ref for components that you write to, and in for components that you only read.
        //Marking components as read - only helps the job scheduler execute your jobs more efficiently.

        JobHandle jobHandle = Entities.WithNativeDisableParallelForRestriction(randomArray).WithNativeDisableContainerSafetyRestriction(placesHM).
            ForEach((Entity entity, int nativeThreadIndex, ref NeedPathParams needPathParams, ref HumanComponent humanComponent, ref TileComponent tileComponent, in Translation translation, in NeedComponent needComponent) =>
        {

            GetXY(translation.Value, Vector3.zero, cellSize, out int startX, out int startY);

            var random = randomArray[nativeThreadIndex];
            // var rnd = random.NextFloat(0f, 100f);
            

            //FIXME validation removed!

            //int pos = FindTarget(startX, startY, hc.status, range, grid, width, height);

            int i, j, endX = -1, endY = -1;
            bool found = false;

            NativeArray<TileMapEnum.TileMapSprite> result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
            switch (needComponent.currentNeed)
            {
                case NeedType.needForFood:
                    //NextDouble() returns a double-precision floating point number which is greater than or equal to 0.0, and less than 1.0.
                    if (random.NextDouble() < 0.30 && !lockdown && !lockPubs)
                    {
                        result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
                        result[0] = TileMapEnum.TileMapSprite.Pub;

                        //tileComponent.currentTile = TileMapEnum.TileMapSprite.Pub;
                        //tileComponent.currentFloor = 0;
                        
                    }
                    else
                    {
                        // Go home to eat most of the times
                        result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
                        found = true;
                        endX = humanComponent.homePosition.x;
                        endY = humanComponent.homePosition.y;

                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                        tileComponent.currentFloor = humanComponent.homePosition.z;
                       
                    }
                    break;
                case NeedType.needForGrocery:
                    result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
                    result[0] = TileMapEnum.TileMapSprite.Supermarket;
                    //tileComponent.currentTile = TileMapEnum.TileMapSprite.Supermarket;
                    //tileComponent.currentFloor = 0;
                    break;
                case NeedType.needForSociality:
                    if ((random.NextDouble() < 0.20 && !lockdown) || (random.NextDouble() < 0.5 * (1 - humanComponent.socialResposibility) && lockdown))
                    {
                        result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
                        found = true;
                        int friendIndex;
                        do
                        {
                            friendIndex = random.NextInt(0, houses.Length);
                        } while (houses[friendIndex].x == humanComponent.homePosition.x && houses[friendIndex].y == humanComponent.homePosition.y && houses[friendIndex].z == humanComponent.homePosition.z); //cerco una casa di una persona fin quando non è vicino casa mia
                        endX = houses[friendIndex].x;
                        endY = houses[friendIndex].y;
                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                        tileComponent.currentFloor = houses[friendIndex].z;
                    }
                    else if (!lockdown && !lockPubs)
                    {
                        result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
                        result[0] = TileMapEnum.TileMapSprite.Pub;
                        result[1] = TileMapEnum.TileMapSprite.Park;
                        //tileComponent.currentTile = TileMapEnum.TileMapSprite.Pub;
                        //tileComponent.currentFloor = 0;

                    }
                    else 
                    {
                        result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
                        result[0] = TileMapEnum.TileMapSprite.Park;
                        //tileComponent.currentTile = TileMapEnum.TileMapSprite.Park;
                        //tileComponent.currentFloor = 0;

                    }

                    break;
                case NeedType.needForSport:
                    result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
                    if (!lockdown && !lockGym)
                    {
                        result[0] = TileMapEnum.TileMapSprite.Gym;
                        result[1] = TileMapEnum.TileMapSprite.Park;
                    }
                    else
                        result[0] = TileMapEnum.TileMapSprite.Park;

                    //tileComponent.currentTile = TileMapEnum.TileMapSprite.Park; //anche se in realtà è gym
                    //tileComponent.currentFloor = 0;

                    break;
                case NeedType.needToRest:
                    result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
                    found = true;
                    endX = humanComponent.homePosition.x;
                    endY = humanComponent.homePosition.y;
                    tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                    tileComponent.currentFloor = humanComponent.homePosition.z;
                    break;
                case NeedType.needToWork:
                    result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
                    found = true;
                    if((humanComponent.age == HumanStatusEnum.HumanStatus.Worker && humanComponent.jobEssentiality > 0.5f) || 
                    (humanComponent.age == HumanStatusEnum.HumanStatus.Student && !lockSchool && !lockdown) )
                    {
                        endX = humanComponent.officePosition.x;
                        endY = humanComponent.officePosition.y;
                        if(humanComponent.age == HumanStatusEnum.HumanStatus.Worker)
                        {
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.Office;
                            tileComponent.currentFloor = humanComponent.officePosition.z;
                        }
                        else if(humanComponent.age == HumanStatusEnum.HumanStatus.Student)
                        {
                            tileComponent.currentTile = TileMapEnum.TileMapSprite.School;
                            tileComponent.currentFloor = 0;
                        }

                    }
                    else //IMPLEMENTATA LA DAD PER STUDENTI E LO SMART WORKING
                    {
                        endX = humanComponent.homePosition.x;
                        endY = humanComponent.homePosition.y;
                        tileComponent.currentTile = TileMapEnum.TileMapSprite.Home;
                        tileComponent.currentFloor = humanComponent.homePosition.z;
                    }
                    break;
                case NeedType.needForVax:
                    if (!lockdown && vaccinationPolicy)
                    {
                        result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
                        result[0] = TileMapEnum.TileMapSprite.Hospital;
                        //tileComponent.currentTile = TileMapEnum.TileMapSprite.Hospital;
                        //tileComponent.currentFloor = 0;
                    }
                    break;

                case NeedType.needToHeal:
                    result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
                    result[0] = TileMapEnum.TileMapSprite.Hospital;
                    //tileComponent.currentTile = TileMapEnum.TileMapSprite.Hospital;
                    //tileComponent.currentFloor = 0;

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

            if (!found)
            {
                int hashMapKey = Human.GetPositionHashMapKey(startX, startY);
                NativeArray<int> keys = placesHM.GetKeyArray(Allocator.Temp);
                TileInfo tileInfo;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

                do
                {
                    if (placesHM.TryGetFirstValue(hashMapKey, out tileInfo, out nativeMultiHashMapIterator))
                    {
                        do
                        {
                            for (int l = 0; l < result.Length && !found; l++)
                            {
                                if (result[l] == tileInfo.type)
                                {
                                    found = true;
                                    endX = tileInfo.x;
                                    endY = tileInfo.y;
                                    tileComponent.currentTile = tileInfo.type;
                                    tileComponent.currentFloor = tileInfo.floor;
                                }
                            }

                        } while (placesHM.TryGetNextValue(out tileInfo, ref nativeMultiHashMapIterator) && !found);
                    }

                    hashMapKey = keys[random.NextInt(0,keys.Length)];

                } while (!found);

                keys.Dispose();

            }

            randomArray[nativeThreadIndex] = random;
            result.Dispose();
           
            ecb.RemoveComponent<NeedPathParams>(nativeThreadIndex, entity);

            ecb.AddComponent<PathfindingParams>(nativeThreadIndex, entity, new PathfindingParams
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(endX, endY)
            });
        }).WithReadOnly(houses).ScheduleParallel(Dependency);

        jobHandle.Complete();

        ecbSystem.AddJobHandleForProducer(jobHandle);
    }

    protected override void OnStopRunning()
    {
        Grid.Dispose();
        directions.Dispose();
        start_offset.Dispose();

    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}