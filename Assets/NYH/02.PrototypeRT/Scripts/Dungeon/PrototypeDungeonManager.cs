using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrototypeRT
{
    public class PrototypeDungeonManager : MonoBehaviour
    {
        [SerializeField] private bool restartOnPlayerDeath = true;
        [SerializeField, Min(0f)] private float restartDelay = 2f;

        private bool _ended;
        public static bool IsRunEnded { get; private set; }

        private void Awake()
        {
            IsRunEnded = false;
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnDied += HandleDied;
            PrototypeRTEvents.OnPlayerEscaped += HandleEscaped;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnDied -= HandleDied;
            PrototypeRTEvents.OnPlayerEscaped -= HandleEscaped;
        }

        private void OnDestroy()
        {
            PrototypeRTEvents.ClearAll();
        }

        private void HandleDied(Health health)
        {
            if (_ended || health.Team != Team.Player) return;
            _ended = true;
            IsRunEnded = true;

            if (restartOnPlayerDeath)
                Invoke(nameof(ReloadScene), restartDelay);
        }

        private void HandleEscaped()
        {
            if (_ended) return;
            _ended = true;
            IsRunEnded = true;
        }

        private void ReloadScene()
        {
            PrototypeRTEvents.ClearAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
