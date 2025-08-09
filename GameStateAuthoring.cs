using Kami.Component.GameManagement;
using Unity.Entities;
using UnityEngine;

namespace Kami.Authoring.GameManagement
{
    public class GameStateAuthoring : MonoBehaviour
    {
    }

    public class GameStateBaker : Baker<GameStateAuthoring>
    {
        public override void Bake(GameStateAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Score { Value = 0 });
        }
    }
}
