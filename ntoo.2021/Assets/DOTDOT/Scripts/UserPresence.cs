using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class UserPresence : MonoBehaviour
{
  [System.Serializable]
  public struct PresenceConfig
  {
    public float presenceDebounceTime;
  }

  [SerializeField]
  private string configFile = "PresenceConfig.json";

  private bool userPresent = false;
  private bool userPresentPersistent = false;
  private float timeSinceTrigger = 0;

  private float debounceTime = 1;

  public bool UserPresent
  {
    get
    {
      return userPresent;
    }

    set
    {
      if (userPresent == value) return;
      userPresent = value;
      timeSinceTrigger = 0;
    }
  }

  private void Start()
  {
    string configFilePath = Path.Combine(Application.persistentDataPath, configFile);
    if (File.Exists(configFilePath))
    {
      try
      {
        string configJson = File.ReadAllText(configFilePath);
        PresenceConfig config = JsonUtility.FromJson<PresenceConfig>(configJson);
        debounceTime = config.presenceDebounceTime;
      }
      catch (System.Exception e)
      {
        Debug.LogError("[User Presence] Error reading config from file: " + e.Message);
      }
    }
    else
    {
      PresenceConfig config = new PresenceConfig()
      {
        presenceDebounceTime = debounceTime,
      };
      string configJson = JsonUtility.ToJson(config);
      try
      {
        File.WriteAllText(configFilePath, configJson);
      }
      catch (System.Exception e)
      {
        Debug.LogError("[User Presence] Error writing config to file: " + e.Message);
      }
    }
  }

  private void Update()
  {
    if (userPresent != userPresentPersistent)
    {
      timeSinceTrigger += Time.deltaTime;
      if (timeSinceTrigger >= debounceTime)
      {
        userPresentPersistent = userPresent;
        if (userPresentPersistent)
        {
          onUserPresent.Invoke();
        }
        else
        {
          onNoUsersPresent.Invoke();
        }
      }
    }
  }

  public UnityEvent onUserPresent;

  public UnityEvent onNoUsersPresent;
}
