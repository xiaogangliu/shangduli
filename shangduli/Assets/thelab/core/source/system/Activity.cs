using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

namespace thelab.core
{
    /// <summary>
    /// Class that handles a single activity running in the main pool or a thread.
    /// </summary>
    [System.Serializable]
    public class Activity : IUpdateable
    {
        /// <summary>
        /// Adds a custom node into the manager pool.
        /// </summary>
        /// <param name="p_node"></param>
        /// <returns></returns>
        static public bool Add(object p_node) { return ActivityManager.instance.Add(p_node); }

        /// <summary>
        /// Removes a node from the ActivityManager pool.
        /// </summary>
        /// <param name="p_node"></param>
        /// <returns></returns>
        static public bool Remove(object p_node) { return ActivityManager.instance.Remove(p_node); }

        /// <summary>
        /// Creates a new Activity and calls 'callback' during exectuion.
        /// </summary>
        /// <param name="p_callback"></param>
        /// <param name="p_delay"></param>
        /// <param name="p_threaded"></param>
        /// <returns></returns>
        static public Activity Run(Action<Activity> p_callback,float p_delay=0f,bool p_threaded=false)
        {
            Activity n = new Activity(p_delay, p_threaded);
            n.OnActivityExecute = p_callback;
            n.Start();    
            return n;
        }


        /// <summary>
        /// Creates a new Activity and calls 'callback' during exectuion informing the elapsed time. If 'false' is returned the Activity is stopped.
        /// </summary>
        /// <param name="p_callback"></param>
        /// <param name="p_delay"></param>
        /// <param name="p_threaded"></param>
        /// <returns></returns>
        static public Activity Run(Predicate<float> p_callback,float p_delay=0f,bool p_threaded=false)
        {
            Activity n = new Activity(p_delay, p_threaded);
            n.OnActivityExecute = delegate(Activity a) { if (p_callback != null) if (!p_callback(a.elapsed)) a.Stop(); };
            n.Start();
            return n;
        }

        /// <summary>
        /// Creates a new Activity and calls 'callback' during the Activity's execution.
        /// </summary>
        /// <param name="p_callback"></param>
        /// <param name="p_delay"></param>
        /// <param name="p_threaded"></param>
        /// <returns></returns>
        static public Activity Run(Func<bool> p_callback,float p_delay=0f,bool p_threaded=false)
        {
            Activity n = new Activity(p_delay, p_threaded);
            n.OnActivityExecute = delegate(Activity a) { if (p_callback != null) if (!p_callback()) a.Stop(); };
            n.Start();
            return n;
        }

        /// <summary>
        /// Creates a new Activity and make it run during 'duration' until stopping it.
        /// </summary>
        /// <param name="p_callback"></param>
        /// <param name="p_duration"></param>
        /// <param name="p_delay"></param>
        /// <param name="p_threaded"></param>
        /// <returns></returns>
        static public Activity Run(Action p_callback,float p_duration=0f,float p_delay=0f,bool p_threaded=false)
        {
            Activity n = new Activity(p_delay, p_threaded);
            n.OnActivityExecute = delegate(Activity a) { if (p_callback != null) p_callback(); if (a.elapsed >= p_duration) a.Stop(); };
            n.Start();
            return n;
        }

        /// <summary>
        /// Executes the callback once.
        /// </summary>
        /// <param name="p_callback"></param>
        /// <param name="p_threaded"></param>
        static public void RunOnce(Action p_callback,float p_delay=0f,bool p_threaded=false)
        {
            Activity n = new Activity(p_delay, p_threaded);
            n.OnActivityExecute = delegate(Activity a) { if (p_callback != null) p_callback(); a.Stop(); };
            n.Start();            
        }

        

        /// <summary>
        /// Elapsed time since start.
        /// </summary>
        public float elapsed { get; internal set; }

        /// <summary>
        /// Activity's execution deltaTime;
        /// </summary>
        public float deltaTime { get; internal set; }

        /// <summary>
        /// Time variables.
        /// </summary>
        private DateTime m_tst;
        private TimeSpan m_tsp { get { return DateTime.UtcNow - m_tst; } }
        private float m_tick;

        /// <summary>
        /// Activity name.
        /// </summary>
        public string name;

        /// <summary>
        /// Flag that indicates if this Activity is running on a thread.
        /// </summary>
        public bool threaded { get; internal set; }

        /// <summary>
        /// Core id assigned to this Activity.
        /// </summary>
        public int core { get; internal set; }

        /// <summary>
        /// Returns the reference to the current manager. It can change during scene changes and recompilation.
        /// </summary>
        public ActivityManager manager { get { return ActivityManager.instance; } }

        /// <summary>
        /// Flag that indicates if this Activity is active.
        /// </summary>
        public bool active { get { if (!manager)return false; IList l = manager.GetList(this); if (l == null) return false; return l.IndexOf(this) >= 0; } }

        /// <summary>
        /// Flag that indicates if this Activity is active and the delay has ended.
        /// </summary>
        public bool running { get { return (elapsed >= 0f) && active; } }

        /// <summary>
        /// Flag that indicate this Activity is paused.
        /// </summary>
        public bool paused;

        /// <summary>
        /// Callbacks called when this activity is active.
        /// </summary>
        public Action<Activity> OnActivityExecute;
        public Action<Activity> OnActivityStart;
        public Action<Activity> OnActivityStop;

        /// <summary>
        /// Creates a new Activity.
        /// </summary>
        /// <param name="p_threaded">Flag that indicates if this thread will run on a thread.</param>
        public Activity(float p_delay=0f,bool p_threaded=false)
        {
            elapsed  = -p_delay;
            threaded = p_threaded;
            if (threaded) core = manager.nextCore;
            paused = false;
            name = GetType().Name + "-" + GetHashCode().ToString("X");
        }

        /// <summary>
        /// Stops this activity's execution.
        /// </summary>
        public void Stop()
        {            
            bool found = manager.Remove(this);
            if (found)
            {
                OnStop();
                if (OnActivityStop != null) OnActivityStop(this);
            }
        }
        
        /// <summary>
        /// Starts this activity.
        /// </summary>
        public void Start()
        {
            m_tst = DateTime.UtcNow;
            m_tick = 0f;
            manager.Add(this);
        }

        /// <summary>
        /// Update callback.
        /// </summary>
        virtual public void OnUpdate()
        {            
            if (paused) return;
            bool unity_time = threaded ? false : Application.isPlaying;
            if(unity_time)
            {
                deltaTime = Time.deltaTime; 
            }
            else
            {
                float tms = (float)m_tsp.TotalMilliseconds;
                deltaTime = (tms - m_tick) * 0.001f;
                if (deltaTime <= 0f) deltaTime = 1f / 60f;
                m_tick = tms;
            }
            bool isn = elapsed <= 0f;
            elapsed += deltaTime;
            bool isp = elapsed >= 0f;
            if (isn && isp)
            {                
                OnStart();
                if (OnActivityStart != null) OnActivityStart(this);
            }
            if (isp)
            {
                OnExecute();
                if (OnActivityExecute != null) OnActivityExecute(this);
            }
        }

        /// <summary>
        /// Callback called when this Activity delay ends and the execution start.
        /// </summary>
        virtual protected void OnStart() { }

        /// <summary>
        /// Callback called when this Activity is stopped.
        /// </summary>
        virtual protected void OnStop() { }

        /// <summary>
        /// 
        /// </summary>
        virtual protected void OnExecute() { }
    }
}