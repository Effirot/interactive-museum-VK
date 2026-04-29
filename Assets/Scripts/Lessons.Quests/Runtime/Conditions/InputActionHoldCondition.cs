

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace Lessons.Quests
{
    public class InputActionHoldCondition : QuestCondition
    {
        [Space]
        [SerializeField]
        private InputActionReference inputAction;
        [SerializeField]
        private bool invert = false;

        public override void Active()
        {
            inputAction.action.performed += OnActionClicked_Handler;
            isCompleted = invert;
        }
        public override void Deactive()
        {
            inputAction.action.performed -= OnActionClicked_Handler;
        }

        private void OnActionClicked_Handler(CallbackContext context)
        {
            isCompleted = context.ReadValueAsButton();

            if (invert)
            {
                isCompleted ^= isCompleted;
            }
        }   
    }
}