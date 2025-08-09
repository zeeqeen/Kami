using Unity.Entities;
using UnityEngine.InputSystem;
using Kami.Component.Player;

namespace Kami.System.Player
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlayerInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            int laneChangeDirection = 0;
            bool jumpPressed = false;
            bool divePressed = false;
            var keyboard = Keyboard.current;

            if (keyboard == null) return; // No keyboard connected

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                laneChangeDirection = -1;
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                laneChangeDirection = 1;
            }

            if (keyboard.kKey.wasPressedThisFrame)
            {
                jumpPressed = true;
            }
            
            if (keyboard.jKey.wasPressedThisFrame)
            {
                divePressed = true;
            }

            // Using SystemAPI.Query to set the input on the player entity
            foreach (var playerInput in SystemAPI.Query<RefRW<Kami.Component.Player.PlayerInput>>().WithAll<PlayerTag>())
            {
                // Only update if there's a new input
                playerInput.ValueRW.LaneChangeDirection = laneChangeDirection;
                playerInput.ValueRW.JumpPressed = jumpPressed;
                playerInput.ValueRW.DivePressed = divePressed;
            }
        }
    }
}
