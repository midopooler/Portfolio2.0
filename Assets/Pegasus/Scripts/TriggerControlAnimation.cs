using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// Control the animation on the target with this trigger.
    /// </summary>
    public class TriggerControlAnimation : TriggerBase
    {
        public Animation m_targetAnimation;
        public PegasusConstants.PoiAnimationTriggerAction m_actionOnStart = PegasusConstants.PoiAnimationTriggerAction.PlayAnimation;
        public PegasusConstants.PoiAnimationTriggerAction m_actionOnEnd = PegasusConstants.PoiAnimationTriggerAction.DoNothing;
        public int m_startAnimationIdx = 0;
        public int m_endAnimation = 0;
        private List<AnimationState> m_animations = new List<AnimationState>();

        void Start()
        {
            if (m_targetAnimation != null)
            {
                foreach (AnimationState state in m_targetAnimation)
                {
                    m_animations.Add(state);
                }
            }
        }

        /// <summary>
        /// Called when the trigger starts
        /// </summary>
        /// <param name="poi"></param>
        public override void OnStart(PegasusPoi poi)
        {
            if (poi == null)
            {
                Debug.LogWarning(string.Format("Poi was not supplied on {0} - exiting", name));
                return;
            }

            if (m_targetAnimation == null)
            {
                Debug.LogWarning(string.Format("Animation was not supplied on {0} - exiting", name));
                return;
            }

            if (m_triggerAtStart)
            {
                if (m_actionOnStart == PegasusConstants.PoiAnimationTriggerAction.PlayAnimation)
                {
                    m_targetAnimation.Play(m_animations[m_startAnimationIdx].name);
                }
                else if (m_actionOnStart == PegasusConstants.PoiAnimationTriggerAction.StopAnimation)
                {
                    m_targetAnimation.Stop();
                }
            }
        }

        /// <summary>
        /// Called when the trigger ends
        /// </summary>
        /// <param name="poi"></param>
        public override void OnEnd(PegasusPoi poi)
        {
            if (poi == null)
            {
                Debug.LogWarning(string.Format("Poi was not supplied on {0} - exiting", name));
                return;
            }

            if (m_targetAnimation == null)
            {
                Debug.LogWarning(string.Format("Animation was not supplied on {0} - exiting", name));
                return;
            }

            if (m_triggerAtEnd)
            {
                if (m_actionOnStart == PegasusConstants.PoiAnimationTriggerAction.PlayAnimation)
                {
                    m_targetAnimation.Play(m_animations[m_startAnimationIdx].name);
                }
                else if (m_actionOnStart == PegasusConstants.PoiAnimationTriggerAction.StopAnimation)
                {
                    m_targetAnimation.Stop();
                }
            }
        }
    }
}
