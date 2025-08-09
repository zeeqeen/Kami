using Kami.Component.Collectible;
using Kami.Component.GameManagement;
using Kami.Component.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Kami.System.Collectible
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct CollectibleCollectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Score>();
            state.RequireForUpdate<SimulationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<Score>(out var scoreEntity))
            {
                // Score entity not ready yet
                return;
            }

            // Create ECB that will be played back at end of fixed step
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            state.Dependency = new CollectibleTriggerJob
            {
                PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true),
                CollectibleLookup = SystemAPI.GetComponentLookup<CollectibleTag>(true),
                ScoreLookup = SystemAPI.GetComponentLookup<Score>(false),
                ScoreEntity = scoreEntity,
                Ecb = ecb
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            
            // Manually playback the ECB after the job completes
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private struct CollectibleTriggerJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
            [ReadOnly] public ComponentLookup<CollectibleTag> CollectibleLookup;
            public ComponentLookup<Score> ScoreLookup;
            public Entity ScoreEntity;
            public EntityCommandBuffer Ecb;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                bool isBodyAPlayer = PlayerLookup.HasComponent(entityA);
                bool isBodyBPlayer = PlayerLookup.HasComponent(entityB);
                bool isBodyACollectible = CollectibleLookup.HasComponent(entityA);
                bool isBodyBCollectible = CollectibleLookup.HasComponent(entityB);

                if ((isBodyAPlayer && isBodyBCollectible) || (isBodyBPlayer && isBodyACollectible))
                {
                    var collectibleEntity = isBodyACollectible ? entityA : entityB;
                    
                    var score = ScoreLookup[ScoreEntity];
                    score.Value++;
                    ScoreLookup[ScoreEntity] = score;

                    Ecb.DestroyEntity(collectibleEntity);
                }
            }
        }
    }
}
