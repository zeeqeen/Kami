using Unity.Entities;
using UnityEngine;
using Kami.Component.Player;
using Unity.Transforms;

namespace Kami.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float laneOffset = 2f;
        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Player
                {
                    MoveSpeed = authoring.moveSpeed,
                    LaneOffset = authoring.laneOffset,
                    CurrentLane = 0
                });
                
                AddComponent(entity, new PlayerInput());
               // AddComponent<LocalTransform>(entity);
                AddComponent<JumpState>(entity);
                AddComponent<SlideState>(entity);
            }
        }
    }
}