using SuperColdVR.VR.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperColdVR.VR.Locomotion.Base
{
    public abstract class SContinuousTurnProviderBase : SLocomotionProvider
    {
        [field : SerializeField] private float TurnSpeed { get; set; } = 60f;

        private bool isTurningXROrigin;

        protected void Update()
        {
            isTurningXROrigin = false;

            // Use the input amount to scale the turn speed.
            Vector2 input = ReadInput();
            float turnAmount = GetTurnAmount(input);

            TurnRig(turnAmount);

            switch (LocomotionPhase)
            {
                case ELocomotionPhase.Idle:
                case ELocomotionPhase.Started:
                    if (isTurningXROrigin)
                        LocomotionPhase = ELocomotionPhase.Moving;
                    break;
                case ELocomotionPhase.Moving:
                    if (!isTurningXROrigin)
                        LocomotionPhase = ELocomotionPhase.Done;
                    break;
                case ELocomotionPhase.Done:
                    LocomotionPhase = isTurningXROrigin ? ELocomotionPhase.Moving : ELocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={LocomotionPhase}");
                    break;
            }
        }

        protected abstract Vector2 ReadInput();

        protected virtual float GetTurnAmount(Vector2 input)
        {
            if (input == Vector2.zero) { return 0f; }

            ECardinal cardinal = SCardinalUtility.GetNearestCardinal(input);
            switch (cardinal)
            {
                case ECardinal.North:
                case ECardinal.South:
                    break;
                case ECardinal.East:
                case ECardinal.West:
                    return input.magnitude * (Mathf.Sign(input.x) * TurnSpeed * Time.deltaTime);
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(ECardinal)}={cardinal}");
                    break;
            }

            return 0f;
        }

        protected void TurnRig(float turnAmount)
        {
            if (Mathf.Approximately(turnAmount, 0f))
                return;

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                Core.SXROrigin xrOrigin = LocomotionSystem.XRRig;
                if (xrOrigin != null)
                {
                    isTurningXROrigin = true;
                    xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);
                }

                EndLocomotion();
            }
        }
    }
}
