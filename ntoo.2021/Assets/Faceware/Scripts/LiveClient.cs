using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

public class LiveClient : LiveBase
{
    void Update()
    {
        BaseUpdate();
		
        if (live != null && live.IsConnected() && ExpressionSetFile)
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
                cachedRigValues = character.ConstructRigValues(animationValues, character.GetNeutralControlValues());
                LiveUnityInterface.ApplyControlValues(this.gameObject, cachedRigValues);
            }
        }
    }
}