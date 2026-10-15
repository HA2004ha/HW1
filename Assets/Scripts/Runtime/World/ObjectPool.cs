using System.Collections.Generic;
using UnityEngine;

namespace Runner.World
{
    /// <summary>
    /// A very small prefab pool. An endless runner recycles the same handful of hazards
    /// hundreds of times, so instantiating them is not an option.
    /// </summary>
    public sealed class ObjectPool : MonoBehaviour
    {
        [SerializeField] private List<GameObject> prefabs = new List<GameObject>();
        [SerializeField] private int prewarmPerPrefab = 6;

        private readonly Dictionary<GameObject, Stack<GameObject>> buckets = new Dictionary<GameObject, Stack<GameObject>>();

        private void Awake()
        {
            for (var i = 0; i < prefabs.Count; i++)
            {
                var prefab = prefabs[i];
                if (prefab == null || buckets.ContainsKey(prefab)) continue;

                var bucket = new Stack<GameObject>();
                for (var p = 0; p < prewarmPerPrefab; p++)
                {
                    var clone = Create(prefab);
                    clone.SetActive(false);
                    bucket.Push(clone);
                }

                buckets.Add(prefab, bucket);
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!buckets.TryGetValue(prefab, out var bucket))
            {
                bucket = new Stack<GameObject>();
                buckets.Add(prefab, bucket);
            }

            var instance = bucket.Count > 0 ? bucket.Pop() : Create(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            instance.SetActive(false);
            var source = GetSource(instance);
            if (source != null && buckets.TryGetValue(source, out var bucket)) bucket.Push(instance);
        }

        public void ReleaseAll()
        {
            foreach (var bucket in buckets.Values)
            {
                while (bucket.Count > 0)
                {
                    var instance = bucket.Pop();
                    if (instance != null) instance.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Destroys every pooled child and forgets the buckets. Used when the editor rebuilds the
        /// scene so stale clones never leak into the next run.
        /// </summary>
        public void Clear()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            buckets.Clear();
        }

        private GameObject Create(GameObject prefab)
        {
            var clone = Instantiate(prefab, transform);
            clone.name = prefab.name;
            clone.AddComponent<PooledInstance>().Init(prefab);
            return clone;
        }

        private static GameObject GetSource(GameObject instance)
        {
            var marker = instance.GetComponent<PooledInstance>();
            return marker != null ? marker.Source : null;
        }
    }
}
