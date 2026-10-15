using UnityEngine;

namespace Runner.World
{
    /// <summary>
    /// Remembers which prefab an instantiated object came from so the pool can bucket it back.
    /// Added automatically by <see cref="ObjectPool"/>.
    /// </summary>
    public sealed class PooledInstance : MonoBehaviour
    {
        public GameObject Source { get; private set; }

        public void Init(GameObject source) => Source = source;
    }
}
