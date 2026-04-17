using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using InteractiveMuseum.MiniGames;

namespace InteractiveMuseum.MiniGames.WireSystem
{
    /// <summary>
    /// Mini-game where the player connects wire endpoints by matching colors.
    /// </summary>
    public class WireColorMiniGame : MiniGameBase
    {
        [System.Serializable]
        private class WireConnection
        {
            public WireEndpoint left;
            public WireEndpoint right;
            public LineRenderer line;
        }

        [Header("Endpoints")]
        [SerializeField] private WireEndpoint[] _leftEndpoints;
        [SerializeField] private WireEndpoint[] _rightEndpoints;

        [Header("Line Visuals")]
        [SerializeField] private Material _lineMaterial;
        [SerializeField] private float _lineWidth = 0.04f;
        [SerializeField] private float _lineZOffset = 0f;

        [Header("Completion Flow")]
        [SerializeField] private UnityEvent _onPuzzleSolved;
        [SerializeField] private bool _autoExitOnSolved = true;
        [SerializeField] private bool _clearConnectionsOnDeactivate = true;

        private readonly List<WireConnection> _connections = new List<WireConnection>();
        private WireEndpoint _selectedLeftEndpoint;
        private bool _isSolved;

        public UnityEvent onPuzzleSolved => _onPuzzleSolved;
        public bool autoExitOnSolved
        {
            get => _autoExitOnSolved;
            set => _autoExitOnSolved = value;
        }

        protected override void OnMiniGameActivated()
        {
            base.OnMiniGameActivated();

            if (_isSolved)
            {
                return;
            }

            if (_selectedLeftEndpoint != null)
            {
                _selectedLeftEndpoint.SetSelected(false);
                _selectedLeftEndpoint = null;
            }
        }

        protected override void OnMiniGameDeactivated()
        {
            base.OnMiniGameDeactivated();

            if (_selectedLeftEndpoint != null)
            {
                _selectedLeftEndpoint.SetSelected(false);
                _selectedLeftEndpoint = null;
            }

            if (_clearConnectionsOnDeactivate && !_isSolved)
            {
                ResetPuzzle();
            }
        }

        public void SetEndpoints(WireEndpoint[] leftEndpoints, WireEndpoint[] rightEndpoints)
        {
            _leftEndpoints = leftEndpoints;
            _rightEndpoints = rightEndpoints;
            ResetPuzzle();
        }

        public void HandleEndpointClick(WireEndpoint endpoint)
        {
            if (!_isActive || endpoint == null || _isSolved)
            {
                return;
            }

            if (endpoint.isLocked)
            {
                return;
            }

            if (endpoint.side == WireEndpointSide.Left)
            {
                SelectLeftEndpoint(endpoint);
                return;
            }

            if (_selectedLeftEndpoint == null)
            {
                endpoint.ShowInvalidFlash();
                return;
            }

            TryConnect(_selectedLeftEndpoint, endpoint);
        }

        public void ResetPuzzle()
        {
            if (_selectedLeftEndpoint != null)
            {
                _selectedLeftEndpoint.SetSelected(false);
                _selectedLeftEndpoint = null;
            }

            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].line != null)
                {
                    Destroy(_connections[i].line.gameObject);
                }

                if (_connections[i].left != null)
                {
                    _connections[i].left.SetLocked(false);
                }

                if (_connections[i].right != null)
                {
                    _connections[i].right.SetLocked(false);
                }
            }

            _connections.Clear();
            _isSolved = false;
        }

        private void SelectLeftEndpoint(WireEndpoint endpoint)
        {
            if (_selectedLeftEndpoint != null)
            {
                _selectedLeftEndpoint.SetSelected(false);
            }

            _selectedLeftEndpoint = endpoint;
            _selectedLeftEndpoint.SetSelected(true);
        }

        private void TryConnect(WireEndpoint left, WireEndpoint right)
        {
            if (left == null || right == null)
            {
                return;
            }

            if (left.colorChannel != right.colorChannel)
            {
                left.ShowInvalidFlash();
                right.ShowInvalidFlash();
                left.SetSelected(false);
                _selectedLeftEndpoint = null;
                return;
            }

            if (HasConnection(left) || HasConnection(right))
            {
                left.ShowInvalidFlash();
                right.ShowInvalidFlash();
                left.SetSelected(false);
                _selectedLeftEndpoint = null;
                return;
            }

            var connection = new WireConnection
            {
                left = left,
                right = right,
                line = CreateLine(left, right)
            };

            _connections.Add(connection);
            left.SetLocked(true);
            right.SetLocked(true);
            _selectedLeftEndpoint = null;

            CheckSolved();
        }

        private bool HasConnection(WireEndpoint endpoint)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].left == endpoint || _connections[i].right == endpoint)
                {
                    return true;
                }
            }

            return false;
        }

        private void CheckSolved()
        {
            int targetCount = Mathf.Min(_leftEndpoints != null ? _leftEndpoints.Length : 0, _rightEndpoints != null ? _rightEndpoints.Length : 0);
            if (_connections.Count < targetCount || targetCount <= 0)
            {
                return;
            }

            if (_isSolved)
            {
                return;
            }

            _isSolved = true;
            _onPuzzleSolved?.Invoke();

            if (_autoExitOnSolved)
            {
                DeactivateMiniGame();
            }
        }

        private LineRenderer CreateLine(WireEndpoint left, WireEndpoint right)
        {
            var lineObject = new GameObject($"WireLine_{left.colorChannel}_{left.index}_{right.index}");
            lineObject.transform.SetParent(transform, false);

            var line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = _lineWidth;
            line.endWidth = _lineWidth;
            line.material = _lineMaterial != null ? _lineMaterial : new Material(Shader.Find("Sprites/Default"));
            line.startColor = WireEndpoint.ToUnityColor(left.colorChannel);
            line.endColor = line.startColor;

            Vector3 startPosition = left.GetAnchorPosition() + new Vector3(0f, 0f, _lineZOffset);
            Vector3 endPosition = right.GetAnchorPosition() + new Vector3(0f, 0f, _lineZOffset);
            line.SetPosition(0, startPosition);
            line.SetPosition(1, endPosition);

            return line;
        }
    }
}
