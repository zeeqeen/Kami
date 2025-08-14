# 概要

像下面这样，我们为玩家定义了一个 `Player` 组件，它包括移动的速度、路径和当前的路径三个字段；这些类型为 `IComponentData` 的结构体便是组件，
比如说一个 `PlayerInput` 组件可以有 `MoveLeft`, `MoveRight`, `Jump`, `Slide` 字段。

```cs
public struct Player : IComponentData
{
  public float MoveSpeed;
  public float LaneOffset; 
  public int CurrentLane;       
}
```

---

在 `PlayerMovement.cs` 中，核心的实现是一个类型为 `ISystem` 的结构体：
```cs
public partical struct PlayerMovementSystem : ISystem
```

它有三个用 `[BurstCompile]` 属性修饰的 `OnCreate()`, `OnUpdate()` 和 `OnDestory()` 函数，其中我们几乎只关注 `OnUpdate` 因为它是游戏循环的主体。

下面是类似公式般的做法，主体是一个 `foreach (var VARIABLE in EXPRESSION {}` 形式的遍历。先看第二行， `SystemAPI.Query<>` 是为了读写我们的组件及其字段，
如 `Player`, `PlayerInput` 这些我们自己声明定义的组件，以及 Unity 用来表示 *position*, *rotation*, *scale* 三个重要空间相关字段的 `LocalTransform` 组件。

position 就是二维或三维坐标的位置，rotation 就是旋转，scale 就是伸缩变换。`RefRO` 是 *Read Only* 的引用，自然 `RefRW` 是 **Read and Wirte** 读写的引用。

```cs
foreach (var (player, playerInput, transform) in 
    SystemAPI.Query<RefRW<Player>, RefRO<PlayerInput>, RefRW<LocalTransform>>())
{
}
```

然后我们分别用 `player`, `playerInput` 和 `transform` 简化引用的表示，当需要读写其字段的值时，比如 `LocalTransform` 组件的 `Position` 字段的值，
用 `transform.ValueRO.Position` 访问

一个具体例子是：`SlideState` 是一个包含 `IsSliding`, `SlideTime` 字段的组件，分别用 `playerInput` 更新 `slideState` 和 `transform` 表示对组件的引用后，
1. 在没有滑行时若下滑，那么立刻标记 `IsSliding` 为真和计时滑行。
2. 用不随帧率变化的时间 `deltaTime` 用于后面滑行时间等计算
3. 重置滑行时间为 `deltaTime`，硬编码滑行持续的时间，如果处于滑行时更新位置等数据，一旦结束执行 `else` 的行为也就是重置原先的状态，最后再把计算的 `pos` 通过 `transform.ValueRW.Position = pos;` 来更新。
```cs
          foreach (var (slideState, transform, playerInput) in
                     SystemAPI.Query<RefRW<SlideState>, RefRW<LocalTransform>, RefRW<PlayerInput>>())
            {
                if (playerInput.ValueRO.Slide && !slideState.ValueRO.IsSliding)
                {
                    slideState.ValueRW.IsSliding = true;
                    slideState.ValueRW.SlideTime = 0f;
                }

                if (slideState.ValueRO.IsSliding)
                {
                    slideState.ValueRW.SlideTime += deltaTime;
                    
                    float slideDuration = 1f;
                    if (slideState.ValueRO.SlideTime < slideDuration)
                    {
                        float3 pos = transform.ValueRO.Position;
                        pos.y = -0.5f;
                        transform.ValueRW.Position = pos;
                    }
                    else
                    {
                        slideState.ValueRW.IsSliding = false;
                        slideState.ValueRW.SlideTime = 0f;

                        float3 pos = transform.ValueRO.Position;
                        pos.y = 0f;
                        transform.ValueRW.Position = pos;
                    }
                }
            }
            
            
        }

```

返回前面一点，在 `PlayerMovementSystem` 的最开头有两个属性修饰，`[UpdateInGroup()]` 是惯用属性，除了 `SimulationSystemGroup` 还有 `InitializationSystemGroup` 等
```cs
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerInputSystem))]
```

---

下面是一个完整的烘焙的用法：

```cs
using Unity.Entities;
using UnityEngine;
using Kami.Component.Player;
using Unity.Transforms;

namespace Kami.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float laneOffset = 2f;
        private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Player
                {
                    MoveSpeed = authoring.moveSpeed,
                    LaneOffset = authoring.laneOffset,
                    CurrentLane = 0
                });
                
                AddComponent(entity, new PlayerInput());
               // AddComponent<LocalTransform>(entity);
                AddComponent<JumpState>(entity);
                AddComponent<SlideState>(entity);
            }
        }
    }
}
```
