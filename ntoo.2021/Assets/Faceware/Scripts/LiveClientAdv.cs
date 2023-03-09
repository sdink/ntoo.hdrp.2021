using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

public class LiveClientAdv : LiveBase
{
    // Boolean used to parse secondary controls (As an example for future implementation)
    private bool m_ParseAdditionalData = false;

    protected override void Start()
    {
        //Setup Connection to Live
        live = new LiveConnection(LiveServerHostIP, LiveServerHostPort);
        live.m_Reconnect = ReconnectOnLostConnection;
        // Connect on Play if toggled true
        if (ConnectOnPlay)
            Connect();
        live.m_DropPackets = DropPackets;

        base.Start();
    }

    void Update()
    {
        BaseUpdate();
        if (live != null && live.IsConnected())
        {
            currentDatSet = live.GetLiveData();
            if (currentDatSet != null && currentDatSet.Count > 0)
            {
                var values = currentDatSet["animationValues"];
                Dictionary<string, float> animationValues = new Dictionary<string, float>();
                foreach (var key in values.Keys)
                {
                    if (key == "head_RightTilt" || key == "head_LeftTilt")
                        animationValues.Add(key, Math.Abs(values[key].AsFloat));
                    else
                        animationValues.Add(key, values[key].AsFloat);
                }
                // Get the SkinnedMeshRenderer on Character
                SkinnedMeshRenderer[] smrs = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                foreach (SkinnedMeshRenderer smr in smrs)
                {

                    /********************************************************************************************\
                    * Faceware Motion Logic for 'Victor'
                    * 
                    * An advanced way to setup a character!
                    * 
                    * This is where we use some basic scripting to setup the connection between Faceware Live and
                    * the Victor character. Since Victor is not a 'rig' but is simply an FBX with blendshapes, we
                    * want to ensure certain blendshapes don't mix incorrectly to form bad looking motion. To do
                    * this we use some clever scripting to multiply, ease, and tweak blendshapes so that it can
                    * look good.
                    * 
                    * This is custom to each character so this logic only applies to 'Victor'. This is why driving
                    * a standardized 'rig' inside the engine (as opposed to just blendshapes) is so valuable and
                    * will always look better if you can build it that way.
                    * 
                    * The comments below should help you understand how it works, but essentially we're grabbing
                    * the data from Faceware Live into an array, and then applying that data to particular 
                    * blendshapes on 'Victor'.
                    * 
                    * For more information on how this works or with help applying it to your characters, please
                    * email 'support@facewaretech.com'.
                    \********************************************************************************************/

                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // EYES //////////////////////////////////////////////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////////////////////////////////////


                    // Notes as we get started:
                    // smr = the SkinnedMeshRenderer on Victor
                    // SetBlendShapeWeight = the method to set the weight on a particular blendshape
                    // Mathf.Clamp = Clamps the value between 0 and 100 since Unity allows you to overdrive shapes
                    // values["shape name here"].AsFloat = The Faceware Live value.

                    // Eyes Looking Around (Blendshapes Only, Joints Handled Below)
                    smr.SetBlendShapeWeight(10, Mathf.Clamp(values["eyes_lookDown"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(11, Mathf.Clamp(values["eyes_lookLeft"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(12, Mathf.Clamp(values["eyes_lookRight"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(13, Mathf.Clamp(values["eyes_lookUp"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(14, Mathf.Clamp(values["eyes_lookDown"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(15, Mathf.Clamp(values["eyes_lookLeft"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(16, Mathf.Clamp(values["eyes_lookRight"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(17, Mathf.Clamp(values["eyes_lookUp"].AsFloat * 100.0F, 0, 100));

                    // Eye Looking Around (Joint Rotations)
                    float leftRightDegrees = 30;
                    float upDownDegrees = 25;
                    float eyeLeftRight = (values["eyes_lookRight"].AsFloat + (values["eyes_lookLeft"].AsFloat * -1.0F)) * leftRightDegrees;
                    float eyeUpDown = (values["eyes_lookDown"].AsFloat + (values["eyes_lookUp"].AsFloat * -1.0F)) * upDownDegrees;
                    this.transform.FindDeepChild("def:l_eye").rotation = Quaternion.Euler(90 + eyeUpDown, eyeLeftRight, 0);
                    this.transform.FindDeepChild("def:r_eye").rotation = Quaternion.Euler(90 + eyeUpDown, eyeLeftRight, 0);

                    // Eyes Blink (L & R)
                    // Victor closes the eyes a little when the smile is on, so this logic reduces the
                    // blink a little if the character is also smiling so we don't double up controls.
                    // This is something the 'rig' would typically handle but needs to be addressed here when
                    // direct driving blendshapes only.
                    float blinkModifier = 0.5F;
                    float leftSmile = values["mouth_leftMouth_smile"].AsFloat;
                    float rightSmile = values["mouth_rightMouth_smile"].AsFloat;
                    float leftBlink = values["eyes_leftEye_blink"].AsFloat - ((leftSmile * leftSmile) * blinkModifier);
                    float rightBlink = values["eyes_rightEye_blink"].AsFloat - ((rightSmile * rightSmile) * blinkModifier);
                    smr.SetBlendShapeWeight(20, Mathf.Clamp(leftBlink * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(22, Mathf.Clamp(rightBlink * 100.0F, 0, 100));

                    // Eyes Wide (L & R)
                    smr.SetBlendShapeWeight(29, Mathf.Clamp(values["eyes_leftEye_wide"].AsFloat * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(30, Mathf.Clamp(values["eyes_rightEye_wide"].AsFloat * 100.0F, 0, 100));


                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // EYEBROWS //////////////////////////////////////////////////////////////////////////////////
                    //////////////////////////////////////////////////////////////////////////////////////////////

                    // Brows Down (Mid)
                    // To achieve an appealing look for this particular character, we're reducing the overall
                    // downward movement of the Eyebrows.
                    float browsDownModifier = 0.50F;
	                smr.SetBlendShapeWeight(7, Mathf.Clamp((values["brows_midBrows_down"].AsFloat * 100.0F) * browsDownModifier,0,100));

	                // Brows Down (L & R)
	                smr.SetBlendShapeWeight(1, Mathf.Clamp((values["brows_leftBrow_down"].AsFloat * 100.0F) * browsDownModifier,0,100));
	                smr.SetBlendShapeWeight(2, Mathf.Clamp((values["brows_rightBrow_down"].AsFloat * 100.0F) * browsDownModifier,0,100));

                    // Brows Up (Mid)
	                smr.SetBlendShapeWeight(4, Mathf.Clamp(values["brows_midBrows_up"].AsFloat * 100.0F,0,100));

                    // Brows Up (L & R)
	                // This logic reduces a visible cross-talk between eyes and brows
                    // that is present on this particular character.
	                float leftBrowUp = values["brows_leftBrow_up"].AsFloat;
	                float rightBrowUp = values["brows_rightBrow_up"].AsFloat;
	                float blinkReductionModifier = 0.5F;
	                if (leftBlink > 0.0F && rightBlink > 0.0F) {
		                leftBrowUp -= (leftBlink * blinkReductionModifier);
		                rightBrowUp -= (rightBlink * blinkReductionModifier);
	                }
                    smr.SetBlendShapeWeight(5, Mathf.Clamp(leftBrowUp * 100.0F,0,100));
	                smr.SetBlendShapeWeight(6, Mathf.Clamp(rightBrowUp * 100.0F,0,100));


                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // MOUTH /////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////////

                    // Jaw Open
                    // This logic reduces Jaw ever so slightly if MBP is trying to turn on in order to make
                    // sure that this very important shape reads as best it can.
                    float mouthClosed = values["mouth_phoneme_mbp"].AsFloat;
                    float jawOpen = values["jaw_open"].AsFloat - (mouthClosed * mouthClosed * mouthClosed);
                    smr.SetBlendShapeWeight(64, Mathf.Clamp(jawOpen * 100.0F, 0, 100));
	
                    // Jaw Left & Jaw Right
	                smr.SetBlendShapeWeight(63, Mathf.Clamp((values["jaw_left"].AsFloat * 100.0F), 0, 100));
	                smr.SetBlendShapeWeight(65, Mathf.Clamp((values["jaw_right"].AsFloat * 100.0F), 0, 100));

                    // Smile (L & R)
                    smr.SetBlendShapeWeight(114, Mathf.Clamp((values["mouth_leftMouth_smile"].AsFloat * 100.0F), 0, 100));
	                smr.SetBlendShapeWeight(115, Mathf.Clamp((values["mouth_rightMouth_smile"].AsFloat * 100.0F), 0, 100));

                    // Mouth Up & Mouth Down
                    // Mouth up and down tend to overpower a lot of other shapes on Victor so we're toning them down.
                    float mouthUpModifier = 0.75F;
	                float mouthDownModifier = 0.25F;
	                smr.SetBlendShapeWeight(34, Mathf.Clamp((values["mouth_down"].AsFloat * 100.0F) * mouthDownModifier, 0, 100));
	                smr.SetBlendShapeWeight(35, Mathf.Clamp((values["mouth_up"].AsFloat * 100.0F) * mouthUpModifier, 0, 100));

	                // Mouth Left & Mouth Right
	                smr.SetBlendShapeWeight(85, Mathf.Clamp(values["mouth_left"].AsFloat * 100.0F, 0, 100));
	                smr.SetBlendShapeWeight(89, Mathf.Clamp(values["mouth_right"].AsFloat * 100.0F, 0, 100));

                    // Frown (L & R)
	                smr.SetBlendShapeWeight(47, Mathf.Clamp(values["mouth_rightMouth_frown"].AsFloat * 100.0F, 0, 100));
	                smr.SetBlendShapeWeight(46, Mathf.Clamp(values["mouth_leftMouth_frown"].AsFloat * 100.0F, 0, 100));

                    // Mouth Narrow (L & R)
	                smr.SetBlendShapeWeight(107, Mathf.Clamp(values["mouth_leftMouth_narrow"].AsFloat * 100.0F, 0, 100));
	                smr.SetBlendShapeWeight(108, Mathf.Clamp(values["mouth_rightMouth_narrow"].AsFloat * 100.0F, 0, 100));

                    // Cheek Raises During Smile (SIMULATED SHAPE)
                    // This logic eases in the cheek raise as the Smile increases so it's not a simple linear increase.
                    float cheekRaiseModifier = 0.35F;
                    smr.SetBlendShapeWeight(32, Mathf.Clamp(((cheekRaiseModifier * (leftSmile * leftSmile))) * 100.0F, 0, 100));
                    smr.SetBlendShapeWeight(33, Mathf.Clamp(((cheekRaiseModifier * (rightSmile * rightSmile))) * 100.0F, 0, 100));

                    // Sneer (L & R) (SIMULATED SHAPE)
                    // This logic eases in the sneer as the mid brows come down and the mouth raises.
                    float browWeight = 1.0F;
	                float mouthWeight = 1.0F;
	                float leftBrowDown = values["brows_leftBrow_down"].AsFloat;
	                float rightBrowDown = values["brows_rightBrow_down"].AsFloat;
	                float mouthUp = values["mouth_up"].AsFloat;
                    float leftSneer =  ((browWeight * (leftBrowDown * leftBrowDown)) + (mouthWeight * (mouthUp * mouthUp))) / 2;
                    float rightSneer = ((browWeight * (rightBrowDown * rightBrowDown)) + (mouthWeight * (mouthUp * mouthUp))) /2;
                    smr.SetBlendShapeWeight(117, Mathf.Clamp(leftSneer * 100.0F, 0, 100));
	                smr.SetBlendShapeWeight(118, Mathf.Clamp(rightSneer * 100.0F, 0, 100));

                    // Cheek Suck (L & R) (SIMULATED SHAPE)
                    // This logic eases in the Mouth Suck when the mouth narrows and the mouth moves down.
                    float suckOOWeight = 1.0F;
                    float suckMouthWeight = 0.5F;
                    float suckOO = values["mouth_phoneme_oo"].AsFloat;
                    float suckMouthDown = values["mouth_down"].AsFloat;
                    float leftSuck = ((suckOOWeight * (suckOO * suckOO)) + (suckMouthWeight * (suckMouthDown * suckMouthDown))) / 2;
                    float rightSuck = ((suckOOWeight * (suckOO * suckOO)) + (suckMouthWeight * (suckMouthDown * suckMouthDown))) / 2;
                    smr.SetBlendShapeWeight(125, Mathf.Clamp(leftSuck * 75.0F, 0, 100));
                    smr.SetBlendShapeWeight(126, Mathf.Clamp(rightSuck * 75.0F, 0, 100));

                    // The lowerMouthModifier is used to gradually reduce the OO shape if other shapes are on that
                    // it should not combine with. This is another example of what you'd typically handle in the 'rig'
                    // but is a big problem when directly driving blendshapes.
                    float mouthLeft = values["mouth_left"].AsFloat;
                    float mouthRight = values["mouth_right"].AsFloat;
                    float lowerMouthModifier = 1.0F;

                    // This code reduces the lowerMouthModifier when certain shapes are on so that other shapes can
                    // be turned off gradually.
	                if (mouthLeft > 0.0F) 
	                {
		                lowerMouthModifier -= (mouthLeft * mouthLeft); 
	                }
	                if (mouthRight > 0.0F) 
	                {
		                lowerMouthModifier -= (mouthRight * mouthRight);
	                }

                    // Phoneme OO
                    float oo = values["mouth_phoneme_oo"].AsFloat * lowerMouthModifier;
                    smr.SetBlendShapeWeight(102, Mathf.Clamp(oo * 90.0F, 0, 100));
                    smr.SetBlendShapeWeight(106, Mathf.Clamp(oo * 20.0F, 0, 100));

                    // Phoneme CH
                    smr.SetBlendShapeWeight(48, Mathf.Clamp(values["mouth_phoneme_ch"].AsFloat * 100.0F, 0, 100));

                    // Phoneme MBP
                    smr.SetBlendShapeWeight(86, Mathf.Clamp(values["mouth_phoneme_mbp"].AsFloat * 100.0F, 0, 100));

                    // Mouth Stretch (L & R)
                    smr.SetBlendShapeWeight(72, Mathf.Clamp(((values["mouth_leftMouth_stretch"].AsFloat * 100.0F) * lowerMouthModifier), 0, 100));
	                smr.SetBlendShapeWeight(73, Mathf.Clamp(((values["mouth_rightMouth_stretch"].AsFloat * 100.0F) * lowerMouthModifier), 0, 100));

                    // Lower Lip Down (L & R)
                    // Logic Reduces this as jaw opens wider.
                    float lowerLipDownLeft = values["mouth_lowerLip_left_down"].AsFloat;
                    float lowerLipDownRight = values["mouth_lowerLip_right_down"].AsFloat;
                    lowerLipDownLeft -= (jawOpen * leftSmile) * 0.5F;
                    lowerLipDownRight -= (jawOpen * rightSmile) * 0.5F;
                    smr.SetBlendShapeWeight(83, Mathf.Clamp(((lowerLipDownLeft * 100.0F) * lowerMouthModifier), 0,100));
	                smr.SetBlendShapeWeight(84, Mathf.Clamp(((lowerLipDownRight * 100.0F) * lowerMouthModifier), 0, 100));

                    // Upper Lip Up (L & R)
	                smr.SetBlendShapeWeight(134, Mathf.Clamp(((values["mouth_upperLip_left_up"].AsFloat * 100.0F) * lowerMouthModifier),0,100));
	                smr.SetBlendShapeWeight(135, Mathf.Clamp(((values["mouth_upperLip_right_up"].AsFloat * 100.0F) * lowerMouthModifier), 0, 100));

                    // Head Rotation
                    // These values are in Radians so we need to convert them to Degrees to retarget to 
                    // Victor's head and neck joints. We then round it to 2 decimals.
                    var headRot = GetControl("headRot");

                    double xD = Math.Round(headRot[0] * (180 / Math.PI), 2);
                    double yD = Math.Round(headRot[1] * (180 / Math.PI), 2);
                    double zD = Math.Round(headRot[2] * (180 / Math.PI), 2);

                    float x = (float)xD;
                    float y = (float)yD;
                    float z = (float)zD;

                    this.transform.FindDeepChild("def:sternumRoot").localEulerAngles = new Vector3(13.859F + (x) / 4, -(y) / 4, -(z) / 4);              
                    this.transform.FindDeepChild("def:neck_1").localEulerAngles = new Vector3(-2.18F + (x)/2, -(y) / 2, -(z) / 2);
                    this.transform.FindDeepChild("def:head").localEulerAngles = new Vector3(11.68F + (-(x)/2), 180F + -(y) / 2, z / 2);


                    // Head Position
                    // This one is a little tricky because we need to convert image space to scene space.
                    // This is entirely dependent on the scale of your scene and how you want the character
                    // to move around. 
                    var headPos = GetControl("headPos");
                    float xPos = headPos[0] / 50.0F;
                    float yPos = headPos[1] / 50.0F;
                    float zPos = (headPos[2] / 75.0F) * 0.25F;

                    this.transform.FindDeepChild("def:sternumRoot").localPosition = new Vector3(xPos, yPos + 100, zPos);
                }

                // Flag m_ParseAdditionalData to true to parse secondary controls
                // Secondary Controls that are available but not used in this Demo are
                // Camera World Transform 4x4 Matrix
                // Head World Transform 4x4 Matrix
                // Head Local Transform 4x4 Matrix
                // Tracking Point UV array
                if (m_ParseAdditionalData)
                {
                    string msg = "Secondary Controls:\n";

                    // Camera and Head position in a 4x4 matrix
                    // Streamed as a 1d array, needs to be converted back into a 4x4 matrix to be of use
                    msg += "cameraWorldTransform: " + GetControl("cameraWorldTransform").ToString() + "\n";
                    msg += "headLocalTransform:  " + GetControl("headLocalTransform").ToString() + "\n";
                    msg += "headWorldTransform:  " + GetControl("headWorldTransform").ToString() + "\n";
                    // UVs for x/y coordingates of the tracking points shown on live server
                    // Can be used for showing the points within Unity or for any calculations in UV space
                    msg += "uvs: " + GetControl("uvs").ToString() + "\n";
                    Debug.Log("[Faceware Live]" + msg);
                }
            }
        }
    }
}