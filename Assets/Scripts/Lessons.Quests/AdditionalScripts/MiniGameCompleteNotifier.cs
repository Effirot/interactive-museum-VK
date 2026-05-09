using UnityEngine;
using InteractiveMuseum.MiniGames;
using InteractiveMuseum.Interaction;

namespace Lessons.Quests
{
    public class MiniGameCompleteNotifier : MonoBehaviour
    {
        [Header("Quest Marker")]
        [SerializeField]
        private GameObject questMarkerToActivate;

        [Header("Settings")]
        [SerializeField]
        private bool autoDetectMiniGame = true;

        [Space]
        [Header("Debug")]
        [SerializeField]
        private bool showDebugMessages = true;

        private MiniGameBase _miniGame;
        private bool _wasCompleted = false;
        private float _activationTimer = -1f;

        private void Awake()
        {
            if (autoDetectMiniGame)
            {
                _miniGame = GetComponent<MiniGameBase>();
                if (_miniGame == null)
                {
                    _miniGame = GetComponentInParent<MiniGameBase>();
                }
                if (_miniGame == null)
                {
                    FocusableInteractable focusable = GetComponent<FocusableInteractable>();
                    if (focusable == null)
                    {
                        focusable = GetComponentInParent<FocusableInteractable>();
                    }
                    if (focusable != null && focusable.miniGame != null)
                    {
                        _miniGame = focusable.miniGame;
                    }
                }
            }

            if (_miniGame == null)
            {
                enabled = false;
                return;
            }

            _miniGame.OnMiniGameCompleted += OnMiniGameCompleted;
        }

        private void Update()
        {
            if (_activationTimer >= 0f)
            {
                _activationTimer -= Time.deltaTime;
                if (_activationTimer <= 0f)
                {
                    ActivateMarker();
                }
            }
        }

        private void OnDestroy()
        {
            if (_miniGame != null)
            {
                _miniGame.OnMiniGameCompleted -= OnMiniGameCompleted;
            }
        }

        private void OnMiniGameCompleted()
        {
            if (_wasCompleted)
                return;
        }

        private void ActivateMarker()
        {
            if (_wasCompleted)
                return;

            _wasCompleted = true;
            _activationTimer = -1f;

            if (questMarkerToActivate != null)
            {
                questMarkerToActivate.SetActive(true);
            }
        }

        public void ActivateMarkerManually()
        {
            if (!_wasCompleted)
            {
                _wasCompleted = true;
                ActivateMarker();
            }
        }

        public void ResetNotifier()
        {
            _wasCompleted = false;
            _activationTimer = -1f;

            if (questMarkerToActivate != null)
            {
                questMarkerToActivate.SetActive(false);
            }
        }
    }
}