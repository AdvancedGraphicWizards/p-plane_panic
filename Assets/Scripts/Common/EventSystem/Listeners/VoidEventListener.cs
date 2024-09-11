using UnityEngine;
using UnityEngine.Events;

//
// An event with no arguments cant be inherited from the GenericEventListener class for some specific reasons
// If you want to create new event listeners inherit from GenericEventListener instead of copy-pasting this
//

public class VoidEventListener : MonoBehaviour
{
    public VoidEvent Event;
    public UnityEvent Response;

    void OnEnable()
    {
        Event.Subscribe(CallResponse);
    }

    void OnDisable()
    {
        Event.Unsubscribe(CallResponse);
    }

    private void CallResponse()
    {
        Response.Invoke();
    }
}

