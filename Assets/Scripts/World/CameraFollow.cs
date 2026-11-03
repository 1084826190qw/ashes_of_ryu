using UnityEngine;

namespace AshesOfRyu
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public float SmoothTime = 0.12f;

        private Vector3 velocity;
        private readonly Vector3 offset = new Vector3(0f, 0f, -10f);

        public void SetTarget(Transform target)
        {
            Target = target;
            velocity = Vector3.zero;
            if (Target != null)
                transform.position = Target.position + offset;
        }

        private void LateUpdate()
        {
            if (Target == null && GameManager.Instance != null && GameManager.Instance.Player != null)
                Target = GameManager.Instance.Player.transform;
            if (Target == null) return;

            Vector3 desired = Target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, SmoothTime);
        }
    }
}
