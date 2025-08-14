using Unity.Entities;
using Unity.Mathematics;

namespace Kami.Component.Camera
{
    public struct CameraFollow : IComponentData
    {
        public Entity Target; // The target we follow, here, the player mainly
        public float3 Offset; // The offset of the camaera
        public float FollowSpeed;
        public float LookAtSpeed;
        public CameraViewMode ViewMode;

    }

    public enum CameraViewMode : byte
    {
        ThirdPerson = 0,
        FirstPerson = 1,
        TopDown = 2,
        Side = 3,
        Dynamic = 4
    }

    public struct CameraConstraints : IComponentData
    {
        public float3 MinPosition;
        public float3 MaxPosition;
        public float MinDistance;
        public float MaxDistance;
        public bool UseConstraints;
    }

    public struct CameraShake : IComponentData
    {
        public float Intensity;
        public float Duration;
        public float CurrentTime;
        public float3 OriginalOffset;
        public bool IsActive;
    }

    public struct CameraSmoothness : IComponentData
    {
        public float PositionSmoothness;
        public float RotationSmoothness;
        public float ZoomSmoothness;
        public bool UseSmoothing;
        
    }

    public struct CameraTargetPosition : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public bool HasTarget;
    }
}