using TMPro;
using UnityEngine;

public class CountDownController : MonoBehaviour
{
    [SerializeField] private TMP_Text m_startCountDown;
    [Tooltip("Difine how much the counter is going to take")]
    [SerializeField] private float m_counterStart = 3;
    
    [Tooltip("The time to wait after the counter is 0 to disable the component")]
    [SerializeField] private uint m_delayTimeBeforeDisabling = 2;

    private void OnEnable()
    {
        if (!m_startCountDown)
        {
            throw new System.NullReferenceException("TextMeshPro reference is missing, the CountDownController needs a TextMeshPro component to display the counter");
        }
        m_counterStart++; //It add one more so when it gets casted to uint the user can see the whole number.

    }
    private void Update()
    {
        CountDown();
        FadeOutCounter();
    }

    private void CountDown()
    {
        
        m_counterStart -= 1 * Time.deltaTime; //Is outside the if because we need -t second more to display the "Go!" message

        if (m_counterStart < 0) return;
        uint counter = (uint)m_counterStart;
        m_startCountDown.text = m_counterStart > 1 ? counter.ToString() : "Go!";
    }

    private void FadeOutCounter()
    {
        //TODO this method should fadeout the counter, for now is just disabling it
        if (m_counterStart < (m_delayTimeBeforeDisabling * -1)) //wait 3 seconds more
            gameObject.SetActive(false);
    }
}