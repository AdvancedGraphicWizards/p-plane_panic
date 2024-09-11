using System;
using UnityEngine;

//
// This is an example of an event with custom data
//

public class Player
{
    public String name;
}

[CreateAssetMenu(fileName = "PlayerEvent", menuName = "ScriptableObject/Event/PlayerEvent")]
public class PlayerEvent : GenericEvent<Player> { }
