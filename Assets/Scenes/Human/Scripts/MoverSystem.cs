﻿
/*
public class MoverSystem : SystemBase
{
    protected override void OnUpdate() {
        float deltaTime = (float) Time.DeltaTime;

        Entities.ForEach((ref Translation t, ref MoveSpeedComponent ms ) => {
            t.Value.x += ms.moveSpeedX * deltaTime;
            t.Value.y += ms.moveSpeedY * deltaTime;

            if (t.Value.y > 150f) {
                ms.moveSpeedY = -Math.Abs(ms.moveSpeedY);
            }
            if (t.Value.y < 0)
            {
                ms.moveSpeedY = +Math.Abs(ms.moveSpeedY);
            }

            if (t.Value.x > 150f)
            {
                ms.moveSpeedX = -Math.Abs(ms.moveSpeedX);
            }
            if (t.Value.x < 0)
            {
                ms.moveSpeedX = +Math.Abs(ms.moveSpeedX);
            }
        }).Schedule();
    }
}
*/