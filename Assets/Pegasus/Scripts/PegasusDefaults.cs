using UnityEngine;
using System.Collections;

namespace Pegasus
{
    /// <summary>
    /// Keybaord short cuts / defaults. These will be assigned to new Pegasus as they are created.
    /// </summary>
    public class PegasusDefaults : ScriptableObject
    {
        [Header("POI Selection Ctrl")]
        public KeyCode m_keyPrevPoi = KeyCode.Home;
        public KeyCode m_keyNextPoi = KeyCode.End;

        [Header("Positioning Ctrl-POI, ShiftCtrl-LookAt")]
        public KeyCode m_keyUp = KeyCode.PageUp;
        public KeyCode m_keyDown = KeyCode.PageDown;
        public KeyCode m_keyLeft = KeyCode.LeftArrow;
        public KeyCode m_keyRight = KeyCode.RightArrow;
        public KeyCode m_keyForward = KeyCode.UpArrow;
        public KeyCode m_keyBackward = KeyCode.DownArrow;
    }
}


