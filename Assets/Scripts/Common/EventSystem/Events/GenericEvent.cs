using UnityEngine;
using UnityEngine.Events;

//
// This is the base for all events
// Inherit from this class if you want a new event
//

public class GenericEvent<T> : ScriptableObject
{
    private UnityAction<T> Event;

    public void Raise(T value)
    {
        Event?.Invoke(value);
    }

    public void Subscribe(UnityAction<T> callback)
    {
        Event += callback;
    }
    public void Unsubscribe(UnityAction<T> callback)
    {
        Event -= callback;
    }
}

//
// EXAMPLE OF HOW TO SUBSCRIBE WITHOUT LISTENER
//
// public class DebugOutput : MonoBehaviour
// {
//     public StringEvent InputEvent;
//
//     void OnEnable()
//     {
//         InputEvent.Subscribe(OnChanged);
//     }
//     void OnDisabled()
//     {
//         InputEvent.Unsubscribe(OnChanged);
//     }
//      
//     // This is a callback function which you can name whatever you want
//     // Don't forget to match the input value with the type of the event
//     void OnChanged(String value)
//     {
//         Debug.Log(value)
//     }
// }
//
