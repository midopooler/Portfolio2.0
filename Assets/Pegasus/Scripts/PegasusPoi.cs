using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pegasus
{
    /// <summary>
    /// Add this object to a a game object in the scene to include it in a flythrough
    /// </summary>
    /// 
    public class PegasusPoi : MonoBehaviour
    {
        [Tooltip("The type of POI. Auto generated POI's are subject to automatic deletion. Manual POI's will always be kept.")]
        public PegasusConstants.PoiType m_poiType = PegasusConstants.PoiType.Manual;

        [Tooltip("The mechanism used to check height for the POI.")]
        public PegasusConstants.PoiHeightCheckType m_heightCheckType = PegasusConstants.PoiHeightCheckType.ManagerSettings;

        [Tooltip("The lookat type for this POI segement. Changing this will re-generate the lookat location.")]
        public PegasusConstants.LookatType m_lookatType = PegasusConstants.LookatType.Path;

        [Tooltip("The lookat angle from the POI.")]
        public float                m_lookAtAngle = 0f;

        [Tooltip("The lookat distance from the POI.")]
        public float                m_lookAtDistance = 0f;

        [Tooltip("The lookat height above the ground from the POI.")]
        public float                m_lookAtHeight = 0f;

        [Tooltip("The actual lookat location for the POI segment.")]
        public Vector3              m_lookatLocation = Vector3.zero;

        [Tooltip("The start speed type for this segment. The segement will always start at this speed and ease to the next segments start speed.")]
        public PegasusConstants.SpeedType m_startSpeedType = PegasusConstants.SpeedType.Medium;

        [Tooltip("The actual start speed for this segment. Speed varies between stat speed for this segment and start speed for next segment.")]
        public float                m_startSpeed = 1f;

        [HideInInspector]
        public PegasusConstants.EasingType m_rotationEasingType = PegasusConstants.EasingType.Linear;

        [HideInInspector]
        public PegasusConstants.EasingType m_velocityEasingType = PegasusConstants.EasingType.EaseInOut;

        [HideInInspector]
        public PegasusConstants.EasingType m_positionEasingType = PegasusConstants.EasingType.Linear;

        [HideInInspector]
        public PegasusManager       m_manager;                  //The manager which created this poi and owns it

        [HideInInspector]
        public bool                 m_alwaysShowGizmos = true;  //Whether we should show gizmos even when not selected

        [HideInInspector]
        public float                m_segmentDistance = 0f;     //The actual travel distance of this segment based on spline

        [HideInInspector]
        public TimeSpan             m_segmentStartTime = TimeSpan.Zero; //The time after start time that this segment will start playing

        [HideInInspector]
        public TimeSpan             m_segmentDuration = TimeSpan.Zero; //The amount of time it will take to traverse this segment

        [HideInInspector]
        public int                  m_segmentIndex = 0;         //The index of this segment in the overal flythrough

        [HideInInspector]
        public bool                 m_isFirstPOI = true;        //Whether or not we are the first POI

        [HideInInspector]
        public bool                 m_isLastPOI = true;        //Whether or not we are the last POI

        [HideInInspector]
        public PegasusPoi           m_prevPoi;                  //The previous POI, will initially be set this

        [HideInInspector]
        public PegasusPoi           m_nextPoi;                  //The next POI, will initially be set to this

        [HideInInspector]
        public List<Vector3>        m_poiSteps = new List<Vector3>(); //The steps that this POI segment will traverse through

        [HideInInspector]
        public bool                 m_isSelected = false;       //Whether or not this poi is selected - draws a different line

        [HideInInspector]           
        public List<TriggerBase> m_poiTriggers = new List<TriggerBase>();   //The triggers associated with this poi

        #region Unity gizmos n editing

        /// <summary>
        /// Draw gizmos when selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }

        /// <summary>
        /// Draw gizmos when not selected
        /// </summary>
        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }

        /// <summary>
        /// Draw the gizmos
        /// </summary>
        /// <param name="isSelected"></param>
        private void DrawGizmos(bool isSelected)
        {
            //Determine whether to drop out
            if (!isSelected && !m_alwaysShowGizmos)
            {
                return;
            }

            //Check to see if we have a change in the state of our list - force an initialise if so
            if (transform.parent.childCount != m_manager.m_poiList.Count)
            {
                m_manager.InitialiseFlythrough();
            }

            #if UNITY_EDITOR
            //Make a check to see if we are selected - disable if not
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetInstanceID() != this.gameObject.GetInstanceID())
            {
                m_isSelected = false;
            }
            #endif

            //Now draw the gizmos

            //Segment path spline
            float velRange = PegasusConstants.SpeedReallyFast - PegasusConstants.SpeedReallySlow;
            if (m_nextPoi != null)
            {
                //See if we need to draw the segments for this
                bool drawSegments = true;
                if (m_isLastPOI && m_manager.m_flythroughType == PegasusConstants.FlythroughType.SingleShot)
                {
                    drawSegments = false;
                }

                if (drawSegments)
                {
                    float inc = 0.05f;
                    float vel1, vel2;
                    Vector3 pos1, pos2;

                    pos1 = CalculatePositionSpline(0f);
                    vel1 = CalculateVelocity(0f);
                    for (float pct = inc; pct <= 1.02f; pct += inc)
                    {
                        //pos2 = CalculatePositionSpline(pct);
                        pos2 = CalculatePositionLinear(pct);
                        vel2 = CalculateVelocity(pct);
                        if (m_isSelected)
                        {
                            Gizmos.color = Color.magenta * Color.Lerp(Color.cyan, Color.red, ((vel1 + vel2 / 2f) - PegasusConstants.SpeedReallySlow) / velRange);
                        }
                        else
                        {
                            Gizmos.color = Color.Lerp(Color.cyan, Color.red, ((vel1 + vel2 / 2f) - PegasusConstants.SpeedReallySlow) / velRange);
                        }
                        Gizmos.DrawLine(pos1, pos2);
                        vel1 = vel2;
                        pos1 = pos2;
                    }
                }
            }


            /*
            Vector3 point1 = m_poiList[segment + 1].m_targetLocation;

            Vector3 point2 = m_poiList[segment + 2].m_targetLocation;

            Vector3 tangent2 = m_poiList[segment + 3].m_targetLocation;

            Vector3 t2 = Vector3.Normalize(m_poiList[segment + 2].m_targetLocation - m_poiList[segment + 3].m_targetLocation) * smooth;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_poiList[segment + 2].m_targetLocation, m_poiList[segment + 2].m_targetLocation + t2);


            tangent2 = m_poiList[segment + 2].m_targetLocation + t2;

            Gizmos.color = Color.green;

            for (int i = 0; i < 30; i++)
            {
                Gizmos.DrawLine(Hermite(point1, tangent1, point2, tangent2, (float) i/40f),
                    Hermite(point1, tangent1, point2, tangent2, (float)(i + 1)/40f));
            }
            */

            //Only dray this in correct scenario
            if (m_lookatType == PegasusConstants.LookatType.Target)
            {
                //Line to lookat location
                Gizmos.color = Color.Lerp(Color.cyan, Color.red, (m_startSpeed - PegasusConstants.SpeedReallySlow) / velRange);
                Gizmos.DrawLine(transform.position, m_lookatLocation);

                //Lookat sphere
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(m_lookatLocation, 0.25f);
            }

            //Are we last one, and is play next chosen
            Gizmos.color = Color.yellow;
            if (m_isLastPOI == true && m_manager.m_flythroughType == PegasusConstants.FlythroughType.SingleShot &&
                m_manager.m_flythroughEndAction == PegasusConstants.FlythroughEndAction.PlayNextPegasus &&
                m_manager.m_nextPegasus != null)
            {
                PegasusPoi nextPoi = m_manager.m_nextPegasus.GetFirstPOI();
                if (nextPoi != null)
                {
                    Gizmos.DrawLine(transform.position, nextPoi.transform.position);
                }
            }

            //Flythrough location
            Gizmos.DrawSphere(transform.position, m_manager.m_poiGizmoSize);
        }

        #endregion

        /// <summary>
        /// Return true if the other object is the same as this one
        /// </summary>
        /// <param name="poi">The other object to check</param>
        /// <returns></returns>
        public bool IsSameObject(PegasusPoi poi)
        {
            if (poi == null)
            {
                return false;
            }
            if (this.GetInstanceID() == poi.GetInstanceID())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialise this POI's settings, leveraging the manager
        /// </summary>
        public void Initialise(bool updateSegments = true)
        {
            //Make sure prev and next set up - if not then exit
            if (m_prevPoi == null || m_nextPoi == null)
            {
                return;
            }

            //Move us if we are below the minimum height on the terrain
            transform.position = m_manager.GetValidatedPoiPosition(transform.position, m_heightCheckType);

            //Set up easing calculators
            switch (m_velocityEasingType)
            {
                case PegasusConstants.EasingType.Linear:
                    m_velocityEasingCalculator = new Easing(EaseLinear);
                    break;
                case PegasusConstants.EasingType.EaseIn:
                    m_velocityEasingCalculator = new Easing(EaseIn);
                    break;
                case PegasusConstants.EasingType.EaseOut:
                    m_velocityEasingCalculator = new Easing(EaseOut);
                    break;
                case PegasusConstants.EasingType.EaseInOut:
                    m_velocityEasingCalculator = new Easing(EaseInOut);
                    break;
            }
            switch (m_rotationEasingType)
            {
                case PegasusConstants.EasingType.Linear:
                    m_rotationEasingCalculator = new Easing(EaseLinear);
                    break;
                case PegasusConstants.EasingType.EaseIn:
                    m_rotationEasingCalculator = new Easing(EaseIn);
                    break;
                case PegasusConstants.EasingType.EaseOut:
                    m_rotationEasingCalculator = new Easing(EaseOut);
                    break;
                case PegasusConstants.EasingType.EaseInOut:
                    m_rotationEasingCalculator = new Easing(EaseInOut);
                    break;
            }
            switch (m_positionEasingType)
            {
                case PegasusConstants.EasingType.Linear:
                    m_positionEasingCalculator = new Easing(EaseLinear);
                    break;
                case PegasusConstants.EasingType.EaseIn:
                    m_positionEasingCalculator = new Easing(EaseIn);
                    break;
                case PegasusConstants.EasingType.EaseOut:
                    m_positionEasingCalculator = new Easing(EaseOut);
                    break;
                case PegasusConstants.EasingType.EaseInOut:
                    m_positionEasingCalculator = new Easing(EaseInOut);
                    break;
            }

            //Now move things around and update variables based on lookat type
            switch (m_lookatType)
            {
                case PegasusConstants.LookatType.Path:
                    m_lookatLocation = CalculatePositionSpline(0.005f);
                    GetRelativeOffsets(transform.position, m_lookatLocation, out m_lookAtDistance, out m_lookAtHeight, out m_lookAtAngle);
                    break;
                case PegasusConstants.LookatType.Target:
                    GetRelativeOffsets(transform.position, m_lookatLocation, out m_lookAtDistance, out m_lookAtHeight, out m_lookAtAngle);
                    break;
            }

            //Setup the start and end rotations - required for progression calculations - note this assumes that the next poi lookat location has been calculated already
            Vector3 rotationDir = m_lookatLocation - transform.position;
            if (rotationDir != Vector3.zero)
            {
                m_rotationStart = Quaternion.LookRotation(m_lookatLocation - transform.position) * transform.localRotation;
            }
            else
            {
                m_rotationStart = transform.localRotation;
            }
            rotationDir = m_nextPoi.m_lookatLocation - m_nextPoi.transform.position;
            if (rotationDir != Vector3.zero)
            {
                m_rotationEnd = Quaternion.LookRotation(rotationDir) * m_nextPoi.transform.localRotation;
            }
            else
            {
                m_rotationEnd = m_nextPoi.transform.localRotation;
            }

            //Speed
            switch (m_startSpeedType)
            {
                case PegasusConstants.SpeedType.ReallySlow:
                    m_startSpeed = PegasusConstants.SpeedReallySlow;
                    break;
                case PegasusConstants.SpeedType.Slow:
                    m_startSpeed = PegasusConstants.SpeedSlow;
                    break;
                case PegasusConstants.SpeedType.Medium:
                    m_startSpeed = PegasusConstants.SpeedMedium;
                    break;
                case PegasusConstants.SpeedType.Fast:
                    m_startSpeed = PegasusConstants.SpeedFast;
                    break;
                case PegasusConstants.SpeedType.ReallyFast:
                    m_startSpeed = PegasusConstants.SpeedReallyFast;
                    break;
            }

            //Update travel distance for the segment
            if (updateSegments)
            {
                UpdateSegment();
            }

            //Get the POI triggers
            m_poiTriggers.Clear();
            m_poiTriggers.AddRange(gameObject.GetComponentsInChildren<TriggerBase>());
        }

        /// <summary>
        /// Called when this poi starts its flythrough
        /// </summary>
        public void OnStartTriggers()
        {
            for (int idx = 0; idx < m_poiTriggers.Count; idx++)
            {
                m_poiTriggers[idx].OnStart(this);
            }
        }

        /// <summary>
        /// Called when this poi continues its flythrough
        /// </summary>
        public void OnUpdateTriggers(float progress)
        {
            for (int idx = 0; idx < m_poiTriggers.Count; idx++)
            {
                m_poiTriggers[idx].OnUpdate(this, progress);
            }
        }


        /// <summary>
        /// Called when this poi starts its flythrough
        /// </summary>
        public void OnEndTriggers()
        {
            for (int idx = 0; idx < m_poiTriggers.Count; idx++)
            {
                m_poiTriggers[idx].OnEnd(this);
            }
        }


        #region Handy value getters

        /// <summary>
        /// Get the poi start speed based on the speed type
        /// </summary>
        /// <param name="speedType"></param>
        /// <returns></returns>
        public float GetStartSpeed(PegasusConstants.SpeedType speedType)
        {
            switch (speedType)
            {
                case PegasusConstants.SpeedType.ReallySlow:
                    return PegasusConstants.SpeedReallySlow;
                case PegasusConstants.SpeedType.Slow:
                    return PegasusConstants.SpeedSlow;
                case PegasusConstants.SpeedType.Medium:
                    return PegasusConstants.SpeedMedium;
                case PegasusConstants.SpeedType.Fast:
                    return PegasusConstants.SpeedFast;
                case PegasusConstants.SpeedType.ReallyFast:
                    return PegasusConstants.SpeedReallyFast;
            }
            return m_startSpeed;
        }

        /// <summary>
        /// Update the current segment distance
        /// </summary>
        public void UpdateSegment()
        {
            m_segmentDistance = 0f;
            m_segmentDuration = TimeSpan.Zero;
            m_segmentStartTime = TimeSpan.Zero;
            if (!m_isFirstPOI)
            {
                m_segmentStartTime = m_prevPoi.m_segmentStartTime + m_prevPoi.m_segmentDuration;
            }
            m_poiSteps.Clear();

            if (m_manager.m_flythroughType == PegasusConstants.FlythroughType.SingleShot && m_manager.GetNextPOI(this, false) == null)
            {
                return;
            }

            float pct = 0f;
            Vector3 pos1 = Vector3.zero;
            Vector3 pos2 = Vector3.zero;

            if (m_nextPoi != null)
            {
                //Calculate the segment distance by iterating over the spline - at a resolution related to the overall distance
                int stepsPerMeter = 3;
                int measurementsPerMeter = stepsPerMeter * 20; //Another magic multiplier - the more steps per meter the more measurements are required
                int measurement;
                float straightLineDistance = Vector3.Distance(transform.position, m_nextPoi.transform.position);
                int totalMeasurments = (int)Mathf.Ceil((float)measurementsPerMeter * straightLineDistance);
                float measurementIncrement = 1f / (float)totalMeasurments;
                float measurementDistance = 0f;
                float steppedDistance = 0f;
                float minMeasurementDistance = 0f;
                float maxMeasurementDistance = 0f;
                float totalSteppedDistance = 0f;
                float minMeasuredStepDistance = 0f;
                float maxMeasuredStepDistance = 0f;

                pos1 = transform.position;
                for (measurement = 1, pct = 0f, minMeasurementDistance = 0f, maxMeasurementDistance = 0f; measurement <= totalMeasurments; measurement++)
                {
                    pct += measurementIncrement;
                    pos2 = CalculatePositionSpline(pct);
                    pos2 = m_manager.GetValidatedPoiPosition(pos2, m_heightCheckType);
                    measurementDistance = Vector3.Distance(pos1, pos2);
                    m_segmentDistance += measurementDistance;
                    pos1 = pos2;

                    //For debugging
                    if (ApproximatelyEqual(minMeasurementDistance, 0f) || (measurementDistance < minMeasurementDistance))
                    {
                        minMeasurementDistance = measurementDistance;
                    }
                    if (ApproximatelyEqual(maxMeasurementDistance, 0f) || (measurementDistance > maxMeasurementDistance))
                    {
                        maxMeasurementDistance = measurementDistance;
                    }
                }
                //Debug.Log(string.Format("{0} - meas dist {1:0.0000},  min dist {2:0.0000}, max dist {3:0.0000}, staight length {4:0.000}, spline length {5:0.000}", transform.name, 1f / (float)measurementsPerMeter   , minMeasurementDistance, maxMeasurementDistance, straightLineDistance, m_segmentDistance));

                //Refine steps - increase them if there is a small distance in oder to create a smoother movement
                if (m_segmentDistance < 2f)
                {
                    stepsPerMeter *= 3; //Arbitrar magic value
                }

                //Now add in the actual steps - the biggest thing they need to be to get consistent speed is as close to eqidistant as possible
                float expectedStepDistance = 1f / (float)stepsPerMeter; //We want an exact multiple that gets to the whole length, but is nearest to 1 / stepsPerMeter
                expectedStepDistance = m_segmentDistance / Mathf.Floor(m_segmentDistance / expectedStepDistance);

                pos1 = transform.position;
                m_poiSteps.Add(pos1);
                for (measurement = 1, pct = 0f, minMeasuredStepDistance = 0f, maxMeasuredStepDistance = 0f; measurement <= totalMeasurments; measurement++)
                {
                    pct += measurementIncrement;
                    pos2 = CalculatePositionSpline(pct);
                    pos2 = m_manager.GetValidatedPoiPosition(pos2, m_heightCheckType);

                    measurementDistance = Vector3.Distance(pos1, pos2);
                    steppedDistance += measurementDistance;
                    if (steppedDistance >= expectedStepDistance)
                    {
                        //For debugging
                        if (ApproximatelyEqual(minMeasuredStepDistance, 0f) || (steppedDistance < minMeasuredStepDistance))
                        {
                            minMeasuredStepDistance = steppedDistance;
                        }
                        if (ApproximatelyEqual(maxMeasuredStepDistance, 0f) || (steppedDistance > maxMeasuredStepDistance))
                        {
                            maxMeasuredStepDistance = steppedDistance;
                        }

                        //Lerp the intermediary steps to maintain distances
                        while (steppedDistance >= expectedStepDistance)
                        {
                            m_poiSteps.Add(Vector3.Lerp(m_poiSteps[m_poiSteps.Count-1], pos2, expectedStepDistance / steppedDistance));
                            steppedDistance -= expectedStepDistance;
                            totalSteppedDistance += expectedStepDistance;
                        }
                    }
                    pos1 = pos2;
                }

                //Debug.Log(string.Format("{0} - exp step dist {1:0.0000},  min step dist {2:0.0000}, max step dist {3:0.0000}, straight length {4:0.000}, spline length {5:0.000}, stepped length {6:0.000}, steps {7}, ERROR {8:0.000}, ERROR % {9:0.00}", transform.name, expectedStepDistance, minMeasuredStepDistance, maxMeasuredStepDistance, straightLineDistance, m_segmentDistance, totalSteppedDistance, m_poiSteps.Count, totalSteppedDistance - m_segmentDistance, ((totalSteppedDistance - m_segmentDistance) / expectedStepDistance) * 100f));

                //Check the last position, if we just missed out then add another, otherwise move the last one so it is exact
                if (((totalSteppedDistance - m_segmentDistance) / expectedStepDistance) < -0.5f)
                {
                    m_poiSteps.Add(m_nextPoi.transform.position);
                }
                else
                {
                    m_poiSteps[m_poiSteps.Count - 1] = m_nextPoi.transform.position;
                }
            }

            //Update the duration
            UpdateSegmentDuration();
        }
        
        /// <summary>
        /// Update the segments duration - needs to be called after update segment
        /// </summary>
        public void UpdateSegmentDuration()
        {
            //Update the expected traversal time (based on either fps setting, or an average of 60fps)
            m_segmentDuration = TimeSpan.Zero;
            float pct = 0f;
            Vector3 pos1 = CalculatePositionLinear(0f);
            Vector3 pos2 = Vector3.zero;
            float duration = 0f;
            for (pct = 0f; pct < 1f; pct += 0.05f)
            {
                pos2 = CalculatePositionLinear(pct);
                duration += Vector3.Distance(pos1, pos2) / CalculateVelocity(pct);
                pos1 = pos2;
            }
            m_segmentDuration = TimeSpan.FromSeconds(duration);
        }

        #endregion

        #region Progression variables and calculations - uses more memory to reduce allocations that can cause framerate jitter

        /// <summary>
        /// Easing calculator used to smooth calculations - stops the jolts that kill immersion
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        delegate float Easing(float time, float duration = 1f);
        private Easing m_velocityEasingCalculator = new Easing(EaseLinear);
        private Easing m_positionEasingCalculator = new Easing(EaseLinear);
        private Easing m_rotationEasingCalculator = new Easing(EaseLinear);

        /// <summary>
        /// Variables used to determine lookat target and rotation
        /// </summary>
        public Quaternion m_rotationStart = Quaternion.identity;
        public Quaternion m_rotationEnd = Quaternion.identity;

        /// <summary>
        /// Calculate progress on the variables supplied
        /// </summary>
        /// <param name="percent">The percentage through the segment that the calculations should be made for from 0 == 0% to 1 == 100%</param>
        /// <param name="velocity">Velocity at that point</param>
        /// <param name="position">Position at that point</param>
        /// <param name="rotation">Rotation at that point</param>
        public void CalculateProgress(float percent, out float velocity, out Vector3 position, out Quaternion rotation)
        {
            velocity = CalculateVelocity(percent);
            rotation = CalculateRotation(percent);
            position = CalculatePositionLinear(percent);
        }

        /// <summary>
        /// Calculate position based on the variables supplied
        /// </summary>
        /// <param name="percent">The percentage through the segment that the calculations should be made for from 0 == 0% to 1 == 100%</param>
        /// <returns>Position at that point</returns>
        public Vector3 CalculatePositionSpline(float percent)
        {
            return CatmullRom(m_prevPoi.transform.position, transform.position, m_nextPoi.transform.position, m_nextPoi.m_nextPoi.transform.position, percent);
        }

        /// <summary>
        /// Calculate position based on the variables supplied. This must only be called after initialisation as it depends on the initialisation setup to work.
        /// </summary>
        /// <param name="percent">The percentage through the segment that the calculations should be made for from 0 == 0% to 1 == 100%</param>
        /// <returns>Return the next position</returns>
        public Vector3 CalculatePositionLinear(float percent)
        {
            //Ease the percentage
            percent = m_positionEasingCalculator(percent);

            //Handle no data
            if (m_poiSteps.Count == 0)
            {
                return Vector3.zero;
            }

            //Handle only one item
            if (m_poiSteps.Count == 1)
            {
                return m_poiSteps[0];
            }

            int maxSegments = m_poiSteps.Count - 1;
            int firstSegment = (int)(percent * (float)maxSegments);
            if (firstSegment == maxSegments)
            {
                return m_poiSteps[firstSegment];
            }

            float progress = (percent * (float)maxSegments) - (float)firstSegment;
            //Debug.Log(string.Format("Pct is {0:0.000}, {1} {2}, progress is {3:0.000} lerp is {4:0.000} {5:0.000} ", percent, firstSegment, firstSegment+1, progress, Vector3.Lerp(m_poiSteps[(int)(firstSegment)], m_poiSteps[(int)(firstSegment)+1], progress).x, Vector3.Lerp(m_poiSteps[(int)(firstSegment)], m_poiSteps[(int)(firstSegment)+1], progress).z));
            return Vector3.Lerp(m_poiSteps[firstSegment], m_poiSteps[firstSegment+1], progress);
        }

        /// <summary>
        /// Calculate the velocity at the given position in the segment
        /// </summary>
        /// <param name="percent">Valuer between 0..1</param>
        /// <returns>Velocity at that location</returns>
        public float CalculateVelocity(float percent)
        {
            return Mathf.Lerp(m_startSpeed, m_nextPoi.m_startSpeed, m_velocityEasingCalculator(percent));
        }

        /// <summary>
        /// Calculate the rotation at the given position in the segment
        /// </summary>
        /// <param name="percent">Value between 0..1</param>
        /// <returns>Rotation at that location</returns>
        public Quaternion CalculateRotation(float percent)
        {
            return Quaternion.Lerp(m_rotationStart, m_rotationEnd, m_rotationEasingCalculator(percent));
        }

        #endregion

        #region Routines influenced by terrain

        /// <summary>
        /// Get the offsets of targetDistance, targetHeight relative to ground, and targetAngle of y rotation from source position to target position, as if they were on the same plane
        /// </summary>
        /// <param name="source">Source postiion</param>
        /// <param name="target">Target position</param>
        /// <param name="targetDistance">Distance from source to target as if on same plane</param>
        /// <param name="targetHeight">Height of target relative to the terrain</param>
        /// <param name="targetAngle">Angle from source to target, allowing only for rotation on y axis</param>
        public void GetRelativeOffsets(Vector3 source, Vector3 target, out float targetDistance, out float targetHeight, out float targetAngle)
        {
            targetHeight = m_manager.GetValidatedLookatHeightRelativeToMinimum(target, m_heightCheckType);
            Vector3 planarTargetPosition = new Vector3(target.x, source.y, target.z);
            targetDistance = Vector3.Distance(source, planarTargetPosition);

            Vector3 targetDirection = source - target;
            if (targetDirection != Vector3.zero)
            {
                targetAngle = Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles.y;
            }
            else
            {
                targetAngle = 0;
            }
        }

        #endregion

        #region Handy general maths routines

        /// <summary>
        /// Return true if the values are approximately equal
        /// </summary>
        /// <param name="a">Parameter A</param>
        /// <param name="b">Parameter B</param>
        /// <returns>True if approximately equal</returns>
        public static bool ApproximatelyEqual(float a, float b)
        {
            if (a == b || Mathf.Abs(a - b) < float.Epsilon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Rotate the point around the pivot - used to handle rotation
        /// </summary>
        /// <param name="point">Point to move</param>
        /// <param name="pivot">Pivot</param>
        /// <param name="angle">Angle to pivot</param>
        /// <returns>New location</returns>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
        {
            Vector3 dir = point - pivot;
            dir = Quaternion.Euler(angle)*dir;
            point = dir + pivot;
            return point;
        }


        /// <summary>
        /// Linear easing
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private static float EaseLinear(float time, float duration = 1f)
        {
            return time/duration;
        }

        /// <summary>
        /// Ease in
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private static float EaseIn(float time, float duration = 1f)
        {
            return (time /= duration)*time;
        }

        /// <summary>
        /// Ease out
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private static float EaseOut(float time, float duration = 1f)
        {
            return -1f*(time /= duration)*(time - 2f);
        }

        /// <summary>
        /// Ease in and out
        /// </summary>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private static float EaseInOut(float time, float duration = 1f)
        {
            if ((time /= duration/2f) < 1f)
                return 0.5f*time*time;
            return -0.5f*((--time)*(time - 2f) - 1f);
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of CatmullRom interpolation.</returns>
        public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
        {
            return new Vector3(CalcCatmullRom(value1.x, value2.x, value3.x, value4.x, amount), CalcCatmullRom(value1.y, value2.y, value3.y, value4.y, amount), CalcCatmullRom(value1.z, value2.z, value3.z, value4.z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        /// <param name="value1">The first vector in interpolation.</param>
        /// <param name="value2">The second vector in interpolation.</param>
        /// <param name="value3">The third vector in interpolation.</param>
        /// <param name="value4">The fourth vector in interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
        public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
        {
            result.x = CalcCatmullRom(value1.x, value2.x, value3.x, value4.x, amount);
            result.y = CalcCatmullRom(value1.y, value2.y, value3.y, value4.y, amount);
            result.z = CalcCatmullRom(value1.z, value2.z, value3.z, value4.z, amount);
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="value1">The first position in the interpolation.</param>
        /// <param name="value2">The second position in the interpolation.</param>
        /// <param name="value3">The third position in the interpolation.</param>
        /// <param name="value4">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A position that is the result of the Catmull-Rom interpolation.</returns>
        public static float CalcCatmullRom(float value1, float value2, float value3, float value4, float amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precision
            double amountSquared = amount*amount;
            double amountCubed = amountSquared*amount;
            return (float) (0.5*(2.0*value2 + (value3 - value1)*amount + (2.0*value1 - 5.0*value2 + 4.0*value3 - value4)*amountSquared + (3.0*value2 - value1 - 3.0*value3 + value4)*amountCubed));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The hermite spline interpolation vector.</returns>
        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
        {
            return new Vector3(CalcHermite(value1.x, tangent1.x, value2.x, tangent2.x, amount), CalcHermite(value1.y, tangent1.y, value2.y, tangent2.y, amount), CalcHermite(value1.z, tangent1.z, value2.z, tangent2.z, amount));
        }

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains hermite spline interpolation.
        /// </summary>
        /// <param name="value1">The first position vector.</param>
        /// <param name="tangent1">The first tangent vector.</param>
        /// <param name="value2">The second position vector.</param>
        /// <param name="tangent2">The second tangent vector.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
        {
            result.x = CalcHermite(value1.x, tangent1.x, value2.x, tangent2.x, amount);
            result.y = CalcHermite(value1.y, tangent1.y, value2.y, tangent2.y, amount);
            result.z = CalcHermite(value1.z, tangent1.z, value2.z, tangent2.z, amount);
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">Source position.</param>
        /// <param name="tangent1">Source tangent.</param>
        /// <param name="value2">Source position.</param>
        /// <param name="tangent2">Source tangent.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static float CalcHermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s*s*s;
            double sSquared = s*s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2*v1 - 2*v2 + t2 + t1)*sCubed + (3*v2 - 3*v1 - 2*t1 - t2)*sSquared + t1*s + v1;
            return (float) result;
        }

        #endregion
    }
}
