using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeathersNPRController : MonoBehaviour
{
    public Mesh characterMesh;
    public Shader shellFurShader;
    public FeatherNPRSettings featherSettings;
    public Color featherColor;

    //private:
    private GameObject[] shells;
    private Material furryMaterial;
    private Vector3 displacementDirection = new Vector3(0, 0, 0);
    //TODO ------------ WIP
    Vector3 windDirection = new Vector3(0, 0, 0);
    public float windStrength = 5.0f;
    public float windFrequency = 2.0f;
    float windY = 0.0f;
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

    void Update()
    {

        Vector3 camW = transform.localToWorldMatrix * Camera.main.transform.position;

        Vector3 direction = camW.normalized;//new Vector3(0, 0, 0);

        //TODO remove inputs
        // direction.x = Convert.ToInt32(Input.GetKey(KeyCode.D)) - Convert.ToInt32(Input.GetKey(KeyCode.A));
        // direction.y = Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
        // direction.z = Convert.ToInt32(Input.GetKey(KeyCode.Q)) - Convert.ToInt32(Input.GetKey(KeyCode.E));

        // This changes the direction that the hair is going to point in, when we are not inputting any movements then we subtract the gravity vector
        // The gravity vector just being (0, -1, 0)
        displacementDirection -= direction * Time.deltaTime * 10.0f;
        if (direction == Vector3.zero)
            displacementDirection.y -= 10.0f * Time.deltaTime;

        //TODO moving wind
        windDirection = camW.normalized;// Camera.main.transform.position.normalized;
        windY = Mathf.Sin(Time.time * windStrength) * windFrequency;
        //Debug.Log(windY);
        windDirection.y += windY;
        windDirection.x += windY;

        //displacementDirection += windDirection;


        if (displacementDirection.magnitude > 1) displacementDirection.Normalize();

        // In order to avoid setting this variable on every single shell's material instance, we instead set this is as a global shader variable
        // That every shader will have access to, which sounds bad, because it kind of is, but just be aware of your global variable names and it's not a big deal.
        // Regardless, setting the variable one time instead of 256 times is just better.
        Shader.SetGlobalVector("_ShellDirection", displacementDirection);

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

        material.SetFloat("_Hardness", featherSettings.hardness);
        material.SetFloat("_PhongExponent", featherSettings.phongExponent);
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
