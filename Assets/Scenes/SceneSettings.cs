using UnityEngine;
using System.Collections;
public class SceneSettings : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("Setting framerate to 60");
        Application.targetFrameRate = 60;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
