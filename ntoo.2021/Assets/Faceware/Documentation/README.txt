*** Faceware Live Client for Unity ***

For full help and documentation, please visit the following website:

http://support.facewaretech.com/live-client-for-unity

Need assistance with anything?  Reach out to us anytime: support@facewaretech.com

Website: http://www.facewaretech.com
Support: http://support.facewaretech.com
Tutorials and Assets: http://www.facewaretech.com/learn

----------------------------------------------------------------------------------



*** Live Client for Unity Package Structure ***

    Assets
    └───Faceware
        ├───Advanced Demo
        │   ├───Materials
        │   └───Victor_Faceware_Unity_Demo.fbm
        ├───Demo
        │   ├───Materials
        │   └───Victor_Faceware_Unity_Demo.fbm
        ├───Documentation
        └───Scripts
            └───Editor
                └───Icons
            
----------------------------------------------------------------------------------
    
Folder Descriptions Below
    
    
    
*** Advanced Demo ***

The Advanced Demo shows how you can drive a custom rig with custom controls programmatically, using the values returned from Live Server.

The LiveClientAdv.cs script is what drives the rig in the advanced demo. This script is heavily commented with examples showing how you can take animation values from Live Server and manipulate them to drive specific controls on your rig.

Example: Head Position
    // This one is a little tricky because we need to convert image space to scene space.
    // This is entirely dependent on the scale of your scene and how you want the character
    // to move around. 
    var headPos = GetControl("headPos");
    float xPos = headPos[0] / 50.0F;
    float yPos = headPos[1] / 50.0F;
    float zPos = headPos[2] / 75.0F;

    this.transform.FindDeepChild("def:sternumRoot").localPosition = new Vector3(xPos, yPos + 100, zPos);

For other examples, please review the LiveClientAdv.cs script: 

Assets/Faceware/Scripts/LiveClientAdv.cs 

And read our article on Motion Logic support for Unity at

http://support.facewaretech.com/unity-motion-logic

----------------------------------------------------------------------------------



*** Basic Demo ***

This is a Demo Scene that uses our Character Setup Tool to Configure a rig to be driven by Live Server. This is the quickest and Easiest way to get facial animation from Live Server onto your rig.

For a full tutorial on how to use the Character Setup Tool please see our Getting Started Documentation

Assets/Faceware/Documentation/Faceware_LiveClient_Unity_GettingStarted.pdf

And please read our Live for Unity Support documentation:

http://support.facewaretech.com/live-client-for-unity

----------------------------------------------------------------------------------



*** Documentation ***

Contains any documentation and links to information to aide in your Faceware Live Animation Driving Experiences.

----------------------------------------------------------------------------------



*** Scripts ***

Contains all the Scripts that make up the Live Client for Unity.

Important Scripts to reference.

LiveClient.cs -> This is the script you want to attach to a Rig that is driven by a Character Setup File. Note: This script should automatically be applied during character setup so you shouldn't have to manually apply this yourself.

LiveClientAdv.cs -> This is the script to reference if you want to create your own advanced motion logic script for your rig.

----------------------------------------------------------------------------------




