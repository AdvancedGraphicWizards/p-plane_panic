using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set;}
    [SerializeField] private GameState m_gameState;
    [SerializeField] private GameObject m_fuelHUD;
    [SerializeField] private GameObject m_distanceHUD;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

}
