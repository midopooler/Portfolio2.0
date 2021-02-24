using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pegasus
{
    /// <summary>
    /// Spawn a bunch of special objects in a scene, and the flythrough manager will find and visit them
    /// </summary>
    /// 
    public class PegasusManager : MonoBehaviour
    {
        /// <summary>
        /// The target game object we will be driving - or main scene camera if nothing selected
        /// </summary>
        public GameObject m_target;

        /// <summary>
        /// Default flythrough type
        /// </summary>
        public PegasusConstants.FlythroughType m_flythroughType = PegasusConstants.FlythroughType.Looped;

        /// <summary>
        /// What to do when we fet to the end - only relevant on Once through flythroughs
        /// </summary>
        public PegasusConstants.FlythroughEndAction m_flythroughEndAction = PegasusConstants.FlythroughEndAction.StopFlythrough;

        /// <summary>
        /// Select the targetted frame rate for the fly through
        /// </summary>
        public PegasusConstants.TargetFrameRate m_targetFramerateType = PegasusConstants.TargetFrameRate.MaxFps;

        /// <summary>
        /// The algorithm the system uses to check minimum heights against
        /// </summary>
        public PegasusConstants.HeightCheckType m_heightCheckType = PegasusConstants.HeightCheckType.Terrain;

        /// <summary>
        /// Autostart the manager at runtime
        /// </summary>
        public bool m_autoStartAtRuntime = true;

        /// <summary>
        /// The POI list
        /// </summary>
        public List<PegasusPoi> m_poiList = new List<PegasusPoi>();

        /// <summary>
        /// The minimum height above the terrain
        /// </summary>
        public float m_minHeightAboveTerrain = PegasusConstants.FlybyOffsetDefaultHeight;

        /// <summary>
        /// Display debug messages
        /// </summary>
        public bool m_displayDebug = false;      //Set to true if we want debug messages

        /// <summary>
        /// Always show gizmos
        /// </summary>
        public bool m_alwaysShowGizmos = true;

        public PegasusConstants.FlythroughState m_currentState = PegasusConstants.FlythroughState.Stopped;
        public int m_currentSegmentIdx;
        public PegasusPoi m_currentSegment;
        public float m_currentSegmentDistanceTravelled = 0f;
        public float m_totalDistanceTravelled = 0f;
        public float m_totalDistanceTravelledPct = 0f;
        public float m_totalDistance = 0f;
        public TimeSpan m_totalDuration = TimeSpan.Zero;
        public float m_currentVelocity = 0f;
        public Vector3 m_currentPosition = Vector3.zero;
        public Quaternion m_currentRotation = Quaternion.identity;

        public bool m_canUpdateNow = false; //Used to trigger when the camera and targetLocation can be updated
        public DateTime m_lastUpdateTime = DateTime.MinValue;
        public float m_frameUpdateTime = 1f / 60f;
        public float m_frameUpdateDistance = 0f;
        public float m_rotationDamping = 0.75f;
        public float m_positionDamping = 0.3f;

        public PegasusManager m_nextPegasus = null; //Used when single shot is assigned and end action is play next pegasus

        //Editor related
        public bool m_alwaysShowPath = false;
        public bool m_showScrubber = false;
        public bool m_showPOIHelpers = false;
        public float m_poiGizmoSize = 0.75f;
        public bool m_showAdvanced = false;
        public float m_collisionHeightOffset = 1000f; //How far above a location to check for collisions

        //Defaults
        public PegasusDefaults m_defaults = null;

        /// <summary>
        /// Scene playback and initialisation
        /// </summary>
        void Start()
        {
            //Make sure we are stopped
            m_currentState = PegasusConstants.FlythroughState.Stopped;

            //Grab the main camera if nothing defined
            if (m_target == null)
            {
                if (Camera.main == null)
                {
                    Debug.LogWarning("Can not start Pegasus - no target has been assigned.");
                    return;
                }
                else
                {
                    if (m_displayDebug == true)
                    {
                        Debug.Log("Assigning main camera to target : " + Camera.main.name);
                    }
                    m_target = Camera.main.gameObject;
                }
            }

            //Set the applications target framerate
            ChangeFramerate(m_targetFramerateType);

            //Initialise the flythrough - does all the expensive calcs in one hit
            InitialiseFlythrough();

            //Auto start at runtime if necessary
            if (m_autoStartAtRuntime == true)
            {
                StartFlythrough();
            }
        }

        #region Setup and control routines

        /// <summary>
        /// Initialise a flythrough - setup all the segments
        /// </summary>
        public void InitialiseFlythrough()
        {
            if (m_displayDebug)
            {
                Debug.Log("Initialising flythrough...");
            }

            m_currentState = PegasusConstants.FlythroughState.Initialising;

            //Rebuild the POI list based on order of poi children
            m_poiList.Clear();
            int idx;
            for (idx = 0; idx < transform.childCount; idx++)
            {
                m_poiList.Add(transform.GetChild(idx).GetComponent<PegasusPoi>());
            }

            //Iterate thru the list and set first, last, prev next on the poi
            PegasusPoi poi = null;
            for (idx = 0; idx < m_poiList.Count; idx++)
            {
                poi = m_poiList[idx];

                //Update the index
                poi.m_segmentIndex = idx;

                //Set first / last flags
                if (idx == 0)
                {
                    poi.m_isFirstPOI = true;
                }
                else
                {
                    poi.m_isFirstPOI = false;
                }
                if (idx == m_poiList.Count - 1)
                {
                    poi.m_isLastPOI = true;
                }
                else
                {
                    poi.m_isLastPOI = false;
                }

                //Set prev / next pointers
                if (m_flythroughType == PegasusConstants.FlythroughType.SingleShot)
                {
                    //Previous
                    if (poi.m_isFirstPOI)
                    {
                        if (m_poiList.Count > 1)
                        {
                            poi.m_prevPoi = m_poiList[1];
                        }
                        else
                        {
                            poi.m_prevPoi = poi;
                        }
                    }
                    else
                    {
                        poi.m_prevPoi = m_poiList[idx - 1];
                    }
                    //Next
                    if (poi.m_isLastPOI)
                    {
                        if (m_poiList.Count > 1)
                        {
                            poi.m_nextPoi = m_poiList[idx - 1];
                        }
                        else
                        {
                            poi.m_nextPoi = poi;
                        }
                    }
                    else
                    {
                        poi.m_nextPoi = m_poiList[idx + 1];
                    }
                }
                else
                {
                    //Previous
                    if (idx == 0)
                    {
                        poi.m_prevPoi = m_poiList[m_poiList.Count - 1];
                    }
                    else
                    {
                        poi.m_prevPoi = m_poiList[idx - 1];
                    }
                    //Next
                    if (idx == m_poiList.Count - 1)
                    {
                        poi.m_nextPoi = m_poiList[0];
                    }
                    else
                    {
                        poi.m_nextPoi = m_poiList[idx + 1];
                    }
                }

                poi.m_alwaysShowGizmos = m_alwaysShowGizmos;
            }

            //Get the poi to initialise themselves - run twice as some settings depend on the next in the sequence which may not yet be initialised
            for (idx = 0; idx < m_poiList.Count; idx++)
            {
                m_poiList[idx].Initialise(false);
            }

            //Next time thru grab the total distance and duration
            m_totalDuration = TimeSpan.Zero;
            m_totalDistanceTravelledPct = 0f;
            m_totalDistanceTravelled = 0f;
            m_totalDistance = 0f;
            for (idx = 0; idx < m_poiList.Count; idx++)
            {
                m_poiList[idx].Initialise(true);
                m_poiList[idx].m_segmentStartTime = new TimeSpan(m_totalDuration.Ticks);
                m_totalDistance += m_poiList[idx].m_segmentDistance;
                m_totalDuration += m_poiList[idx].m_segmentDuration;
            }

            //Set up the initial state
            m_currentSegmentIdx = 0;
            if (m_poiList.Count > 0)
            {
                m_currentSegment = m_poiList[m_currentSegmentIdx];
            }
            else
            {
                m_currentSegment = null;
            }
            m_currentSegmentDistanceTravelled = 0f;

            //Current time
            m_lastUpdateTime = DateTime.Now;

            //Signal that we can update
            m_canUpdateNow = true;
        }

        /// <summary>
        /// Restart the flythrough
        /// </summary>
        private void RestartFlythrough()
        {
            if (m_displayDebug)
            {
                Debug.Log("Restarting flythrough...");
            }

            m_currentState = PegasusConstants.FlythroughState.Initialising;
            m_totalDistanceTravelledPct = 0f;
            m_totalDistanceTravelled = 0f;

            //Set up the initial state
            m_currentSegmentIdx = 0;
            if (m_poiList.Count > 0)
            {
                m_currentSegment = m_poiList[m_currentSegmentIdx];
            }
            else
            {
                m_currentSegment = null;
            }
            m_currentSegmentDistanceTravelled = 0f;

            //Current time
            m_lastUpdateTime = DateTime.Now;

            //Signal that we can update
            m_canUpdateNow = true;
        }

        /// <summary>
        /// Update the meta data influenced by the connectivity of the flythrough - distance and duration.
        /// Assumes that the segments have previously been initialised so that distance and durations are correct.
        /// </summary>
        public void UpdateFlythroughMetaData()
        {
            m_totalDuration = TimeSpan.Zero;
            m_totalDistance = 0f;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                m_poiList[idx].m_segmentStartTime = new TimeSpan(m_totalDuration.Ticks);
                m_totalDuration += m_poiList[idx].m_segmentDuration;
                m_totalDistance += m_poiList[idx].m_segmentDistance;
            }
        }

        /// <summary>
        /// Update the dependencies that will change when changes are made to an individual segment as
        /// they flow back 2, and forward 1 segments due to the underlying spline system.
        /// </summary>
        /// <param name="segment"></param>
        public void UpdateSegmentWithDependencies(PegasusPoi segment)
        {
            if (segment == null)
            {
                Debug.LogError("Attempting to update null segment!");
                return;
            }
            if (m_flythroughType == PegasusConstants.FlythroughType.SingleShot)
            {
                int idx = segment.m_segmentIndex;
                SafeInitialise(idx - 2, false, true);
                SafeInitialise(idx - 1, false, true);
                SafeInitialise(idx, false, true);
                SafeInitialise(idx + 1, false, true);
            }
            else
            {
                int idx = segment.m_segmentIndex;
                SafeInitialise(idx - 2, true, true);
                SafeInitialise(idx - 1, true, true);
                SafeInitialise(idx, true, true);
                SafeInitialise(idx + 1, true, true);
            }

            //Then do a full flythrough metadata update
            UpdateFlythroughMetaData();
        }

        /// <summary>
        /// Run a safe initialise - bury out of range and handle wrapping
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="wrap"></param>
        /// <param name="updateSegments"></param>
        private void SafeInitialise(int idx, bool wrap, bool updateSegments)
        {
            if (!wrap)
            {
                if (idx >= 0 && idx < m_poiList.Count)
                {
                    m_poiList[idx].Initialise(updateSegments);
                }
            }
            else
            {
                idx = idx % m_poiList.Count;
                if (idx < 0)
                {
                    idx += m_poiList.Count;
                }
                m_poiList[idx].Initialise(updateSegments);
            }
        }

        #endregion

        #region Public API routines

        /// <summary>
        /// Start a flythrough from scratch
        /// </summary>
        public void StartFlythrough(bool fullInitialise = false)
        {
            //Enable the application to be able to run in the background
            Application.runInBackground = true;

            //Initialise it 
            if (fullInitialise == true)
            {
                InitialiseFlythrough();
            }
            else
            {
                RestartFlythrough();
            }

            //Now start the flythough
            if (m_displayDebug)
            {
                Debug.Log("Starting flythrough..");
            }

            //Move target to its initial position and location
            if (m_target != null)
            {
                m_currentSegment.CalculateProgress(0, out m_currentVelocity, out m_currentPosition, out m_currentRotation);
                m_target.transform.rotation = m_currentRotation;
                m_target.transform.position = m_currentPosition;
            }
            else
            {
                Debug.LogWarning("Cannot start Pegasus - no target has been assigned!");
                m_currentState = PegasusConstants.FlythroughState.Stopped;
                return;
            }

            //Kick off triggers
            m_currentSegment.OnStartTriggers();

            //Mark as started
            m_currentState = PegasusConstants.FlythroughState.Started;
        }

        /// <summary>
        /// Resume a flythrough
        /// </summary>
        public void ResumeFlythrough()
        {
            if (m_displayDebug)
            {
                Debug.Log("Resuming flythrough");
            }

            if (m_currentState != PegasusConstants.FlythroughState.Paused)
            {
                Debug.LogWarning("Can not resume flythrough - it was not paused.");
                return;
            }

            //Mark as started
            m_currentState = PegasusConstants.FlythroughState.Started;
        }

        /// <summary>
        /// Pause a flythrough - can be resumed
        /// </summary>
        public void PauseFlythrough()
        {
            if (m_displayDebug)
            {
                Debug.Log("Pausing flythrough");
            }
            m_currentState = PegasusConstants.FlythroughState.Paused;
        }

        /// <summary>
        /// Stop a flythrough - cant be resumed - can be restarted
        /// </summary>
        public void StopFlythrough()
        {
            if (m_displayDebug)
            {
                Debug.Log("Stopping flythrough");
            }
            m_currentState = PegasusConstants.FlythroughState.Stopped;
            m_canUpdateNow = false;
        }

        /// <summary>
        /// Change the frame rate and apply it immediately
        /// </summary>
        /// <param name="newRate">New framerate</param>
        public void ChangeFramerate(PegasusConstants.TargetFrameRate newRate)
        {
            m_targetFramerateType = newRate;

            //Apply the target framerate only if we are playing
            if (Application.isPlaying)
            {
                switch (m_targetFramerateType)
                {
                    case PegasusConstants.TargetFrameRate.NineFps:
                        Application.targetFrameRate = 9;
#if HELIOS3D
                            Time.captureFramerate = 9;
#endif
                        m_frameUpdateTime = 1f / 9f;
                        break;
                    case PegasusConstants.TargetFrameRate.FifteenFps:
                        Application.targetFrameRate = 15;
#if HELIOS3D
                            Time.captureFramerate = 15;
#endif
                        m_frameUpdateTime = 1f / 15f;
                        break;
                    case PegasusConstants.TargetFrameRate.TwentyFourFps:
                        Application.targetFrameRate = 24;
#if HELIOS3D
                            Time.captureFramerate = 24;
#endif
                        m_frameUpdateTime = 1f / 24f;
                        break;
                    case PegasusConstants.TargetFrameRate.TwentyFiveFps:
                        Application.targetFrameRate = 25;
#if HELIOS3D
                            Time.captureFramerate = 25;
#endif
                        m_frameUpdateTime = 1f / 25f;
                        break;
                    case PegasusConstants.TargetFrameRate.ThirtyFps:
                        Application.targetFrameRate = 30;
#if HELIOS3D
                            Time.captureFramerate = 30;
#endif
                        m_frameUpdateTime = 1f / 30f;
                        break;
                    case PegasusConstants.TargetFrameRate.SixtyFps:
                        Application.targetFrameRate = 60;
#if HELIOS3D
                            Time.captureFramerate = 60;
#endif
                        m_frameUpdateTime = 1f / 60f;
                        break;
                    case PegasusConstants.TargetFrameRate.NinetyFps:
                        Application.targetFrameRate = 90;
#if HELIOS3D
                            Time.captureFramerate = 90;
#endif
                        m_frameUpdateTime = 1f / 90f;
                        break;
                    case PegasusConstants.TargetFrameRate.MaxFps:
                        Application.targetFrameRate = -1;
#if HELIOS3D
                            Time.captureFramerate = 60;
#endif
                        m_frameUpdateTime = 0f;
                        break;
                }
            }
        }

        #endregion

        #region Editor specific routines - use at own peril

        /// <summary>
        /// Select the POI - influences gizmo rendering
        /// </summary>
        /// <param name="poi"></param>
        public void SelectPoi(PegasusPoi poi)
        {
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                m_poiList[idx].m_isSelected = false;
            }
            if (poi != null)
            {
                poi.m_isSelected = true;
            }
        }


        /// <summary>
        /// Do a validated poi movement
        /// </summary>
        /// <param name="poi">Point of interest to move</param>
        /// <param name="movement">Amount of movement</param>
        public void MovePoi(PegasusPoi poi, Vector3 movement)
        {
            poi.transform.position = GetValidatedPoiPosition(poi.transform.position + movement, poi.m_heightCheckType);
            poi.GetRelativeOffsets(poi.transform.position, poi.m_lookatLocation, out poi.m_lookAtDistance, out poi.m_lookAtHeight, out poi.m_lookAtAngle);
            UpdateSegmentWithDependencies(poi);
        }

        /// <summary>
        /// Do a validated poi lookat movement - including setting it into target mode
        /// </summary>
        /// <param name="movement"></param>
        public void MovePoiLookat(PegasusPoi poi, Vector3 movement)
        {
            poi.m_lookatType = PegasusConstants.LookatType.Target;
            Vector3 lookAtLocation = GetValidatedLookatPosition(poi.m_lookatLocation + movement, poi.m_heightCheckType);
            if (lookAtLocation != poi.m_lookatLocation)
            {
                poi.m_lookatLocation = lookAtLocation;
                poi.m_lookatType = PegasusConstants.LookatType.Target;
                poi.GetRelativeOffsets(poi.transform.position, poi.m_lookatLocation, out poi.m_lookAtDistance, out poi.m_lookAtHeight, out poi.m_lookAtAngle);
                UpdateSegmentWithDependencies(poi);
            }
        }

        /// <summary>
        /// Move target to current location
        /// </summary>
        public void MoveTargetNow()
        {
            MoveTargetTo(m_totalDistanceTravelled / m_totalDistance);
        }

        /// <summary>
        /// Move the target if one has been set to the point in the sequence selected - designed to be an editor only method
        /// </summary>
        /// <param name="percent">Value betrween 0..1 which represents completion</param>
        public void MoveTargetTo(float percent)
        {
            if (m_target == null)
            {
                Debug.LogWarning("Can not move target as none has been set");
                return;
            }

            PegasusPoi poi;
            float targetPoint = percent * m_totalDistance;
            float segmentStart = 0f;
            float segmentEnd = 0f;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                poi = m_poiList[idx];
                segmentEnd = segmentStart + poi.m_segmentDistance;
                if (targetPoint >= segmentStart && targetPoint <= segmentEnd)
                {
                    m_totalDistanceTravelled = targetPoint;
                    m_totalDistanceTravelledPct = m_totalDistanceTravelled / m_totalDistance;
                    m_currentPosition = poi.CalculatePositionLinear((targetPoint - segmentStart) / poi.m_segmentDistance);
                    m_currentRotation = poi.CalculateRotation((targetPoint - segmentStart) / poi.m_segmentDistance);
                    m_currentVelocity = poi.CalculateVelocity((targetPoint - segmentStart) / poi.m_segmentDistance);
                    m_target.transform.position = m_currentPosition;
                    m_target.transform.rotation = m_currentRotation;
                    return;
                }
                segmentStart += poi.m_segmentDistance;
            }
        }

        /// <summary>
        /// Move target to the given poi
        /// </summary>
        /// <param name="targetPoi">Poi to move target to</param>
        public void MoveTargetToPoi(PegasusPoi targetPoi)
        {
            if (m_target == null)
            {
                Debug.LogWarning("Can not move target as none has been set");
                return;
            }
            m_totalDistanceTravelled = 0f;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                if (m_poiList[idx].GetInstanceID() == targetPoi.GetInstanceID())
                {
                    m_totalDistanceTravelledPct = m_totalDistanceTravelled / m_totalDistance;
                    m_currentPosition = targetPoi.CalculatePositionLinear(0f);
                    m_currentRotation = targetPoi.CalculateRotation(0f);
                    m_currentVelocity = targetPoi.CalculateVelocity(0f);
                    m_target.transform.position = m_currentPosition;
                    m_target.transform.rotation = m_currentRotation;
                }
                else
                {
                    m_totalDistanceTravelled += m_poiList[idx].m_segmentDistance;
                }
            }
        }

        /// <summary>
        /// Step the target 1m backward - only called by editor / /scrubber functions
        /// </summary>
        public void StepTargetBackward(float distMeters)
        {
            m_totalDistanceTravelled -= distMeters;
            if (m_totalDistanceTravelled < 0)
            {
                m_totalDistanceTravelled = 0f;
            }
            MoveTargetTo(m_totalDistanceTravelled / m_totalDistance);
        }

        /// <summary>
        /// Step the target 1m forward - only called by editor / /scrubber functions
        /// </summary>
        public void StepTargetForward(float distMeters)
        {
            m_totalDistanceTravelled += distMeters;
            if (m_totalDistanceTravelled > m_totalDistance)
            {
                m_totalDistanceTravelled = m_totalDistance;
            }
            MoveTargetTo(m_totalDistanceTravelled / m_totalDistance);
        }

        /// <summary>
        /// Create debug objects for visualisation
        /// </summary>
        public void CreateDebugObjects()
        {
            PegasusPoi poi;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                poi = m_poiList[idx];
                if (poi.transform.childCount == 0)
                {
                    GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    DestroyImmediate(newCube.GetComponent<BoxCollider>());
                    newCube.transform.position = poi.transform.position;
                    newCube.transform.localScale = new Vector3(0.05f, 10f, 0.05f);
                    newCube.transform.parent = poi.transform;
                    newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    DestroyImmediate(newCube.GetComponent<BoxCollider>());
                    newCube.transform.position = poi.transform.position;
                    newCube.transform.localScale = new Vector3(0.05f, 0.05f, 5f);
                    newCube.transform.parent = poi.transform;
                    newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    DestroyImmediate(newCube.GetComponent<BoxCollider>());
                    newCube.transform.position = poi.transform.position;
                    newCube.transform.localScale = new Vector3(5f, 0.05f, 0.05f);
                    newCube.transform.parent = poi.transform;
                }
            }
        }

        /// <summary>
        /// Delete visualisation debug objects
        /// </summary>
        public void DeleteDebugObjects()
        {
            PegasusPoi poi;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                poi = m_poiList[idx];
                while (poi.transform.childCount > 0)
                {
                    DestroyImmediate(poi.transform.GetChild(0).gameObject);
                }
            }
        }

        /// <summary>
        /// Return a validated / corrected poi position
        /// </summary>
        /// <param name="source">Source location being checked</param>
        /// <param name="heightCheckOverride">A height check overide</param>
        /// <returns>Updated position taking into account minimum height</returns>
        public Vector3 GetValidatedPoiPosition(Vector3 source, PegasusConstants.PoiHeightCheckType heightCheckOverride = PegasusConstants.PoiHeightCheckType.ManagerSettings)
        {
            //Update if there is an override
            PegasusConstants.HeightCheckType heightCheckType = m_heightCheckType;
            if (heightCheckOverride != PegasusConstants.PoiHeightCheckType.ManagerSettings)
            {
                if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Collision)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Collision;
                }
                else if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Terrain)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Terrain;
                }
                else
                {
                    heightCheckType = PegasusConstants.HeightCheckType.None;
                }
            }

            //Peform the height check
            if (heightCheckType == PegasusConstants.HeightCheckType.None)
            {
                return source;
            }
            else if (heightCheckType == PegasusConstants.HeightCheckType.Collision)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(source.x, source.y + m_collisionHeightOffset, source.z), Vector3.down, out hit, 10000f))
                {
                    if ((hit.point.y + m_minHeightAboveTerrain) > source.y)
                    {
                        source.y = hit.point.y + m_minHeightAboveTerrain;
                    }
                }
                return source;
            }
            else
            {
                Terrain t = GetTerrain(source);
                if (t != null)
                {
                    float height = t.SampleHeight(source);
                    if ((height + m_minHeightAboveTerrain) > source.y)
                    {
                        source.y = height + m_minHeightAboveTerrain;
                    }
                }
                return source;
            }
        }

        /// <summary>
        /// Return a validated / poi position at the minimum height
        /// </summary>
        /// <param name="source">Source location being checked</param>
        /// <param name="heightCheckOverride">A height check overide</param>
        /// <returns>Updated position taking into account minimum height</returns>
        public Vector3 GetLowestPoiPosition(Vector3 source, PegasusConstants.PoiHeightCheckType heightCheckOverride = PegasusConstants.PoiHeightCheckType.ManagerSettings)
        {
            //Update if there is an override
            PegasusConstants.HeightCheckType heightCheckType = m_heightCheckType;
            if (heightCheckOverride != PegasusConstants.PoiHeightCheckType.ManagerSettings)
            {
                if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Collision)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Collision;
                }
                else if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Terrain)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Terrain;
                }
                else
                {
                    heightCheckType = PegasusConstants.HeightCheckType.None;
                }
            }

            //Peform the height check
            if (heightCheckType == PegasusConstants.HeightCheckType.None)
            {
                return source;
            }
            else if (heightCheckType == PegasusConstants.HeightCheckType.Collision)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(source.x, source.y + m_collisionHeightOffset, source.z), Vector3.down, out hit, 10000f))
                {
                    source.y = hit.point.y + m_minHeightAboveTerrain;
                }
                return source;
            }
            else
            {
                Terrain t = GetTerrain(source);
                if (t != null)
                {
                    source.y = t.SampleHeight(source) + m_minHeightAboveTerrain;
                }
                return source;
            }
        }

        /// <summary>
        /// Return a validated / corrected lookat position
        /// </summary>
        /// <param name="source">Source location</param>
        /// <returns>Updated position taking into account minimum height</returns>
        public Vector3 GetValidatedLookatPosition(Vector3 source, PegasusConstants.PoiHeightCheckType heightCheckOverride = PegasusConstants.PoiHeightCheckType.ManagerSettings)
        {
            //Update if there is an override
            PegasusConstants.HeightCheckType heightCheckType = m_heightCheckType;
            if (heightCheckOverride != PegasusConstants.PoiHeightCheckType.ManagerSettings)
            {
                if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Collision)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Collision;
                }
                else if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Terrain)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Terrain;
                }
                else
                {
                    heightCheckType = PegasusConstants.HeightCheckType.None;
                }
            }

            //Perform the height check
            if (heightCheckType == PegasusConstants.HeightCheckType.None)
            {
                return source;
            }
            else if (heightCheckType == PegasusConstants.HeightCheckType.Collision)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(source.x, source.y + m_collisionHeightOffset, source.z), Vector3.down, out hit, 2000f))
                {
                    if (hit.point.y > source.y)
                    {
                        source.y = hit.point.y;
                    }
                }
                return source;
            }
            else
            {
                Terrain t = GetTerrain(source);
                if (t != null)
                {
                    float height = t.SampleHeight(source);
                    if (height > source.y)
                    {
                        source.y = height;
                    }
                }
                return source;
            }
        }

        /// <summary>
        /// Return a validated lookat position at the minimum height
        /// </summary>
        /// <param name="source">Source location being checked</param>
        /// <param name="heightCheckOverride">A height check overide</param>
        /// <returns>Updated position taking into account minimum height</returns>
        public Vector3 GetLowestLookatPosition(Vector3 source, PegasusConstants.PoiHeightCheckType heightCheckOverride = PegasusConstants.PoiHeightCheckType.ManagerSettings)
        {
            //Update if there is an override
            PegasusConstants.HeightCheckType heightCheckType = m_heightCheckType;
            if (heightCheckOverride != PegasusConstants.PoiHeightCheckType.ManagerSettings)
            {
                if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Collision)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Collision;
                }
                else if (heightCheckOverride == PegasusConstants.PoiHeightCheckType.Terrain)
                {
                    heightCheckType = PegasusConstants.HeightCheckType.Terrain;
                }
                else
                {
                    heightCheckType = PegasusConstants.HeightCheckType.None;
                }
            }

            //Peform the height check
            if (heightCheckType == PegasusConstants.HeightCheckType.None)
            {
                return source;
            }
            else if (heightCheckType == PegasusConstants.HeightCheckType.Collision)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(source.x, source.y + m_collisionHeightOffset, source.z), Vector3.down, out hit, 2000f))
                {
                    source.y = hit.point.y;
                }
                return source;
            }
            else
            {
                Terrain t = GetTerrain(source);
                if (t != null)
                {
                    source.y = t.SampleHeight(source);
                }
                return source;
            }
        }


        /// <summary>
        /// Return a validated / corrected lookat position
        /// </summary>
        /// <param name="source">Source location</param>
        /// <returns>Updated position taking into account minimum height</returns>
        public float GetValidatedLookatHeightRelativeToMinimum(Vector3 source, PegasusConstants.PoiHeightCheckType heightCheckOverride = PegasusConstants.PoiHeightCheckType.ManagerSettings)
        {
            Vector3 minPosition = GetLowestLookatPosition(source, heightCheckOverride);
            return source.y - minPosition.y;
        }

        /// <summary>
        /// Get the terrain in this location, otherwise return null
        /// </summary>
        /// <param name="location">Location to check in world units</param>
        /// <returns>Terrain here or null</returns>
        public Terrain GetTerrain(Vector3 location)
        {
            Terrain terrain;
            Vector3 terrainMin = new Vector3();
            Vector3 terrainMax = new Vector3();

            //First check active terrain - most likely already selected
            terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;
                if (location.x >= terrainMin.x && location.x <= terrainMax.x)
                {
                    if (location.z >= terrainMin.z && location.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }
            }

            //Then check rest of terrains
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                terrainMin = terrain.GetPosition();
                terrainMax = terrainMin + terrain.terrainData.size;
                if (location.x >= terrainMin.x && location.x <= terrainMax.x)
                {
                    if (location.z >= terrainMin.z && location.z <= terrainMax.z)
                    {
                        return terrain;
                    }
                }
            }
            return null;
        }


        #endregion

        #region Update processing

        /// <summary>
        /// Both drive (if not coroutine based) and apply the flythrough into the scene
        /// </summary>
        void LateUpdate()
        {
            //Keep updating current time if paused to stop big resume time delta issue **** THis may no longer be valit
            if (m_currentState == PegasusConstants.FlythroughState.Paused)
            {
                m_lastUpdateTime = DateTime.Now;
            }

            //Exit if we are not in running mode
            if (m_currentState != PegasusConstants.FlythroughState.Started)
            {
                return;
            }

            //Calculate the update
            CalculateFlythroughUpdates();

            //Apply the update into the scene
            if (m_canUpdateNow && m_target != null)
            {

                //Apply the rotation smoothly
                if (m_rotationDamping > 0f)
                {
                    m_target.transform.rotation = Quaternion.Slerp(m_target.transform.rotation, m_currentRotation, Time.deltaTime * (1f / m_rotationDamping));
                }
                else
                {
                    m_target.transform.rotation = m_currentRotation;
                }

                //Apply the position update smoothly
                if (m_positionDamping > 0f)
                {
                    m_target.transform.position = Vector3.Slerp(m_target.transform.position, m_currentPosition, Time.deltaTime * (1f / m_positionDamping));
                }
                else
                {
                    m_target.transform.position = m_currentPosition;
                }

                m_canUpdateNow = false;
            }
        }

        /// <summary>
        /// Perform the calculations required for the next update - will trigger an update in the LateUpdate method to apply it to the scene
        /// </summary>
        private void CalculateFlythroughUpdates()
        {
            //Make sure we are on a segment
            if (m_currentSegment != null)
            {
                //Calculate progress and update velocity, position and rotation variables (will be physically applied in late update)
                m_currentSegment.CalculateProgress(m_currentSegmentDistanceTravelled / m_currentSegment.m_segmentDistance, out m_currentVelocity, out m_currentPosition, out m_currentRotation);

                //Update the rotation to allow for the rotation offset
                //m_currentRotation = Quaternion.FromToRotation(Vector3.up, m_targetRotationCorrection) * m_currentRotation;

                //Update distance travelled for next iteration through
                if (m_targetFramerateType == PegasusConstants.TargetFrameRate.MaxFps)
                {
                    m_frameUpdateTime = (float)((DateTime.Now - m_lastUpdateTime).Milliseconds) / 1000f;
                    m_lastUpdateTime = DateTime.Now;
                }
                m_frameUpdateDistance = m_frameUpdateTime * m_currentVelocity;
                m_currentSegmentDistanceTravelled += m_frameUpdateDistance;
                m_totalDistanceTravelled += m_frameUpdateDistance;
                m_totalDistanceTravelledPct = m_totalDistanceTravelled / m_totalDistance;

                //Handle segment changes
                if (m_currentSegmentDistanceTravelled >= m_currentSegment.m_segmentDistance)
                {
                    //Call any end state triggers
                    m_currentSegment.OnEndTriggers();

                    //Increment to next segment
                    m_currentSegmentIdx++;
                    if (m_currentSegmentIdx >= m_poiList.Count)
                    {
                        //Detect if we are at end game
                        if (m_flythroughType == PegasusConstants.FlythroughType.Looped)
                        {
                            m_currentSegmentIdx = 0;
                            m_currentSegmentDistanceTravelled -= m_currentSegment.m_segmentDistance;
                            m_totalDistanceTravelled = m_currentSegmentDistanceTravelled;
                        }
                        else
                        {
                            m_currentSegmentIdx--;
                            m_currentSegmentDistanceTravelled = m_currentSegment.m_segmentDistance;
                            m_totalDistanceTravelled = m_totalDistance;
                            m_totalDistanceTravelledPct = 1f;

                            if (m_flythroughEndAction == PegasusConstants.FlythroughEndAction.StopFlythrough)
                            {
                                StopFlythrough();
                                return;
                            }
                            else if (m_flythroughEndAction == PegasusConstants.FlythroughEndAction.QuitApplication)
                            {
                                StopFlythrough();
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#else
                                Application.Quit();
#endif
                                return;
                            }
                            else
                            {
                                StopFlythrough();
                                if (m_nextPegasus != null)
                                {
                                    m_nextPegasus.StartFlythrough();
                                }
                                else
                                {
                                    Debug.Log("Next Pegasus has not been configured. Can not start.");
                                }
                                return;
                            }
                        }
                    }
                    else
                    {
                        m_currentSegmentDistanceTravelled -= m_currentSegment.m_segmentDistance;
                    }
                    m_totalDistanceTravelledPct = m_totalDistanceTravelled / m_totalDistance;
                    m_currentSegment = m_poiList[m_currentSegmentIdx];
                    m_currentSegment.OnStartTriggers();
                }

                //Override lookat for path travellers
//                if (m_currentSegment.m_lookatType == PegasusConstants.LookatType.Path && m_currentSegment.m_nextPoi.m_lookatType == PegasusConstants.LookatType.Path)
//                {
//                    //This is inherently jittery - needs more investigation
//                    Vector3 forward = m_currentPosition - m_target.transform.position;
//
//                    //And then apply if non zero
//                    if (forward != Vector3.zero)
//                    {
//                        m_currentRotation = Quaternion.LookRotation(forward);
//                    }
//                }

                //Call the update for the current segment
                m_currentSegment.OnUpdateTriggers(m_currentSegmentDistanceTravelled / m_currentSegment.m_segmentDistance);

                //Flag that we can do an update now
                m_canUpdateNow = true;
            }
        }

        #endregion

        #region POI routines

        /// <summary>
        /// Add a new POI at the targetLocation given
        /// </summary>
        /// <param name="targetLocation">Location to add the POI at</param>
        public void AddPOI(Vector3 targetLocation, Vector3 lookatLocation)
        {
            GameObject newPoiGo = new GameObject("POI " + m_poiList.Count);
            newPoiGo.transform.parent = this.transform;
            newPoiGo.transform.position = targetLocation;
            PegasusPoi poi = newPoiGo.AddComponent<PegasusPoi>();
            poi.m_manager = this;
            poi.m_lookatLocation = lookatLocation;
            m_poiList.Add(poi);
            InitialiseFlythrough();
        }

        /// <summary>
        /// Add a new POI halfway between this POI and the next POI, taking traversal into account
        /// </summary>
        /// <param name="currentPoi"></param>
        /// <returns></returns>
        public PegasusPoi AddPoiAfter(PegasusPoi currentPoi)
        {
            GameObject newPoiGo = new GameObject("POI " + m_poiList.Count);
            PegasusPoi newPoi = newPoiGo.AddComponent<PegasusPoi>();
            newPoi.m_manager = this;
            m_poiList.Insert(m_poiList.IndexOf(currentPoi) + 1, newPoi);
            Vector3 newPoiLocation = GetValidatedPoiPosition(currentPoi.CalculatePositionLinear(0.5f), newPoi.m_heightCheckType);
            newPoiGo.transform.position = newPoiLocation;
            newPoiGo.transform.parent = this.transform;
            newPoiGo.transform.SetSiblingIndex(currentPoi.m_segmentIndex + 1);
            InitialiseFlythrough();
            return newPoi;
        }

        /// <summary>
        /// Add a new POI halfway between this POI and the previous POI, taking traversal into account
        /// </summary>
        /// <param name="currentPoi"></param>
        /// <returns></returns>
        public PegasusPoi AddPoiBefore(PegasusPoi currentPoi)
        {
            return AddPoiAfter(GetPrevPOI(currentPoi));
        }

        /// <summary>
        /// Get the first poi
        /// </summary>
        /// <returns>First POI or null if no data</returns>
        public PegasusPoi GetFirstPOI()
        {
            if (m_poiList.Count < 1)
            {
                return null;
            }
            return m_poiList[0];
        }

        /// <summary>
        /// Get the poi from POI index 
        /// </summary>
        /// <param name="poiIndex">POI to get</param>
        /// <returns>POI or null if no data or invalid index</returns>
        public PegasusPoi GetPOI(int poiIndex)
        {
            if (m_poiList.Count == 0 || poiIndex < 0 || poiIndex >= m_poiList.Count)
            {
                return null;
            }
            return m_poiList[poiIndex];
        }

        /// <summary>
        /// Get the prev poi from POI passed in
        /// </summary>
        /// <param name="currentPoi">POI to start from</param>
        /// <param name="wrap">Whether or not to wrap around</param>
        /// <returns>Prev POI or null if no data, invalid params or wrap boundary hit</returns>
        public PegasusPoi GetPrevPOI(PegasusPoi currentPoi, bool wrap = true)
        {
            if (currentPoi != null)
            {
                if (currentPoi.m_segmentIndex > 0)
                {
                    return m_poiList[currentPoi.m_segmentIndex - 1];
                }
                else
                {
                    if (wrap)
                    {
                        return m_poiList[m_poiList.Count - 1];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the next poi on from poi passed in
        /// </summary>
        /// <param name="currentPoi">POI to start from</param>
        /// <param name="wrap">Whether or not to wrap around</param>
        /// <returns>Next POI or null if no data, invalid params or wrap boundary hit</returns>
        /// <returns></returns>
        public PegasusPoi GetNextPOI(PegasusPoi currentPoi, bool wrap = true)
        {
            if (currentPoi != null)
            {
                if (currentPoi.m_segmentIndex < m_poiList.Count - 1)
                {
                    return m_poiList[currentPoi.m_segmentIndex + 1];
                }
                else
                {
                    if (wrap)
                    {
                        return m_poiList[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Go and update all POI heights to min height of the manager
        /// </summary>
        public void SetPoiToMinHeight()
        {
            PegasusPoi poi;
            for (int idx = 0; idx < m_poiList.Count; idx++)
            {
                poi = m_poiList[idx];
                poi.transform.position = poi.m_manager.GetLowestPoiPosition(poi.transform.position, poi.m_heightCheckType);
            }
            InitialiseFlythrough();
        }

        #endregion
    }
}