using UnityEngine;
using UnityEngine.Events;

public class PlaneCrashController : MonoBehaviour
{
    public VoidEvent Event;
    public UnityEvent Response;
    [SerializeField] private string terrainTag = "";

    private void Update()
    {
        if (transform.position.y < -3) CallResponse();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(terrainTag)){
            CallResponse();
        }
    }

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
    public void TriggerLoseEvent()
    {
        CallResponse();
        return;
    }
}
