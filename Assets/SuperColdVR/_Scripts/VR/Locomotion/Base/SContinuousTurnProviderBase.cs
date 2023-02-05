using SuperColdVR.VR.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperColdVR.VR.Locomotion.Base
{
    public abstract class SContinuousTurnProviderBase : SLocomotionProvider
    {
        [SerializeField]
        float m_TurnSpeed = 60f;
        public float turnSpeed
        {
            get => m_TurnSpeed;
            set => m_TurnSpeed = value;
        }

        bool m_IsTurningXROrigin;

        protected void Update()
        {
            m_IsTurningXROrigin = false;

            // Use the input amount to scale the turn speed.
            var input = ReadInput();
            var turnAmount = GetTurnAmount(input);

            TurnRig(turnAmount);

            switch (LocomotionPhase)
            {
                case ELocomotionPhase.Idle:
                case ELocomotionPhase.Started:
                    if (m_IsTurningXROrigin)
                        LocomotionPhase = ELocomotionPhase.Moving;
                    break;
                case ELocomotionPhase.Moving:
                    if (!m_IsTurningXROrigin)
                        LocomotionPhase = ELocomotionPhase.Done;
                    break;
                case ELocomotionPhase.Done:
                    LocomotionPhase = m_IsTurningXROrigin ? ELocomotionPhase.Moving : ELocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={LocomotionPhase}");
                    break;
            }
        }

        protected abstract Vector2 ReadInput();

        protected virtual float GetTurnAmount(Vector2 input)
        {
            if (input == Vector2.zero)
                return 0f;

            var cardinal = SCardinalUtility.GetNearestCardinal(input);
            switch (cardinal)
            {
                case ECardinal.North:
                case ECardinal.South:
                    break;
                case ECardinal.East:
                case ECardinal.West:
                    return input.magnitude * (Mathf.Sign(input.x) * m_TurnSpeed * Time.deltaTime);
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
                var xrOrigin = LocomotionSystem.XRRig;
                if (xrOrigin != null)
                {
                    m_IsTurningXROrigin = true;
                    xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);
                }

                EndLocomotion();
            }
        }
    }
}
