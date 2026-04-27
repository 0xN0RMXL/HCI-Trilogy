using System;

namespace HCITrilogy.Signal.Gameplay
{
    /// <summary>
    /// Static event hub for Multimodal Feedback. Anything that needs to
    /// react to a hit/miss (lane flash, particles, SFX, screen-shake, HUD pop)
    /// subscribes here; the Judge raises events on decisions.
    ///
    /// HCI principle: Multimodal Feedback — simultaneous visual + audio +
    /// pseudo-haptic cues reinforce action perception and reduce uncertainty
    /// (Norman 2013; Hoggan et al., 2008).
    /// </summary>
    public static class FeedbackBus
    {
        public static event Action<Judgment, int> OnFeedback;
        public static void Trigger(Judgment j, int laneIndex) => OnFeedback?.Invoke(j, laneIndex);

        public static event Action OnComboBreak;
        public static void TriggerComboBreak() => OnComboBreak?.Invoke();
    }
}
