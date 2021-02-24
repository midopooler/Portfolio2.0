using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pegasus
{
    /// <summary>
    /// Editor for flythrough manager
    /// </summary>
    [CustomEditor(typeof(PegasusManager))]

    public class PegasusManagerEditor : Editor
    {
        private GUIStyle m_boxStyle;
        private GUIStyle m_wrapStyle;
        private PegasusManager m_manager;
        private bool m_environment = false;
        private int m_editor_control_id = 0;
        private Vector3 m_lastHitPoint = Vector3.zero;


        #region Menus

        /// <summary>
        /// Add pegasus to scene
        /// </summary>
        [MenuItem("GameObject/Pegasus/Add Pegasus Manager", false, 15)]
        public static void AddPegasusToScene(MenuCommand menuCommand)
        {
            GameObject pegasusGo = new GameObject("Pegasus Manager");
            PegasusManager manager = pegasusGo.AddComponent<PegasusManager>();
            manager.m_defaults = GetDefaults();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(pegasusGo, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(pegasusGo, "Created " + pegasusGo.name);
            Selection.activeObject = pegasusGo;
        }

#if HELIOS3D
        /// <summary>
        /// Add pegasus to scene
        /// </summary>
        [MenuItem("GameObject/Pegasus/Add Pegasus Manager With Helios 2D", false, 16)]
        public static void AddPegasusToSceneHelios2D()
        {
            //Locate the helios prefab
            string[] heliosGuids = AssetDatabase.FindAssets("Helios2D_For_Pegasus");
            if (heliosGuids.Length == 0)
            {
                Debug.LogWarning("Unable to locate Helios 2D for Pegasus.");
                return;
            }
            string heliosPath = AssetDatabase.GUIDToAssetPath(heliosGuids[0]);
            if (string.IsNullOrEmpty(heliosPath))
            {
                Debug.LogWarning("Unable to locate path of Helios 2D for Pegasus.");
                return;
            }
            GameObject heliosPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(heliosPath);
            if (heliosPrefab == null)
            {
                Debug.LogWarning("Unable to locate load prefab for Helios 2D for Pegasus.");
                return;
            }
            GameObject helios = PrefabUtility.InstantiatePrefab(heliosPrefab) as GameObject;
            if (helios == null)
            {
                Debug.LogWarning("Unable to instantiate Helios 2D Camera");
                return;
            }
            GameObject pegasusGo = new GameObject("Pegasus Manager H2D");
            Selection.activeGameObject = pegasusGo;
            PegasusManager pm = pegasusGo.AddComponent<PegasusManager>();
            pm.m_flythroughType = PegasusConstants.FlythroughType.SingleShot;
            pm.m_flythroughEndAction = PegasusConstants.FlythroughEndAction.QuitApplication;
            pm.m_target = helios;

            Debug.Log("Pegasus : Make sure you add your Camera FX to the Front camera. It is a child of the Helios 2D Camera.");
        }

        /// <summary>
        /// Add pegasus to scene
        // </summary>
        [MenuItem("GameObject/Pegasus/Add Pegasus Manager With Helios 3D", false, 17)]
        public static void AddPegasusToSceneHelios32D()
        {
            //Locate the helios prefab
            string[] heliosGuids = AssetDatabase.FindAssets("Helios3D_For_Pegasus");
            if (heliosGuids.Length == 0)
            {
                Debug.LogWarning("Unable to locate Helios 3D for Pegasus.");
                return;
            }
            string heliosPath = AssetDatabase.GUIDToAssetPath(heliosGuids[0]);
            if (string.IsNullOrEmpty(heliosPath))
            {
                Debug.LogWarning("Unable to locate path of Helios 3D for Pegasus.");
                return;
            }
            GameObject heliosPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(heliosPath);
            if (heliosPrefab == null)
            {
                Debug.LogWarning("Unable to locate load prefab for Helios 3D for Pegasus.");
                return;
            }
            GameObject helios = PrefabUtility.InstantiatePrefab(heliosPrefab) as GameObject;
            if (helios == null)
            {
                Debug.LogWarning("Unable to instantiate Helios 3D");
                return;
            }
            GameObject pegasusGo = new GameObject("Pegasus Manager H3D");
            Selection.activeGameObject = pegasusGo;
            PegasusManager pm = pegasusGo.AddComponent<PegasusManager>();
            pm.m_flythroughType = PegasusConstants.FlythroughType.SingleShot;
            pm.m_flythroughEndAction = PegasusConstants.FlythroughEndAction.QuitApplication;
            pm.m_target = helios;
        }
#endif

        /// <summary>
        /// Add pegasus to scene
        /// </summary>
        [MenuItem("GameObject/Pegasus/Show Pegasus Forum...", false, 40)]
        public static void ShowPegasusForum()
        {
            Application.OpenURL("http://forum.unity3d.com/threads/pegasus-no-fuss-cutscene-and-flythrough-generator.428488/");
        }

        /// <summary>
        /// Documentation
        /// </summary>
        [MenuItem("GameObject/Pegasus/Show Pegasus Tutorials...", false, 41)]
        public static void ShowPegasusTutorials()
        {
            Application.OpenURL("http://www.procedural-worlds.com/pegasus/tutorials/");
        }

        /// <summary>
        /// Add pegasus to scene
        /// </summary>
        [MenuItem("GameObject/Pegasus/Please Review Pegasus...", false, 43)]
        public static void ShowAssetStore()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/65397");
        }

        /// <summary>
        /// Helios
        /// </summary>
        [MenuItem("GameObject/Pegasus/Show Helios Video Renderer (Sister Product)...", false, 44)]
        public static void ShowHelios()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/63643");
        }

        /// <summary>
        /// Other products
        /// </summary>
        [MenuItem("GameObject/Pegasus/Show other Procedural Worlds Products...", false, 45)]
        public static void ShowOtherProducts()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:Adam Goodrich");
        }


        #endregion

        private void OnEnable()
        {
            //Check for target
            if (target == null)
            {
                return;
            }

            //Get the control id
            m_editor_control_id = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);

            //Get our manager
            m_manager = (PegasusManager)target;

            //Set up the default camera if we can
            if (m_manager.m_target == null)
            {
                if (Camera.main != null)
                {
                    m_manager.m_target = Camera.main.gameObject;
                    EditorUtility.SetDirty(m_manager);
                }
            }

            //Set up any segments / Update segments for the manager
            if (!Application.isPlaying)
            {
                m_manager.InitialiseFlythrough();
            }

            //And select nothing
            m_manager.SelectPoi(null);

            //And add Pegasus to the environment
            SetPegasusDefines();
        }

        public override void OnInspectorGUI()
        {
            //Get our manager
            m_manager = (PegasusManager)target;

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
                m_wrapStyle.fontStyle = FontStyle.Normal;
                m_wrapStyle.wordWrap = true;
            }

            //Text intro
            GUILayout.BeginVertical(string.Format("Pegasus ({0}.{1})", PegasusConstants.MajorVersion, PegasusConstants.MinorVersion), m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Welcome to Pegasus!\nTo visualise flythrough in editor mode select Window->Layouts->2 x 3 so that both Scene & Game windows are showing.\nCtrl + Left Click: Add POI.\nCrtl + ScrollWheel: Scrub timeline.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            GUILayout.BeginVertical("Configuration", m_boxStyle);
            GUILayout.Space(20);
            GameObject poiTarget = (GameObject)EditorGUILayout.ObjectField(GetLabel("Target Object"), m_manager.m_target, typeof(GameObject), true);
            PegasusConstants.FlythroughType flythroughType = (PegasusConstants.FlythroughType)EditorGUILayout.EnumPopup(GetLabel("Flythrough Type"), m_manager.m_flythroughType);
            PegasusConstants.FlythroughEndAction flythroughEndAction = m_manager.m_flythroughEndAction;
            PegasusManager nextPegasus = m_manager.m_nextPegasus;
            if (flythroughType == PegasusConstants.FlythroughType.SingleShot)
            {
                flythroughEndAction = (PegasusConstants.FlythroughEndAction)EditorGUILayout.EnumPopup(GetLabel("Flythrough End"), flythroughEndAction);
                if (flythroughEndAction == PegasusConstants.FlythroughEndAction.PlayNextPegasus)
                {
                    nextPegasus = (PegasusManager)EditorGUILayout.ObjectField(GetLabel("Next Pegasus"), nextPegasus, typeof(PegasusManager), true);
                }
            }
            bool autoStartAtRuntime = EditorGUILayout.Toggle(GetLabel("Play On Start"), m_manager.m_autoStartAtRuntime);
            PegasusConstants.TargetFrameRate targetFrameRateType = m_manager.m_targetFramerateType;
            PegasusConstants.HeightCheckType heightCheckType = m_manager.m_heightCheckType;
            float minHeightAboveTerrain = m_manager.m_minHeightAboveTerrain;
            float collisionHeightOffset = m_manager.m_collisionHeightOffset;
            float rotationDamping = m_manager.m_rotationDamping;
            float positionDamping = m_manager.m_positionDamping;
            float poiSize = m_manager.m_poiGizmoSize;

            bool showAdvanced = EditorGUILayout.BeginToggleGroup(GetLabel(" Advanced"), m_manager.m_showAdvanced);
            if (showAdvanced)
            {
                EditorGUI.indentLevel++;
                targetFrameRateType = (PegasusConstants.TargetFrameRate)EditorGUILayout.EnumPopup(GetLabel("Framerate"), m_manager.m_targetFramerateType);
                heightCheckType = (PegasusConstants.HeightCheckType)EditorGUILayout.EnumPopup(GetLabel("Check Height"), m_manager.m_heightCheckType);
                if (heightCheckType != PegasusConstants.HeightCheckType.None)
                {
                    if (heightCheckType == PegasusConstants.HeightCheckType.Collision)
                    {
                        collisionHeightOffset = EditorGUILayout.FloatField(GetLabel("Collision Offset"), collisionHeightOffset);
                    }
                    minHeightAboveTerrain = EditorGUILayout.FloatField(GetLabel("Min POI Height"), minHeightAboveTerrain);
                }
                rotationDamping = EditorGUILayout.Slider(GetLabel("Rotation Damping"), m_manager.m_rotationDamping, 0f, 3f);
                positionDamping = EditorGUILayout.Slider(GetLabel("Position Damping"), m_manager.m_positionDamping, 0f, 3f);
                poiSize = EditorGUILayout.Slider(GetLabel("Gizmo Size"), poiSize, 0.1f, 5f);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndToggleGroup();
            GUILayout.Space(3);

            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical("Statistics", m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Distance", string.Format("{0:0.00m}", m_manager.m_totalDistance));
            EditorGUILayout.LabelField("Duration", string.Format("{0}:{1:00}.{2:000}", m_manager.m_totalDuration.Minutes, m_manager.m_totalDuration.Seconds, m_manager.m_totalDuration.Milliseconds));
            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical(m_boxStyle);
            float scrubber = m_manager.m_totalDistanceTravelledPct;
            m_manager.m_showScrubber = EditorGUILayout.BeginToggleGroup(GetLabel(" Visualisation"), m_manager.m_showScrubber);
            bool showDebug = m_manager.m_displayDebug;
            bool alwaysShowGizmos = m_manager.m_alwaysShowGizmos;

            if (m_manager.m_showScrubber)
            {
                EditorGUILayout.LabelField("Switch to Game View and use the Scrubber and Step controls to visualise the flythrough path while in edit mode. This will physically move your Target in the scene so make sure you put it back to its original location afterwards.", m_wrapStyle);
                scrubber = EditorGUILayout.Slider(GetLabel("Scrubber"), scrubber, 0f, 1f);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(GetLabel("Step Backward")))
                {
                    m_manager.StepTargetBackward(1f);
                    scrubber = m_manager.m_totalDistanceTravelledPct;
                }
                if (GUILayout.Button(GetLabel("Step Forward")))
                {
                    m_manager.StepTargetForward(1f);
                    scrubber = m_manager.m_totalDistanceTravelledPct;
                }
                GUILayout.EndHorizontal();
                alwaysShowGizmos = EditorGUILayout.Toggle(GetLabel("Show Gizmos"), alwaysShowGizmos);
                showDebug = EditorGUILayout.Toggle(GetLabel("Show debug"), showDebug);
            }
            EditorGUILayout.EndToggleGroup();
            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.BeginVertical(m_boxStyle);
            m_manager.m_showPOIHelpers = EditorGUILayout.BeginToggleGroup(GetLabel(" Utilities"), m_manager.m_showPOIHelpers);
            if (m_manager.m_showPOIHelpers)
            {
                if (GUILayout.Button(GetLabel("Go To First POI")))
                {
                    PegasusPoi poi = m_manager.GetPOI(0);
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
                if (GUILayout.Button(GetLabel("Set POI To Min Height")))
                {
                    m_manager.SetPoiToMinHeight();
                }
                if (GUILayout.Button(GetLabel("Show Debug On POI")))
                {
                    m_manager.CreateDebugObjects();
                }
                if (GUILayout.Button(GetLabel("Hide Debug on POI")))
                {
                    m_manager.DeleteDebugObjects();
                }
            }
            EditorGUILayout.EndToggleGroup();
            GUILayout.Space(3);
            GUILayout.EndVertical();

            GUILayout.Space(5);

            //Display some playback controls
            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginVertical("Playback Status", m_boxStyle);
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Status", m_manager.m_currentState.ToString());
                if (m_manager.m_currentState == PegasusConstants.FlythroughState.Started)
                {
                    EditorGUILayout.LabelField("Delta Time", string.Format("{0:0.000}", m_manager.m_frameUpdateTime));
                    EditorGUILayout.LabelField("Delta Dist", string.Format("{0:0.000}", m_manager.m_frameUpdateDistance));
                    EditorGUILayout.LabelField("Current Speed", string.Format("{0:0.00}", m_manager.m_currentVelocity));
                    EditorGUILayout.LabelField("Distance Travelled", string.Format("{0:0.00}", m_manager.m_totalDistanceTravelled));
                    EditorGUILayout.LabelField("Total Distance", string.Format("{0:0.00}", m_manager.m_totalDistance));
                }
                else
                {
                    EditorGUILayout.LabelField("Delta Time", string.Format("{0:0.000}", 0f));
                    EditorGUILayout.LabelField("Delta Dist", string.Format("{0:0.000}", 0f));
                    EditorGUILayout.LabelField("Current Speed", string.Format("{0:0.00}", 0f));
                    EditorGUILayout.LabelField("Distance Travelled", string.Format("{0:0.00}", m_manager.m_totalDistanceTravelled));
                    EditorGUILayout.LabelField("Total Distance", string.Format("{0:0.00}", m_manager.m_totalDistance));
                }

                if (m_manager.m_currentState == PegasusConstants.FlythroughState.Stopped)
                {
                    if (GUILayout.Button(GetLabel("Play")))
                    {
                        m_manager.StartFlythrough();
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (m_manager.m_currentState == PegasusConstants.FlythroughState.Paused)
                    {
                        if (GUILayout.Button(GetLabel("Resume")))
                        {
                            m_manager.ResumeFlythrough();
                        }
                    }
                    else if (m_manager.m_currentState == PegasusConstants.FlythroughState.Started)
                    {
                        if (GUILayout.Button(GetLabel("Pause")))
                        {
                            m_manager.PauseFlythrough();
                        }
                    }
                    if (GUILayout.Button(GetLabel("Stop")))
                    {
                        m_manager.StopFlythrough();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(3);
                GUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_manager, "Made changes");
                m_manager.m_autoStartAtRuntime = autoStartAtRuntime;
                m_manager.m_displayDebug = showDebug;
                m_manager.m_alwaysShowGizmos = alwaysShowGizmos;
                m_manager.m_collisionHeightOffset = collisionHeightOffset;
                m_manager.m_showAdvanced = showAdvanced;
                m_manager.m_poiGizmoSize = poiSize;

                if (m_manager.m_flythroughType != flythroughType)
                {
                    m_manager.m_flythroughType = flythroughType;
                    m_manager.InitialiseFlythrough();
                }
                m_manager.m_nextPegasus = nextPegasus;

                if (m_manager.m_heightCheckType != heightCheckType)
                {
                    m_manager.m_heightCheckType = heightCheckType;
                    m_manager.InitialiseFlythrough();
                }
                m_manager.m_flythroughEndAction = flythroughEndAction;
                m_manager.m_target = poiTarget;

                if (m_manager.m_targetFramerateType != targetFrameRateType)
                {
                    m_manager.ChangeFramerate(targetFrameRateType);
                }
                if (!PegasusPoi.ApproximatelyEqual(scrubber, m_manager.m_totalDistanceTravelledPct))
                {
                    m_manager.MoveTargetTo(scrubber);
                }
                if (!PegasusPoi.ApproximatelyEqual(minHeightAboveTerrain, m_manager.m_minHeightAboveTerrain))
                {
                    m_manager.m_minHeightAboveTerrain = minHeightAboveTerrain;
                    m_manager.InitialiseFlythrough();
                }
                m_manager.m_rotationDamping = rotationDamping;
                m_manager.m_positionDamping = positionDamping;

                EditorUtility.SetDirty(m_manager);
            }
        }


        /// <summary>
        /// Detect and handle events for current spawner
        /// </summary>
        void OnSceneGUI()
        {
            //Exit if we dont have a manager
            if (m_manager == null)
            {
                return;
            }

            if (Event.current == null)
            {
                return;
            }

            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            {
                return;
            }

            //Scroll wheel - pan back and forward along the line
            if (Event.current.control == true && Event.current.type == EventType.ScrollWheel)
            {
                GUIUtility.hotControl = m_editor_control_id;
                if (Event.current.delta.y < 0f)
                {
                    m_manager.StepTargetBackward(Event.current.delta.y * -1f);
                }
                else
                {
                    m_manager.StepTargetForward(Event.current.delta.y);
                }
                Event.current.Use();
                GUIUtility.hotControl = 0;
                return;
            }



            //Check for the ctrl + left mouse button event - spawn
            if (Event.current.control == true && Event.current.isMouse == true)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    GUIUtility.hotControl = m_editor_control_id;

                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, 10000f))
                    {
                        if (m_manager.m_poiList.Count == 0)
                        {
                            if (!(hitInfo.collider is TerrainCollider)) //Lets assume we arent in an environment that uses terrains
                            {
                                m_manager.m_heightCheckType = PegasusConstants.HeightCheckType.Collision;
                            }
                        }
                        Vector3 newPoint = m_manager.GetValidatedPoiPosition(hitInfo.point);
                        if (newPoint != m_lastHitPoint)
                        {
                            m_lastHitPoint = newPoint;
                            m_manager.AddPOI(m_lastHitPoint, m_lastHitPoint);
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
                        if (m_manager.m_poiList.Count == 0) //Lets assume we are in space
                        {
                            m_manager.m_heightCheckType = PegasusConstants.HeightCheckType.None;
                        }
                        m_manager.AddPOI(m_lastHitPoint, m_lastHitPoint);
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
        /// Set up the pegasus defines
        /// </summary>
        public void SetPegasusDefines()
        {
            if (m_environment == true)
            {
                return;
            }

            m_environment = true;

            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject 
            if (!currBuildSettings.Contains("PEGASUS_PRESENT"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";PEGASUS_PRESENT");
            }
        }

        /// <summary>
        /// Get or create the pegasus defaults - allows people to override keys
        /// </summary>
        /// <returns></returns>
        public static PegasusDefaults GetDefaults()
        {
            PegasusDefaults defaults = null;
            string[] guids = AssetDatabase.FindAssets("PegasusDefaults");
            for (int idx = 0; idx < guids.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[idx]);
                if (path.Contains("PegasusDefaults.asset"))
                {
                    defaults = AssetDatabase.LoadAssetAtPath<PegasusDefaults>(path);
                    return defaults;
                }
            }
            if (defaults == null)
            {
                defaults = ScriptableObject.CreateInstance<PegasusDefaults>();
                AssetDatabase.CreateAsset(defaults, "Assets/PegasusDefaults.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return defaults;
        }

        /// <summary>
        /// Display a button that takes editor indentation into account
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool DisplayButton(GUIContent content)
        {
            TextAnchor oldalignment = GUI.skin.button.alignment;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            Rect btnR = EditorGUILayout.BeginHorizontal();
            btnR.xMin += (EditorGUI.indentLevel * 18f);
            btnR.height += 20f;
            btnR.width -= 4f;
            bool result = GUI.Button(btnR, content);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(22);
            GUI.skin.button.alignment = oldalignment;
            return result;
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GUIContent GetLabel(string name)
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
        static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Target Object", "The object that will be controlled by Pegasus manager. You would typically drop a game object with a camera attached to it – but this could just as easily be any game object you wanted to drive through the scene." },
            { "Flythrough Type", "The type of flythrough - a single shot or a connected loop." },
            { "Flythrough End", "What to do at the end of the flythrough\nStop Flythrough - Stop the flythough\nQuit Application - to quit the application\nPlay Next Pegasus - to start another pegasus flythrough (great for changing camera angles)." },
            { "Next Pegasus", "Play this Pegasus after the current one has completede ." },
            { "Framerate", "The framerate that the game will be controlled at. Set V Sync Count to Don't Sync in your projects Quality settings or Unity will ignore this setting." },
            { "Check Height", "Used to control how poi, lookat target and flythrough path heights are constrained. Collision - use whatever it collides with, Terrain - use the terrain height, None - don't constrain." },
            { "Collision Offset", "The height above the selected point from which to check downwards for collisions. Making this small can have the undesired effect of having the collision check fail as the offset may be below the collision. in general larger is better - however of you have a cramped space where the roof has a collider then make this smaller." },
            { "Min POI Height", "The minimum height that POI and collisions will be tested for." },
            { "Rotation Offset", "An offset that will be applied to all rotations. Used to fine tune rotation on objects being driven, and quite useful for fixing broken rotations on game objects." },
            { "Rotation Damping", "The amount of damping or smoothing to apply to the rotation of the target. Larger values mean slower rotations." },
            { "Position Damping", "The amount of damping or smoothing to apply to the position of the target. Larger values will do smoother flythroughs, but with less precision through POIs so it should be used with care." },
            { "Gizmo Size", "The size of the Gizmos. Larger Gizmos are easier to see." },
            { "Scrubber", "Drag this control to move the target along the timeline - designed for edit mode visualisation. Select the Game View to get the best effect." },
            { "Show debug", "Shows debug messages when the fly through changes state."},
            { "Play On Start", "Plays the flythrough on startup when selected." },
        };

    }
}