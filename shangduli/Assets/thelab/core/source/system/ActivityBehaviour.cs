using UnityEngine;
using System.Collections;


namespace thelab.core
{
    /// <summary>
    /// Activity combined with a behaviour.
    /// </summary>
    public class ActivityBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Returns the reference to the global manager.
        /// </summary>
        public ActivityManager manager { get { return ActivityManager.instance; } }

        /// <summary>
        /// Workflow callbacks.
        /// </summary>
        void OnEnable()  { Activity.Add(this); }
        void OnDisable() 
        {            
            #if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) Activity.Remove(this);
            #else
            Activity.Remove(this);
            #endif
       }
    
    }
}