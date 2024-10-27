using System.Collections;
using UnityEngine;

public class FireTooltipScript : MonoBehaviour
{
    [Header("Tooltip Settings")]
    [SerializeField] private TooltipData tooltipData;
    [Tooltip("How many fires we need to extinguish before we hide the tooltips.")]
    [SerializeField] private int firesExtinguishedBeforeHideTooltip = 2;

    private void Start()
    {
        if (tooltipData == null)
        {
            Debug.LogError("No tooltipData scriptable object assigned.");
            Destroy(gameObject);
        }

        else
        {
            if (tooltipData.firesExtinguished >= firesExtinguishedBeforeHideTooltip) Destroy(gameObject);
        }
    }

    void Update()
    {
        if (tooltipData != null)
        {
            if (tooltipData.firesExtinguished >= firesExtinguishedBeforeHideTooltip) Destroy(gameObject);
        }
    }
}
