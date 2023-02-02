using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceCombat.Scene
{
    public class GameManager : MonoBehaviour
    {
        private ManualXRControl ManualXRControl { get; set; } = null;

        private PlayerInput PlayerInput { get; set; } = null;
        private InputAction Move { get; set; } = null;

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            Move = PlayerInput.actions["Move"];
            ManualXRControl = GetComponent<ManualXRControl>();
        }
        
        // Start is called before the first frame update
        void Start()
        {
            Move.performed += (context) => { Debug.Log(Move.ReadValue<float>()); };
            InputSystem.onDeviceChange += InputUserOnChange;
        }

        private void InputUserOnChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added)
            {
                if(device.name.Contains("Vive") || 
                   device.name.Contains("Oculus"))
                {
                    StartCoroutine(ManualXRControl.StartXRCoroutine());
                }
            }

            if (change is InputDeviceChange.Disabled or InputDeviceChange.Removed)
            {
                if(device.name.Contains("Vive") || 
                   device.name.Contains("Oculus"))
                {
                    ManualXRControl.StopXR();
                }
            }
        }
    }
}
