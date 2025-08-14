using System; // 注意这里直接 using System，MonoBehaviour 中能用 Exception
using Kami.Component.Camera;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Kami.System.Camera
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraBridge : MonoBehaviour
    {
        private EntityManager _entityManager;
        private Entity _cameraEntity = Entity.Null;
        private UnityEngine.Camera _unityCamera;

        [Header("调试信息")]
        [SerializeField] private bool cameraEntityFound = false;
        [SerializeField] private string currentStatus = "未初始化";

        private void Start()
        {
            _unityCamera = GetComponent<UnityEngine.Camera>();

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                currentStatus = "找不到 ECS 世界";
                Debug.LogError(currentStatus);
                return;
            }

            _entityManager = world.EntityManager;
            InvokeRepeating(nameof(FindCameraEntity), 0.1f, 0.5f); // 持续尝试查找
        }

        private void Update()
        {
            if (_cameraEntity != Entity.Null && _entityManager.Exists(_cameraEntity))
            {
                SyncFromECS();
            }
        }

        private void FindCameraEntity()
        {
            try
            {
                var query = _entityManager.CreateEntityQuery(
                    typeof(CameraFollow),
                    typeof(LocalTransform)
                );

                var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
                if (entities.Length > 0)
                {
                    _cameraEntity = entities[0];
                    cameraEntityFound = true;
                    currentStatus = $"找到相机实体 {_cameraEntity}";
                    CancelInvoke(nameof(FindCameraEntity)); // 找到了就停止查找
                }
                else
                {
                    cameraEntityFound = false;
                    currentStatus = "未找到相机实体";
                }
                entities.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogError($"查找相机实体出错: {e.Message}");
            }
        }

        private void SyncFromECS()
        {
            try
            {
                if (_entityManager.HasComponent<LocalTransform>(_cameraEntity))
                {
                    var ecsTransform = _entityManager.GetComponentData<LocalTransform>(_cameraEntity);
                    transform.position = ecsTransform.Position;
                    transform.rotation = ecsTransform.Rotation;
                    currentStatus = $"同步中: {ecsTransform.Position}";
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"同步相机位置出错: {e.Message}");
            }
        }
    }
}
