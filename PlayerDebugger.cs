using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Kami.Component.Player;

namespace Kami.GameManagement
{
    /// <summary>
    /// Debug script to verify player entity exists and has proper components
    /// </summary>
    public class PlayerDebugger : MonoBehaviour
    {
        private EntityManager _entityManager;
        private float _lastLogTime;
        
        void Update()
        {
            if (Time.time - _lastLogTime < 2f) return; // Log every 2 seconds
            _lastLogTime = Time.time;
            
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                Debug.Log("[PlayerDebugger] No ECS World yet.");
                return;
            }
            
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            using (var query = _entityManager.CreateEntityQuery(typeof(PlayerTag)))
            {
                int count = query.CalculateEntityCount();
                Debug.Log($"[PlayerDebugger] Found {count} PlayerTag entities");
                
                if (count > 0)
                {
                    var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                    var entity = entities[0];
                    
                    Debug.Log($"[PlayerDebugger] Player Entity {entity.Index}:");
                    Debug.Log($"  - Has LocalToWorld: {_entityManager.HasComponent<LocalToWorld>(entity)}");
                    Debug.Log($"  - Has LocalTransform: {_entityManager.HasComponent<LocalTransform>(entity)}");
                    Debug.Log($"  - Has PlayerMovementData: {_entityManager.HasComponent<PlayerMovementData>(entity)}");
                    Debug.Log($"  - Has PlayerConfig: {_entityManager.HasComponent<PlayerConfig>(entity)}");
                    Debug.Log($"  - Has PlayerComponent: {_entityManager.HasComponent<PlayerComponent>(entity)}");
                    
                    if (_entityManager.HasComponent<LocalToWorld>(entity))
                    {
                        var ltw = _entityManager.GetComponentData<LocalToWorld>(entity);
                        Debug.Log($"  - Position: {ltw.Position}");
                    }
                    
                    entities.Dispose();
                }
            }
        }
    }
}
