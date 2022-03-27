using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


public class PlagueSystem : SystemBase
{

    private EndSimulationEntityCommandBufferSystem ecbSystem;
    public bool heatmap;
    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        heatmap = Human.Instance.heatmap;
    }
    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        var heatMap = heatmap;
        JobHandle jobHandle = Entities//.WithChangeFilter<InfectionComponent>()
            .ForEach((Entity entity, int nativeThreadIndex, ref SpriteSheetAnimation_Data spriteSheetAnimationData, in Translation translation, in InfectionComponent ic) =>
            {
                if (heatMap)
                {
                    float uvOffsetX = 0f;
                    float uvWidth = 1f / 100;
                    float uvHeight = 1f;
                    float uvOffsetY = 0f;
                    Vector3 scale = new Vector3(10f, 10f);

                    if (ic.status == Status.recovered)
                    {
                        uvOffsetX = 0.9f;
                    }

                    if (ic.status == Status.infectious && ic.symptomatic)
                    {
                        uvOffsetX = 0.05f;
                    }

                    if (ic.status == Status.infectious && !ic.symptomatic)
                    {
                        uvOffsetX = 0.25f;
                    }

                    if (ic.status == Status.exposed)
                    {
                        uvOffsetX = 0.5f;
                    }

                    if (ic.status == Status.susceptible)
                    {
                        uvOffsetX = 0f;
                        scale.x = 0.5f;
                        scale.y = 0.5f;
                    }


                    spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

                    Vector3 position = translation.Value;
                    spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, scale);
                }
                else
                {

                    float uvOffsetY = 0.8f;

                    if (ic.status == Status.recovered)
                    {
                        uvOffsetY = 0.0f;
                    }

                    if (ic.status == Status.infectious && ic.symptomatic)
                    {
                        uvOffsetY = 0.2f;
                    }

                    if (ic.status == Status.infectious && !ic.symptomatic)
                    {
                        uvOffsetY = 0.4f;
                    }

                    if (ic.status == Status.exposed)
                    {
                        uvOffsetY = 0.6f;
                    }

                    float uvWidth = 1f;
                    float uvHeight = 1f / 5;
                    float uvOffsetX = 0f;

                    spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

                    Vector3 position = translation.Value;
                    spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                }
            }).ScheduleParallel(Dependency);

        jobHandle.Complete();

        ecbSystem.AddJobHandleForProducer(jobHandle);
    }
}