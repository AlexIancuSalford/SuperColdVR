using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperColdVR.VR.Input
{
    public class SInputActionManager : MonoBehaviour
    {
        [field : SerializeField] private List<InputActionAsset> ActionAssets { get; set; }

        protected void OnEnable()
        {
            EnableInput();
        }

        protected void OnDisable()
        {
            DisableInput();
        }

        public void EnableInput()
        {
            if (ActionAssets == null) { return; }

            foreach (InputActionAsset actionAsset in ActionAssets)
            {
                if (actionAsset != null)
                {
                    actionAsset.Enable();
                }
            }
        }

        public void DisableInput()
        {
            if (ActionAssets == null) { return; }

            foreach (InputActionAsset actionAsset in ActionAssets)
            {
                if (actionAsset != null)
                {
                    actionAsset.Disable();
                }
            }
        }
    }
}
