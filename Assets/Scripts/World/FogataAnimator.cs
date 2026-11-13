using UnityEngine;

namespace AshesOfRyu
{
    /// <summary>
    /// Anima una fogata con los frames ya cortados de Assets/fogata.png.
    /// Mantiene fija la base del sprite para que la fogata no "salte" al
    /// cambiar entre frames de distinta altura.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FogataAnimator : MonoBehaviour
    {
        [Tooltip("Frames de la fogata en orden (fogata_6 ... fogata_21).")]
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 10f;

        private SpriteRenderer sr;
        private float baseBottomY;
        private float timer;
        private int frameIndex;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null || frames == null || frames.Length == 0)
            {
                enabled = false;
                return;
            }

            // Guarda la altura de la base actual para no mover la fogata.
            baseBottomY = WorldBottom(sr.sprite);
            ApplyFrame(0);
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;

            timer += Time.deltaTime;
            float frameTime = 1f / Mathf.Max(1f, framesPerSecond);
            if (timer < frameTime) return;
            timer -= frameTime;

            frameIndex = (frameIndex + 1) % frames.Length;
            ApplyFrame(frameIndex);
        }

        private void ApplyFrame(int index)
        {
            if (frames[index] == null) return;

            sr.sprite = frames[index];

            // Compensa la diferencia de altura del frame para que la base
            // quede siempre en el mismo punto del mundo.
            Vector3 position = transform.position;
            position.y = baseBottomY - frames[index].bounds.min.y * transform.lossyScale.y;
            transform.position = position;
        }

        private float WorldBottom(Sprite sprite)
        {
            return transform.position.y + sprite.bounds.min.y * transform.lossyScale.y;
        }
    }
}
