using System;
using UnityEngine;

namespace Pegasus
{
    /// <summary>
    /// Control another pegasus with this trigger.
    /// </summary>
    public class TriggerControlPegasus : TriggerBase
    {
        public PegasusConstants.PoiPegasusTriggerAction m_actionOnStart = PegasusConstants.PoiPegasusTriggerAction.PlayPegasus;
        public PegasusConstants.PoiPegasusTriggerAction m_actionOnEnd = PegasusConstants.PoiPegasusTriggerAction.StopPegasus;
        public PegasusManager m_pegasus;

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

            if (m_pegasus == null)
            {
                Debug.LogWarning(string.Format("Pegasus was not supplied on {0} - exiting", name));
                return;
            }

            if (m_triggerAtStart)
            {
                switch (m_actionOnStart)
                {
                    case PegasusConstants.PoiPegasusTriggerAction.PlayPegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Started flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        if (m_pegasus.m_currentState == PegasusConstants.FlythroughState.Paused)
                        {
                            m_pegasus.ResumeFlythrough();
                        }
                        else
                        {
                            m_pegasus.StartFlythrough();
                        }
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.PausePegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Pausing flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.PauseFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.ResumePegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Resuming flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.ResumeFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.StopPegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Stopping flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.StopFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.DoNothing:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Doing nothing on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        break;
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

            if (m_pegasus == null)
            {
                Debug.LogWarning(string.Format("Pegasus was not supplied on {0} - exiting", name));
                return;
            }

            if (m_triggerAtEnd)
            {
                switch (m_actionOnEnd)
                {
                    case PegasusConstants.PoiPegasusTriggerAction.PlayPegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Started flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        if (m_pegasus.m_currentState == PegasusConstants.FlythroughState.Paused)
                        {
                            m_pegasus.ResumeFlythrough();
                        }
                        else
                        {
                            m_pegasus.StartFlythrough();
                        }
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.PausePegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Pausing flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.PauseFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.ResumePegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Resuming flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.ResumeFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.StopPegasus:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Stopping flythrough on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        m_pegasus.StopFlythrough();
                        break;
                    case PegasusConstants.PoiPegasusTriggerAction.DoNothing:
                        if (poi.m_manager.m_displayDebug == true)
                        {
                            Debug.Log(string.Format("Doing nothing on {0} from {1}", poi.m_manager.name, poi.name));
                        }
                        break;
                }
            }
        }
    }
}
