using UnityEngine;
using UnityEngine.Events;

//
// An event with no arguments cant be inherited from the GenericEvent class for some specific reasons
// If you want to create new events inherit from GenericEvent instead of copy-pasting this
//

[CreateAssetMenu(fileName = "VoidEvent", menuName = "ScriptableObject/Event/VoidEvent", order = 1)]
public class VoidEvent : ScriptableObject
{
    private UnityAction Event;

    public void Raise()
    {
        Event?.Invoke();
    }

    public void Subscribe(UnityAction callback)
    {
        Event += callback;
    }
    public void Unsubscribe(UnityAction callback)
    {
        Event -= callback;
    }
}
