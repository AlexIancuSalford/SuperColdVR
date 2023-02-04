using UnityEngine;
using UnityEngine.InputSystem;

namespace SuperColdVR.Controller
{
    public class HandController : MonoBehaviour
    {
        private Animator Animator { get; set; } = null;

        public InputActionProperty pinchActionProperty;
        public InputActionProperty gripActionProperty;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            HandleHandAnimation();
        }

        private void HandleHandAnimation()
        {
            float triggerValue = pinchActionProperty.action.ReadValue<float>();
            Animator.SetFloat("Trigger", triggerValue);

            float gripValue = gripActionProperty.action.ReadValue<float>();
            Animator.SetFloat("Grip", gripValue);
        }
    }
}
