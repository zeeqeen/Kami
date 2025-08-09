using Kami.Component.Player;
using Kami.GameManagement;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Kami.Authoring.Player
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float moveSpeed = 10f;
        public float laneWidth = 2.5f;
        public float laneChangeSpeed = 10f;
        public float jumpForce = 20f;
        public float gravity = -50f;
    }

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<PlayerTag>(entity);
            
            // Initialize player component with default values
            AddComponent(entity, new PlayerComponent
            {
                CurrentForm = PlayerComponent.PaperForm.Main,
                LaneTransitionProgress = 0f,
                VerticalPosition = 0f,
                VerticalVelocity = 0f,
                DistanceTravelled = 0f,
                PowerUpTimer = 0f
            });
            
            AddComponent(entity, new PlayerInput
            {
                LaneChangeDirection = 0,
                JumpPressed = false,
                DivePressed = false
            });

            AddComponent(entity, new PlayerMovementData
            {
                CurrentLaneIndex = 0,
                TargetLaneIndex = 0,
                LaneChangeProgress = 1.0f
            });

            AddComponent(entity, new PlayerConfig
            {
                MoveSpeed = authoring.moveSpeed,
                LaneWidth = authoring.laneWidth,
                LaneChangeSpeed = authoring.laneChangeSpeed,
                JumpForce = authoring.jumpForce,
                Gravity = authoring.gravity
            });
            
            // Add Physics components for collision
            var capsuleGeometry = new CapsuleGeometry
            {
                Vertex0 = new float3(0, 0.5f, 0),
                Vertex1 = new float3(0, 1.5f, 0),
                Radius = 0.5f
            };
            var material = Unity.Physics.Material.Default;
            material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            var filter = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayers.Player,
                CollidesWith = (uint)CollisionLayers.Collectible | (uint)CollisionLayers.Obstacle,
            };
            // AddComponent(entity, new PhysicsCollider { Value = Unity.Physics.CapsuleCollider.Create(capsuleGeometry, filter, material) });
            
            // Required for trigger events
            AddComponent<PhysicsMass>(entity);
            AddComponent<PhysicsVelocity>(entity);
        }
    }
}
