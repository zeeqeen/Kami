using Kami.Component.Player;
using Kami.Component.Camera;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Kami.System.Camera
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class CameraInitializationSystem : SystemBase
    {
        private bool isInitialized = false;
        protected override void OnCreate()
        {
            RequireForUpdate<Player>();
        }

        protected override void OnUpdate()
        {
            // Initializes once for the first time
            if (isInitialized) return;

            var playerEntity = GetPlayerEntity();
            if (playerEntity == Entity.Null) return; // Not found player

            // Creates the camera entity
            var cameraEntity = EntityManager.CreateEntity(); 
            
            // Add Transform component
            EntityManager.AddComponent<LocalTransform>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new LocalTransform
            {
                Position = new float3(0, 8f, -12f), // Position initialized
                Rotation =  quaternion.identity,
                Scale = 1f
            });

            // Add CameraFollow component
            EntityManager.AddComponent<CameraFollow>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new CameraFollow
            {
                Target = playerEntity,
                Offset = new float3(0f, 8f, -12f),
                FollowSpeed = 5f,
                LookAtSpeed = 3f,
                ViewMode = CameraViewMode.ThirdPerson
            });

            // Add the position of the target component
            EntityManager.AddComponent<CameraTargetPosition>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new CameraTargetPosition
            {
                Position = float3.zero,
                Rotation = quaternion.identity,
                HasTarget = false
            });

            // Add smoothness component
            EntityManager.AddComponent<CameraSmoothness>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new CameraSmoothness
            {
                PositionSmoothness = 8f,
                RotationSmoothness = 5f,
                ZoomSmoothness = 3f,
                UseSmoothing = true
            });

            // Add constraint component
            EntityManager.AddComponent<CameraConstraints>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new CameraConstraints
            {
                MinPosition = new float3(-50f, 2f, -1000f),
                MaxPosition = new float3(50f, 50f, 1000f),
                MinDistance = 5f,
                MaxDistance = 30f,
                UseConstraints = true
            });

            // Add shaking component
            EntityManager.AddComponent<CameraShake>(cameraEntity);
            EntityManager.SetComponentData(cameraEntity, new CameraShake
            {
                Intensity = 0f,
                Duration = 0f,
                CurrentTime = 0f,
                OriginalOffset = float3.zero,
                IsActive = false
            });
            
            Debug.Log($"Camera entity created and initialized. Camera: {cameraEntity}, Player: {playerEntity}");

            isInitialized = true;
        }

        private Entity GetPlayerEntity()
        {
            // Query the player entity
            var query = GetEntityQuery(typeof(Player));
            if (query.CalculateEntityCount() > 0)
            {
                return query.GetSingletonEntity();
            }
            
            return Entity.Null;
            

        }


    }
}