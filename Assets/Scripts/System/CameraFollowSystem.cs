using Kami.Component.Camera;
using Kami.Component.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Kami.System.Camera
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct CameraFollowSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraFollow>();
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Phase 1: Calculate the position and rotation for the Target
            foreach (var (cameraFollow, targetPos, entity) in
                     SystemAPI.Query<RefRO<CameraFollow>, RefRW<CameraTargetPosition>>().WithEntityAccess())
            {
                if (SystemAPI.Exists(cameraFollow.ValueRO.Target))
                {
                    var targetTransform = SystemAPI.GetComponent<LocalTransform>(cameraFollow.ValueRO.Target);
                    var calculatedTransform = CalculateTargetTransform(
                        ref state,
                        targetTransform,
                        cameraFollow.ValueRO.ViewMode,
                        cameraFollow.ValueRO.Offset);

                    targetPos.ValueRW.Position = calculatedTransform.Position;
                    targetPos.ValueRW.Rotation = calculatedTransform.Rotation;
                    targetPos.ValueRW.HasTarget = true;
                }
                else
                {
                    targetPos.ValueRW.HasTarget = false;
                }
            }

            // Phase 2: Apply smooth to the actual Transform
            foreach (var (cameraFollow, targetPos, transform, smoothness) in
                     SystemAPI
                         .Query<RefRO<CameraFollow>, RefRW<CameraTargetPosition>, RefRW<LocalTransform>,
                             RefRO<CameraSmoothness>>())
            {
                if (!targetPos.ValueRO.HasTarget) continue;

                if (smoothness.ValueRO.UseSmoothing)
                {
                    float3 smoothedPosition = math.lerp(
                        transform.ValueRO.Position,
                        targetPos.ValueRO.Position,
                        smoothness.ValueRO.PositionSmoothness * deltaTime);

             
                    quaternion smoothedRotation = math.slerp(
                        transform.ValueRO.Rotation,
                        targetPos.ValueRO.Rotation,
                        smoothness.ValueRO.RotationSmoothness * deltaTime);

                    transform.ValueRW.Position = smoothedPosition;
                    transform.ValueRW.Rotation = smoothedRotation;
                }
                else
                {
                    transform.ValueRW.Position = targetPos.ValueRO.Position;
                    transform.ValueRW.Rotation = targetPos.ValueRO.Rotation;
                }
            }
            
            ApplyCameraConstraints(ref state);
            
            ProcessCameraShake(ref state, deltaTime);
        }
        
        

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
        
        
        [BurstCompile]
        private static CameraTarget CalculateTargetTransform(ref SystemState state,
            LocalTransform playerTransform,
            CameraViewMode viewMode,
            float3 offset)
        {
            float3 playerPos = playerTransform.Position;
            quaternion playRot = playerTransform.Rotation;

            switch (viewMode)
            {
                case CameraViewMode.ThirdPerson:
                    return CalculateThirdPersionView(playerPos, playRot, offset);
                case CameraViewMode.FirstPerson:
                    return CalculateFirstPersonView(playerPos, playRot, offset);
                
                default:
                    return CalculateThirdPersionView(playerPos, playRot, offset);
            }
        }

        [BurstCompile]
        private static CameraTarget CalculateThirdPersionView(float3 playPos, quaternion playRot, float3 offset)
        {
            // Typical play view mode

            float3 worldOffset = math.mul(playRot, offset);
            float3 cameraPos = playPos + worldOffset;

            // Look at a little front
            float3 lookAtTarget = playPos + new float3(0, 1.5f, 0); 
            
            quaternion cameraRot = quaternion.LookRotationSafe(lookAtTarget - cameraPos, math.up());

            return new CameraTarget
            {
                Position = cameraPos,
                Rotation = cameraRot,
            };
        }

        [BurstCompile]
        private static CameraTarget CalculateFirstPersonView(float3 playerPos, quaternion playRot, float3 offset)
        {
            float3 eyeOffset = new float3(0, 1.8f, 0);
            float3 cameraPos = playerPos + eyeOffset + offset;

            return new CameraTarget
            {
                Position = cameraPos,
                Rotation = playRot,
            };
        }

        [BurstCompile]
        private void ApplyCameraConstraints(ref SystemState state)
        {
            foreach (var (constaints, transform) in
                     SystemAPI.Query<RefRO<CameraConstraints>, RefRW<LocalTransform>>())
            {
                if (!constaints.ValueRO.UseConstraints) continue;
                {
                    float3 pos = transform.ValueRO.Position;

                    // Apply position constraintation
                    pos = math.clamp(pos, constaints.ValueRO.MinPosition, constaints.ValueRO.MaxPosition);

                    transform.ValueRW.Position = pos;
                }
            }
        }

        [BurstCompile]
        private void ProcessCameraShake(ref SystemState state, float deltaTime)
        {
            foreach (var (shake, transform) in
                     SystemAPI.Query<RefRW<CameraShake>, RefRW<LocalTransform>>())
            {
                if (!shake.ValueRO.IsActive) continue;

                shake.ValueRW.CurrentTime += deltaTime;

                if (shake.ValueRO.CurrentTime < shake.ValueRO.Duration)
                {
                    var random = Unity.Mathematics.Random.CreateFromIndex((uint)(SystemAPI.Time.ElapsedTime * 1000));
                    float intensity = shake.ValueRO.Intensity *
                                      (1f - shake.ValueRO.CurrentTime / shake.ValueRO.Duration);
                    float3 randomShake = new float3(
                        random.NextFloat(-1f, 1f),
                        random.NextFloat(-1f, 1f),
                        random.NextFloat(-1f, 1f)
                    ) * intensity;
                    
                    transform.ValueRW.Position += randomShake;
                }
                else
                {
                    // Ends the shaking
                    shake.ValueRW.IsActive  = false;
                    shake.ValueRW.CurrentTime = 0f;
                }
            }
        }
        
        private struct CameraTarget
        {
            public float3 Position;
            public quaternion Rotation;
        }
    }
}