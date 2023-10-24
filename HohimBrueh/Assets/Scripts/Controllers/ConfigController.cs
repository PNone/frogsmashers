using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigController : MonoBehaviour
{
    public static FrogConfig Config;
    // Start is called before the first frame update
    void Start()
    {
        //Get the JSON string from the file on disk.
        string savedJson = File.ReadAllText($"{Application.streamingAssetsPath}/Config.json");
        //Convert the JSON string back to a Config object.
        Config = JsonUtility.FromJson<FrogConfig>(savedJson);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class FrogConfig
{
    public List<string> DeathTexts = new List<string>();
}
