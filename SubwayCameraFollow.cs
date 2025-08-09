using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Kami.GameManagement
{
    /// <summary>
    /// Subway-style third-person camera that follows behind the player with smoothing,
    /// lateral look-ahead for lane changes, slight tilt, and optional FOV scaling.
    /// Attach this to the Main Camera and tune the parameters in the inspector.
    /// </summary>
    [DefaultExecutionOrder(10000)] // After transform producers
    public class SubwayCameraFollow : MonoBehaviour
    {
        [Header("Follow")]
        [Tooltip("Distance behind the player along the run axis.")]
        public float followDistance = 8f;
        [Tooltip("Camera height above the player.")]
        public float height = 4f;
        [Tooltip("Base lateral offset from player (for asymmetry if desired)." )]
        public float baseLateralOffset = 0f;
        [Tooltip("If true, player runs toward +Z. If false, toward -Z.")]
        public bool forwardIsPositiveZ = true;

        [Header("Look-Ahead & Tilt")]
        [Tooltip("How much the camera looks ahead along the run direction.")]
        public float forwardLookAhead = 2.5f;
        [Tooltip("Extra lateral look-ahead applied while lane-changing.")]
        public float lateralLookAhead = 1.25f;
        [Tooltip("Max roll tilt (degrees) applied during lane change.")]
        public float maxRollTilt = 8f;
        [Tooltip("How quickly tilt interpolates.")]
        public float tiltLerpSpeed = 6f;

        [Header("Smoothing")]
        [Min(0f)] public float positionLerpSpeed = 8f;
        [Min(0f)] public float rotationLerpSpeed = 10f;

        [Header("FOV")]
        public Camera targetCamera;
        [Tooltip("Enable dynamic FOV scaling with speed.")]
        public bool dynamicFov = true;
        public float minFov = 60f;
        public float maxFov = 75f;
        public float fovAtSpeed = 20f; // speed at which FOV approaches max
        public float fovLerpSpeed = 4f;

        private Entity _playerEntity;
        private EntityManager _entityManager;
        private bool _isPlayerTracked;
        private float _currentRoll;
        private bool _hasInitialPosition = false;

        [Header("Debug")] public bool debugLogs = false;
        private float _lastLogTime;

        void Awake()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
        }

        void Start()
        {
            // Initial camera position if needed
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
                
            Debug.Log($"[SubwayCameraFollow] Started. Debug logs: {debugLogs}, Forward is +Z: {forwardIsPositiveZ}");
        }

        void LateUpdate()
        {
            // Get or refresh the entity manager each frame to ensure it's valid
            if (World.DefaultGameObjectInjectionWorld == null) 
            {
                if (debugLogs && Time.time - _lastLogTime > 2f)
                {
                    _lastLogTime = Time.time;
                    Debug.Log("[SubwayCameraFollow] Waiting for World.DefaultGameObjectInjectionWorld...");
                }
                return;
            }
            
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (_entityManager == default) return;

            // Try to find player entity (re-check periodically in case it's spawned later)
            if (!_isPlayerTracked || !_entityManager.Exists(_playerEntity))
            {
                using (var q = _entityManager.CreateEntityQuery(typeof(Kami.Component.Player.PlayerTag), 
                                                                typeof(Unity.Transforms.LocalToWorld)))
                {
                    if (!q.IsEmptyIgnoreFilter)
                    {
                        // Get first valid player entity
                        var arr = q.ToEntityArray(Unity.Collections.Allocator.Temp);
                        if (arr.Length > 0)
                        {
                            _playerEntity = arr[0];
                            _isPlayerTracked = true;
                            Debug.Log($"[SubwayCameraFollow] Now tracking player entity {_playerEntity.Index} at frame {Time.frameCount}");
                        }
                        arr.Dispose();
                    }
                    else if (debugLogs && Time.time - _lastLogTime > 1.5f)
                    {
                        _lastLogTime = Time.time;
                        Debug.Log($"[SubwayCameraFollow] No PlayerTag entity with LocalToWorld found yet. Frame: {Time.frameCount}");
                    }
                }
            }

            // Exit if we still don't have a valid player
            if (!_isPlayerTracked || !_entityManager.Exists(_playerEntity))
            {
                return;
            }
            
            // Double-check LocalToWorld exists
            if (!_entityManager.HasComponent<LocalToWorld>(_playerEntity))
            {
                _isPlayerTracked = false; // Reset to search again
                if (debugLogs)
                {
                    Debug.LogWarning($"[SubwayCameraFollow] Player entity {_playerEntity.Index} lost LocalToWorld component.");
                }
                return;
            }

            // Get player transform data
            var ltw = _entityManager.GetComponentData<LocalToWorld>(_playerEntity);
            float3 playerPos3 = ltw.Position;
            
            // Validate position
            if (!math.all(math.isfinite(playerPos3)))
            {
                if (debugLogs)
                {
                    Debug.LogWarning($"[SubwayCameraFollow] Player position is not finite: {playerPos3}, resetting to origin");
                }
                // Try to get LocalTransform and fix it
                if (_entityManager.HasComponent<LocalTransform>(_playerEntity))
                {
                    var localTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
                    localTransform.Position = float3.zero;
                    _entityManager.SetComponentData(_playerEntity, localTransform);
                }
                return;
            }

            // Get player movement data for lane tracking
            int currentLane = 0, targetLane = 0;
            float laneProgress = 1f;
            float laneWidth = 2.5f;
            float moveSpeed = 10f;
            
            if (_entityManager.HasComponent<Kami.Component.Player.PlayerMovementData>(_playerEntity))
            {
                var mv = _entityManager.GetComponentData<Kami.Component.Player.PlayerMovementData>(_playerEntity);
                currentLane = mv.CurrentLaneIndex;
                targetLane = mv.TargetLaneIndex;
                laneProgress = math.saturate(mv.LaneChangeProgress);
                
                if (debugLogs && Time.frameCount % 60 == 0) // Log every second
                {
                    Debug.Log($"[SubwayCameraFollow] Lane: {currentLane} -> {targetLane}, Progress: {laneProgress:F2}");
                }
            }
            
            if (_entityManager.HasComponent<Kami.Component.Player.PlayerConfig>(_playerEntity))
            {
                var cfg = _entityManager.GetComponentData<Kami.Component.Player.PlayerConfig>(_playerEntity);
                laneWidth = cfg.LaneWidth;
                moveSpeed = cfg.MoveSpeed;
            }

            // Calculate camera positioning
            Vector3 playerPos = (Vector3)playerPos3;
            
            // Lateral look-ahead based on lane change
            int laneDelta = math.clamp(targetLane - currentLane, -1, 1);
            float lateralLead = laneDelta * lateralLookAhead * (1f - (laneProgress * laneProgress));
            
            // Direction sign (assuming player runs forward along +Z by default)
            float dirSign = forwardIsPositiveZ ? 1f : -1f;
            
            // Look target: slightly ahead and above the player
            Vector3 lookTarget = playerPos + new Vector3(0f, 1.5f, dirSign * forwardLookAhead);
            
            // Camera position: behind and above the player
            float targetX = math.lerp(currentLane * laneWidth, targetLane * laneWidth, laneProgress);
            Vector3 desired = new Vector3(
                targetX + baseLateralOffset + lateralLead,
                playerPos.y + height,
                playerPos.z - dirSign * followDistance
            );
            
            if (debugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[SubwayCameraFollow] Player at {playerPos}, Camera target: {desired}");
            }

            // Stable lerp factors
            float pt = 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime);
            float rt = 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime);

            // Set initial position if this is the first valid frame
            if (!_hasInitialPosition)
            {
                transform.position = desired;
                _hasInitialPosition = true;
                Debug.Log($"[SubwayCameraFollow] Set initial camera position to {desired}, player at {playerPos}");
            }
            else
            {
                // Move with smoothing
                Vector3 newPos = Vector3.Lerp(transform.position, desired, Mathf.Clamp01(pt));
                if (float.IsFinite(newPos.x) && float.IsFinite(newPos.y) && float.IsFinite(newPos.z))
                {
                    transform.position = newPos;
                }
            }

            // Face the look target
            Vector3 forward = (lookTarget - transform.position);
            if (forward.sqrMagnitude > 1e-4f)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward.normalized, Vector3.up);

                // Apply gentle roll tilt during lane changes
                float targetRoll = -laneDelta * maxRollTilt * (1f - laneProgress);
                _currentRoll = Mathf.Lerp(_currentRoll, targetRoll, Mathf.Clamp01(tiltLerpSpeed * Time.deltaTime));
                targetRot = targetRot * Quaternion.AngleAxis(_currentRoll, Vector3.forward);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Mathf.Clamp01(rt));
            }

            // Dynamic FOV based on forward speed
            if (dynamicFov && targetCamera != null)
            {
                float forwardSpeed = moveSpeed; // Use the player's configured move speed
                float fov01 = math.saturate(forwardSpeed / math.max(0.001f, fovAtSpeed));
                float targetFov = math.lerp(minFov, maxFov, fov01);
                float fovT = 1f - math.exp(-fovLerpSpeed * Time.deltaTime);
                targetCamera.fieldOfView = math.lerp(targetCamera.fieldOfView, targetFov, fovT);
            }
        }
    }
}

