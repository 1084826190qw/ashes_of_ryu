using System.Collections;
using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerVisuals : MonoBehaviour
    {
        private readonly Color baseColor = Color.white;
        private readonly Color furyColor = new Color(1.3f, 0.5f, 0.5f);
        private readonly Color dragonColor = new Color(1.2f, 0.9f, 0.6f);

        // Ryu es el personaje principal: un poco mas grande que el resto del mundo.
        private const float BaseScale = 1.3f;

        private SpriteRenderer sr;
        private PlayerAnimator animator;
        private bool dragonForm;
        private bool furyActive;
        private bool dead;
        private bool flicker;

        public bool IsDead => dead;
        public bool IsDragonForm => dragonForm;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            animator = GetComponent<PlayerAnimator>();
            sr.color = baseColor;
        }

        private void Update()
        {
            // Al morir el sprite permanece visible para que se vea la animacion de Death.
            sr.enabled = dead || !(flicker && Mathf.Sin(Time.time * 45f) < -0.2f);
        }

        public void SetDragonForm(bool active)
        {
            dragonForm = active;
            transform.localScale = Vector3.one * (active ? BaseScale * 1.45f : BaseScale);
            ApplyColor();
            if (animator != null)
            {
                if (active) animator.PlayFlight();
                else animator.PlayFall(); // aterrizaje; al terminar vuelve solo al idle
            }
        }

        public void SetFuryActive(bool active)
        {
            furyActive = active;
            ApplyColor();
        }

        public void SetDead(bool value)
        {
            dead = value;
            if (animator != null)
            {
                if (value) animator.PlayDeath();
                else animator.PlayIdle();
            }
        }

        public void SetInvuln(bool value)
        {
            flicker = value;
        }

        public void FlashDamage()
        {
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            ApplyColor();
        }

        private void ApplyColor()
        {
            sr.color = dragonForm ? dragonColor : (furyActive ? furyColor : baseColor);
        }
    }
}
