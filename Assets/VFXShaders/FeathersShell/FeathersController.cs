using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeathersController : MonoBehaviour
{
    public Mesh characterMesh;
    public Shader shellFurShader;
    public FeatherSettings featherSettings;
    public Color featherColor;
    
    //private:
    private GameObject[] shells;
    private Material furryMaterial;
    private Vector3 displacementDirection = new Vector3(0, 0, 0);

    private void Awake()
    {
        if (!featherSettings)
        {
            throw new System.NullReferenceException("Fur setting missing");
        }
        if (!shellFurShader)
        {
            throw new System.NullReferenceException("Fur shader missing");
        }

        characterMesh = GetComponent<MeshFilter>().mesh;
        featherColor = featherSettings.shellColor;
    }

    private void OnEnable()
    {
        shells = new GameObject[featherSettings.shellCount];
        furryMaterial = new Material(shellFurShader);

        for (int i = 0; i < featherSettings.shellCount; i++)
        {
            shells[i] = new GameObject("Shell " + i.ToString());
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();

            shells[i].GetComponent<MeshFilter>().mesh = characterMesh;
            shells[i].GetComponent<MeshRenderer>().material = furryMaterial;
            shells[i].transform.SetParent(this.transform, false);

            setShaderAttribute(shells[i].GetComponent<MeshRenderer>().material, i);
        }
    }
    //TODO ------------ WIP
        Vector3 windDirection = new Vector3(0, 0, 0);
        
        float windStrength = 5.0f;
    void Update()
    {
        float velocity = 1.0f;
        float GRAVITY = 10.0f;

        Vector3 direction = new Vector3(0, 0, 0);
        Vector3 oppositeDirection = new Vector3(0, 0, 0);

        

        // This determines the direction we are moving from wasd input. It's probably a better idea to use Unity's input system, since it handles
        // all possible input devices at once, but I did it the old fashioned way for simplicity.
        //TODO remove inputs
        //direction.x = Convert.ToInt32(Input.GetKey(KeyCode.D)) - Convert.ToInt32(Input.GetKey(KeyCode.A));
        //direction.y = Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
        //direction.z = Convert.ToInt32(Input.GetKey(KeyCode.Q)) - Convert.ToInt32(Input.GetKey(KeyCode.E));

        //Debug.Log(direction);

        // This moves the ball according the input direction
        Vector3 currentPosition = this.transform.position;
        direction.Normalize();
        currentPosition += direction * velocity * Time.deltaTime;
        this.transform.position = currentPosition;

        // This changes the direction that the hair is going to point in, when we are not inputting any movements then we subtract the gravity vector
        // The gravity vector just being (0, -1, 0)
        displacementDirection -= direction * Time.deltaTime * 10.0f;
        //Debug.Log(displacementDirection);

        if (direction == Vector3.zero)
            displacementDirection.y -= GRAVITY * Time.deltaTime;

        if (displacementDirection.magnitude > 1) displacementDirection.Normalize();

        // In order to avoid setting this variable on every single shell's material instance, we instead set this is as a global shader variable
        // That every shader will have access to, which sounds bad, because it kind of is, but just be aware of your global variable names and it's not a big deal.
        // Regardless, setting the variable one time instead of 256 times is just better.
        Shader.SetGlobalVector("_ShellDirection", displacementDirection);

        //TODO moving wind
        float windY = Mathf.Sin(Time.time * windStrength) * 2f;
        //Debug.Log(windY);
        windDirection.y += windY * Time.deltaTime;
        windDirection.x += windY * Time.deltaTime;

        Shader.SetGlobalVector("_WindDirection", windDirection);

        if (featherSettings.updateStatics)
        {
            for (int i = 0; i < featherSettings.shellCount; ++i)
            {
                setShaderAttribute(shells[i].GetComponent<MeshRenderer>().material, i);
            }
        }


    }

    private void setShaderAttribute(Material material, int i)
    {
        material.SetInt("_ShellCount", featherSettings.shellCount);
        material.SetInt("_ShellIndex", i);
        material.SetFloat("_ShellLength", featherSettings.shellLength);
        material.SetFloat("_Density", featherSettings.density);
        material.SetFloat("_Thickness", featherSettings.thickness);
        material.SetFloat("_Attenuation", featherSettings.occlusionAttenuation);
        material.SetFloat("_ShellDistanceAttenuation", featherSettings.distanceAttenuation);
        material.SetFloat("_Curvature", featherSettings.curvature);
        material.SetFloat("_DisplacementStrength", featherSettings.displacementStrength);
        material.SetFloat("_OcclusionBias", featherSettings.occlusionBias);
        material.SetFloat("_NoiseMin", featherSettings.noiseMin);
        material.SetFloat("_NoiseMax", featherSettings.noiseMax);
        material.SetVector("_ShellColor", featherColor);

        //TODO WIP wind
        material.SetVector("_WindDirection", windDirection);
    }

    void OnDisable()
    {
        for (int i = 0; i < shells.Length; ++i)
        {
            Destroy(shells[i]);
        }

        shells = null;
    }
}
