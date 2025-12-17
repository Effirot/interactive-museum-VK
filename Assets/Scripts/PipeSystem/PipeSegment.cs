using UnityEngine;
using System.Collections;

namespace InteractiveMuseum.PipeSystem
{
    /// <summary>
    /// Types of pipe segments.
    /// </summary>
    public enum PipeType
    {
        Straight,      // Two opposite connections
        Corner,        // Two adjacent connections (90 degrees)
        TJunction,     // Three connections (T-shaped)
        Cross          // Four connections (all directions)
    }

    /// <summary>
    /// Represents a single pipe segment that can be rotated.
    /// </summary>
    public class PipeSegment : MonoBehaviour
    {
        [Header("Pipe Configuration")]
        [Tooltip("Type of pipe segment")]
        [SerializeField]
        private PipeType _pipeType = PipeType.Straight;
        
        [Header("Connection Directions (North, East, South, West)")]
        [Tooltip("Connection directions: 0=North, 1=East, 2=South, 3=West")]
        [SerializeField]
        private bool[] _connections = new bool[4];
        
        [Header("Rotation Settings")]
        [Tooltip("Speed of rotation animation")]
        [SerializeField]
        private float _rotationSpeed = 5f;
        
        [HideInInspector]
        [SerializeField]
        private int _gridX = -1;
        
        [HideInInspector]
        [SerializeField]
        private int _gridY = -1;

        public PipeType pipeType
        {
            get => _pipeType;
            set => _pipeType = value;
        }

        public bool[] connections
        {
            get => _connections;
            set => _connections = value;
        }

        public float rotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        public int gridX
        {
            get => _gridX;
            set => _gridX = value;
        }

        public int gridY
        {
            get => _gridY;
            set => _gridY = value;
        }
        
        private int _rotationAngle = 0; // Current rotation: 0, 90, 180, 270
        private bool _isRotating = false;
        
        private void Awake()
        {
            InitializeConnections();
        }
        
        private void InitializeConnections()
        {
            // Set initial connections based on pipe type
            _connections = new bool[4];
            
            switch (_pipeType)
            {
                case PipeType.Straight:
                    _connections[0] = true; // North
                    _connections[2] = true; // South
                    break;
                case PipeType.Corner:
                    _connections[0] = true; // North
                    _connections[1] = true; // East
                    break;
                case PipeType.TJunction:
                    _connections[0] = true; // North
                    _connections[1] = true; // East
                    _connections[2] = true; // South
                    break;
                case PipeType.Cross:
                    _connections[0] = true; // North
                    _connections[1] = true; // East
                    _connections[2] = true; // South
                    _connections[3] = true; // West
                    break;
            }
        }
        
        /// <summary>
        /// Rotates the pipe segment by 90 degrees.
        /// </summary>
        public void Rotate()
        {
            if (_isRotating)
                return;
                
            StartCoroutine(RotateCoroutine());
        }
        
        private IEnumerator RotateCoroutine()
        {
            _isRotating = true;
            
            // Rotate by 90 degrees
            _rotationAngle = (_rotationAngle + 90) % 360;
            
            Quaternion targetRotation = Quaternion.Euler(0, _rotationAngle, 0);
            Quaternion startRotation = transform.localRotation;
            
            float elapsedTime = 0f;
            float rotationDuration = 1f / _rotationSpeed;
            
            while (elapsedTime < rotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / rotationDuration;
                transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);
                yield return null;
            }
            
            transform.localRotation = targetRotation;
            
            // Rotate connections array
            RotateConnections();
            
            _isRotating = false;
        }
        
        private void RotateConnections()
        {
            // Rotate connections array clockwise
            bool last = _connections[3];
            for (int i = 3; i > 0; i--)
            {
                _connections[i] = _connections[i - 1];
            }
            _connections[0] = last;
        }
        
        /// <summary>
        /// Gets connection in a specific direction (after rotation).
        /// </summary>
        /// <param name="direction">Direction index: 0=North, 1=East, 2=South, 3=West</param>
        public bool HasConnection(int direction)
        {
            if (direction < 0 || direction >= 4)
                return false;
            return _connections[direction];
        }
        
        /// <summary>
        /// Gets connection direction adjusted for rotation.
        /// </summary>
        public bool HasConnectionInDirection(Vector2Int direction)
        {
            // Convert world direction to connection index
            // North = (0, 1) = 0
            // East = (1, 0) = 1
            // South = (0, -1) = 2
            // West = (-1, 0) = 3
            
            int index = -1;
            if (direction == Vector2Int.up) index = 0;
            else if (direction == Vector2Int.right) index = 1;
            else if (direction == Vector2Int.down) index = 2;
            else if (direction == Vector2Int.left) index = 3;
            
            if (index >= 0)
                return HasConnection(index);
            return false;
        }
        
        /// <summary>
        /// Gets the current rotation angle.
        /// </summary>
        public int GetRotationAngle()
        {
            return _rotationAngle;
        }
        
        /// <summary>
        /// Returns whether the pipe is currently rotating.
        /// </summary>
        public bool IsRotating()
        {
            return _isRotating;
        }
        
        // For editor/debugging
        private void OnValidate()
        {
            if (_connections == null || _connections.Length != 4)
            {
                _connections = new bool[4];
            }
            InitializeConnections();
        }
    }
}
