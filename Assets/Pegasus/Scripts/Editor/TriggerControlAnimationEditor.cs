using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pegasus
{
    [CustomEditor(typeof(TriggerControlAnimation))]
    public class PegasusTriggerControlAnimationEditor : Editor
    {
        GUIStyle m_boxStyle;
        GUIStyle m_wrapStyle;
        TriggerControlAnimation m_trigger;

        /// <summary>
        /// This is called when we select the poi in the editor
        /// </summary>
        private void OnEnable()
        {
            if (target == null)
            {
                return;
            }

            //Get our poi
            m_trigger = (TriggerControlAnimation)target;
        }

        /// <summary>
        /// Draw the POI gui
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get our trigger
            m_trigger = (TriggerControlAnimation)target;

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
            GUILayout.BeginVertical("Pegasus Animation Trigger", m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("This trigger controls animations.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            //See if we can grab the animation off the manager target object if there is no animation
            if (m_trigger.m_targetAnimation == null)
            {
                PegasusPoi poi = m_trigger.gameObject.GetComponentInChildren<PegasusPoi>();
                if (poi != null)
                {
                    if (poi.m_manager.m_target != null)
                    {
                        m_trigger.m_targetAnimation = poi.m_manager.m_target.GetComponentInChildren<Animation>();
                    }
                }
            }

            //As
            Animation targetAnimation = (Animation)EditorGUILayout.ObjectField(GetLabel("Target"), m_trigger.m_targetAnimation, typeof(Animation), true);
            PegasusConstants.PoiAnimationTriggerAction actionOnStart = m_trigger.m_actionOnStart;
            int startAnimationIdx = m_trigger.m_startAnimationIdx;
            PegasusConstants.PoiAnimationTriggerAction actionOnEnd = m_trigger.m_actionOnEnd;

            if (targetAnimation != null)
            {
                actionOnStart = (PegasusConstants.PoiAnimationTriggerAction)EditorGUILayout.EnumPopup(GetLabel("Action On Start"), actionOnStart);

                if (actionOnStart == PegasusConstants.PoiAnimationTriggerAction.PlayAnimation)
                {
                    int assetIdx = 0;
                    GUIContent[] assetChoices = null;
                    assetChoices = new GUIContent[targetAnimation.GetClipCount()];
                    foreach (AnimationState anim in targetAnimation)
                    {
                        assetChoices[assetIdx++] = new GUIContent(anim.name);
                    }
                    startAnimationIdx = EditorGUILayout.Popup(GetLabel("Animation"), startAnimationIdx, assetChoices);
                }
                actionOnEnd = (PegasusConstants.PoiAnimationTriggerAction)EditorGUILayout.EnumPopup(GetLabel("Action On End"), actionOnEnd);
            }


            /*
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
            */

            GUILayout.Space(5);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_trigger, "Made trigger changes");

                m_trigger.m_triggerAtStart = true;
                m_trigger.m_triggerOnUpdate = false;
                m_trigger.m_triggerAtEnd = true;
                m_trigger.m_targetAnimation = targetAnimation;
                m_trigger.m_actionOnStart = actionOnStart;
                m_trigger.m_startAnimationIdx = startAnimationIdx;
                m_trigger.m_actionOnEnd = actionOnEnd;

                //Mark it as dirty
                EditorUtility.SetDirty(m_trigger);
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
        };
    }
}
