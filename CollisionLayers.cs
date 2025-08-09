namespace Kami.GameManagement
{
    // Define collision layers for better filtering
    public enum CollisionLayers : uint
    {
        Player = 1 << 0,
        Collectible = 1 << 1,
        Obstacle = 1 << 2,
        Ground = 1 << 3,
    }
}
