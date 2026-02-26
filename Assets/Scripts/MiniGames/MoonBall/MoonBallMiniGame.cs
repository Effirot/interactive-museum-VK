using UnityEngine;
using InteractiveMuseum.MiniGames;

namespace InteractiveMuseum.MiniGames
{
    public class MoonBallMiniGame : MiniGameBase
    {
        [Header("Moon Ball Game References")]
        [SerializeField] 
        private GameObject _moonBallGameRoot; 
        [SerializeField] 
        private BallController _ballController;
        [SerializeField] 
        private HoleTrigger _holeTrigger;

        protected override void OnMiniGameActivated()
        {
            base.OnMiniGameActivated();

            if (_moonBallGameRoot != null)
                _moonBallGameRoot.SetActive(true);

            Debug.Log("Мини-игра активирована");
        }

        protected override void OnMiniGameDeactivated()
        {
            base.OnMiniGameDeactivated();

            //if (_moonBallGameRoot != null)
            //    _moonBallGameRoot.SetActive(false);

            if (_ballController != null)
                _ballController.ResetPositions();

            Debug.Log("Мини-игра деактивирована");
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