using UnityEngine;
using UnityEngine.Events;

//
// This is the base for all event listeners
// Inherit from this class if you want a new event listener
// 
// NOTE!!
// This is not needed in order to subscribe to events, this is just if you want to use the 
// editor UnityEvent for simple cases
//

public class GenericEventListener<T> : MonoBehaviour
{
    public GenericEvent<T> Event;

    public UnityEvent<T> Response;

    void OnEnable()
    {
        Event.Subscribe(CallResponse);
    }

    void OnDisable()
    {
        Event.Unsubscribe(CallResponse);
    }

    private void CallResponse(T value)
    {
        Debug.Log("RESPOND");
        Response.Invoke(value);
    }
}

