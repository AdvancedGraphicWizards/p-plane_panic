using UnityEngine;

public enum SceneName
{
    Intro,
    //Lobby, //TODO Temporarlly removed for the HelloWorld demo
    Game,
    Result,
}

[CreateAssetMenu(fileName = "SceneSwitchEvent", menuName = "ScriptableObject/Event/SceneSwitchEvent")]
public class SceneSwitchEvent : GenericEvent<SceneName> { }

