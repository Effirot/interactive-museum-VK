using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using InteractiveMuseum.Camera;
using InteractiveMuseum.Interaction;

namespace InteractiveMuseum.PipeSystem
{
    /// <summary>
    /// How pipe types are distributed in the grid.
    /// </summary>
    public enum PipeDistributionMode
    {
        Random,      // Randomly assign pipe types
        AllSame,     // All pipes use the same type
        Custom       // Use custom array to assign types
    }
    
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
        
        [Header("Pipe Prefabs")]
        [Tooltip("Prefab for straight pipe segment")]
        [SerializeField]
        private GameObject _straightPipePrefab;
        
        [Tooltip("Prefab for corner pipe segment")]
        [SerializeField]
        private GameObject _cornerPipePrefab;
        
        [Tooltip("Prefab for T-junction pipe segment")]
        [SerializeField]
        private GameObject _tJunctionPipePrefab;
        
        [Tooltip("Prefab for cross pipe segment")]
        [SerializeField]
        private GameObject _crossPipePrefab;
        
        [Header("Pipe Type Distribution")]
        [Tooltip("How to distribute pipe types: Random, AllSame, or Custom")]
        [SerializeField]
        private PipeDistributionMode _distributionMode = PipeDistributionMode.Random;
        
        [Tooltip("Default pipe type when using AllSame mode")]
        [SerializeField]
        private PipeType _defaultPipeType = PipeType.Straight;
        
        [Tooltip("Custom pipe type for each grid position (only used in Custom mode). Index = y * width + x")]
        [SerializeField]
        private PipeType[] _customPipeTypes;
        
        [Header("Layers")]
        [Tooltip("Layer mask for interaction")]
        [SerializeField]
        private LayerMask _interactionLayer = 1 << 0;
        
        [Header("Puzzle Configuration")]
        [Tooltip("Start position of the puzzle (grid coordinates)")]
        [SerializeField]
        private Vector2Int _startPosition = new Vector2Int(0, 0);
        
        [Tooltip("End position of the puzzle (grid coordinates)")]
        [SerializeField]
        private Vector2Int _endPosition = new Vector2Int(2, 2);
        
        [Tooltip("Randomize initial pipe rotations when generating grid")]
        [SerializeField]
        private bool _randomizeInitialRotations = false;
        
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

        public Vector2Int startPosition
        {
            get => _startPosition;
            set => _startPosition = value;
        }

        public Vector2Int endPosition
        {
            get => _endPosition;
            set => _endPosition = value;
        }

        public bool randomizeInitialRotations
        {
            get => _randomizeInitialRotations;
            set => _randomizeInitialRotations = value;
        }
        
        private PipeSegment[,] _pipeGrid;
        private bool _isActive = false;
        private CameraManager _cameraManager;
        
        private void Start()
        {
            _cameraManager = CameraManager.Instance;
            
            // Generate grid if at least one prefab is assigned
            if (HasAnyPrefab() && _pipeGrid == null)
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
        /// Checks if at least one pipe prefab is assigned.
        /// </summary>
        private bool HasAnyPrefab()
        {
            return _straightPipePrefab != null || 
                   _cornerPipePrefab != null || 
                   _tJunctionPipePrefab != null || 
                   _crossPipePrefab != null;
        }
        
        /// <summary>
        /// Generates a new grid of pipes from the prefabs.
        /// </summary>
        public void GenerateGrid()
        {
            _pipeGrid = new PipeSegment[_gridWidth, _gridHeight];
            
            // Clear existing children
            foreach (Transform child in transform)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
            
            // Generate pipes (rows go vertically, elements in row go horizontally)
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    // Determine pipe type based on distribution mode
                    PipeType pipeType = GetPipeTypeForPosition(x, y);
                    
                    // Get prefab for this pipe type
                    GameObject prefab = GetPrefabForType(pipeType);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"No prefab assigned for pipe type {pipeType} at position ({x}, {y}). Skipping.");
                        continue;
                    }
                    
                    Vector3 position = _gridOrigin + new Vector3(
                        x * _pipeSpacing - (_gridWidth - 1) * _pipeSpacing * 0.5f,
                        y * _pipeSpacing - (_gridHeight - 1) * _pipeSpacing * 0.5f,
                        0
                    );
                    
                    GameObject pipeObj = Instantiate(prefab, position, Quaternion.identity, transform);
                    pipeObj.name = $"Pipe_{pipeType}_{x}_{y}";
                    
                    PipeSegment pipe = pipeObj.GetComponent<PipeSegment>();
                    if (pipe == null)
                    {
                        pipe = pipeObj.AddComponent<PipeSegment>();
                    }
                    
                    // Set pipe type
                    pipe.pipeType = pipeType;
                    pipe.gridX = x;
                    pipe.gridY = y;
                    
                    // Randomize initial rotation if enabled
                    if (_randomizeInitialRotations)
                    {
                        int randomRotations = Random.Range(0, 4); // 0, 1, 2, or 3 rotations (0, 90, 180, 270 degrees)
                        pipe.SetInitialRotation(randomRotations);
                    }
                    
                    // Ensure PipeSegmentInteractable exists
                    PipeSegmentInteractable pipeInteractable = pipeObj.GetComponent<PipeSegmentInteractable>();
                    if (pipeInteractable == null)
                    {
                        pipeInteractable = pipeObj.AddComponent<PipeSegmentInteractable>();
                    }
                    
                    // Ensure InteractableOutline exists for highlighting
                    InteractiveMuseum.Interaction.InteractableOutline outline = pipeObj.GetComponent<InteractiveMuseum.Interaction.InteractableOutline>();
                    if (outline == null)
                    {
                        outline = pipeObj.AddComponent<InteractiveMuseum.Interaction.InteractableOutline>();
                    }
                    
                    _pipeGrid[x, y] = pipe;
                }
            }
        }
        
        /// <summary>
        /// Gets the pipe type for a specific grid position based on distribution mode.
        /// </summary>
        private PipeType GetPipeTypeForPosition(int x, int y)
        {
            switch (_distributionMode)
            {
                case PipeDistributionMode.Random:
                    // Randomly select from available types
                    List<PipeType> availableTypes = new List<PipeType>();
                    if (_straightPipePrefab != null) availableTypes.Add(PipeType.Straight);
                    if (_cornerPipePrefab != null) availableTypes.Add(PipeType.Corner);
                    if (_tJunctionPipePrefab != null) availableTypes.Add(PipeType.TJunction);
                    if (_crossPipePrefab != null) availableTypes.Add(PipeType.Cross);
                    
                    if (availableTypes.Count == 0)
                    {
                        Debug.LogWarning("No pipe prefabs assigned! Using default Straight type.");
                        return PipeType.Straight;
                    }
                    
                    return availableTypes[Random.Range(0, availableTypes.Count)];
                    
                case PipeDistributionMode.AllSame:
                    return _defaultPipeType;
                    
                case PipeDistributionMode.Custom:
                    int index = y * _gridWidth + x;
                    if (_customPipeTypes != null && index >= 0 && index < _customPipeTypes.Length)
                    {
                        return _customPipeTypes[index];
                    }
                    else
                    {
                        Debug.LogWarning($"Custom pipe type array doesn't have entry for position ({x}, {y}). Using default.");
                        return _defaultPipeType;
                    }
                    
                default:
                    return PipeType.Straight;
            }
        }
        
        /// <summary>
        /// Gets the prefab for a specific pipe type.
        /// </summary>
        private GameObject GetPrefabForType(PipeType type)
        {
            switch (type)
            {
                case PipeType.Straight:
                    return _straightPipePrefab;
                case PipeType.Corner:
                    return _cornerPipePrefab;
                case PipeType.TJunction:
                    return _tJunctionPipePrefab;
                case PipeType.Cross:
                    return _crossPipePrefab;
                default:
                    return _straightPipePrefab;
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
        /// <param name="pipe">The pipe segment to rotate</param>
        /// <param name="clockwise">True for clockwise rotation, false for counter-clockwise</param>
        public void OnPipeClicked(PipeSegment pipe, bool clockwise = true)
        {
            if (!_isActive)
                return;
                
            if (pipe == null || pipe.IsRotating())
                return;
                
            pipe.Rotate(clockwise);
            
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
            
            // Validate start and end positions
            if (_startPosition.x < 0 || _startPosition.x >= _gridWidth ||
                _startPosition.y < 0 || _startPosition.y >= _gridHeight)
            {
                Debug.LogWarning("Start position is out of bounds!");
                return false;
            }
            
            if (_endPosition.x < 0 || _endPosition.x >= _gridWidth ||
                _endPosition.y < 0 || _endPosition.y >= _gridHeight)
            {
                Debug.LogWarning("End position is out of bounds!");
                return false;
            }
            
            if (_pipeGrid[_startPosition.x, _startPosition.y] == null)
            {
                Debug.LogWarning("Start position has no pipe!");
                return false;
            }
            
            if (_pipeGrid[_endPosition.x, _endPosition.y] == null)
            {
                Debug.LogWarning("End position has no pipe!");
                return false;
            }
                
            // Use flood fill to check if path exists from start to end
            bool[,] visited = new bool[_gridWidth, _gridHeight];
            bool endReached = false;
            
            // Start flood fill from start position
            FloodFillToEnd(_startPosition.x, _startPosition.y, visited, ref endReached);
            
            return endReached;
        }
        
        /// <summary>
        /// Flood fill from start position to check if end position is reachable.
        /// </summary>
        private void FloodFillToEnd(int x, int y, bool[,] visited, ref bool endReached)
        {
            if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return;
                
            if (visited[x, y] || _pipeGrid[x, y] == null)
                return;
            
            // Check if we reached the end position
            if (x == _endPosition.x && y == _endPosition.y)
            {
                endReached = true;
                return;
            }
                
            visited[x, y] = true;
            
            PipeSegment pipe = _pipeGrid[x, y];
            
            // Check all four directions and continue flood fill if connected
            // North (y+1)
            if (pipe.HasConnection(0) && y + 1 < _gridHeight && _pipeGrid[x, y + 1] != null)
            {
                PipeSegment neighbor = _pipeGrid[x, y + 1];
                if (neighbor.HasConnection(2)) // Neighbor has south connection
                {
                    FloodFillToEnd(x, y + 1, visited, ref endReached);
                    if (endReached) return;
                }
            }
            
            // East (x+1)
            if (pipe.HasConnection(1) && x + 1 < _gridWidth && _pipeGrid[x + 1, y] != null)
            {
                PipeSegment neighbor = _pipeGrid[x + 1, y];
                if (neighbor.HasConnection(3)) // Neighbor has west connection
                {
                    FloodFillToEnd(x + 1, y, visited, ref endReached);
                    if (endReached) return;
                }
            }
            
            // South (y-1)
            if (pipe.HasConnection(2) && y - 1 >= 0 && _pipeGrid[x, y - 1] != null)
            {
                PipeSegment neighbor = _pipeGrid[x, y - 1];
                if (neighbor.HasConnection(0)) // Neighbor has north connection
                {
                    FloodFillToEnd(x, y - 1, visited, ref endReached);
                    if (endReached) return;
                }
            }
            
            // West (x-1)
            if (pipe.HasConnection(3) && x - 1 >= 0 && _pipeGrid[x - 1, y] != null)
            {
                PipeSegment neighbor = _pipeGrid[x - 1, y];
                if (neighbor.HasConnection(1)) // Neighbor has east connection
                {
                    FloodFillToEnd(x - 1, y, visited, ref endReached);
                    if (endReached) return;
                }
            }
        }
        
        /// <summary>
        /// Legacy flood fill method for checking all pipes connectivity (kept for compatibility).
        /// </summary>
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
