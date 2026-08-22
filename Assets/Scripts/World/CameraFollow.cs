using UnityEngine;

namespace AshesOfRyu
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public float SmoothTime = 0.12f;

        private Vector3 velocity;
        private readonly Vector3 offset = new Vector3(0f, 0f, -10f);

        private void LateUpdate()
        {
            if (Target == null) return;
            Vector3 desired = Target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, SmoothTime);
        }
    }
}
