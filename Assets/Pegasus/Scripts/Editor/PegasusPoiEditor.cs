using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pegasus
{
    [CustomEditor(typeof(PegasusPoi))]
    public class PegasusPoiEditor : Editor
    {
        GUIStyle m_boxStyle;
        GUIStyle m_wrapStyle;
        PegasusPoi m_poi;
        private int m_editor_control_id = 0;
        Vector3 m_lastHitPoint = Vector3.zero;

        /// <summary>
        /// This is called when we select the poi in the editor
        /// </summary>
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            //Get the control id
            m_editor_control_id = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);

            //Get our poi
            m_poi = (PegasusPoi)target;

            //And select us
            if (m_poi != null)
            {
                m_poi.m_manager.SelectPoi(m_poi);
                m_poi.m_manager.MoveTargetToPoi(m_poi);
            }
        }

        /// <summary>
        /// Draw the POI gui
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get our poi
            m_poi = (PegasusPoi)target;

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Setup the wrap style
            if (m_wrapStyle == null)
            {
                m_wrapStyle = new GUIStyle(GUI.skin.label);
                m_wrapStyle.wordWrap = true;
            }

            //Create a nice text intro
            GUILayout.BeginVertical("Point of Interest", m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("The yellow sphere will be flown through, and the blue sphere is what the camera is looking at. If the blue sphere is not shown then the camera will look along the path of the flythrough.\n\nClick on POI in scene then...\nCTRL + Keys - Move selected POI.\nSHIFT+CTRL + Keys - Move selected LookAt.\nSee PegasusDefaults for Keys.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            //PegasusConstants.PoiType poiType = (PegasusConstants.PoiType)EditorGUILayout.EnumPopup(GetLabel("POI Type"), m_poi.m_poiType);
            PegasusConstants.PoiType poiType = m_poi.m_poiType;

            GUILayout.Space(5);

            GUILayout.BeginVertical("Target Lookat", m_boxStyle);
                GUILayout.Space(20);
                PegasusConstants.LookatType lookatType = (PegasusConstants.LookatType)EditorGUILayout.EnumPopup(GetLabel("Target"), m_poi.m_lookatType);
                float lookAtAngle = m_poi.m_lookAtAngle;
                float lookAtDistance = m_poi.m_lookAtDistance;
                float lookAtHeight = m_poi.m_lookAtHeight;
                if (lookatType == PegasusConstants.LookatType.Path)
                {
                    GUI.enabled = false;
                }
                lookAtAngle = EditorGUILayout.Slider(GetLabel("  Angle"), m_poi.m_lookAtAngle, 0f, 359.9f);
                lookAtDistance = EditorGUILayout.FloatField(GetLabel("  Distance"), m_poi.m_lookAtDistance);
                lookAtHeight = EditorGUILayout.FloatField(GetLabel("  Height"), m_poi.m_lookAtHeight);
                GUI.enabled = true;
                GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical("Target Speed", m_boxStyle);
                GUILayout.Space(20);
                PegasusConstants.SpeedType speedType = (PegasusConstants.SpeedType)EditorGUILayout.EnumPopup(GetLabel("Type"), m_poi.m_startSpeedType);
                float startSpeed = EditorGUILayout.Slider(GetLabel("Speed"), m_poi.m_startSpeed, PegasusConstants.SpeedReallySlow, PegasusConstants.SpeedReallyFast);
                GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical("Height Constraint", m_boxStyle);
                GUILayout.Space(20);
                PegasusConstants.PoiHeightCheckType heightCheckType = (PegasusConstants.PoiHeightCheckType)EditorGUILayout.EnumPopup(GetLabel("Min Height From"), m_poi.m_heightCheckType);
                GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical("Statistics", m_boxStyle);
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Distance", string.Format("{0:0.00m}", m_poi.m_segmentDistance));
                EditorGUILayout.LabelField("Start Time", string.Format("{0}:{1:00}.{2:000}", m_poi.m_segmentStartTime.Minutes, m_poi.m_segmentStartTime.Seconds, m_poi.m_segmentStartTime.Milliseconds));
                EditorGUILayout.LabelField("Segment Time", string.Format("{0}:{1:00}.{2:000}", m_poi.m_segmentDuration.Minutes, m_poi.m_segmentDuration.Seconds, m_poi.m_segmentDuration.Milliseconds));
                EditorGUILayout.LabelField("Total Time", string.Format("{0}:{1:00}.{2:000}", m_poi.m_manager.m_totalDuration.Minutes, m_poi.m_manager.m_totalDuration.Seconds, m_poi.m_manager.m_totalDuration.Milliseconds));
            GUILayout.Space(3);
            GUILayout.EndVertical();

            /*
            PegasusConstants.EasingType velocityEasing = (PegasusConstants.EasingType)EditorGUILayout.EnumPopup(GetLabel("Velocity Easing"), m_poi.m_velocityEasingType);
            PegasusConstants.EasingType rotationEasing = (PegasusConstants.EasingType)EditorGUILayout.EnumPopup(GetLabel("Rotation Easing"), m_poi.m_rotationEasingType);
            PegasusConstants.EasingType positionEasing = (PegasusConstants.EasingType)EditorGUILayout.EnumPopup(GetLabel("Position Easing"), m_poi.m_positionEasingType);
            */
            PegasusConstants.EasingType velocityEasing = m_poi.m_velocityEasingType;
            PegasusConstants.EasingType rotationEasing = m_poi.m_rotationEasingType;
            PegasusConstants.EasingType positionEasing = m_poi.m_positionEasingType;

            //EditorGUILayout.LabelField(GetLabel("Distance"), GetLabel(string.Format("{0:0.00}m", m_poi.m_segmentDistance)));

            GUILayout.Space(5);

            GUILayout.BeginVertical("Utilities", m_boxStyle);
            GUILayout.Space(20);

                if (GUILayout.Button(GetLabel("Select Manager")))
                {
                    if (Selection.activeTransform != null)
                    {
                        Selection.activeTransform = m_poi.m_manager.transform;
                    }
                }
                if (GUILayout.Button(GetLabel("Select First POI")))
                {
                    PegasusPoi poi = m_poi.m_manager.GetFirstPOI();
                    if (poi != null)
                    {
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        poi.m_manager.SelectPoi(poi);
                        poi.m_manager.MoveTargetToPoi(poi);
                    }
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(GetLabel("Select Previous POI")))
                {
                    PegasusPoi poi = m_poi.m_manager.GetPrevPOI(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        poi.m_manager.SelectPoi(poi);
                        poi.m_manager.MoveTargetToPoi(poi);
                    }
                }
                if (GUILayout.Button(GetLabel("Select Next POI")))
                {
                    PegasusPoi poi = m_poi.m_manager.GetNextPOI(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        poi.m_manager.SelectPoi(poi);
                        poi.m_manager.MoveTargetToPoi(poi);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(GetLabel("Add POI Before")))
                {
                    PegasusPoi poi = m_poi.m_manager.AddPoiBefore(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        poi.m_manager.SelectPoi(poi);
                        poi.m_manager.MoveTargetToPoi(poi);
                    }
                }
                if (GUILayout.Button(GetLabel("Add POI After")))
                {
                    PegasusPoi poi = m_poi.m_manager.AddPoiAfter(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        poi.m_manager.SelectPoi(poi);
                        poi.m_manager.MoveTargetToPoi(poi);
                    }
                }
                GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.EndVertical();

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_poi, "Made changes");
                m_poi.m_poiType = poiType;

                if (m_poi.m_lookatType != lookatType && lookatType == PegasusConstants.LookatType.Target)
                {
                    lookAtHeight = 0f;
                    lookAtDistance = 5f;
                }
                m_poi.m_lookatType = lookatType;

                if (heightCheckType != m_poi.m_heightCheckType)
                {
                    m_poi.m_heightCheckType = heightCheckType;
                    m_poi.m_manager.UpdateSegmentWithDependencies(m_poi);
                }

                if (!PegasusPoi.ApproximatelyEqual(m_poi.m_lookAtAngle, lookAtAngle))
                {
                    m_poi.m_lookAtAngle = lookAtAngle;
                    m_poi.m_lookatLocation = m_poi.transform.position + (m_poi.transform.localRotation * Quaternion.AngleAxis(m_poi.m_lookAtAngle, Vector3.up)) * new Vector3(0f, 0f, -m_poi.m_lookAtDistance);
                    m_poi.m_lookatLocation = m_poi.m_manager.GetLowestLookatPosition(m_poi.m_lookatLocation, m_poi.m_heightCheckType);
                    m_poi.m_lookatLocation.y += m_poi.m_lookAtHeight;
                }
                else if (!PegasusPoi.ApproximatelyEqual(m_poi.m_lookAtDistance, lookAtDistance))
                {
                    m_poi.m_lookAtDistance = lookAtDistance;
                    m_poi.m_lookatLocation = m_poi.transform.position + (m_poi.transform.localRotation * Quaternion.AngleAxis(m_poi.m_lookAtAngle, Vector3.up)) * new Vector3(0f, 0f, -m_poi.m_lookAtDistance);
                    m_poi.m_lookatLocation = m_poi.m_manager.GetLowestLookatPosition(m_poi.m_lookatLocation, m_poi.m_heightCheckType);
                    m_poi.m_lookatLocation.y += m_poi.m_lookAtHeight;
                }
                else if (!PegasusPoi.ApproximatelyEqual(m_poi.m_lookAtHeight, lookAtHeight))
                {
                    m_poi.m_lookAtHeight = lookAtHeight;
                    m_poi.m_lookatLocation = m_poi.transform.position + (m_poi.transform.localRotation * Quaternion.AngleAxis(m_poi.m_lookAtAngle, Vector3.up)) * new Vector3(0f, 0f, -m_poi.m_lookAtDistance);
                    m_poi.m_lookatLocation = m_poi.m_manager.GetLowestLookatPosition(m_poi.m_lookatLocation, m_poi.m_heightCheckType);
                    m_poi.m_lookatLocation.y += m_poi.m_lookAtHeight;
                }

                if (m_poi.m_startSpeedType != speedType)
                {
                    m_poi.m_startSpeedType = speedType;
                    m_poi.m_startSpeed = m_poi.GetStartSpeed(speedType);
                    m_poi.UpdateSegmentDuration();
                    m_poi.m_manager.UpdateFlythroughMetaData();
                }
                else
                {
                    if (!PegasusPoi.ApproximatelyEqual(m_poi.m_startSpeed,startSpeed))
                    {
                        m_poi.m_startSpeedType = PegasusConstants.SpeedType.Custom;
                        m_poi.m_startSpeed = startSpeed;
                        m_poi.UpdateSegmentDuration();
                        m_poi.m_manager.UpdateFlythroughMetaData();
                    }
                }

                m_poi.m_velocityEasingType = velocityEasing;
                m_poi.m_rotationEasingType = rotationEasing;
                m_poi.m_positionEasingType= positionEasing;

                //Mark it as dirty
                EditorUtility.SetDirty(m_poi);
            }
        }

        void OnSceneGUI()
        {
            //Exit if we dont have an event
            if (Event.current == null)
            {
                return;
            }

            //Exit if we dont have poi or manager
            if (m_poi == null || m_poi.m_manager == null)
            {
                return;
            }

            //Check for and handle attempts to push transform under ground - this is generally accidental
            if (m_poi.transform.hasChanged)
            {
                Undo.RecordObject(m_poi, "Made changes");
                m_poi.transform.position = m_poi.m_manager.GetValidatedPoiPosition(m_poi.transform.position, m_poi.m_heightCheckType);
                m_poi.GetRelativeOffsets(m_poi.transform.position, m_poi.m_lookatLocation, out m_poi.m_lookAtDistance, out m_poi.m_lookAtHeight, out m_poi.m_lookAtAngle);
                m_poi.m_manager.UpdateSegmentWithDependencies(m_poi);
                m_poi.m_manager.MoveTargetToPoi(m_poi);
                m_poi.transform.hasChanged = false;
                EditorUtility.SetDirty(m_poi);
                return;
            }

            //Now handle manual lookat location changes
            if (m_poi.m_lookatType == PegasusConstants.LookatType.Target)
            {
                Vector3 lookAtLocation = m_poi.m_manager.GetValidatedLookatPosition(Handles.DoPositionHandle(m_poi.m_lookatLocation, Quaternion.identity), m_poi.m_heightCheckType);
                if (lookAtLocation != m_poi.m_lookatLocation)
                {
                    Undo.RecordObject(m_poi, "Made changes");
                    m_poi.m_lookatLocation = lookAtLocation;
                    m_poi.m_lookatType = PegasusConstants.LookatType.Target;
                    m_poi.GetRelativeOffsets(m_poi.transform.position, m_poi.m_lookatLocation, out m_poi.m_lookAtDistance, out m_poi.m_lookAtHeight, out m_poi.m_lookAtAngle);
                    m_poi.m_manager.UpdateSegmentWithDependencies(m_poi);
                    m_poi.m_manager.MoveTargetToPoi(m_poi);
                    EditorUtility.SetDirty(m_poi);
                    return;
                }
            }

            //Ignore layout and repaint events
            //if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            //{
            //    return;
            //}

            //Make sure we have defaults for key presses
            if (m_poi.m_manager.m_defaults == null)
            {
                m_poi.m_manager.m_defaults = PegasusManagerEditor.GetDefaults();
            }

            //Keyboard events
            if (Event.current.control == true && Event.current.type == EventType.KeyDown)
            {
                //Up key
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyUp)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, Vector3.up * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, Vector3.up * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Down key
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyDown)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, Vector3.down * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, Vector3.down * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Left key
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyLeft)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.left;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, movement * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, movement * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Forward key
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyForward)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.forward;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, movement * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, movement * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Right
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyRight)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.right;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, movement * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, movement * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Backward
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyBackward)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.back;

                    //Plain control is the POI
                    if (Event.current.shift != true)
                    {
                        m_poi.m_manager.MovePoi(m_poi, movement * 0.5f);
                    }
                    else
                    //Shift is the POI target
                    {
                        m_poi.m_manager.MovePoiLookat(m_poi, movement * 0.5f);
                    }
                    m_poi.m_manager.MoveTargetToPoi(m_poi);

                    EditorUtility.SetDirty(m_poi);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Next POI
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyPrevPoi)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    PegasusPoi poi = m_poi.m_manager.GetPrevPOI(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        m_poi = poi;
                        m_poi.m_manager.MoveTargetToPoi(m_poi);
                    }
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Prev POI
                if (Event.current.keyCode == m_poi.m_manager.m_defaults.m_keyNextPoi)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    PegasusPoi poi = m_poi.m_manager.GetNextPOI(m_poi);
                    if (poi != null)
                    {
                        if (Selection.activeTransform != null)
                        {
                            Selection.activeTransform = poi.transform;
                        }
                        if (SceneView.lastActiveSceneView != null)
                        {
                            SceneView.lastActiveSceneView.pivot = poi.transform.position;
                        }
                        m_poi = poi;
                        m_poi.m_manager.MoveTargetToPoi(m_poi);
                    }
                    Event.current.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }
            }

            //Now handle mouse clicks that would add new pegasus poi
            if (Event.current.control == true && Event.current.isMouse == true)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 10000f))
                    {
                        Vector3 newPoint = m_poi.m_manager.GetValidatedPoiPosition(hitInfo.point);
                        if (newPoint != m_lastHitPoint)
                        {
                            m_lastHitPoint = newPoint;
                            m_poi.m_manager.AddPOI(m_lastHitPoint, m_lastHitPoint);
                        }
                    }
                    else
                    {
                        if (SceneView.lastActiveSceneView != null)
                        {
                            float dist = Vector3.Distance(ray.origin, SceneView.lastActiveSceneView.pivot);
                            m_lastHitPoint = Vector3.MoveTowards(ray.origin, SceneView.lastActiveSceneView.pivot, dist / 2f);
                        }
                        else
                        {
                            m_lastHitPoint = ray.origin;
                        }
                        if (m_poi.m_manager.m_poiList.Count == 0)
                        {
                            m_poi.m_manager.m_heightCheckType = PegasusConstants.HeightCheckType.None;
                        }
                        m_poi.m_manager.AddPOI(m_lastHitPoint, m_lastHitPoint);
                    }

                    SceneView.RepaintAll();
                }
                else if (GUIUtility.hotControl == m_editor_control_id && Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    GUIUtility.hotControl = 0;
                }
                return;
            }
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {
                return new GUIContent(name);
            }
        }

        /// <summary>
        /// The tooltips
        /// </summary>
        private static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Min Height From", "Used to control how poi, lookat target and flythrough path heights are constrained. Manager - use the managers settings, collision - use whatever it collides with, terrain - use the terrain height, none - don't constrain." },
            { "Target", "Where the target should look. Path - the target will look along the path of the flythrough. Target - the target will look at a custom target." },
            { "  Angle", "The angle from the POI to the camera target." },
            { "  Distance", "The distance from the POI to the camera target." },
            { "  Height", "The height of the POI above the terrain or collider at the target location." },
            { "Type", "Change the flythrough speed in common units." },
            { "Speed", "Manually control the flythrough speed." },
            { "Distance", "The distance of this POI segment." },
            { "Start Time", "The time after the flythrough starts that this segment will be entered." },
            { "Segment Time", "The amount of time that it will take to fly through this segment. Changing the camera speed will change this value." },
            { "Total Iime", "The total amount of time that it will take for the flythrough to complete." },
        };
    }
}
