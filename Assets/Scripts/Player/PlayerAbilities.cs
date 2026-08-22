using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerAbilities : MonoBehaviour
    {
        public FireBreath FireBreath { get; private set; }
        public DragonForm DragonForm { get; private set; }

        private void Awake()
        {
            FireBreath = GetComponent<FireBreath>();
            DragonForm = GetComponent<DragonForm>();
        }

        private void Update()
        {
            var input = InputReader.Instance;
            if (input == null || GameManager.Instance.PlayerIsDead) return;
            if (input.FirePressed && FireBreath != null) FireBreath.TryFire();
            if (input.DragonPressed && DragonForm != null) DragonForm.TryActivate();
        }
    }
}
