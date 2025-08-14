using Kami.Component.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;


namespace Kami.System
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerInputSystem))]
    public partial struct PlayerMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Player>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (player, playerInput, transform) in 
                     SystemAPI.Query<RefRW<Player>, RefRO<PlayerInput>, RefRW<LocalTransform>>())
            {
                if (playerInput.ValueRO.MoveLeft && player.ValueRO.CurrentLane > -1)
                {
                    player.ValueRW.CurrentLane--;
                    player.ValueRW.CurrentLane = math.max(player.ValueRO.CurrentLane, -1); 
                }
                else if (playerInput.ValueRO.MoveRight && player.ValueRO.CurrentLane < 1)
                {
                    player.ValueRW.CurrentLane++;
                    player.ValueRW.CurrentLane = math.min(player.ValueRW.CurrentLane, 1);
                }
                
                // Move smoothly
                float targetX = player.ValueRO.CurrentLane * player.ValueRO.LaneOffset;
                float3 currentPos = transform.ValueRO.Position;
                currentPos.x = math.lerp(currentPos.x, targetX, player.ValueRO.MoveSpeed * deltaTime);

                currentPos.z += 10f * deltaTime;

                transform.ValueRW.Position = currentPos;
                
            }

            foreach (var (jumpState, transform, playerInput) in SystemAPI.Query<RefRW<JumpState>, RefRW<LocalTransform>, RefRO<PlayerInput>>())
            {
                if (playerInput.ValueRO.Jump && !jumpState.ValueRO.IsJumping)
                {
                    jumpState.ValueRW.IsJumping = true;
                    jumpState.ValueRW.JumpTime = 0f;
                }

                if (jumpState.ValueRO.IsJumping)
                {
                    jumpState.ValueRW.JumpTime += deltaTime;
                    
                    // 抛物线跳跃
                    float jumpDuration = 1f;
                    float jumpHeight = 3f;

                    if (jumpState.ValueRO.JumpTime < jumpDuration)
                    {
                        float t = jumpState.ValueRO.JumpTime / jumpDuration;
                        float y = jumpHeight * 4 * t * (1 - t); // 抛物线公式

                        float3 pos = transform.ValueRO.Position;
                        pos.y = y;
                        transform.ValueRW.Position = pos;
                    }
                    else
                    {
                        jumpState.ValueRW.IsJumping = false;
                        jumpState.ValueRW.JumpTime = 0f;

                        float3 pos = transform.ValueRO.Position;
                        pos.y = 0f;
                        transform.ValueRW.Position = pos;
                    }
                }
                
            }



            foreach (var (slideState, transform, playerInput) in
                     SystemAPI.Query<RefRW<SlideState>, RefRW<LocalTransform>, RefRW<PlayerInput>>())
            {
                if (playerInput.ValueRO.Slide && !slideState.ValueRO.IsSliding)
                {
                    slideState.ValueRW.IsSliding = true;
                    slideState.ValueRW.SlideTime = 0f;
                }

                if (slideState.ValueRO.IsSliding)
                {
                    slideState.ValueRW.SlideTime += deltaTime;
                    
                    float slideDuration = 1f;
                    if (slideState.ValueRO.SlideTime < slideDuration)
                    {
                        float3 pos = transform.ValueRO.Position;
                        pos.y = -0.5f;
                        transform.ValueRW.Position = pos;
                    }
                    else
                    {
                        slideState.ValueRW.IsSliding = false;
                        slideState.ValueRW.SlideTime = 0f;

                        float3 pos = transform.ValueRO.Position;
                        pos.y = 0f;
                        transform.ValueRW.Position = pos;
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