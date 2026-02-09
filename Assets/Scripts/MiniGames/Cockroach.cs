using UnityEngine;
using System;

namespace InteractiveMuseum.MiniGames
{
    /// <summary>
    /// Компонент для отдельного таракана. Управляет движением и обработкой уничтожения.
    /// </summary>
    public class Cockroach : MonoBehaviour
    {
        private float _speed = 2f;
        private float _directionChangeInterval = 1f;
        private Bounds _movementBounds;
        
        private Vector3 _currentDirection;
        private float _timeSinceDirectionChange = 0f;
        private bool _isSquashed = false;
        
        public event Action<Cockroach> OnSquashed;
        
        /// <summary>
        /// Инициализирует таракана с заданными параметрами.
        /// </summary>
        public void Initialize(float speed, float directionChangeInterval, Bounds movementBounds)
        {
            _speed = speed;
            _directionChangeInterval = directionChangeInterval;
            _movementBounds = movementBounds;
            
            // Устанавливаем случайное начальное направление
            ChangeDirection();
            
            _isSquashed = false;
        }
        
        private void Update()
        {
            if (_isSquashed)
                return;
            
            // Обновляем направление движения периодически
            _timeSinceDirectionChange += Time.deltaTime;
            if (_timeSinceDirectionChange >= _directionChangeInterval)
            {
                ChangeDirection();
                _timeSinceDirectionChange = 0f;
            }
            
            // Двигаем таракана
            Move();
            
            // Проверяем границы и корректируем направление
            CheckBounds();
        }
        
        private void Move()
        {
            Vector3 movement = _currentDirection * _speed * Time.deltaTime;
            transform.position += movement;
            
            // Поворачиваем таракана в направлении движения
            if (_currentDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_currentDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        
        private void ChangeDirection()
        {
            // Генерируем случайное направление в горизонтальной плоскости
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _currentDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
        }
        
        private void CheckBounds()
        {
            Vector3 pos = transform.position;
            bool needsDirectionChange = false;
            
            // Проверяем границы по X
            if (pos.x < _movementBounds.min.x || pos.x > _movementBounds.max.x)
            {
                _currentDirection.x *= -1f;
                needsDirectionChange = true;
            }
            
            // Проверяем границы по Z
            if (pos.z < _movementBounds.min.z || pos.z > _movementBounds.max.z)
            {
                _currentDirection.z *= -1f;
                needsDirectionChange = true;
            }
            
            // Ограничиваем позицию границами
            pos.x = Mathf.Clamp(pos.x, _movementBounds.min.x, _movementBounds.max.x);
            pos.z = Mathf.Clamp(pos.z, _movementBounds.min.z, _movementBounds.max.z);
            transform.position = pos;
            
            if (needsDirectionChange)
            {
                _currentDirection.Normalize();
            }
        }
        
        /// <summary>
        /// Уничтожает таракана при клике.
        /// </summary>
        public void Squash()
        {
            if (_isSquashed)
                return;
            
            _isSquashed = true;
            
            // Можно добавить эффект уничтожения (анимация, частицы и т.д.)
            OnSquashed?.Invoke(this);
            
            // Уничтожаем объект с небольшой задержкой для возможной анимации
            Destroy(gameObject, 0.1f);
        }
        
        private void OnDestroy()
        {
            // Отписываемся от события при уничтожении
            OnSquashed = null;
        }
    }
}
