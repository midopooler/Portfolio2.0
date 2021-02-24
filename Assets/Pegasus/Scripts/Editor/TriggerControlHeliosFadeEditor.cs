using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if HELIOS3D
namespace Pegasus
{
    [CustomEditor(typeof(TriggerControlHeliosFade))]
    public class HeliosFadeTriggerEditor : Editor
    {
        GUIStyle m_boxStyle;
        GUIStyle m_wrapStyle;
        TriggerControlHeliosFade m_trigger;

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
            m_trigger = (TriggerControlHeliosFade)target;
        }

        /// <summary>
        /// Draw the POI gui
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get our trigger
            m_trigger = (TriggerControlHeliosFade)target;

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
            GUILayout.BeginVertical("Helios Fade Trigger", m_boxStyle);
            GUILayout.Space(20);
            EditorGUILayout.LabelField("This trigger controls Helios Fades.", m_wrapStyle);
            GUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(5);

            PegasusConstants.PoiHeliosTriggerAction actionOnStart = m_trigger.m_actionOnStart;
            PegasusConstants.PoiHeliosTriggerAction actionOnEnd = m_trigger.m_actionOnEnd;
            float startDuration = m_trigger.m_startDuration;
            float endDuration = m_trigger.m_endDuration;
            Color startColor = m_trigger.m_startColour;
            Color endColor = m_trigger.m_endColour;

            actionOnStart = (PegasusConstants.PoiHeliosTriggerAction)EditorGUILayout.EnumPopup(GetLabel("Action On Start"), actionOnStart);
            if (actionOnStart != PegasusConstants.PoiHeliosTriggerAction.DoNothing)
            {
                startColor = EditorGUILayout.ColorField(GetLabel("Color"), startColor);
                startDuration = EditorGUILayout.Slider(GetLabel("Duration"), startDuration, 0.1f, 10f);
            }

            actionOnEnd = (PegasusConstants.PoiHeliosTriggerAction)EditorGUILayout.EnumPopup(GetLabel("Action On End"), actionOnEnd);
            if (actionOnEnd != PegasusConstants.PoiHeliosTriggerAction.DoNothing)
            {
                endColor = EditorGUILayout.ColorField(GetLabel("Color"), endColor);
                endDuration = EditorGUILayout.Slider(GetLabel("Duration"), endDuration, 0.1f, 10f);
            }

            GUILayout.Space(5);

            //Check for changes, make undo record, make changes and let editor know we are dirty
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_trigger, "Made trigger changes");

                m_trigger.m_triggerAtStart = true;
                m_trigger.m_triggerOnUpdate = true;
                m_trigger.m_triggerAtEnd = false;
                m_trigger.m_actionOnStart = actionOnStart;
                m_trigger.m_startColour = startColor;
                m_trigger.m_startDuration = startDuration;
                m_trigger.m_actionOnEnd = actionOnEnd;
                m_trigger.m_endColour = endColor;
                m_trigger.m_endDuration = endDuration;

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
#endif