/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class Pathfinding : ComponentSystem
{

    protected override void OnUpdate()
    {

        List<FindPathJob> findPathJobList = new List<FindPathJob>();
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
        


        Entities.ForEach((Entity entity, ref PathfindingParams pathfindingParams) =>
        {
            NativeList<int2> pathNodeList = new NativeList<int2>(Allocator.TempJob);
            FindPathJob findPathJob = new FindPathJob
            {
                pathNodeList = pathNodeList,
                startPosition = pathfindingParams.startPosition,
                endPosition = pathfindingParams.endPosition,
                entity = entity,
            };
            findPathJobList.Add(findPathJob);
            jobHandleList.Add(findPathJob.Schedule());

            PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);

        });
     
       JobHandle.CompleteAll(jobHandleList);

        foreach (FindPathJob findPathJob in findPathJobList)
        {
            new SetBufferPathJob
            {
                pathNodeList = findPathJob.pathNodeList,
                startPosition = findPathJob.startPosition,
                endPosition = findPathJob.endPosition,
                entity = findPathJob.entity,
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>(),
            }.Run();


            
        }

        //pathNodeList.Dispose();
    }

    private struct FindPathJob : IJob
    {

        public int2 startPosition;
        public int2 endPosition;
        public NativeList<int2> pathNodeList;
        public Entity entity;
        public void Execute()
        {
            //Get closes backbone node for start position and endposition
            int startBBId = ClosestBB.GetClosestBB(startPosition.x, startPosition.y);
            int endBBId = ClosestBB.GetClosestBB(endPosition.x, endPosition.y);

            //Get the path node array of the backbone
            NativeArray<int> pathNodeArray = PathMatrix.GetPath(startBBId, endBBId).ToNativeArray<int>(Allocator.Temp);

            // Add start
            pathNodeList.Add(new int2(startPosition.x, startPosition.y));

            //FOR DI CONVERSIONE DEI BBID IN INT2 TRAMITE FILE nodes.csv
            for (int i = 0; i < pathNodeArray.Length; i++)
            {
                pathNodeList.Add(NodesBB.GetXYfromID(pathNodeArray[i]));
            }

            //Add end
            pathNodeList.Add(new int2(endPosition.x, endPosition.y));

            
        }

    }
    private struct SetBufferPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;

        //[DeallocateOnJobCompletion]
        public NativeArray<int2> pathNodeList;

        public Entity entity;

        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;
        public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

        public void Execute()
        {
            DynamicBuffer<PathPosition> pathPositionBuffer = pathPositionBufferFromEntity[entity];
            pathPositionBuffer.Clear();

           
           
            for (int i = 0; i < pathNodeList.Length; i++)
            {
                pathPositionBuffer.Add(new PathPosition { position = pathNodeList[i] });
            }


            pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 2 };
            

        }
    }


}
