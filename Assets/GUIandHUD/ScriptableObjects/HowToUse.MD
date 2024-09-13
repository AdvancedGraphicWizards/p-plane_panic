# How to use Scriptable Objects

*Go to /Assets/Scripts/Common/Data/READ_ABOUT_THIS_DATA_TYPES.MD to learn how to write scriptable objects

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