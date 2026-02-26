using InteractiveMuseum.MiniGames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoleTrigger : MonoBehaviour
{
    public GameObject areneActive;
    public GameObject areneInactive;
    public GameObject targetBall;

    public float maxSpeedForHole = 1.5f;
    public float delayBeforeChange = 2f;

    private BallController ballController;
    public GameObject playerBall;
    void Start()
    {
        if (playerBall != null)
        {
            ballController = playerBall.GetComponent<BallController>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetBall)
        {
            Rigidbody ballRigidbody = other.GetComponent<Rigidbody>();

            float currentSpeed = ballRigidbody.linearVelocity.magnitude;

            Debug.Log($"Скорость шара при входе в лунку: {currentSpeed:F2}, максимум на попадание: {maxSpeedForHole}");

            if (currentSpeed <= maxSpeedForHole)
            {
                targetBall.GetComponent<Collider>().enabled = false;
                Invoke(nameof(ProcessSuccessful), delayBeforeChange);
            }
        }
    }

    void ProcessSuccessful()
    {
        Debug.Log("Победа! Шар в лунке :)");

        areneActive.SetActive(false);
        areneInactive.SetActive(true);

        MoonBallMiniGame miniGame = GetComponentInParent<MoonBallMiniGame>();
        if (miniGame != null)
        {
            miniGame.OnGameComplete();
        }
    }
    public void RestartLevel()
    {
        playerBall.GetComponent<BallController>().ResetPositions();

        targetBall.GetComponent<BallController>().ResetPositions();

        targetBall.GetComponent<Collider>().enabled = true;

        areneInactive.SetActive(false);
        areneActive.SetActive(true);
    }
}