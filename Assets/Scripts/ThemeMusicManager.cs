using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ThemeMusicManager : MonoBehaviour
{
    public static ThemeMusicManager Instance { get; private set; }
    [SerializeField] private AudioClip[] m_themes;
    /*
    "Nightmares Inn" - RKVC Rod Kim rodkim.com @rodkim
    "Owls" - @LishGrooves
    */
    [SerializeField] private float MAX_VOLUME = 0.2f;
    private float FADE_IN_SPEED = 0.05f;
    public enum THEMES
    {
        MENU = 0,
        GAME_PLAY = 1,
    }

    private AudioSource m_audioSource;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            m_audioSource = GetComponent<AudioSource>();
            if (m_audioSource)
            {
                m_audioSource.clip = m_themes[0];
                m_audioSource.loop = true;
                m_audioSource.volume = MAX_VOLUME;
                m_audioSource.Play();

            }

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {

        FadeInVolume();
        // if(InputManager.Instance)
        // {
        //     if(InputManager.Instance.TestInputEvent == 1)
        //         SetThemeSong(THEMES.GAME_PLAY);
        // }
    }

    public void SetThemeSong(THEMES theme)
    {
        m_audioSource.Stop();
        m_audioSource.clip = m_themes[(int)theme];
        m_audioSource.loop = true;
        m_audioSource.volume = 0.04f;
        m_audioSource.Play();
    }

    private void FadeInVolume()
    {
        //Debug.Log(m_audioSource.volume);
        if (m_audioSource.volume == MAX_VOLUME) return;
        if (m_audioSource.volume < MAX_VOLUME)
            m_audioSource.volume += FADE_IN_SPEED * Time.deltaTime;
        else
            m_audioSource.volume = MAX_VOLUME;
    }
}
