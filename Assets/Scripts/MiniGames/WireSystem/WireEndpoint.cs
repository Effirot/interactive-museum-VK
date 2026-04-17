using System.Collections;
using UnityEngine;

namespace InteractiveMuseum.MiniGames.WireSystem
{
    public enum WireEndpointSide
    {
        Left,
        Right
    }

    public enum WireColorChannel
    {
        Red,
        Green,
        Blue,
        Yellow,
        Orange,
        Purple
    }

    /// <summary>
    /// Single wire socket that can be selected and connected by color.
    /// </summary>
    public class WireEndpoint : MonoBehaviour
    {
        [Header("Endpoint Data")]
        [SerializeField] private WireColorChannel _colorChannel = WireColorChannel.Red;
        [SerializeField] private WireEndpointSide _side = WireEndpointSide.Left;
        [SerializeField] private int _index = 0;

        [Header("Visuals")]
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _lockedColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f);

        private bool _isSelected;
        private bool _isLocked;
        private Coroutine _invalidFlashRoutine;

        public WireColorChannel colorChannel => _colorChannel;
        public WireEndpointSide side => _side;
        public int index => _index;
        public bool isLocked => _isLocked;

        private void Awake()
        {
            if (_targetRenderer == null)
            {
                _targetRenderer = GetComponentInChildren<Renderer>();
            }

            ApplyVisual();
        }

        public void Configure(WireColorChannel colorChannel, WireEndpointSide side, int index)
        {
            _colorChannel = colorChannel;
            _side = side;
            _index = index;
            ApplyVisual();
        }

        public void SetSelected(bool selected)
        {
            if (_isLocked)
            {
                return;
            }

            _isSelected = selected;
            ApplyVisual();
        }

        public void SetLocked(bool locked)
        {
            _isLocked = locked;
            if (_isLocked)
            {
                _isSelected = false;
            }

            ApplyVisual();
        }

        public void ShowInvalidFlash(float duration = 0.15f)
        {
            if (_invalidFlashRoutine != null)
            {
                StopCoroutine(_invalidFlashRoutine);
            }

            _invalidFlashRoutine = StartCoroutine(InvalidFlashCoroutine(duration));
        }

        public Vector3 GetAnchorPosition()
        {
            return transform.position;
        }

        private IEnumerator InvalidFlashCoroutine(float duration)
        {
            SetColor(_invalidColor);
            yield return new WaitForSeconds(duration);
            ApplyVisual();
            _invalidFlashRoutine = null;
        }

        private void ApplyVisual()
        {
            if (_targetRenderer == null)
            {
                return;
            }

            if (_isLocked)
            {
                SetColor(_lockedColor);
                return;
            }

            if (_isSelected)
            {
                SetColor(_selectedColor);
                return;
            }

            SetColor(ToUnityColor(_colorChannel));
        }

        private void SetColor(Color color)
        {
            if (_targetRenderer == null)
            {
                return;
            }

            if (_targetRenderer.sharedMaterial != null)
            {
                _targetRenderer.sharedMaterial.color = color;
            }
        }

        public static Color ToUnityColor(WireColorChannel channel)
        {
            switch (channel)
            {
                case WireColorChannel.Red:
                    return new Color(0.95f, 0.15f, 0.15f);
                case WireColorChannel.Green:
                    return new Color(0.2f, 0.85f, 0.2f);
                case WireColorChannel.Blue:
                    return new Color(0.25f, 0.45f, 1f);
                case WireColorChannel.Yellow:
                    return new Color(0.95f, 0.9f, 0.15f);
                case WireColorChannel.Orange:
                    return new Color(1f, 0.55f, 0.1f);
                case WireColorChannel.Purple:
                    return new Color(0.7f, 0.35f, 0.9f);
                default:
                    return Color.white;
            }
        }
    }
}
