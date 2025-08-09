// 8/9/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using Kami.Component.Collectible;
using Kami.GameManagement;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions; // Ensure this is included for `SphereCollider.Create`
using UnityEngine;

namespace Kami.Authoring.Collectible
{
    public class CollectibleAuthoring : MonoBehaviour
    {
        public CollectibleType type;
    }

    public class CollectibleBaker : Baker<CollectibleAuthoring>
    {
        public override void Bake(CollectibleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent<CollectibleTag>(entity);
            AddComponent(entity, new CollectibleData { Type = authoring.type });

            // Add Physics components for collision
            var sphereGeometry = new SphereGeometry
            {
                Center = float3.zero,
                Radius = 0.5f
            };
            var material = Unity.Physics.Material.Default;
            material.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
            var filter = new CollisionFilter
            {
                BelongsTo = (uint)CollisionLayers.Collectible,
                CollidesWith = (uint)CollisionLayers.Player,
            };

            // Explicitly specify Unity.Physics.SphereCollider
            AddComponent(entity, new PhysicsCollider { Value = Unity.Physics.SphereCollider.Create(sphereGeometry, filter, material) });
            
            // Required for trigger events
            AddComponent<PhysicsMass>(entity);
            AddComponent<PhysicsVelocity>(entity);
        }
    }
}
