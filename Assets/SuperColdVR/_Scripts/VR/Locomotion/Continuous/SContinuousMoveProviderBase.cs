using SuperColdVR.VR.Core;
using SuperColdVR.VR.Locomotion.Base;
using SuperColdVR.VR.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperColdVR.VR.Locomotion.Continuous
{
    public abstract class SContinuousMoveProviderBase : SLocomotionProvider
    {
        [field: SerializeField] private float MoveSpeed { get; set; } = 1f;
        [field: SerializeField] private bool EnableStrafe { get; set; } = true;
        [field: SerializeField] private bool EnableFly { get; set; }
        [field: SerializeField] private bool UseGravity { get; set; } = true;
        [field: SerializeField] private EGravityApplicationMode GravityApplicationMode { get; set; }
        [field: SerializeField] private Transform ForwardSource { get; set; }

        private CharacterController characterController;
        private bool attemptedGetCharacterController;
        private bool isMovingXROrigin;
        Vector3 m_VerticalVelocity;

        protected void Update()
        {
            isMovingXROrigin = false;

            if (LocomotionSystem.XRRig == null) { return; }

            Vector2 input = ReadInput();
            Vector3 translationInWorldSpace = ComputeDesiredMove(input);

            switch (GravityApplicationMode)
            {
                case EGravityApplicationMode.Immediately:
                    MoveRig(translationInWorldSpace);
                    break;
                case EGravityApplicationMode.AttemptingMove:
                    if (input != Vector2.zero || m_VerticalVelocity != Vector3.zero) { MoveRig(translationInWorldSpace); }
                    break;
                default:
                    Assert.IsTrue(false, $"{nameof(GravityApplicationMode)}={GravityApplicationMode} outside expected range.");
                    break;
            }

            switch (LocomotionPhase)
            {
                case ELocomotionPhase.Idle:
                case ELocomotionPhase.Started:
                    if (isMovingXROrigin)
                        LocomotionPhase = ELocomotionPhase.Moving;
                    break;
                case ELocomotionPhase.Moving:
                    if (!isMovingXROrigin)
                        LocomotionPhase = ELocomotionPhase.Done;
                    break;
                case ELocomotionPhase.Done:
                    LocomotionPhase = isMovingXROrigin ? ELocomotionPhase.Moving : ELocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(ELocomotionPhase)}={LocomotionPhase}");
                    break;
            }
        }

        protected abstract Vector2 ReadInput();

        protected virtual Vector3 ComputeDesiredMove(Vector2 input)
        {
            if (input == Vector2.zero) { return Vector3.zero; }

            SXROrigin xrRig = LocomotionSystem.XRRig;
            if (xrRig == null) { return Vector3.zero; }

            Vector3 inputMove = Vector3.ClampMagnitude(new Vector3(EnableStrafe ? input.x : 0f, 0f, input.y), 1f);

            // Determine frame of reference for what the input direction is relative to
            Transform forwardSourceTransform = ForwardSource == null ? xrRig.Camera.transform : ForwardSource;
            Vector3 inputForwardInWorldSpace = forwardSourceTransform.forward;

            Transform originTransform = xrRig.OriginBaseGameObject.transform;
            float speedFactor = MoveSpeed * Time.deltaTime * originTransform.localScale.x; // Adjust speed with user scale

            // If flying, just compute move directly from input and forward source
            if (EnableFly)
            {
                Vector3 inputRightInWorldSpace = forwardSourceTransform.right;
                Vector3 combinedMove = inputMove.x * inputRightInWorldSpace + inputMove.z * inputForwardInWorldSpace;
                return combinedMove * speedFactor;
            }

            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, originTransform.up)), 1f))
            {
                inputForwardInWorldSpace = -forwardSourceTransform.up;
            }

            Vector3 inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, originTransform.up);
            Quaternion forwardRotation = Quaternion.FromToRotation(originTransform.forward, inputForwardProjectedInWorldSpace);

            Vector3 translationInRigSpace = forwardRotation * inputMove * speedFactor;
            Vector3 translationInWorldSpace = originTransform.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
        }

        protected virtual void MoveRig(Vector3 translationInWorldSpace)
        {
            SXROrigin xrRig = LocomotionSystem.XRRig;
            if (xrRig == null) { return; }

            FindCharacterController();

            Vector3 motion = translationInWorldSpace;

            if (characterController != null && characterController.enabled)
            {
                // Step vertical velocity from gravity
                if (characterController.isGrounded || !UseGravity || EnableFly)
                {
                    m_VerticalVelocity = Vector3.zero;
                }
                else
                {
                    m_VerticalVelocity += Physics.gravity * Time.deltaTime;
                }

                motion += m_VerticalVelocity * Time.deltaTime;

                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    isMovingXROrigin = true;
                    characterController.Move(motion);
                    EndLocomotion();
                }
            }
            else
            {
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    isMovingXROrigin = true;
                    xrRig.OriginBaseGameObject.transform.position += motion;
                    EndLocomotion();
                }
            }
        }

        void FindCharacterController()
        {
            SXROrigin xrRig = LocomotionSystem.XRRig;
            if (xrRig == null) { return; }

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (characterController == null && !attemptedGetCharacterController)
            {
                characterController = xrRig.OriginBaseGameObject.GetComponent<CharacterController>();
                attemptedGetCharacterController = true;
            }
        }
    }
}