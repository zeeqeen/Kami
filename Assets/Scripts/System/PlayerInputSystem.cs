using Kami.Component.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Screen = UnityEngine.Screen;

namespace  Kami.System
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AndroidTouchSystem))]
    public partial struct PlayerInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
           state.RequireForUpdate<Player>(); 
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var swiptQuery = SystemAPI.QueryBuilder().WithAll<SwipeInputData>().Build();
            var touchQuery = SystemAPI.QueryBuilder().WithAll<TouchInputData>().Build();
            
            // Turn gesture into player input
            foreach (var (player, playerInput) in
                     SystemAPI.Query<RefRW<Player>, RefRW<PlayerInput>>())
            {
                playerInput.ValueRW = new PlayerInput()
                {
                    MoveLeft = false,
                    MoveRight = false,
                    Jump = false,
                    Slide = false
                };

                foreach (var swipt in SystemAPI.Query<RefRO<SwipeInputData>>())
                {
                    var swiptData = swipt.ValueRO;

                    switch (swiptData.SwipeDir)
                    {
                        case SwipeDirection.Left:
                            playerInput.ValueRW.MoveLeft = true;
                            break;
                        case SwipeDirection.Right:
                            playerInput.ValueRW.MoveRight = true;
                            break;
                        case SwipeDirection.Up:
                            playerInput.ValueRW.Jump = true;
                            break;
                        case SwipeDirection.Down:
                            playerInput.ValueRW.Slide = true;
                            break;
                        
                            
                    }
                  
                }

                foreach (var touch in SystemAPI.Query<RefRO<TouchInputData>>())
                {
                    var touchData = touch.ValueRO;

                    if (touchData.Phase == 0)
                    {
                        float screenWidth = Screen.width;
                        float screenHeight = Screen.height;

                        if (touchData.Position.x < screenWidth * 0.33f)
                        {
                            playerInput.ValueRW.MoveLeft = true;
                        }
                        else if (touchData.Position.x > screenWidth * 0.66f)
                        {
                            playerInput.ValueRW.MoveRight = true;
                        }

                        if (touchData.Position.y > screenHeight * 0.66f)
                        {
                            playerInput.ValueRW.Jump = true;
                        }
                        else if (touchData.Position.y < screenHeight * 0.33f)
                        {
                            playerInput.ValueRW.Slide = true;

                        }
                    }
                }
            }
            
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

    }
}