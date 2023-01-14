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
       

        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);



        Entities.ForEach((Entity entity, ref PathfindingParams pathfindingParams) =>
        {
            //Get closes backbone node for start position and endposition
            int startBBId = ClosestBB.GetClosestBB(pathfindingParams.startPosition.x, pathfindingParams.startPosition.y);
            int endBBId = ClosestBB.GetClosestBB(pathfindingParams.endPosition.x, pathfindingParams.endPosition.y);

            //Get the path node array of the backbone
            NativeArray<int> pathNodeArray = PathMatrix.GetPath(startBBId, endBBId).ToNativeArray<int>(Allocator.TempJob);
           
            SetBufferPathJob SetBufferPathJob = new SetBufferPathJob
            {
                pathNodeArray = pathNodeArray,
                startPosition = pathfindingParams.startPosition,
                endPosition = pathfindingParams.endPosition,
                entity = entity,
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
                pathPositionBufferFromEntity = GetBufferFromEntity<PathPosition>(),
            };
            jobHandleList.Add(SetBufferPathJob.Schedule());
            PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
            
        });
     
       JobHandle.CompleteAll(jobHandleList);
        
        
       
    }


    private struct SetBufferPathJob : IJob
    {
        public int2 startPosition;
        public int2 endPosition;

        [DeallocateOnJobCompletion]
        public NativeArray<int> pathNodeArray;

        public Entity entity;

        public ComponentDataFromEntity<PathFollow> pathFollowComponentDataFromEntity;
        public BufferFromEntity<PathPosition> pathPositionBufferFromEntity;

        public void Execute()
        {
            DynamicBuffer<PathPosition> pathPositionBuffer = pathPositionBufferFromEntity[entity];
            pathPositionBuffer.Clear();

            // Add start
            pathPositionBuffer.Add(new PathPosition { position = new int2(startPosition.x, startPosition.y) });


            //FOR DI CONVERSIONE DEI BBID IN INT2 TRAMITE FILE nodes.csv
            for (int i = 0; i < pathNodeArray.Length; i++)
            {
                pathPositionBuffer.Add(new PathPosition { position = NodesBB.GetXYfromID(pathNodeArray[i]) });
            }

            pathPositionBuffer.Add(new PathPosition { position = new int2(endPosition.x, endPosition.y) });

            pathFollowComponentDataFromEntity[entity] = new PathFollow { pathIndex = pathPositionBuffer.Length - 2 };
            

        }
    }


}
