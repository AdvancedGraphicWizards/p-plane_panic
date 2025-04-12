using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLightingRandomizer : MonoBehaviour
{
    [SerializeField] private Light sun;
    // Start is called before the first frame update
    void Start()
    {
        sun.transform.rotation = Quaternion.Euler(Random.Range(0, 180), 30, 0);
        sun.color = Color.Lerp(new Color(255f/255f, 244f/255f, 214f/255f), new Color(231f/255f, 91f/255f, 64f/255f), Mathf.Abs(sun.transform.rotation.eulerAngles.x - 90f) / 90f);
    }
}
