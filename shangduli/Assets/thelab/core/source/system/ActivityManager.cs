using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;


namespace thelab.core
{
    /// <summary>
    /// Class that handles all running activities.
    /// </summary>
    public class ActivityManager : MonoBehaviour
    {
        /// <summary>
        /// Global instance.
        /// </summary>
        static internal ActivityManager instance { get { return m_instance == null ? (m_instance = Unique()) : m_instance; } }
        static internal ActivityManager m_instance;
        

        /// <summary>
        /// Trimm extra managers and returns the older one.
        /// </summary>
        /// <returns></returns>
        static internal ActivityManager Unique()
        {
            ActivityManager[] ml = GameObject.FindObjectsOfType<ActivityManager>();
            ActivityManager c;
            if (ml.Length <= 0)
            {
                //Debug.Log("ActivityManager> Init");
                GameObject g = new GameObject();
                g.name = "@activity-manager";
                c = g.AddComponent<ActivityManager>();
                c.m_nextCore = 0;                
                c.ThreadStart();
                g.hideFlags = HideFlags.HideInHierarchy;
                //Debug.Log("ActivityManager> Creating Instance 0x" + g.GetInstanceID().ToString("X"));
                return c;
            }   

            c = ml[0];
            for (int i = 1; i < ml.Length; i++)
            {
                if (c.GetInstanceID() < ml[i].GetInstanceID()) c = ml[i];
            }

            for (int i = 0; i < ml.Length; i++)
            {
                if (ml[i] != c) DestroyImmediate(ml[i].gameObject);
            }
            return c;
        }

        /// <summary>
        /// List of interfaces instances.
        /// </summary>
        public List<IUpdateable> updates { get { return m_updates == null ? (m_updates = new List<IUpdateable>()) : m_updates; } }
        private List<IUpdateable> m_updates;
        public List<IConditionalUpdateable> conditional_updates { get { return m_conditional_updates == null ? (m_conditional_updates = new List<IConditionalUpdateable>()) : m_conditional_updates; } }
        private List<IConditionalUpdateable> m_conditional_updates;
        public List<ILateUpdateable> late_updates { get { return m_late_updates == null ? (m_late_updates = new List<ILateUpdateable>()) : m_late_updates; } }
        private List<ILateUpdateable> m_late_updates;
        public List<IFixedUpdateable> fixed_updates { get { return m_fixed_updates == null ? (m_fixed_updates = new List<IFixedUpdateable>()) : m_fixed_updates; } }
        private List<IFixedUpdateable> m_fixed_updates;
        public List<Activity> activities { get { return m_activities == null ? (m_activities = new List<Activity>()) : m_activities; } }
        [SerializeField]
        private List<Activity> m_activities;
        public List<ActivityBehaviour> behaviours { get { return m_behaviours == null ? (m_behaviours = new List<ActivityBehaviour>()) : m_behaviours; } }
        [SerializeField]
        private List<ActivityBehaviour> m_behaviours;

        /// <summary>
        /// CPU core counts.
        /// </summary>
        public int cores { get { return Environment.ProcessorCount; } }

        /// <summary>
        /// Thread List.
        /// </summary>
        public List<Thread> threads { get { return m_threads==null ? (m_threads = new List<Thread>()) : m_threads; } }
        private List<Thread> m_threads;
        

        /// <summary>
        /// Returns the next cpu core id for thread distribution purposes.
        /// </summary>
        internal int nextCore { get { return m_nextCore = (m_nextCore + 1) % cores; } }        
        internal int m_nextCore;

        /// <summary>
        /// Adds a node to the most compatible execution list.
        /// </summary>
        /// <param name="p_node"></param>
        public bool Add(object p_node)
        {
            IList l = GetList(p_node);
            if (l.IndexOf(p_node) >= 0) return false;
            l.Add(p_node);
            return true;
        }

        /// <summary>
        /// Removes a node to the most compatible execution list.
        /// </summary>
        /// <param name="p_node"></param>
        public bool Remove(object p_node)
        {
            IList l = GetList(p_node);
            if (l.IndexOf(p_node) < 0) return false;
            l.Remove(p_node);
            return true;
        }

        /// <summary>
        /// Removes a given Activity or ActivityBehaviour by name.
        /// </summary>
        /// <param name="p_name"></param>
        /// <returns></returns>
        public int RemoveByName(string p_name)
        {
            int k = 0;
            List<Activity> al = activities;
            for (int i = 0; i < al.Count; i++) if (al[i].name == p_name) { al.RemoveAt(i--); k++; }
            List<ActivityBehaviour> bl = behaviours;
            for (int i = 0; i < bl.Count; i++) if (bl[i].name == p_name) { bl.RemoveAt(i--); k++; }
            return k;
        }

        /// <summary>
        /// Returns the most suitable list given the object.
        /// </summary>
        /// <param name="p_node"></param>
        /// <returns></returns>
        public IList GetList(object p_node)
        {
            if(p_node is ActivityBehaviour)         return behaviours;
            if(p_node is Activity)                  return activities;
            if(p_node is IUpdateable)               return updates;
            if(p_node is IConditionalUpdateable)    return conditional_updates;
            if(p_node is ILateUpdateable)           return late_updates;
            if(p_node is IFixedUpdateable)          return fixed_updates;
            return null;
        }

        /// <summary>
        /// Starts the threading modules.
        /// </summary>
        public void ThreadStart()
        {
            for(int i=0;i<cores;i++)
            {
                Thread t = new Thread(delegate()
                {
                    int k = i;
                    while(true)
                    {                        
                        ThreadUpdate(k);                     
                        Thread.Sleep(1);
                    }
                });
                threads.Add(t);
                t.Name = "@activity-" + i;
                t.Priority = System.Threading.ThreadPriority.AboveNormal;
                t.Start();                
                //Debug.Log("ActivityManager> Created Thread [" + t.Name + "]");
            }
            //Debug.Log(threads.Count);
        }

        public void OnEnable()
        {
            //Debug.Log("enable");
        }

        public void OnDisable()
        {
            //Debug.Log("disable");
        }

        public void OnDestroy()
        {
            //Debug.Log("destroy");            
        }

        /// <summary>
        /// Unity callback.
        /// </summary>
        public void Update()
        {
            List<IUpdateable> ul = updates;
            for (int i = 0; i < ul.Count; i++) ul[i].OnUpdate();

            List<Activity> al = activities;
            for (int i = 0; i < al.Count; i++) if(!al[i].threaded) al[i].OnUpdate();

            List<ActivityBehaviour> bl = behaviours;
            for (int i = 0; i < bl.Count; i++)
            {
                if (!bl[i]) { bl.RemoveAt(i--); }
                if (bl[i] is IUpdateable) { IUpdateable n = (IUpdateable)bl[i]; n.OnUpdate(); }
                if (bl[i] is IConditionalUpdateable) { IConditionalUpdateable n = (IConditionalUpdateable)bl[i]; if (!n.OnConditionUpdate()) { bl.RemoveAt(i--); } }
            }
        }

        /// <summary>
        /// Updates Activities in a threaded scope.
        /// </summary>
        public void ThreadUpdate(int p_core)
        {
            bool need_abort = (bool)(this);
            List<Activity> al;
            if (!need_abort) 
            { 
                Thread t = Thread.CurrentThread;
                //Debug.Log("ActivityManager> Thread [" + t.Name + "] abort.");                
                t.Abort(); 
                return; 
            }
            al = activities;
            int k = 0;
            for (int i = 0; i < al.Count; i++)
            {
                if (al[i].threaded) if (al[i].core == p_core) { al[i].OnUpdate(); k++; }
            }
            //Debug.Log("ActivityManager> Core[" + p_core + "] Running[" + k + "]");
        }

        /// <summary>
        /// Unity callback.
        /// </summary>
        public void LateUpdate()
        {
            List<ILateUpdateable> ul = late_updates;
            for (int i = 0; i < ul.Count; i++) ul[i].OnLateUpdate();

            List<ActivityBehaviour> bl = behaviours;
            for (int i = 0; i < bl.Count; i++)
            {
                if (!bl[i]) { bl.RemoveAt(i--); }
                if (bl[i] is ILateUpdateable) { ILateUpdateable n = (ILateUpdateable)bl[i]; n.OnLateUpdate(); }
            }
        }

        /// <summary>
        /// Unity callback.
        /// </summary>
        public void FixedUpdate()
        {
            List<IFixedUpdateable> ul = fixed_updates;
            for (int i = 0; i < ul.Count; i++) ul[i].OnFixedUpdate();

            List<ActivityBehaviour> bl = behaviours;
            for (int i = 0; i < bl.Count; i++)
            {
                if (!bl[i]) { bl.RemoveAt(i--); }
                if (bl[i] is IFixedUpdateable) { IFixedUpdateable n = (IFixedUpdateable)bl[i]; n.OnFixedUpdate(); }
            }
        }
    }

}