using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using InteractiveMuseum.Camera;

namespace InteractiveMuseum.PipeSystem
{
    /// <summary>
    /// Manages a grid of pipe segments that form a puzzle.
    /// </summary>
    public class PipeGridSystem : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [Tooltip("Number of pipes horizontally")]
        [SerializeField]
        private int _gridWidth = 3;
        
        [Tooltip("Number of pipes vertically")]
        [SerializeField]
        private int _gridHeight = 3;
        
        [Tooltip("Distance between pipe centers")]
        [SerializeField]
        private float _pipeSpacing = 2f;
        
        [Tooltip("Starting position of the grid")]
        [SerializeField]
        private Vector3 _gridOrigin = Vector3.zero;
        
        [Header("Pipe Prefab")]
        [Tooltip("Prefab for pipe segment (optional - can be assigned manually)")]
        [SerializeField]
        private GameObject _pipePrefab;
        
        [Header("Layers")]
        [Tooltip("Layer mask for interaction")]
        [SerializeField]
        private LayerMask _interactionLayer = 1 << 0;
        
        [Header("Events")]
        [Tooltip("Invoked when the puzzle is solved")]
        [SerializeField]
        private UnityEvent _onPuzzleSolved;

        public int gridWidth
        {
            get => _gridWidth;
            set => _gridWidth = value;
        }

        public int gridHeight
        {
            get => _gridHeight;
            set => _gridHeight = value;
        }

        public float pipeSpacing
        {
            get => _pipeSpacing;
            set => _pipeSpacing = value;
        }

        public Vector3 gridOrigin
        {
            get => _gridOrigin;
            set => _gridOrigin = value;
        }

        public GameObject pipePrefab
        {
            get => _pipePrefab;
            set => _pipePrefab = value;
        }

        public LayerMask interactionLayer
        {
            get => _interactionLayer;
            set => _interactionLayer = value;
        }

        public UnityEvent onPuzzleSolved
        {
            get => _onPuzzleSolved;
            set => _onPuzzleSolved = value;
        }
        
        private PipeSegment[,] _pipeGrid;
        private bool _isActive = false;
        private CameraManager _cameraManager;
        
        private void Start()
        {
            _cameraManager = CameraManager.Instance;
            
            // Generate grid if prefab is assigned
            if (_pipePrefab != null && _pipeGrid == null)
            {
                GenerateGrid();
            }
            else
            {
                // Find existing pipes in children
                CollectExistingPipes();
            }
        }
        
        /// <summary>
        /// Generates a new grid of pipes from the prefab.
        /// </summary>
        public void GenerateGrid()
        {
            if (_pipePrefab == null)
            {
                Debug.LogError("Pipe prefab not assigned!");
                return;
            }
            
            _pipeGrid = new PipeSegment[_gridWidth, _gridHeight];
            
            // Clear existing children
            foreach (Transform child in transform)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            
            // Generate pipes
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    Vector3 position = _gridOrigin + new Vector3(
                        x * _pipeSpacing - (_gridWidth - 1) * _pipeSpacing * 0.5f,
                        0,
                        y * _pipeSpacing - (_gridHeight - 1) * _pipeSpacing * 0.5f
                    );
                    
                    GameObject pipeObj = Instantiate(_pipePrefab, position, Quaternion.identity, transform);
                    pipeObj.name = $"Pipe_{x}_{y}";
                    
                    PipeSegment pipe = pipeObj.GetComponent<PipeSegment>();
                    if (pipe == null)
                    {
                        pipe = pipeObj.AddComponent<PipeSegment>();
                    }
                    
                    pipe.gridX = x;
                    pipe.gridY = y;
                    _pipeGrid[x, y] = pipe;
                }
            }
        }
        
        private void CollectExistingPipes()
        {
            _pipeGrid = new PipeSegment[_gridWidth, _gridHeight];
            PipeSegment[] pipes = GetComponentsInChildren<PipeSegment>();
            
            foreach (var pipe in pipes)
            {
                if (pipe.gridX >= 0 && pipe.gridX < _gridWidth &&
                    pipe.gridY >= 0 && pipe.gridY < _gridHeight)
                {
                    _pipeGrid[pipe.gridX, pipe.gridY] = pipe;
                }
            }
        }
        
        /// <summary>
        /// Activates pipe mode and switches to pipe camera.
        /// </summary>
        public void ActivatePipeMode()
        {
            _isActive = true;
            
            if (_cameraManager != null)
            {
                _cameraManager.SwitchToPipeCamera();
            }
        }
        
        /// <summary>
        /// Deactivates pipe mode and switches back to player camera.
        /// </summary>
        public void DeactivatePipeMode()
        {
            _isActive = false;
            
            if (_cameraManager != null)
            {
                _cameraManager.SwitchToPlayerCamera();
            }
        }
        
        /// <summary>
        /// Returns whether pipe mode is currently active.
        /// </summary>
        public bool IsActive()
        {
            return _isActive;
        }
        
        /// <summary>
        /// Called when a pipe segment is clicked.
        /// </summary>
        public void OnPipeClicked(PipeSegment pipe)
        {
            if (!_isActive)
                return;
                
            if (pipe == null || pipe.IsRotating())
                return;
                
            pipe.Rotate();
            
            // Check connections after rotation animation completes
            StartCoroutine(CheckConnectionsAfterDelay(1f / pipe.rotationSpeed + 0.1f));
        }
        
        private IEnumerator CheckConnectionsAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            CheckAllConnections();
        }
        
        /// <summary>
        /// Checks all pipe connections and triggers puzzle solved event if complete.
        /// </summary>
        public void CheckAllConnections()
        {
            if (_pipeGrid == null)
                return;
                
            bool allConnected = true;
            
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    PipeSegment pipe = _pipeGrid[x, y];
                    if (pipe == null)
                        continue;
                        
                    // Check connection with neighbors
                    // North (y+1)
                    if (y + 1 < _gridHeight)
                    {
                        PipeSegment neighbor = _pipeGrid[x, y + 1];
                        if (neighbor != null)
                        {
                            bool pipeHasNorth = pipe.HasConnection(0); // North
                            bool neighborHasSouth = neighbor.HasConnection(2); // South
                            if (pipeHasNorth != neighborHasSouth)
                            {
                                allConnected = false;
                            }
                        }
                    }
                    
                    // East (x+1)
                    if (x + 1 < _gridWidth)
                    {
                        PipeSegment neighbor = _pipeGrid[x + 1, y];
                        if (neighbor != null)
                        {
                            bool pipeHasEast = pipe.HasConnection(1); // East
                            bool neighborHasWest = neighbor.HasConnection(3); // West
                            if (pipeHasEast != neighborHasWest)
                            {
                                allConnected = false;
                            }
                        }
                    }
                    
                    // South (y-1)
                    if (y - 1 >= 0)
                    {
                        PipeSegment neighbor = _pipeGrid[x, y - 1];
                        if (neighbor != null)
                        {
                            bool pipeHasSouth = pipe.HasConnection(2); // South
                            bool neighborHasNorth = neighbor.HasConnection(0); // North
                            if (pipeHasSouth != neighborHasNorth)
                            {
                                allConnected = false;
                            }
                        }
                    }
                    
                    // West (x-1)
                    if (x - 1 >= 0)
                    {
                        PipeSegment neighbor = _pipeGrid[x - 1, y];
                        if (neighbor != null)
                        {
                            bool pipeHasWest = pipe.HasConnection(3); // West
                            bool neighborHasEast = neighbor.HasConnection(1); // East
                            if (pipeHasWest != neighborHasEast)
                            {
                                allConnected = false;
                            }
                        }
                    }
                }
            }
            
            if (allConnected && IsPuzzleSolved())
            {
                _onPuzzleSolved?.Invoke();
                Debug.Log("Puzzle solved!");
            }
        }
        
        private bool IsPuzzleSolved()
        {
            // Additional validation: ensure all pipes are connected
            if (_pipeGrid == null || _gridWidth <= 0 || _gridHeight <= 0)
                return false;
                
            // Use flood fill to check if all pipes are connected
            bool[,] visited = new bool[_gridWidth, _gridHeight];
            int connectedCount = 0;
            
            // Start from first pipe
            if (_pipeGrid[0, 0] != null)
            {
                FloodFill(0, 0, visited, ref connectedCount);
            }
            
            // Count total pipes
            int totalPipes = 0;
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    if (_pipeGrid[x, y] != null)
                        totalPipes++;
                }
            }
            
            return connectedCount == totalPipes && totalPipes > 0;
        }
        
        private void FloodFill(int x, int y, bool[,] visited, ref int count)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return;
                
            if (visited[x, y] || _pipeGrid[x, y] == null)
                return;
                
            visited[x, y] = true;
            count++;
            
            PipeSegment pipe = _pipeGrid[x, y];
            
            // Check all four directions
            if (pipe.HasConnection(0) && y + 1 < _gridHeight && _pipeGrid[x, y + 1] != null)
            {
                PipeSegment neighbor = _pipeGrid[x, y + 1];
                if (neighbor.HasConnection(2))
                    FloodFill(x, y + 1, visited, ref count);
            }
            
            if (pipe.HasConnection(1) && x + 1 < _gridWidth && _pipeGrid[x + 1, y] != null)
            {
                PipeSegment neighbor = _pipeGrid[x + 1, y];
                if (neighbor.HasConnection(3))
                    FloodFill(x + 1, y, visited, ref count);
            }
            
            if (pipe.HasConnection(2) && y - 1 >= 0 && _pipeGrid[x, y - 1] != null)
            {
                PipeSegment neighbor = _pipeGrid[x, y - 1];
                if (neighbor.HasConnection(0))
                    FloodFill(x, y - 1, visited, ref count);
            }
            
            if (pipe.HasConnection(3) && x - 1 >= 0 && _pipeGrid[x - 1, y] != null)
            {
                PipeSegment neighbor = _pipeGrid[x - 1, y];
                if (neighbor.HasConnection(1))
                    FloodFill(x - 1, y, visited, ref count);
            }
        }
        
        /// <summary>
        /// Gets the pipe segment at the specified grid coordinates.
        /// </summary>
        public PipeSegment GetPipeAt(int x, int y)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return null;
            return _pipeGrid[x, y];
        }
    }
}
