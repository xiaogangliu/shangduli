
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace thelab.core
{
    
    /// <summary>
    /// Delegate that describes the event method.
    /// </summary>
    /// <param name="p_event"></param>
    public delegate void EventComponentDelegate(UIEvent p_event);

    [Serializable]
    public class EventComponentCallback : UnityEvent<UIEvent> { }

    #region enum UIEventType

    /// <summary>
    /// Event Type enumeration.
    /// </summary>
    public enum UIEventType
    {
        None = -1,
        Down,
        Up,
        Hold,
        Click,
        Enter,
        Stay,
        Move,
        Exit,
        DragStart,
        DragOver,
        DragUpdate,
        DragEnd,
        Drop,
        Scroll,
    }

    #endregion

    #region class Event

    [Serializable]
    public class UIEvent
    {
        /// <summary>
        /// Event type.
        /// </summary>
        public UIEventType type;

        /// <summary>
        /// Target.
        /// </summary>
        public EventComponent target;

    }

    #endregion

    /// <summary>
    /// Class that describes a simple component that handles UI events.
    /// </summary>
    public class EventComponent : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IMoveHandler, IDropHandler, IEndDragHandler, IScrollHandler
    {

        /// <summary>
        /// Allowed events.
        /// </summary>
        public UIEventType[] allowed = new UIEventType[] { UIEventType.Click };

        /// <summary>
        /// Flag that indicates the mouse is down.
        /// </summary>
        public bool down;

        /// <summary>
        /// Flag that indicates the mouse is over.
        /// </summary>
        public bool over;

        /// <summary>
        /// Flag that indicates that dragging is occuring.
        /// </summary>
        public bool drag;

        /// <summary>
        /// Time in seconds the mouse button is being held.
        /// </summary>
        public float hold;

        /// <summary>
        /// Time in seconds the mouse is over.
        /// </summary>
        public float stay;

        /// <summary>
        /// Data from move events.
        /// </summary>
        public AxisEventData axis;

        /// <summary>
        /// Event data.
        /// </summary>
        public PointerEventData data;

        /// <summary>
        /// Target object used in some events.
        /// </summary>
        public GameObject element;

        /// <summary>
        /// Callback.
        /// </summary>
        public EventComponentCallback OnEvent;
        
        /// <summary>
        /// Returns a flag indicating the event will be emmited.
        /// </summary>
        /// <param name="p_type"></param>
        /// <returns></returns>
        public bool WillDispatch(UIEventType p_type) { return enabled ? (Array.IndexOf(allowed, p_type) >= 0) : false; }
        
        /// <summary>
        /// Emmits an event.
        /// </summary>
        /// <param name="p_type"></param>
        public void Dispatch(UIEventType p_type)
        {
            if(OnEvent == null) return;
            if(WillDispatch(p_type))
            {   
                UIEvent ev = new UIEvent();
                ev.target = this;
                ev.type = p_type;
                OnEvent.Invoke(ev);
            }
        }

        /// <summary>
        /// Handlers.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)  { data = eventData; down = true; hold = 0f;  Dispatch(UIEventType.Down); Activity.Run(OnHoldUpdate); }
        public void OnPointerClick(PointerEventData eventData) { data = eventData; down = false; Dispatch(UIEventType.Click); }
        public void OnPointerUp(PointerEventData eventData)    { data = eventData; down = false; Dispatch(UIEventType.Up);   }
        public void OnPointerEnter(PointerEventData eventData) 
        {
            data = eventData;
            over = true; 
            stay = 0f; 
            Dispatch(UIEventType.Enter);
            if(eventData.pointerDrag) 
            { 
                element = eventData.pointerDrag; 
                Dispatch(UIEventType.DragOver);
                EventComponent t = element.GetComponent<EventComponent>();
                if(t) { t.element = gameObject; t.Dispatch(UIEventType.DragOver); }
            }
            Activity.Run(OnStayUpdate); 
        }
        public void OnPointerExit(PointerEventData eventData)  { data = eventData; over = false; Dispatch(UIEventType.Exit);  }

        public void OnMove(AxisEventData eventData) { axis = eventData; Dispatch(UIEventType.Move); }

        public void OnDrag(PointerEventData eventData)    { data = eventData; DragStart(); Dispatch(UIEventType.DragUpdate); }
        public void OnDrop(PointerEventData eventData)    
        {            
            data = eventData;
            DragEnd(); 
            element = eventData.pointerDrag;            
            Dispatch(UIEventType.Drop);
            EventComponent t = element.GetComponent<EventComponent>();
            if(t){ t.element = gameObject; t.Dispatch(UIEventType.Drop); }
        }        
        public void OnEndDrag(PointerEventData eventData) { data = eventData; DragEnd(); }
        
        private void DragStart() { if(!drag) Dispatch(UIEventType.DragStart); drag = true; }        
        private void DragEnd()   { if(drag)  Dispatch(UIEventType.DragEnd); drag = false; }        
        private bool OnHoldUpdate(float t) { if(down) { Dispatch(UIEventType.Hold); hold += Time.deltaTime; } return down; }
        private bool OnStayUpdate(float t) { if(over) { Dispatch(UIEventType.Stay); stay += Time.deltaTime; } return over; }

        public void OnScroll(PointerEventData eventData) { data = eventData; Dispatch(UIEventType.Scroll); }
    }

}