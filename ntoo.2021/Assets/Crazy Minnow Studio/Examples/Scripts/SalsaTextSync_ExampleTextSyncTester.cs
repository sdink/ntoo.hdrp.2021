using UnityEngine;
using System.Collections;
using CrazyMinnow.SALSA.TextSync;

public class SalsaTextSync_ExampleTextSyncTester : MonoBehaviour
{
	public SalsaTextSync salsaTextSync;
	public string dialogue = "Here is some additional text to demonstrate two methods of activating TextSync.";
	public bool sendMessage = false;
	public bool sendEvent = false;

	void Start()
	{
		if (!salsaTextSync) salsaTextSync = GetComponent<SalsaTextSync>();
	}

	void Update ()
	{
		if (sendMessage) // SendMessage
		{
			sendMessage = false;
			salsaTextSync.SendMessage("Say", dialogue, SendMessageOptions.DontRequireReceiver);
		}

		if (sendEvent) // Call method
		{
			sendEvent = false;
			salsaTextSync.Say(dialogue);
        }
	}
}