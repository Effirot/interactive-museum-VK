using UnityEngine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.Interaction;
using Unity.Cinemachine;

namespace InteractiveMuseum.MiniGames
{
    /// <summary>
    /// Вспомогательный скрипт для автоматической настройки мини-игры с тараканами.
    /// Можно использовать в редакторе для быстрой настройки.
    /// </summary>
    public class CockroachMiniGameSetup : MonoBehaviour
    {
        [Header("Setup Settings")]
        [Tooltip("Создать зону взаимодействия автоматически")]
        [SerializeField]
        private bool _createInteractionZone = true;
        
        [Tooltip("Позиция зоны взаимодействия")]
        [SerializeField]
        private Vector3 _interactionZonePosition = new Vector3(0, 0, 5);
        
        [Tooltip("Размер зоны взаимодействия")]
        [SerializeField]
        private Vector3 _interactionZoneSize = new Vector3(3, 2, 3);
        
        [Tooltip("Создать камеру для мини-игры автоматически")]
        [SerializeField]
        private bool _createCamera = true;
        
        [Tooltip("Позиция камеры мини-игры")]
        [SerializeField]
        private Vector3 _cameraPosition = new Vector3(0, 3, -5);
        
        [Tooltip("Целевая точка для камеры (куда смотрит камера)")]
        [SerializeField]
        private Vector3 _cameraLookAt = Vector3.zero;
        
        private void Start()
        {
            // Автоматическая настройка при запуске (можно отключить)
            // SetupMiniGame();
        }
        
        /// <summary>
        /// Настраивает мини-игру с тараканами.
        /// </summary>
        [ContextMenu("Setup Cockroach Mini-Game")]
        public void SetupMiniGame()
        {
            // Находим или создаем CockroachMiniGame
            CockroachMiniGame miniGame = FindFirstObjectByType<CockroachMiniGame>();
            if (miniGame == null)
            {
                GameObject miniGameObj = new GameObject("CockroachMiniGame");
                miniGame = miniGameObj.AddComponent<CockroachMiniGame>();
            }
            
            // Настраиваем зону спавна тараканов
            Bounds spawnArea = new Bounds(Vector3.zero, new Vector3(5f, 0.1f, 5f));
            miniGame.spawnArea = spawnArea;
            
            // Создаем камеру для мини-игры
            CinemachineCamera miniGameCamera = null;
            if (_createCamera)
            {
                GameObject cameraObj = GameObject.Find("CockroachMiniGameCamera");
                if (cameraObj == null)
                {
                    cameraObj = new GameObject("CockroachMiniGameCamera");
                    cameraObj.transform.position = _cameraPosition;
                    
                    // Добавляем CinemachineCamera
                    miniGameCamera = cameraObj.AddComponent<CinemachineCamera>();
                    
                    // Настраиваем камеру для наблюдения за зоной
                    // Можно добавить CinemachineVirtualCamera для более продвинутой настройки
                }
                else
                {
                    miniGameCamera = cameraObj.GetComponent<CinemachineCamera>();
                    if (miniGameCamera == null)
                    {
                        miniGameCamera = cameraObj.AddComponent<CinemachineCamera>();
                    }
                }
                
                miniGame.miniGameCamera = miniGameCamera;
            }
            
            // Создаем зону взаимодействия
            if (_createInteractionZone)
            {
                GameObject interactionZone = GameObject.Find("CockroachInteractionZone");
                if (interactionZone == null)
                {
                    interactionZone = new GameObject("CockroachInteractionZone");
                    interactionZone.transform.position = _interactionZonePosition;
                    
                    // Добавляем коллайдер
                    BoxCollider collider = interactionZone.AddComponent<BoxCollider>();
                    collider.size = _interactionZoneSize;
                    collider.isTrigger = true;
                    
                    // Добавляем FocusableInteractable
                    FocusableInteractable focusable = interactionZone.AddComponent<FocusableInteractable>();
                    focusable.targetCamera = miniGameCamera;
                    focusable.miniGame = miniGame;
                    
                    // Добавляем Interactable
                    Interactable interactable = interactionZone.AddComponent<Interactable>();
                    interactable.onLookText = "Взаимодействовать с тараканами";
                    
                    // Добавляем InteractableOutline
                    InteractableOutline outline = interactionZone.AddComponent<InteractableOutline>();
                    
                    Debug.Log("[CockroachMiniGameSetup] Создана зона взаимодействия для мини-игры с тараканами.");
                }
            }
            
            // Настраиваем CameraManager если нужно
            CameraManager cameraManager = CameraManager.Instance;
            if (cameraManager != null && miniGameCamera != null)
            {
                if (cameraManager.miniGameCamera == null)
                {
                    cameraManager.miniGameCamera = miniGameCamera;
                }
            }
            
            Debug.Log("[CockroachMiniGameSetup] Мини-игра с тараканами настроена!");
        }
    }
}
