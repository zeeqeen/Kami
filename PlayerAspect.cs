using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Kami.Component.Player
{
    public readonly partial struct PlayerAspect : IAspect
    {
        public readonly Entity Self;

        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRW<PlayerMovementData> _movementData;
        private readonly RefRW<PlayerComponent> _playerComponent;
        private readonly RefRO<PlayerConfig> _config;
        private readonly RefRO<PlayerInput> _input;

        public void Move(float deltaTime)
        {
            // Validate delta time to prevent NaN propagation
            if (!math.isfinite(deltaTime) || deltaTime <= 0)
                return;
                
            // Get current position and validate it
            float3 currentPos = _transform.ValueRO.Position;
            if (!math.all(math.isfinite(currentPos)))
            {
                // Reset to a valid position if corrupted
                currentPos = new float3(0, 0, 0);
                _transform.ValueRW.Position = currentPos;
            }
            
            // --- Vertical Movement ---
            bool isOnGround = _playerComponent.ValueRO.VerticalPosition <= 0.0f;

            if (isOnGround)
            {
                _playerComponent.ValueRW.VerticalVelocity = 0;
                _playerComponent.ValueRW.VerticalPosition = 0;
                if (_input.ValueRO.JumpPressed)
                {
                    _playerComponent.ValueRW.VerticalVelocity = _config.ValueRO.JumpForce;
                }
            }
            else
            {
                _playerComponent.ValueRW.VerticalVelocity += _config.ValueRO.Gravity * deltaTime;
            }
            
            if (_input.ValueRO.DivePressed)
            {
                // Instantly move down, could be a stronger downward velocity too
                _playerComponent.ValueRW.VerticalVelocity = -_config.ValueRO.JumpForce;
            }

            _playerComponent.ValueRW.VerticalPosition += _playerComponent.ValueRW.VerticalVelocity * deltaTime;

            // --- Horizontal and Forward Movement ---
            if (_movementData.ValueRW.CurrentLaneIndex != _movementData.ValueRW.TargetLaneIndex)
            {
                _movementData.ValueRW.LaneChangeProgress += _config.ValueRO.LaneChangeSpeed * deltaTime;
                if (_movementData.ValueRW.LaneChangeProgress >= 1.0f)
                {
                    _movementData.ValueRW.LaneChangeProgress = 1.0f;
                    _movementData.ValueRW.CurrentLaneIndex = _movementData.ValueRW.TargetLaneIndex;
                }
            }

            if (_input.ValueRO.LaneChangeDirection != 0 && _movementData.ValueRW.CurrentLaneIndex == _movementData.ValueRW.TargetLaneIndex)
            {
                _movementData.ValueRW.TargetLaneIndex += _input.ValueRO.LaneChangeDirection;
                _movementData.ValueRW.TargetLaneIndex = math.clamp(_movementData.ValueRW.TargetLaneIndex, -1, 1);
                _movementData.ValueRW.LaneChangeProgress = 0.0f;
            }

            float currentX = _movementData.ValueRO.CurrentLaneIndex * _config.ValueRO.LaneWidth;
            float targetX = _movementData.ValueRO.TargetLaneIndex * _config.ValueRO.LaneWidth;

            // Build new position with validation
            float3 newPosition = new float3(
                math.lerp(currentX, targetX, _movementData.ValueRO.LaneChangeProgress),
                _playerComponent.ValueRO.VerticalPosition,
                currentPos.z + _config.ValueRO.MoveSpeed * deltaTime
            );
            
            // Final validation before applying
            if (math.all(math.isfinite(newPosition)))
            {
                _transform.ValueRW.Position = newPosition;
            }
        }
    }
}
