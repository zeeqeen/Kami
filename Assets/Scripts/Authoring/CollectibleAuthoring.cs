using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Kami.Component.Collectible;

namespace Kami.Authoring
{
    public class CollectibleAuthoring : MonoBehaviour
    {
        private class CollectibleAuthoringBaker : Baker<CollectibleAuthoring>
        {
            public override void Bake(CollectibleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent<CollectibleTag>(entity);
                
            }
        }
    }
}