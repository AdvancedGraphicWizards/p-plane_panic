using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerAnimComponent : MonoBehaviour
{
    [SerializeField] private Animator PCAnimator;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float maxWaitTime = 5f;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float minWaitTime = 3f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float doubleBlinkChance = 0.1f;

    private float timer = 0f;
    private float blinkWaitTime = 0f;

    void Start() {
        blinkWaitTime = Random.Range(minWaitTime,maxWaitTime);
    }

    void Update() {
        

        if (timer >= blinkWaitTime) {
            timer = 0f;
            PCAnimator.SetTrigger("Blink");
            
            if (Random.Range(0f,1f) < doubleBlinkChance) {
                blinkWaitTime=0.1f;
            }
            else {
                blinkWaitTime = Random.Range(minWaitTime,maxWaitTime);
            }
        }
        else {
            timer += Time.deltaTime;
        }
    }
}
