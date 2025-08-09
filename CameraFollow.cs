using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Kami.GameManagement
{
    public class CameraFollow : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 5, -10);
        [Min(0f)] public float smoothSpeed = 10f;

        private Entity _playerEntity;
        private EntityManager _entityManager;
        private bool _isPlayerTracked = false;

        void Start()
        {
            // Get the entity manager from the default world
            if (World.DefaultGameObjectInjectionWorld != null)
            {
                _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            }
        }

        void LateUpdate()
        {
            if (_entityManager == default) return;

            if (!_isPlayerTracked)
            {
                // Find the player entity using a query for the PlayerTag
                using (var playerQuery = _entityManager.CreateEntityQuery(typeof(Kami.Component.Player.PlayerTag)))
                {
                    if (!playerQuery.IsEmptyIgnoreFilter)
                    {
                        _playerEntity = playerQuery.GetSingletonEntity();
                        _isPlayerTracked = true;
                    }
                }
            }

            if (_isPlayerTracked && _entityManager.Exists(_playerEntity))
            {
                // Ensure the transform data exists before accessing
                if (!_entityManager.HasComponent<LocalToWorld>(_playerEntity)) return;

                // Get the player's world position from its LocalToWorld component
                LocalToWorld playerTransform = _entityManager.GetComponentData<LocalToWorld>(_playerEntity);
                var playerPos = playerTransform.Position;

                // Guard against invalid/NaN positions
                if (!(float.IsFinite(playerPos.x) && float.IsFinite(playerPos.y) && float.IsFinite(playerPos.z)))
                {
                    return;
                }

                Vector3 targetPosition = (Vector3)playerPos + offset;

                if (!(float.IsFinite(targetPosition.x) && float.IsFinite(targetPosition.y) && float.IsFinite(targetPosition.z)))
                {
                    return;
                }

                // Framerate-independent smoothing factor in [0,1]
                float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
                t = Mathf.Clamp01(t);

                // Smoothly move the camera towards the target position
                transform.position = Vector3.Lerp(transform.position, targetPosition, t);
                transform.LookAt((Vector3)playerPos);
            }
        }
    }
}
