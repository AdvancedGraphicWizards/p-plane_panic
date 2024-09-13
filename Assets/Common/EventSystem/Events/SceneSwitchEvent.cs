using UnityEngine;

public enum SceneName
{
    Intro,
    Lobby,
    Game,
    Result,
}

[CreateAssetMenu(fileName = "SceneSwitchEvent", menuName = "ScriptableObject/Event/SceneSwitchEvent")]
public class SceneSwitchEvent : GenericEvent<SceneName> { }

