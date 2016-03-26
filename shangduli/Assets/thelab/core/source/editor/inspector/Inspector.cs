using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace thelab.core
{    
    /// <summary>
    /// Base class for all inspector scripts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Inspector<T> : Editor where T : Object
    {

        /// <summary>
        /// Reference to the desired object.
        /// </summary>
        new public T target { get { return (T)base.target; } }
         
        /// <summary>
        /// Inspector icon.
        /// </summary>
        public Texture2D icon { get { return m_icon; } set { m_icon = value; EditorTools.SetAssetIcon(target,m_icon); } }
        private Texture2D m_icon;
        
        /// <summary>
        /// Flag that tells something changed.
        /// </summary>
        private bool m_control_changed;
        
        /// <summary>
        /// Draws the field for changing a given property.
        /// </summary>
        /// <param name="p_target"></param>
        /// <param name="p_property"></param>
        /// <param name="p_undo_label"></param>
        /// <param name="p_options"></param>
        /// <returns></returns>
        public bool Inspect(SerializedObject p_target,string p_property,params GUILayoutOption[] p_options)
        {
            m_control_changed = false;
            SerializedObject t   = p_target;
            SerializedProperty p = t.FindProperty(p_property);
            if(p == null) return false;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(p,true,p_options);
            if(EditorGUI.EndChangeCheck())
            {
                m_control_changed = true;
                HasChange("Change "+p_property);
                t.ApplyModifiedProperties();
                                
            }
            return m_control_changed;
        }
        
        /// <summary>
        /// Draws the field for changing a given property.
        /// </summary>
        /// <param name="p_property"></param>
        /// <param name="p_undo_label"></param>
        /// <param name="p_options"></param>
        /// <returns></returns>
        public bool Inspect(Object p_target,string p_property,params GUILayoutOption[] p_options)
        {
            return Inspect(new SerializedObject(p_target),p_property,p_options);
        }

        /// <summary>
        /// Draws the field for changing a given property.
        /// </summary>
        /// <param name="p_property"></param>
        /// <param name="p_undo_label"></param>
        /// <param name="p_options"></param>
        /// <returns></returns>
        public bool Inspect(string p_property,params GUILayoutOption[] p_options)
        {
            return Inspect(serializedObject,p_property,p_options);
        }

        /// <summary>
        /// Returns a flag indicating if some previous operation incurred in changes.
        /// </summary>
        /// <param name="p_label"></param>
        /// <returns></returns>
        public bool HasChange(string p_label="",Object p_target=null)
        {
            bool changed = GUI.changed || m_control_changed;
            m_control_changed = false;
            GUI.changed = false;
            if(changed)
            {
                Object t = p_target ? p_target : target;
                Undo.RegisterCompleteObjectUndo(t,p_label);
            }
            return changed;
        }
        
    }

    

}