using UnityEngine;

namespace InteractiveMuseum.Rendering
{
    /// <summary>
    /// Renders a flat sprite that always faces the camera.
    /// </summary>
    public class FlatSpriteRenderer : MonoBehaviour
    {
        [Header("Rendering Settings")]
        [Tooltip("Material used for rendering the sprite")]
        [SerializeField]
        private Material _material;
        
        [Tooltip("Mesh to render")]
        [SerializeField]
        private Mesh _mesh;
        
        [Tooltip("Additional Z-axis rotation in degrees")]
        [SerializeField]
        private float _rotateZ;
        
        [Header("Flipbook Settings")]
        [Tooltip("Whether to use shader flipbook animation")]
        [SerializeField]
        private bool _useShaderFlipbook;
        
        [Tooltip("Current frame for flipbook animation")]
        [SerializeField]
        private int _frame;
        
        [Tooltip("Glow intensity for the sprite")]
        [SerializeField]
        private float _glowIntensity;

        public Material material
        {
            get => _material;
            set => _material = value;
        }

        public Mesh mesh
        {
            get => _mesh;
            set => _mesh = value;
        }

        public float rotateZ
        {
            get => _rotateZ;
            set => _rotateZ = value;
        }

        public bool useShaderFlipbook
        {
            get => _useShaderFlipbook;
            set => _useShaderFlipbook = value;
        }

        public int frame
        {
            get => _frame;
            set => _frame = value;
        }

        public float glowIntensity
        {
            get => _glowIntensity;
            set => _glowIntensity = value;
        }

        public void Update()
        {
            Render();
        }

        private void OnRenderObject()
        {
        }

        private void OnPostRender()
        {
        }

        /// <summary>
        /// Renders the sprite facing the camera.
        /// </summary>
        public void Render()
        {
            if (_material == null || _mesh == null)
            {
                return;
            }

            MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
            
            if (_useShaderFlipbook)
            {
                materialProperty.SetFloat("_frame", _frame);
                materialProperty.SetFloat("_GlowIntensity", _glowIntensity);
            }
            
            Vector3 f = (UserInterface.positionCamera - transform.position).normalized;
            Vector2 pw = MyUtils.Vector3ToPitchYaw(f);
            Quaternion quaternion = Quaternion.Euler(pw.x, -pw.y, _rotateZ);
            
            Graphics.DrawMesh(_mesh, Matrix4x4.TRS(transform.position, quaternion, transform.lossyScale), _material, 0, null, 0, materialProperty);
        }
    }
}
