using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Kami.Component.Tracking;

namespace Kami.Authoring
{
    public class TrackAuthoring : MonoBehaviour
    {
        private class TrackAuthoringBaker : Baker<TrackAuthoring>
        {
            public override void Bake(TrackAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent<TrackTag>(entity);
             
            }
        }
    }
}