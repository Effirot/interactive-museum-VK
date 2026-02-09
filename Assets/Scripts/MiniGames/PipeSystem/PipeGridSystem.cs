using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using InteractiveMuseum.Camera;
using InteractiveMuseum.Interaction;
using InteractiveMuseum.MiniGames;

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
    /// Side of the grid for entry/exit pipe placement.
    /// </summary>
    public enum EntryExitSide
    {
        North,  // y+ from grid (top)
        South,  // y- from grid (bottom)
        East,   // x+ from grid (right)
        West    // x- from grid (left)
    }
    
    /// <summary>
    /// Manages a grid of pipe segments that form a puzzle.
    /// </summary>
    public class PipeGridSystem : MiniGameBase
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
        
        [Header("Entry/Exit Configuration")]
        [Tooltip("Enable entry and exit pipes outside the grid")]
        [SerializeField]
        private bool _enableEntryExit = false;
        
        [Tooltip("Side of the grid where entry pipe is placed")]
        [SerializeField]
        private EntryExitSide _entrySide = EntryExitSide.West;
        
        [Tooltip("Side of the grid where exit pipe is placed")]
        [SerializeField]
        private EntryExitSide _exitSide = EntryExitSide.East;
        
        private PipeSegment _entryPipe = null;
        private PipeSegment _exitPipe = null;

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
        
        public bool enableEntryExit
        {
            get => _enableEntryExit;
            set => _enableEntryExit = value;
        }
        
        public EntryExitSide entrySide
        {
            get => _entrySide;
            set => _entrySide = value;
        }
        
        public EntryExitSide exitSide
        {
            get => _exitSide;
            set => _exitSide = value;
        }
        
        private PipeSegment[,] _pipeGrid;
        
        protected override void Start()
        {
            base.Start();
            
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
            
            // Save entry/exit pipes before clearing (they will be recreated if enabled)
            bool hadEntryExit = _enableEntryExit && (_entryPipe != null || _exitPipe != null);
            
            // Clear existing children (except we'll recreate entry/exit if needed)
            // First, remove entry/exit pipes explicitly to clear references
            RemoveEntryExitPipes();
            
            // Then clear remaining children (grid pipes)
            List<Transform> childrenToRemove = new List<Transform>();
            foreach (Transform child in transform)
            {
                childrenToRemove.Add(child);
            }
            foreach (Transform child in childrenToRemove)
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
            
            // Spawn entry/exit pipes if enabled
            if (_enableEntryExit)
            {
                SpawnEntryPipe();
                SpawnExitPipe();
            }
        }
        
        /// <summary>
        /// Calculates the world position for a grid coordinate.
        /// </summary>
        private Vector3 GetWorldPositionForGrid(int x, int y)
        {
            return _gridOrigin + new Vector3(
                x * _pipeSpacing - (_gridWidth - 1) * _pipeSpacing * 0.5f,
                y * _pipeSpacing - (_gridHeight - 1) * _pipeSpacing * 0.5f,
                0
            );
        }
        
        /// <summary>
        /// Calculates the position for entry/exit pipe based on side and grid position.
        /// </summary>
        private Vector3 GetEntryExitPosition(EntryExitSide side, int gridX, int gridY)
        {
            Vector3 gridPos = GetWorldPositionForGrid(gridX, gridY);
            float offset = _pipeSpacing;
            
            switch (side)
            {
                case EntryExitSide.North: // y+ (top)
                    return gridPos + new Vector3(0, offset, 0);
                case EntryExitSide.South: // y- (bottom)
                    return gridPos + new Vector3(0, -offset, 0);
                case EntryExitSide.East: // x+ (right)
                    return gridPos + new Vector3(offset, 0, 0);
                case EntryExitSide.West: // x- (left)
                    return gridPos + new Vector3(-offset, 0, 0);
                default:
                    return gridPos;
            }
        }
        
        /// <summary>
        /// Gets the direction index (0=North, 1=East, 2=South, 3=West) pointing from the pipe towards the grid.
        /// </summary>
        private int GetDirectionToGrid(EntryExitSide side)
        {
            switch (side)
            {
                case EntryExitSide.North:
                    return 2; // South (pipe looks down to grid)
                case EntryExitSide.South:
                    return 0; // North (pipe looks up to grid)
                case EntryExitSide.East:
                    return 3; // West (pipe looks left to grid)
                case EntryExitSide.West:
                    return 1; // East (pipe looks right to grid)
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// Calculates the rotation count needed for a corner pipe to have one connection pointing to grid and one pointing away.
        /// Corner pipes always have two adjacent connections (not opposite), so we choose a neighbor direction for the "away" connection.
        /// </summary>
        private int CalculateCornerRotation(EntryExitSide side, bool isEntry)
        {
            // Corner pipe default connections: North (0) and East (1)
            // For entry/exit pipe: one connection points to/from grid, one points to a neighboring direction (away)
            
            int directionToGrid = GetDirectionToGrid(side);
            
            // For corner pipe, we need two adjacent directions. Choose a neighbor of directionToGrid for the "away" direction.
            // We'll use the clockwise neighbor: (directionToGrid + 1) % 4
            int directionAway = (directionToGrid + 1) % 4;
            
            // Default corner pipe has connections at 0 (North) and 1 (East)
            // We need to rotate so that it has connections at directionToGrid and directionAway
            
            // Try all 4 rotations and find the one that matches
            for (int rotation = 0; rotation < 4; rotation++)
            {
                // Rotated connections for corner (default: 0,1) after rotation
                int conn1 = (0 + rotation) % 4;
                int conn2 = (1 + rotation) % 4;
                
                // Check if rotated connections match what we need
                bool hasDirectionToGrid = (conn1 == directionToGrid || conn2 == directionToGrid);
                bool hasDirectionAway = (conn1 == directionAway || conn2 == directionAway);
                
                if (hasDirectionToGrid && hasDirectionAway)
                {
                    return rotation;
                }
            }
            
            return 0; // Default rotation (shouldn't happen, but safety fallback)
        }
        
        /// <summary>
        /// Spawns the entry pipe outside the grid.
        /// </summary>
        public void SpawnEntryPipe()
        {
            // Remove existing entry pipe
            if (_entryPipe != null)
            {
                if (Application.isPlaying)
                    Destroy(_entryPipe.gameObject);
                else
                    DestroyImmediate(_entryPipe.gameObject);
                _entryPipe = null;
            }
            
            if (_cornerPipePrefab == null)
            {
                Debug.LogWarning("Corner pipe prefab is required for entry pipe but is not assigned!");
                return;
            }
            
            // Calculate position
            Vector3 entryPos = GetEntryExitPosition(_entrySide, _startPosition.x, _startPosition.y);
            
            // Spawn corner pipe
            GameObject entryObj = Instantiate(_cornerPipePrefab, entryPos, Quaternion.identity, transform);
            entryObj.name = "EntryPipe";
            
            PipeSegment entryPipe = entryObj.GetComponent<PipeSegment>();
            if (entryPipe == null)
            {
                entryPipe = entryObj.AddComponent<PipeSegment>();
            }
            
            // Set pipe type
            entryPipe.pipeType = PipeType.Corner;
            entryPipe.gridX = -1; // Special marker for entry pipe
            entryPipe.gridY = -1;
            
            // Calculate and set rotation so one end points to grid, other points away (into wall)
            int rotation = CalculateCornerRotation(_entrySide, true);
            entryPipe.SetInitialRotation(rotation);
            
            // Ensure PipeSegmentInteractable exists
            PipeSegmentInteractable pipeInteractable = entryObj.GetComponent<PipeSegmentInteractable>();
            if (pipeInteractable == null)
            {
                pipeInteractable = entryObj.AddComponent<PipeSegmentInteractable>();
            }
            
            // Ensure InteractableOutline exists
            InteractiveMuseum.Interaction.InteractableOutline outline = entryObj.GetComponent<InteractiveMuseum.Interaction.InteractableOutline>();
            if (outline == null)
            {
                outline = entryObj.AddComponent<InteractiveMuseum.Interaction.InteractableOutline>();
            }
            
            _entryPipe = entryPipe;
        }
        
        /// <summary>
        /// Spawns the exit pipe outside the grid.
        /// </summary>
        public void SpawnExitPipe()
        {
            // Remove existing exit pipe
            if (_exitPipe != null)
            {
                if (Application.isPlaying)
                    Destroy(_exitPipe.gameObject);
                else
                    DestroyImmediate(_exitPipe.gameObject);
                _exitPipe = null;
            }
            
            if (_cornerPipePrefab == null)
            {
                Debug.LogWarning("Corner pipe prefab is required for exit pipe but is not assigned!");
                return;
            }
            
            // Calculate position
            Vector3 exitPos = GetEntryExitPosition(_exitSide, _endPosition.x, _endPosition.y);
            
            // Spawn corner pipe
            GameObject exitObj = Instantiate(_cornerPipePrefab, exitPos, Quaternion.identity, transform);
            exitObj.name = "ExitPipe";
            
            PipeSegment exitPipe = exitObj.GetComponent<PipeSegment>();
            if (exitPipe == null)
            {
                exitPipe = exitObj.AddComponent<PipeSegment>();
            }
            
            // Set pipe type
            exitPipe.pipeType = PipeType.Corner;
            exitPipe.gridX = -2; // Special marker for exit pipe
            exitPipe.gridY = -2;
            
            // Calculate and set rotation so one end points from grid, other points away (into wall)
            int rotation = CalculateCornerRotation(_exitSide, false);
            exitPipe.SetInitialRotation(rotation);
            
            // Ensure PipeSegmentInteractable exists
            PipeSegmentInteractable pipeInteractable = exitObj.GetComponent<PipeSegmentInteractable>();
            if (pipeInteractable == null)
            {
                pipeInteractable = exitObj.AddComponent<PipeSegmentInteractable>();
            }
            
            // Ensure InteractableOutline exists
            InteractiveMuseum.Interaction.InteractableOutline outline = exitObj.GetComponent<InteractiveMuseum.Interaction.InteractableOutline>();
            if (outline == null)
            {
                outline = exitObj.AddComponent<InteractiveMuseum.Interaction.InteractableOutline>();
            }
            
            _exitPipe = exitPipe;
        }
        
        /// <summary>
        /// Removes entry and exit pipes.
        /// </summary>
        public void RemoveEntryExitPipes()
        {
            if (_entryPipe != null)
            {
                if (Application.isPlaying)
                    Destroy(_entryPipe.gameObject);
                else
                    DestroyImmediate(_entryPipe.gameObject);
                _entryPipe = null;
            }
            
            if (_exitPipe != null)
            {
                if (Application.isPlaying)
                    Destroy(_exitPipe.gameObject);
                else
                    DestroyImmediate(_exitPipe.gameObject);
                _exitPipe = null;
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
                // Check if this is an entry pipe
                if (pipe.gridX == -1 && pipe.gridY == -1)
                {
                    _entryPipe = pipe;
                }
                // Check if this is an exit pipe
                else if (pipe.gridX == -2 && pipe.gridY == -2)
                {
                    _exitPipe = pipe;
                }
                // Regular grid pipe
                else if (pipe.gridX >= 0 && pipe.gridX < _gridWidth &&
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
            ActivateMiniGame();
        }
        
        /// <summary>
        /// Deactivates pipe mode and switches back to player camera.
        /// </summary>
        public void DeactivatePipeMode()
        {
            DeactivateMiniGame();
        }
        
        /// <summary>
        /// Returns whether pipe mode is currently active.
        /// </summary>
        public new bool IsActive()
        {
            return base.IsActive();
        }
        
        protected override void OnMiniGameActivated()
        {
            base.OnMiniGameActivated();
            // Additional pipe-specific activation logic can be added here
        }
        
        protected override void OnMiniGameDeactivated()
        {
            base.OnMiniGameDeactivated();
            // Additional pipe-specific deactivation logic can be added here
        }
        
        /// <summary>
        /// Called when a pipe segment is clicked.
        /// </summary>
        /// <param name="pipe">The pipe segment to rotate</param>
        /// <param name="clockwise">True for clockwise rotation, false for counter-clockwise</param>
        public void OnPipeClicked(PipeSegment pipe, bool clockwise = true)
        {
            if (!IsActive())
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
            
            // Check connections within grid
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
            
            // Check connection between entry pipe and start position (if entry/exit enabled)
            if (_enableEntryExit && _entryPipe != null)
            {
                PipeSegment startPipe = _pipeGrid[_startPosition.x, _startPosition.y];
                if (startPipe != null)
                {
                    int directionToGrid = GetDirectionToGrid(_entrySide);
                    int directionFromEntry = (directionToGrid + 2) % 4; // Direction from grid to entry pipe
                    
                    bool entryHasConnection = _entryPipe.HasConnection(directionToGrid);
                    bool startHasConnection = startPipe.HasConnection(directionFromEntry);
                    
                    if (entryHasConnection != startHasConnection)
                    {
                        allConnected = false;
                    }
                }
            }
            
            // Check connection between end position and exit pipe (if entry/exit enabled)
            if (_enableEntryExit && _exitPipe != null)
            {
                PipeSegment endPipe = _pipeGrid[_endPosition.x, _endPosition.y];
                if (endPipe != null)
                {
                    int directionToGrid = GetDirectionToGrid(_exitSide);
                    int directionFromExit = (directionToGrid + 2) % 4; // Direction from grid to exit pipe
                    
                    bool endHasConnection = endPipe.HasConnection(directionFromExit);
                    bool exitHasConnection = _exitPipe.HasConnection(directionToGrid);
                    
                    if (endHasConnection != exitHasConnection)
                    {
                        allConnected = false;
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
                
            // Use flood fill to check if path exists from entry/start to exit/end
            bool[,] visited = new bool[_gridWidth, _gridHeight];
            bool endReached = false;
            
            // Check if entry/exit pipes are connected first
            if (_enableEntryExit)
            {
                // Check entry pipe connection to start
                if (_entryPipe != null)
                {
                    int directionToGrid = GetDirectionToGrid(_entrySide);
                    if (!_entryPipe.HasConnection(directionToGrid))
                    {
                        return false; // Entry pipe not connected to grid
                    }
                    
                    PipeSegment startPipe = _pipeGrid[_startPosition.x, _startPosition.y];
                    if (startPipe != null)
                    {
                        int directionFromEntry = (directionToGrid + 2) % 4;
                        if (!startPipe.HasConnection(directionFromEntry))
                        {
                            return false; // Start pipe not connected to entry
                        }
                    }
                }
                
                // Check exit pipe connection from end
                if (_exitPipe != null)
                {
                    int directionToGrid = GetDirectionToGrid(_exitSide);
                    if (!_exitPipe.HasConnection(directionToGrid))
                    {
                        return false; // Exit pipe not connected to grid
                    }
                    
                    PipeSegment endPipe = _pipeGrid[_endPosition.x, _endPosition.y];
                    if (endPipe != null)
                    {
                        int directionFromExit = (directionToGrid + 2) % 4;
                        if (!endPipe.HasConnection(directionFromExit))
                        {
                            return false; // End pipe not connected to exit
                        }
                    }
                }
            }
            
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
