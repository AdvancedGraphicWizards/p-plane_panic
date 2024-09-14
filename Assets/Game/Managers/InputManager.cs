using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    
    //[SerializeField] private PlayerState so_playerState;
    private float m_startEvent; // A bool triggered by the Space bar to test anything
    private Vector2 m_direction; // Unit 2D vector, default state is [0,0]
    private bool m_jump;
    private Vector2 m_pointer;
    private float m_mousePressed;
    //private static bool m_giveControl = true;
    //public bool GiveControl { get { return m_giveControl; } set { m_giveControl = value; } }
    private Vector2 m_look;
    public Vector2 Look { get => m_look; set => m_look = value; }
    private InputAction m_mouse;
    public Vector2 Direction;

    private void Awake()
    {
    }

    public bool Jump
    {
        get => m_jump;
        private set => m_jump = value;
    }

    // public void EnablePlayerInteraction()
    // {
    //     Cursor.lockState = CursorLockMode.Locked;
    //     Cursor.visible = false;
    //     m_giveControl = true;
    //     //so_playerState.Init();
    // }
    // public void DisablePlayerInteraction()
    // {
    //     Cursor.lockState = CursorLockMode.None;
    //     Cursor.visible = true;
    //     m_giveControl = false;
    //     m_direction = Vector2.zero;
    //     m_jump = false;
    // }

    public void OnMove(InputAction.CallbackContext context)
    {
        
        Direction = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Jump = context.ReadValueAsButton();
        }
        else if (context.canceled)
        {
            Jump = context.ReadValueAsButton();
        }
    }


}
