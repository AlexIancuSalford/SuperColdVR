namespace SuperColdVR.VR.Utils
{
    public enum EGravityApplicationMode
    {
        /// <summary>
        /// Only begin to apply gravity and apply locomotion when a move input occurs.
        /// When using gravity, continues applying each frame, even if input is stopped, until touching ground.
        /// </summary>
        /// <remarks>
        /// Use this style when you don't want gravity to apply when the player physically walks away and off a ground surface.
        /// Gravity will only begin to move the player back down to the ground when they try to use input to move.
        /// </remarks>
        AttemptingMove,

        /// <summary>
        /// Apply gravity and apply locomotion every frame, even without move input.
        /// </summary>
        /// <remarks>
        /// Use this style when you want gravity to apply when the player physically walks away and off a ground surface,
        /// even when there is no input to move.
        /// </remarks>
        Immediately,
    }
}