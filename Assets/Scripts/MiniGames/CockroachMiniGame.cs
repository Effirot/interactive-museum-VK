using UnityEngine;
using InteractiveMuseum.Camera;
using InteractiveMuseum.Player;
using System.Collections.Generic;

namespace InteractiveMuseum.MiniGames
{
    /// <summary>
    /// Мини-игра с тараканами. Игрок должен задавить всех тараканов, кликая по ним.
    /// </summary>
    public class CockroachMiniGame : MiniGameBase
    {
        [Header("Cockroach Settings")]
        [Tooltip("Префаб таракана для спавна")]
        [SerializeField]
        private GameObject _cockroachPrefab;
        
        [Tooltip("Количество тараканов для спавна")]
        [SerializeField]
        private int _cockroachCount = 10;
        
        [Tooltip("Зона спавна тараканов (Bounds)")]
        [SerializeField]
        private Bounds _spawnArea = new Bounds(Vector3.zero, new Vector3(5f, 0.1f, 5f));
        
        [Tooltip("Высота спавна тараканов над поверхностью")]
        [SerializeField]
        private float _spawnHeight = 0.05f;
        
        [Header("Game Settings")]
        [Tooltip("Скорость движения тараканов")]
        [SerializeField]
        private float _cockroachSpeed = 2f;
        
        [Tooltip("Время между изменениями направления движения")]
        [SerializeField]
        private float _directionChangeInterval = 1f;
        
        [Tooltip("Радиус клика для уничтожения таракана")]
        [SerializeField]
        private float _clickRadius = 0.3f;
        
        [Header("Visual Settings")]
        [Tooltip("Материал для тараканов")]
        [SerializeField]
        private Material _cockroachMaterial;
        
        private List<Cockroach> _activeCockroaches = new List<Cockroach>();
        private PlayerMovementController _playerController;
        private bool _isInitialized = false;
        
        public int cockroachCount
        {
            get => _cockroachCount;
            set => _cockroachCount = value;
        }
        
        public Bounds spawnArea
        {
            get => _spawnArea;
            set => _spawnArea = value;
        }
        
        public float cockroachSpeed
        {
            get => _cockroachSpeed;
            set => _cockroachSpeed = value;
        }
        
        protected override void Start()
        {
            base.Start();
            _playerController = FindFirstObjectByType<PlayerMovementController>();
            
            // Если префаб не задан, создадим простой примитив
            if (_cockroachPrefab == null)
            {
                CreateDefaultCockroachPrefab();
            }
        }
        
        protected override void OnMiniGameActivated()
        {
            base.OnMiniGameActivated();
            
            if (!_isInitialized)
            {
                InitializeGame();
            }
            
            SpawnCockroaches();
            
            // Подписываемся на клики мыши
            if (_playerController != null)
            {
                // Используем существующую систему обработки кликов
                // Клики будут обрабатываться через HandleCockroachClick
            }
        }
        
        protected override void OnMiniGameDeactivated()
        {
            base.OnMiniGameDeactivated();
            
            // Удаляем всех тараканов
            ClearAllCockroaches();
        }
        
        private void InitializeGame()
        {
            _isInitialized = true;
        }
        
        private void SpawnCockroaches()
        {
            ClearAllCockroaches();
            
            for (int i = 0; i < _cockroachCount; i++)
            {
                SpawnCockroach();
            }
        }
        
        private void SpawnCockroach()
        {
            if (_cockroachPrefab == null)
            {
                Debug.LogError("[CockroachMiniGame] Cockroach prefab is null!");
                return;
            }
            
            // Генерируем случайную позицию в зоне спавна
            Vector3 randomPosition = new Vector3(
                Random.Range(_spawnArea.min.x, _spawnArea.max.x),
                _spawnArea.center.y + _spawnHeight,
                Random.Range(_spawnArea.min.z, _spawnArea.max.z)
            );
            
            GameObject cockroachObj = Instantiate(_cockroachPrefab, randomPosition, Quaternion.identity, transform);
            cockroachObj.name = $"Cockroach_{_activeCockroaches.Count}";
            
            Cockroach cockroach = cockroachObj.GetComponent<Cockroach>();
            if (cockroach == null)
            {
                cockroach = cockroachObj.AddComponent<Cockroach>();
            }
            
            // Настраиваем таракана
            cockroach.Initialize(_cockroachSpeed, _directionChangeInterval, _spawnArea);
            cockroach.OnSquashed += OnCockroachSquashed;
            
            _activeCockroaches.Add(cockroach);
        }
        
        private void OnCockroachSquashed(Cockroach cockroach)
        {
            _activeCockroaches.Remove(cockroach);
            
            // Проверяем, остались ли тараканы
            if (_activeCockroaches.Count == 0)
            {
                Debug.Log("[CockroachMiniGame] Все тараканы уничтожены! Игра завершена.");
                // Можно добавить логику завершения игры здесь
            }
        }
        
        private void ClearAllCockroaches()
        {
            foreach (var cockroach in _activeCockroaches)
            {
                if (cockroach != null)
                {
                    cockroach.OnSquashed -= OnCockroachSquashed;
                    Destroy(cockroach.gameObject);
                }
            }
            _activeCockroaches.Clear();
        }
        
        /// <summary>
        /// Обрабатывает клик мыши для уничтожения тараканов.
        /// Вызывается из PlayerMovementController в режиме мини-игры.
        /// </summary>
        public void HandleCockroachClick(Vector3 clickPosition)
        {
            if (!_isActive)
                return;
            
            // Проверяем всех тараканов на расстояние от клика
            for (int i = _activeCockroaches.Count - 1; i >= 0; i--)
            {
                if (_activeCockroaches[i] == null)
                {
                    _activeCockroaches.RemoveAt(i);
                    continue;
                }
                
                float distance = Vector3.Distance(clickPosition, _activeCockroaches[i].transform.position);
                if (distance <= _clickRadius)
                {
                    _activeCockroaches[i].Squash();
                    break; // Уничтожаем только одного таракана за клик
                }
            }
        }
        
        private void CreateDefaultCockroachPrefab()
        {
            // Создаем простой префаб таракана из примитива
            GameObject defaultPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            defaultPrefab.name = "DefaultCockroach";
            defaultPrefab.transform.localScale = new Vector3(0.3f, 0.15f, 0.5f);
            
            // Удаляем стандартный коллайдер и добавляем сферу для лучшего клика
            CapsuleCollider capsuleCollider = defaultPrefab.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(capsuleCollider);
                #else
                Destroy(capsuleCollider);
                #endif
            }
            
            SphereCollider collider = defaultPrefab.AddComponent<SphereCollider>();
            collider.radius = 0.3f;
            collider.isTrigger = false;
            
            // Настраиваем материал
            Renderer renderer = defaultPrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (_cockroachMaterial != null)
                {
                    renderer.material = _cockroachMaterial;
                }
                else
                {
                    // Создаем темно-коричневый материал
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.3f, 0.2f, 0.1f);
                    renderer.material = mat;
                }
            }
            
            // Добавляем компонент Cockroach
            defaultPrefab.AddComponent<Cockroach>();
            
            _cockroachPrefab = defaultPrefab;
            
            Debug.Log("[CockroachMiniGame] Создан дефолтный префаб таракана. Рекомендуется создать собственный префаб.");
        }
        
        private void OnDrawGizmosSelected()
        {
            // Рисуем зону спавна в редакторе
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_spawnArea.center, _spawnArea.size);
        }
    }
}
