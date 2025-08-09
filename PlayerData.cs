using Unity.Entities;
using Unity.Mathematics;

namespace Kami.Component.Player
{
    /// <summary>
    /// Player state tags for efficient system queries
    /// </summary>
    public struct PlayerTag : IComponentData { }
 

    /// <summary>
    /// Primary player entity component containing all essential game state
    /// </summary>
    public struct PlayerComponent : IComponentData
    {
        /// <summary>
        /// Available paper forms for environmental adaptation
        /// </summary>
        public enum PaperForm
        {
            Main = 0,
            Bird = 1,
            Fish = 2
        }

        public PaperForm CurrentForm;

        /// <summary>
        /// Lane transition progress (0.0 = at current lane, 1.0 = at target lane)
        /// </summary>
        public float LaneTransitionProgress;

        /// <summary>
        /// Vertical position for jumping/flying mechanics
        /// </summary>
        public float VerticalPosition;
        
        /// <summary>
        /// Vertical velocity for physics-based movement
        /// </summary>
        public float VerticalVelocity;

        /// <summary>
        /// Distance traveled in the Kami world
        /// </summary>
        public float DistanceTravelled;

        /// <summary>
        /// Time remaining for any active power-ups
        /// </summary>
        public float PowerUpTimer;
    }

    
}