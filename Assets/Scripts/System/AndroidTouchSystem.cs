using Kami.Component.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Kami.System
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AndroidTouchSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (touchData, entity) in SystemAPI.Query<RefRO<TouchInputData>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }

            foreach (var (swipeData, entity) in SystemAPI.Query<RefRO<SwipeInputData>>().WithEntityAccess())        
            {
                ecb.DestroyEntity(entity);
                
            }

            foreach (var (pinchData, entity) in SystemAPI.Query<RefRO<PinchInputData>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            
         
        }

        private void ProcessTouchInput(ref SystemState state)
        {
            int touchCount = Input.touchCount;
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new TouchInputData
                {
                    Position = new float2(touch.position.x, touch.position.y),
                    DeltaPosition = new float2(touch.deltaPosition.x, touch.deltaPosition.y),
                    Phase = (int)touch.phase,
                    FingerId = touch.fingerId,
                    Time = Time.time

                });
            }
        }

        private void ProcessSwipeGesture(ref SystemState state)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    float2 deltaPosition = new float2(touch.deltaPosition.x, touch.deltaPosition.y);
                    float magnitude = math.length(deltaPosition);

                    if (magnitude > 50f)
                    {
                        float2 direction = math.normalize(deltaPosition);
                        SwipeDirection swipeDir = DetermineSwipeDirection(direction);
                        
                        var swipeEntity = state.EntityManager.CreateEntity();
                        state.EntityManager.AddComponentData(swipeEntity, new SwipeInputData
                        {
                            StartPosition = new float2(touch.position.x - touch.deltaPosition.x,
                                touch.position.y - touch.deltaPosition.y),
                            EndPosition = new float2(touch.position.x, touch.position.y),
                            Direction = direction,
                            Magnitude = magnitude,
                            SwipeDir = swipeDir

                        });
                    }
                }
            }
        }

        private void ProcessPinchGesture(ref SystemState state)
        {
            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);
                
                // Calculate the current distance
                float2 touch1Pos = new float2(touch1.position.x, touch1.position.y);
                float2 touch2Pos = new float2(touch2.position.x, touch2.position.y);
                float currentDistance = math.distance(touch1Pos, touch2Pos);
                
                // Calculate the distance of the former frame
                float2 touch1PrePos = touch1Pos - new float2(touch1.deltaPosition.x, touch1.deltaPosition.y);
                float2 touch2PrePos = touch2Pos - new float2(touch2.deltaPosition.x, touch2.deltaPosition.y);
                float previousDistance = math.distance(touch1PrePos, touch2PrePos);

                float delta = currentDistance - previousDistance;
                float2 center = (touch1Pos + touch2Pos) * 0.5f;
                
                var pinchEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(pinchEntity, new PinchInputData
                {
                    Delta = delta,
                    Center = center
                });
            }
        }

        private SwipeDirection DetermineSwipeDirection(float2 direction)
        {
            float absX = math.abs(direction.x);
            float absY = math.abs(direction.y);

            if (absX > absY)
            {
                return direction.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                return direction.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }
    }

}