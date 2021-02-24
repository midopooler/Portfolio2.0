using System.Collections;
using UnityEngine;
#if HELIOS3D
using UtopiaWorx.Helios;

namespace Pegasus
{
    /// <summary>
    /// Control the fade on a helios target when this trigger is used.
    /// </summary>
    public class TriggerControlHeliosFade : TriggerBase
    {
        public PegasusConstants.PoiHeliosTriggerAction m_actionOnStart = PegasusConstants.PoiHeliosTriggerAction.FadeIn;
        public PegasusConstants.PoiHeliosTriggerAction m_actionOnEnd = PegasusConstants.PoiHeliosTriggerAction.DoNothing;
        public Color m_startColour = Color.black;
        public Color m_endColour = Color.black;
        public float m_startDuration = 0.25f;
        public float m_endDuration = 0.25f;
        public HeliosUtility m_heliosUtility;
        public float m_endTimeStartMS = 0f;
        public bool m_endProcessStarted = false;

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

            if (m_heliosUtility == null)
            {
                m_heliosUtility = GameObject.FindObjectOfType<HeliosUtility>();
            }

            if (m_heliosUtility == null)
            {
                Debug.LogWarning(string.Format("Helios was not located on {0} - exiting", name));
                return;
            }

            if (m_triggerAtStart)
            {
                if (m_actionOnStart == PegasusConstants.PoiHeliosTriggerAction.FadeIn)
                {
                    StartCoroutine(FadeIn(m_startColour, m_startDuration));
                }
                else if (m_actionOnStart == PegasusConstants.PoiHeliosTriggerAction.FadeOut)
                {
                    StartCoroutine(FadeOut(m_startColour, m_startDuration));
                }
            }

            //Set up when the end fade starts - work it backwards from the segment time
            if (m_triggerAtEnd && m_actionOnEnd != PegasusConstants.PoiHeliosTriggerAction.DoNothing)
            {
                //Calculate when end start time should kick in
                m_endProcessStarted = false;
                m_endTimeStartMS = (float)poi.m_segmentDuration.TotalMilliseconds;
                if (m_endTimeStartMS > (m_endDuration * 1000f))
                {
                    m_endTimeStartMS = m_endDuration * 1000f;
                }
                m_endTimeStartMS = Time.time + (float)poi.m_segmentDuration.TotalMilliseconds - m_endTimeStartMS;
            }
        }

        /// <summary>
        /// Called when the trigger is updated
        /// </summary>
        /// <param name="poi"></param>
        public override void OnUpdate(PegasusPoi poi, float progress)
        {
            if ((poi != null) && (m_actionOnEnd != PegasusConstants.PoiHeliosTriggerAction.DoNothing))
            {
                if ((m_endProcessStarted != true) && ((Time.time * 1000f) >= m_endTimeStartMS))
                {
                    m_endProcessStarted = true;
                    if (m_actionOnEnd == PegasusConstants.PoiHeliosTriggerAction.FadeIn)
                    {
                        StartCoroutine(FadeIn(m_endColour, m_endDuration));
                    }
                    else if (m_actionOnEnd == PegasusConstants.PoiHeliosTriggerAction.FadeOut)
                    {
                        StartCoroutine(FadeOut(m_endColour, m_endDuration));
                    }
                }
            }
        }
        IEnumerator FadeIn(Color colour, float duration)
        {
            float startMS = Time.time * 1000f;
            float currMS = 0;
            float endMS = duration * 1000f;

            HeliosUtility.SetFadeColot(colour);
            for (; currMS <= endMS;)
            {
                currMS = (Time.time * 1000f) - startMS;
                HeliosUtility.SetFade(currMS / endMS);
                yield return null;
            }
        }

        IEnumerator FadeOut(Color colour, float duration)
        {
            float startMS = Time.time * 1000f;
            float currMS = 0f;
            float endMS = duration * 1000f;

            HeliosUtility.SetFadeColot(colour);
            for (; currMS <= endMS;)
            {
                currMS = (Time.time * 1000f) - startMS;
                HeliosUtility.SetFade(1f - (currMS / endMS));
                yield return null;
            }
        }
    }
}
#endif