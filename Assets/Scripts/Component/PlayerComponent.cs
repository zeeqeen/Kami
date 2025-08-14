using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;

namespace Kami.Component.Player
{
    public struct Player : IComponentData
    {
        public float MoveSpeed;
        public float LaneOffset; 
        public int CurrentLane; 
        
    }

    public struct PlayerInput : IComponentData
    {
        public bool MoveLeft;
        public bool MoveRight;
        public bool Jump;
        public bool Slide;
    }

    public struct JumpState : IComponentData
    {
        public bool IsJumping;
        public float JumpTime;
    }

    public struct SlideState : IComponentData
    {
        public bool IsSliding;
        public float SlideTime;
    }

    public struct TouchInputData : IComponentData
    {
        public float2 Position;
        public float2 DeltaPosition;
        public int Phase; // 0=Began, 1=Moved, 2=Stationary, 3=Ended, 4=Canceled
        public int FingerId;
        public float Time;
    }
    
    public struct PinchInputData : IComponentData
    {
        public float Delta;
        public float2 Center;
    }
   
    /// <summary>
    /// Gesture data
    /// </summary>
    public struct SwipeInputData : IComponentData
    {
        public float2 StartPosition;
        public float2 EndPosition;
        public float2 Direction;
        public float Magnitude;
        public SwipeDirection SwipeDir;
    }

    public enum SwipeDirection : byte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Up = 3,
        Down = 4
    }
    
}