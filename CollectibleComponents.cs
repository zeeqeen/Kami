using Unity.Entities;

namespace Kami.Component.Collectible
{
    public struct CollectibleTag : IComponentData { }

    public enum CollectibleType
    {
        Coin,
        PowerUp_Magnet,
        PowerUp_Invincible
    }

    public struct CollectibleData : IComponentData
    {
        public CollectibleType Type;
    }
}
