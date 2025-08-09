using Unity.Entities;
using Unity.Mathematics;

namespace Kami.Component.Player
{
    /// <summary>
    /// Dynamic data for player movement, especially lane switching.
    /// </summary>
    public struct PlayerMovementData : IComponentData
    {
        public int CurrentLaneIndex;
        public int TargetLaneIndex;
        public float LaneChangeProgress; // 0.0 to 1.0
    }

    /// <summary>
    /// Static configuration for player movement.
    /// </summary>
    public struct PlayerConfig : IComponentData
    {
        public float MoveSpeed;
        public float LaneWidth;
        public float LaneChangeSpeed;
        public float JumpForce;
        public float Gravity;
    }

    /// <summary>
    /// Stores the player's input for movement.
    /// </summary>
    public struct PlayerInput : IComponentData
    {
        public int LaneChangeDirection; // -1 for left, 1 for right, 0 for none
        public bool JumpPressed;
        public bool DivePressed;
    }
}
