using UnityEngine;
using InteractiveMuseum.MiniGames;

namespace InteractiveMuseum.MiniGames
{
    public class MoonBallMiniGame : MiniGameBase
    {
        [Header("Moon Ball Game References")]
        [SerializeField]
        private GameObject moonBallGameRoot;
        [SerializeField]
        private BallController ballController;
        // [SerializeField]
        // private BallController playerBall;
        // [SerializeField] 
        // private BallController targetBall;
        [SerializeField]
        private HoleTrigger holeTrigger;
        

        protected override void OnMiniGameActivated()
        {
            base.OnMiniGameActivated();

            if (moonBallGameRoot != null)
                moonBallGameRoot.SetActive(true);

            Debug.Log("����-���� ������������");
        }

        protected override void OnMiniGameDeactivated()
        {
            base.OnMiniGameDeactivated();

            //if (_moonBallGameRoot != null)
            //    _moonBallGameRoot.SetActive(false);

            if (ballController != null)
            {
                ballController.ResetPositions();
                //playerBall.ResetPositions();
            }
                

            Debug.Log("����-���� ��������������");
        }

        public void OnGameComplete()
        {
            if (IsActive())
            {
                DeactivateMiniGame();
            }
        }
    }
}