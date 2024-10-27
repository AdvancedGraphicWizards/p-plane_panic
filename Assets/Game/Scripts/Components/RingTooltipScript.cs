using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RingSpawnScript))]
public class RingTooltipScript : MonoBehaviour
{
    [Header("Tooltip Settings")]
    [SerializeField] private GameObject fuelCollectTooltipPrefab;
    [SerializeField] private TooltipData tooltipData;
    [Tooltip("How high above the centre(?) of the ring we place the tooltip.")]
    [SerializeField] private float heightAboveRing = 5f;
    [Tooltip("How often (in seconds) we update the position of the tooltip.")]
    [SerializeField] private float updateInterval = 1f;
    [Tooltip("How many rings we need to pass through before we hide the tooltips.")]
    [SerializeField] private int ringsPassedBeforeHideTooltip = 3;

    private GameObject activeTooltip;
    private Transform closestRing;
    private RingSpawnScript ringSpawnScript;
    private Coroutine tooltipCoroutine;

    private void Start()
    {
        if (fuelCollectTooltipPrefab != null)
        {
            activeTooltip = Instantiate(fuelCollectTooltipPrefab);
            activeTooltip.SetActive(false);
        }

        if (tooltipData == null)
        {
            Debug.LogError("No tooltipData scriptable object assigned.");
        }

        if (TryGetComponent<RingSpawnScript>(out RingSpawnScript rs))
        {
            ringSpawnScript = rs;
        }

        tooltipCoroutine = StartCoroutine(UpdateTooltipPosition());
    }

    private IEnumerator UpdateTooltipPosition()
    {
        while (tooltipData.ringsPassed < ringsPassedBeforeHideTooltip)
        {
            yield return new WaitForSeconds(updateInterval);

            closestRing = ringSpawnScript.ClosestRing;
            if (closestRing != null)
            {
                if (activeTooltip != null)
                {
                    activeTooltip.SetActive(true);
                    activeTooltip.transform.position = closestRing.position + Vector3.up * heightAboveRing;
                }
            }

            else
            {
                if (activeTooltip != null)
                {
                    activeTooltip.SetActive(false);
                }
            }
        }

        if (activeTooltip != null)
        {
            activeTooltip.SetActive(false);
        }

        tooltipCoroutine = null;
    }
}
