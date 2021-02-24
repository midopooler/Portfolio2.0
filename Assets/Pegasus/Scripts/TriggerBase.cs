using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// A trigger that can be placed on a POI to cause something else to happen. Either derive a new class from this, or used one of the derived classes to 
    /// cause something to happen. 
    /// </summary>
    public class TriggerBase : MonoBehaviour
    {
        public bool m_triggerAtStart = true;
        public bool m_triggerOnUpdate = false;
        public bool m_triggerAtEnd = true;

        /// <summary>
        /// Called when the trigger starts
        /// </summary>
        /// <param name="poi"></param>
        public virtual void OnStart(PegasusPoi poi)
        {
            if (poi != null && m_triggerAtStart)
            {
                if (poi.m_manager.m_displayDebug == true)
                {
                    Debug.Log(string.Format("Started trigger on {0} - {1}", poi.m_manager.name, poi.name));
                }
            }
        }

        /// <summary>
        /// Called when the trigger is updated
        /// </summary>
        /// <param name="poi"></param>
        public virtual void OnUpdate(PegasusPoi poi, float progress)
        {
            if (poi != null && m_triggerOnUpdate)
            {
                if (poi.m_manager.m_displayDebug == true)
                {
                    Debug.Log(string.Format("Udpated trigger on {0} - {1} {2:0.00}", poi.m_manager.name, poi.name, progress));
                }
            }
        }

        /// <summary>
        /// Called when the trigger ends
        /// </summary>
        /// <param name="poi"></param>
        public virtual void OnEnd(PegasusPoi poi)
        {
            if (poi != null && m_triggerAtEnd)
            {
                if (poi.m_manager.m_displayDebug == true)
                {
                    Debug.Log(string.Format("Ended trigger on {0} - {1}", poi.m_manager.name, poi.name));
                }
            }
        }
    }
}
