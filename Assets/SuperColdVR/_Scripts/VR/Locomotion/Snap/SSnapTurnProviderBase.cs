using SuperColdVR.VR.Locomotion.Base;
using SuperColdVR.VR.Utils;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace SuperColdVR.VR.Locomotion.Snap
{
    public abstract class SnapTurnProviderBase : SLocomotionProvider
    {
        [field: SerializeField] private float TurnAmount { get; set; } = 45f;
        [field: SerializeField] private float DebounceTime { get; set; } = 0.5f;
        [field: SerializeField] private bool EnableTurnLeftRight { get; set; } = true;
        [field: SerializeField] private bool EnableTurnAround { get; set; } = true;
        [field: SerializeField] private float DelayTime { get; set; }

        private float currentTurnAmount;
        private float timeStarted;
        private float delayStartTime;

        protected override void Awake()
        {
            base.Awake();
            if (LocomotionSystem != null && DelayTime > 0f && DelayTime > LocomotionSystem.OperationTimeout)
                Debug.LogWarning($"Delay Time ({DelayTime}) is longer than the Locomotion System's Timeout ({LocomotionSystem.OperationTimeout}).", this);
        }

        protected void Update()
        {
            // Wait for a certain amount of time before allowing another turn.
            if (timeStarted > 0f && (timeStarted + DebounceTime < Time.time))
            {
                timeStarted = 0f;
                return;
            }

            // Reset to Idle state at the beginning of the update loop (rather than the end)
            // so that anything that needs to be aware of the Done state can trigger, such as
            // the vignette provider or another comfort mode option.
            if (LocomotionPhase == ELocomotionPhase.Done) { LocomotionPhase = ELocomotionPhase.Idle; }

            Vector2 input = ReadInput();
            float amount = GetTurnAmount(input);
            if (Mathf.Abs(amount) > 0f || LocomotionPhase == ELocomotionPhase.Started)
            {
                StartTurn(amount);
            }
            else if (Mathf.Approximately(currentTurnAmount, 0f) && LocomotionPhase == ELocomotionPhase.Moving)
            {
                LocomotionPhase = ELocomotionPhase.Done;
            }

            if (LocomotionPhase == ELocomotionPhase.Moving && Math.Abs(currentTurnAmount) > 0f && BeginLocomotion())
            {
                Core.SXROrigin xrOrigin = LocomotionSystem.XRRig;
                if (xrOrigin != null)
                {
                    xrOrigin.RotateAroundCameraUsingOriginUp(currentTurnAmount);
                }
                else
                {
                    LocomotionPhase = ELocomotionPhase.Done;
                }
                currentTurnAmount = 0f;
                EndLocomotion();

                if (Mathf.Approximately(amount, 0f)) { LocomotionPhase = ELocomotionPhase.Done; }
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
                    break;
                case ECardinal.South:
                    if (EnableTurnAround)
                        return 180f;
                    break;
                case ECardinal.East:
                    if (EnableTurnLeftRight)
                        return TurnAmount;
                    break;
                case ECardinal.West:
                    if (EnableTurnLeftRight)
                        return -TurnAmount;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(ECardinal)}={cardinal}");
                    break;
            }

            return 0f;
        }

        protected void StartTurn(float amount)
        {
            if (timeStarted > 0f) { return; }

            if (!CanBeginLocomotion()) { return; }

            if (LocomotionPhase == ELocomotionPhase.Idle)
            {
                LocomotionPhase = ELocomotionPhase.Started;
                delayStartTime = Time.time;
            }

            // We set the m_CurrentTurnAmount here so we can still trigger the turn
            // in the case where the input is released before the delay timeout happens.
            if (Math.Abs(amount) > 0f) { currentTurnAmount = amount; }

            // Wait for configured Delay Time
            if (DelayTime > 0f && Time.time - delayStartTime < DelayTime) { return; }

            LocomotionPhase = ELocomotionPhase.Moving;
            timeStarted = Time.time;
        }

        internal void FakeStartTurn(bool isLeft)
        {
            StartTurn(isLeft ? -TurnAmount : TurnAmount);
        }

        internal void FakeStartTurnAround()
        {
            StartTurn(180f);
        }
    }
}
