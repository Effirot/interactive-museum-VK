using InteractiveMuseum.MiniGames;
using UnityEngine;
using UnityEngine.InputSystem;


    public class BallController : MonoBehaviour
    {
        [Header("Mini Game Reference")]
        [SerializeField] 
        private MoonBallMiniGame _moonBallMiniGame;
        [Header("Settings")]
        public float maxPower = 20f;
        public float minPower = 5f;
        public float friction = 0.98f;
        public LayerMask wallLayer;

        public GameObject powerLinePrefab;

        private Rigidbody rb;
        private Vector3 startMousePos;
        private Vector3 currentMousePos;
        private bool isDragging = false;
        private GameObject powerLine;
        private LineRenderer lineRenderer;
        private float currentPower;
        private Camera mainCamera;

        private Vector3 startPositionPlayerBall;
        private Vector3 startPositionTargetBall;
        private HoleTrigger holeTrigger;
        public GameObject playerBall;
        public GameObject targetBall;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.linearDamping = 0.05f;
            mainCamera = Camera.main;


            if (powerLinePrefab != null)
            {
                powerLine = Instantiate(powerLinePrefab, transform.position, Quaternion.identity);
                lineRenderer = powerLine.GetComponent<LineRenderer>();
                if (lineRenderer == null)
                    lineRenderer = powerLine.AddComponent<LineRenderer>();

                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.enabled = false;
            }
            if (gameObject == playerBall)
            {
                startPositionPlayerBall = transform.position;
            }
            else if (gameObject == playerBall)
            {
                playerBall.GetComponent<HoleTrigger>().RestartLevel();
            }
            else if (gameObject == targetBall)
            {
                startPositionTargetBall = transform.position;
            }
            else if (gameObject == targetBall)
            {
                targetBall.GetComponent<HoleTrigger>().RestartLevel();
            }
        }

        void Update()
        {
            if (gameObject.name != "PlayerBall") return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    startMousePos = mouse.position.ReadValue();
                }
            }

            if (isDragging && mouse.leftButton.isPressed)
            {
                currentMousePos = mouse.position.ReadValue();
                Vector3 direction = startMousePos - currentMousePos;
                currentPower = Mathf.Clamp(direction.magnitude * 0.1f, minPower, maxPower);

                if (lineRenderer != null)
                {
                    lineRenderer.enabled = true;
                    Vector3 worldDir = new Vector3(direction.x, 0, direction.y).normalized;
                    Vector3 endPoint = transform.position + worldDir * currentPower * 0.5f;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, endPoint);
                }
            }

            if (isDragging && mouse.leftButton.wasReleasedThisFrame)
            {
                Vector3 direction = startMousePos - currentMousePos;
                Vector3 force = new Vector3(direction.x, 0, direction.y).normalized * currentPower;
                rb.AddForce(force, ForceMode.Impulse);

                isDragging = false;
                if (lineRenderer != null)
                    lineRenderer.enabled = false;
            }
        }
        public void ResetPositions()
        {
            if (gameObject == playerBall)
            {
                transform.position = startPositionPlayerBall;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else if (gameObject == targetBall)
            {
                transform.position = startPositionTargetBall;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    public void CompleteGame()
    {
        if (_moonBallMiniGame != null)
            _moonBallMiniGame.OnGameComplete();
    }
}

