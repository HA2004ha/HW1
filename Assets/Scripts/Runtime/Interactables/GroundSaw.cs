using UnityEngine;

namespace Runner.Interactables
{
    /// <summary>
    /// A spinning buzz-saw that rolls along the road. It survives impact, so the player cannot
    /// trade hits with it - the only answer is clearing it.
    /// </summary>
    public sealed class GroundSaw : GroundObstacle
    {
        [SerializeField] private float spinDegreesPerSecond = -540f;

        protected override void Step(float deltaTime)
        {
            base.Step(deltaTime);
            transform.Rotate(0f, 0f, spinDegreesPerSecond * deltaTime);
        }
    }
}
