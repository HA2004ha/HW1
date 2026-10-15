using UnityEngine;

namespace Runner.World
{
    /// <summary>
    /// One scrolling band of the background. Tiles are laid end to end and recycled to the far
    /// side as they pass the camera, which is what makes the depth illusion endless.
    /// </summary>
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private WorldController world;
        [Tooltip("1 tracks the road, below 1 reads as distance, above 1 as foreground blur.")]
        [SerializeField, Range(0f, 2f)] private float speedFactor = 0.3f;
        [SerializeField] private float wrapMargin = 0.5f;

        private Transform[] tiles;
        private float tileWidth;
        private float span;

        private void Start() => Rebuild();

        /// <summary>Re-reads the children. Called by the scene builder once the tiles are placed.</summary>
        public void Rebuild()
        {
            var count = transform.childCount;
            tiles = new Transform[count];

            for (var i = 0; i < count; i++) tiles[i] = transform.GetChild(i);

            if (count == 0) return;

            var first = tiles[0].GetComponent<SpriteRenderer>();
            tileWidth = first != null && first.sprite != null
                ? first.sprite.bounds.size.x * tiles[0].lossyScale.x
                : 20f;

            span = tileWidth * count;
        }

        private void Update()
        {
            if (world == null || !world.Scrolling || tiles == null || tiles.Length == 0) return;

            var delta = -world.Speed * speedFactor * Time.deltaTime;
            var coverageLeft = world.ViewLeftX - wrapMargin;

            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                var position = tile.position;
                position.x += delta;

                if (position.x + tileWidth * 0.5f < coverageLeft) position.x += span;

                tile.position = position;
            }
        }
    }
}
