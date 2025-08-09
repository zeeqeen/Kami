using Unity.Entities;

namespace Kami.Component.GameManagement
{
    /// <summary>
    /// Singleton component to hold the game score.
    /// </summary>
    public struct Score : IComponentData
    {
        public int Value;
    }
}
