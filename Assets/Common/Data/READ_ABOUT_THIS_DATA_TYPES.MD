# ABOUT DATA TYPES AS SCRIPTABLE OBJECTS

## ScriptableObject for Easy GamePlay Variable Management

This ScriptableObject system allows the team to create and manage gameplay variables easily and efficiently across the project without needing a Singleton manager. These variables can be changed at runtime and shared seamlessly between different GameObjects or systems in the game, which is useful for tweaking values such as health, score, enemy spawn rates, and more during development.

#### Key Benefits:
- **Centralized Variables**: Modify a single variable, and all references across the project automatically update.
- **Runtime Flexibility**: Adjust gameplay values without the need for recompilation or restarting the game.
- **No Singleton Dependency**: Removes the need for complex singleton managers, simplifying the code structure.

#### Example ScriptableObject for an Unsigned Integer:
The provided `UIntVariable` is an example of how to create a ScriptableObject for managing an `unsigned integer (uint)` variable that can be used in the game.

```csharp
using UnityEngine;

class GenericVariable<T> : ScriptableObject
{
    public T Value;
}

[CreateAssetMenu(fileName = "UIntVariable", menuName = "ScriptableObject/Data/UIntVariable")]
class UIntVariable : GenericVariable<uint> { }
```

#### How to Use:

##### 1. **Create a ScriptableObject in Unity:**
   1. Right-click inside the **Assets** folder.
   2. Navigate to `Create > ScriptableObject > Data > UIntVariable`.
   3. Name your new variable (e.g., `PlayerHealth` or `EnemyCount`).
   
This will create a ScriptableObject that holds a `uint` value.

##### 2. **Adding the ScriptableObject to a GameObject:**
   1. Create or select a GameObject in your scene.
   2. Create a MonoBehaviour script that references the `UIntVariable`.
   
```csharp
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public UIntVariable playerHealth;

    void Start()
    {
        Debug.Log("Player health: " + playerHealth.Value);
    }
}
```

   3. Drag and drop your ScriptableObject (e.g., `PlayerHealth`) into the `playerHealth` field in the Inspector.

##### 3. **Modifying Values at Runtime:**
   - During runtime, you can modify the value in the ScriptableObject, and all GameObjects referencing it will automatically reflect the updated value. This allows for quick iteration and testing of gameplay variables.