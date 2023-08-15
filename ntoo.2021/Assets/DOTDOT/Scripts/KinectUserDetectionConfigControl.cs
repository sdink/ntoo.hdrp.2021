using com.rfilkov.kinect;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class KinectUserDetectionConfigControl : MonoBehaviour
{
    [System.Serializable]
    private class KinectUserDetectionConfig
    {
        [Tooltip("Minimum distance from kinect user will be detected. 0 = no restriction")]
        public float minUserDistance;
        [Tooltip("Maximum distance from kinect user will be detected. 0 = no restriction")]
        public float maxUserDistance;
        [Tooltip("Maximum distance user can be from centreline from kinect. 0 = no restriction")]
        public float maxUserLeftRightDistance;
    }

    [SerializeField]
    [Tooltip("Config file to store persistent settings")]
    private string configFile = "KinectUserConfig.json";

    [Header("Default Configuration")]
    [SerializeField]
    [Tooltip("Default configuration")]
    private KinectUserDetectionConfig config = new KinectUserDetectionConfig()
    {
        minUserDistance = 0.0f,
        maxUserDistance = 0.0f,
        maxUserLeftRightDistance = 0.0f,
    };

    [Header("Events")]
    public UnityEvent<float> onMinUserDistanceUpdated;
    public UnityEvent<float> onMaxUserDistanceUpdated;
    public UnityEvent<float> onMaxUserLeftRightDistanceUpdated;

    private string configFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, configFile);
        }
    }

    void OnEnable()
    {
        if (File.Exists(configFilePath))
        {
            try
            {
                string configJson = File.ReadAllText(configFilePath);
                JsonUtility.FromJsonOverwrite(configJson, config);
            }
            catch (Exception e)
            {
                Debug.LogError("[Kinect User Detection Config Control] Error reading config from file: " + e.Message);
            }

            if (KinectManager.Instance != null)
            {
                KinectManager.Instance.minUserDistance = config.minUserDistance;
                KinectManager.Instance.maxUserDistance = config.maxUserDistance;
                KinectManager.Instance.maxLeftRightDistance = config.maxUserLeftRightDistance;
            }
            onMinUserDistanceUpdated.Invoke(config.minUserDistance);
            onMaxUserDistanceUpdated.Invoke(config.maxUserDistance);
            onMaxUserLeftRightDistanceUpdated.Invoke(config.maxUserLeftRightDistance);
        }
        else
        {
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        string configJson = JsonUtility.ToJson(config);
        try
        {
            File.WriteAllText(configFilePath, configJson);
        }
        catch (Exception e)
        {
            Debug.LogError("[Kinect User Detection Config Control] Error writing config to file: " + e.Message);
        }
    }

    public void UpdateMinUserDistance(float value)
    {
        config.minUserDistance = value;
        SaveConfig();
        KinectManager.Instance.minUserDistance = value;
        onMinUserDistanceUpdated.Invoke(value);
    }

    public void UpdateMaxUserDistance(float value)
    {
        config.maxUserDistance = value;
        SaveConfig();
        KinectManager.Instance.maxUserDistance= value;
        onMaxUserDistanceUpdated.Invoke(value);
    }

    public void UpdateMaxUserLeftRightDistance(float value)
    {
        config.maxUserLeftRightDistance = value;
        SaveConfig();
        KinectManager.Instance.maxLeftRightDistance = value;
        onMaxUserLeftRightDistanceUpdated.Invoke(value);
    }
}
