using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class LiveUnityInterface
{
	/****************************************************************************************************/
	static public List< string > ValidateControls( GameObject rootObj, List< string > controlList )
	{
        List< string > missingCtrlList = new List<string>();
        foreach( string ctrl in controlList )
        {
            string name;
            string attr;
            LiveHelpers.SplitNameAttr( ctrl, out name, out attr );
            Transform objTransfrom = rootObj.transform.FindDeepChild(name);
            
            if (objTransfrom != null)
            {
                GameObject obj = objTransfrom.gameObject;
            
                SkinnedMeshRenderer[] smrs = obj.transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer smr in smrs)
                {
                    if (attr == LiveCharacterSetup.translationSuffix)
                    {
                        continue;
                    }
                    else if (attr == LiveCharacterSetup.rotationSuffix)
                    {
                        continue;
                    }
                    int index = GetBlendShapeIndex(smr, attr);
                    if (index >= 0)
                    {
                        continue;
                    }
                    missingCtrlList.Add(ctrl);
                }
            }
            else
            {
                missingCtrlList.Add(ctrl);
            }
        }
        return missingCtrlList;
    }

	/****************************************************************************************************/
	static public Dictionary< string, Vector4 > GetControlValues(GameObject rootObj, List< string > controlList )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>();
		foreach( string control in controlList )
		{
			string name;
			string attr;
            LiveHelpers.SplitNameAttr( control, out name, out attr );
            Transform objTransfrom = rootObj.transform.FindDeepChild(name);
            if (objTransfrom != null )
			{
                GameObject obj = objTransfrom.gameObject;
                Vector4 value = new Vector4();
				if( attr == LiveCharacterSetup.translationSuffix )
				{
					value.x = obj.transform.localPosition.x;
					value.y = obj.transform.localPosition.y;
					value.z = obj.transform.localPosition.z;
					value.w = float.NaN;
				}
				else if( attr == LiveCharacterSetup.rotationSuffix )
				{
					value.x = obj.transform.localRotation.x;
					value.y = obj.transform.localRotation.y;
					value.z = obj.transform.localRotation.z;
					value.w = obj.transform.localRotation.w;
				}
				else
				{
					SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
					if( skinnedMesh != null )
					{
						int index = GetBlendShapeIndex( skinnedMesh, attr );
						if( index >= 0 )
						{
							value.x = skinnedMesh.GetBlendShapeWeight( index );
							value.y = float.NaN;
							value.z = float.NaN;
							value.w = float.NaN;
						}
					}
				}
				ret.Add( control, value );
			}
		}
		return ret;
	}

    /****************************************************************************************************/
    static public void ApplyControlValues(GameObject rootObj, Dictionary<string, Vector4> values)
    {
        foreach (KeyValuePair<string, Vector4> kvp in values)
        {
            string name;
            string attr;
            LiveHelpers.SplitNameAttr(kvp.Key, out name, out attr);
            Transform objTransfrom = rootObj.transform.FindDeepChild(name);
            if (objTransfrom != null)
            {
                GameObject obj = objTransfrom.gameObject;
                if (attr == LiveCharacterSetup.translationSuffix)
                {
                    Vector3 value = new Vector3(kvp.Value.x, kvp.Value.y, kvp.Value.z);
                    obj.transform.localPosition = value;
                }
                else if (attr == LiveCharacterSetup.rotationSuffix)
                {
                    Quaternion value = new Quaternion(kvp.Value.x, kvp.Value.y, kvp.Value.z, kvp.Value.w);
                    obj.transform.localRotation = value;
                }
                else
                {
                    SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
                    if (skinnedMesh != null)
                    {
                        int index = GetBlendShapeIndex(skinnedMesh, attr);
                        if (index >= 0)
                        {
                            skinnedMesh.SetBlendShapeWeight(index, kvp.Value.x);
                        }
                    }
                }
            }
        }
    }

    /****************************************************************************************************/
    static public List< UnityEngine.Object > GetControls(GameObject rootObj, List< string > controlNameList )
	{
		List< UnityEngine.Object > ret = new List< UnityEngine.Object >();
		foreach( string ctrlName in controlNameList )
		{
			string name;
			string attr;
            LiveHelpers.SplitNameAttr( ctrlName, out name, out attr );
            Transform objTransfrom = rootObj.transform.FindDeepChild(name);
            if (objTransfrom != null )
			{
                GameObject obj = objTransfrom.gameObject;
                if ( attr == LiveCharacterSetup.translationSuffix || attr == LiveCharacterSetup.rotationSuffix )
				{
					ret.Add( obj.transform );
				}
				else
				{
					SkinnedMeshRenderer skinnedMesh = obj.GetComponent<SkinnedMeshRenderer>();
			        if( skinnedMesh != null )
					{
						ret.Add( skinnedMesh );
					}
				}
			}
		}
		return ret;
	}

	/****************************************************************************************************/
	static public List< string > GetControls( GameObject gameObject)
	{
		List< string > ret = new List<string>();
		ret.Add(LiveHelpers.JoinNameAttr( gameObject.name, LiveCharacterSetup.translationSuffix ) );
		ret.Add(LiveHelpers.JoinNameAttr( gameObject.name, LiveCharacterSetup.rotationSuffix ) );
		SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
		if( skinnedMesh != null )
		{
			Mesh mesh = skinnedMesh.sharedMesh;
			if( mesh != null )
			{
				for( int i = 0; i < mesh.blendShapeCount; i++ )
				{
					ret.Add(LiveHelpers.JoinNameAttr( gameObject.name, mesh.GetBlendShapeName( i ) ) );
				}
			}
		}
		return ret;
	}

	/****************************************************************************************************/
	static public int GetBlendShapeIndex( SkinnedMeshRenderer skinnedMesh, string attr )
	{
		Mesh mesh = skinnedMesh.sharedMesh;
		if( mesh != null )
		{
			for( int i = 0; i < mesh.blendShapeCount; i++ )
			{
				if( mesh.GetBlendShapeName( i ) == attr )
				{
					return i;
				}
			}
		}
		return -1;
	}

	/****************************************************************************************************/
	static public Dictionary< string, Vector4 > StringToDictionary( string str )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>();
		string[] strKeyValuePairs = str.Split( LiveHelpers.dictionarySeparator );
		for( int i = 0; i < strKeyValuePairs.Length; i += 2 )
		{
			string key = strKeyValuePairs[i];
			string[] values = strKeyValuePairs[i + 1].Split( new char[]{','} );
			Vector4 value = new Vector4();
			switch( values.Length )
			{
			case 1:
				value.x = Convert.ToSingle( values[0] );
				value.y = Convert.ToSingle( float.NaN );
				value.z = Convert.ToSingle( float.NaN );
				value.w = Convert.ToSingle( float.NaN );
				break;
			case 3:
				value.x = Convert.ToSingle( values[0] );
				value.y = Convert.ToSingle( values[1] );
				value.z = Convert.ToSingle( values[2] );
				value.w = Convert.ToSingle( float.NaN );
				break;
			case 4:
				value.x = Convert.ToSingle( values[0] );
				value.y = Convert.ToSingle( values[1] );
				value.z = Convert.ToSingle( values[2] );
				value.w = Convert.ToSingle( values[3] );
				break;
			}
			ret.Add( key, value );
		}
		return ret;
	}

	/****************************************************************************************************/
	static public string DictionaryToString( Dictionary< string, Vector4 > dic )
	{
		StringBuilder ret = new StringBuilder();
		foreach( KeyValuePair< string, Vector4 > kvp in dic )
		{
			if( float.IsNaN( kvp.Value.y ) )
			{
				ret.Append( LiveHelpers.dictionarySeparator + kvp.Key + LiveHelpers.dictionarySeparator + kvp.Value.x );
			}
			else if( float.IsNaN( kvp.Value.w ) )
			{
				ret.Append( LiveHelpers.dictionarySeparator + kvp.Key + LiveHelpers.dictionarySeparator + kvp.Value.x + "," + kvp.Value.y + "," + kvp.Value.z );
			}
			else
			{
				ret.Append( LiveHelpers.dictionarySeparator + kvp.Key + LiveHelpers.dictionarySeparator + kvp.Value.x + "," + kvp.Value.y + "," + kvp.Value.z + "," + kvp.Value.w );
			}
		}
		return ret.ToString().Substring( 1 );			// removes leading separator
	}

    static public string GetRelativeGameObjectPath(GameObject obj, GameObject rootObj)
    {
        string path = "";

        if (obj == rootObj)
            return path;
        else
            path = obj.name;

        while (obj.transform.parent != null && obj.transform.parent.gameObject != rootObj)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;

            if(obj.transform.parent != null)
                if (obj.transform.parent.gameObject == rootObj)
                    return path;
        }
        return path;
    }
}

