using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Utility
{
	public static string CombinePath( params string[] list )
	{
		string ret = "";
		foreach( string item in list )
		{
			ret = Path.Combine( ret, item );
		}
		return ret;
	}
}

[Serializable]
public class MetaObj
{
	public string Application;
	public string version;

	public MetaObj()
	{
		Application = "";
		version = "";
	}
}

[Serializable]
public class Expression
{
	public string Name;
	public string Desc;
	public string Attr;
	public bool InUse;
	public List< Vector4 > Values;

	public Expression()
	{
		Name = "";
		Desc = "";
		Attr = "";
		InUse = false;
		Values = new List<Vector4>();
	}

    public Expression(string pName, string pDesc, string pAttr, bool pInUse)
    {
        Name = pName;
        Desc = pDesc;
        Attr = pAttr;
        InUse = pInUse;
        Values = new List<Vector4>();
    }
}

[Serializable]
public class Data
{
    public MetaObj Meta;
    public List< string > Controls;
    public List< string > ControlsList;
    public List< Expression > Expressions;
    public Data()
    {
        Meta = null;
        Controls = null;
        ControlsList = null;
        Expressions = null;
    }
}

[Serializable]
public class LiveCharacterSetup
{
	public delegate void ReportErrorHandler( string title, string message );
	public event ReportErrorHandler ReportError;

	static public readonly string translationSuffix = "pos";
	static public readonly string rotationSuffix = "rot";
	static readonly string currentVersion = "1.0";

	public Data data;
	string expressionSetTemplateFilename;

	/****************************************************************************************************/
	public LiveCharacterSetup()
	{
		
	}

	/****************************************************************************************************/
	public void Init(List<Expression> expressions, List<String> controls)
	{
        data = new Data();
        data.Expressions = expressions;
        data.Controls = controls;
	}

    public void UpdateData(List<Expression> expressions, List<String> controls)
    {
        data.Expressions = expressions;
        data.Controls = controls;
    }

	/****************************************************************************************************/
	public void Cleanup()
	{
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > GetNeutralControlValues()
	{
		Dictionary< string, Vector4 > offsets = GetControlValues( "neutral" );
		if( offsets == null || offsets.Count != data.Controls.Count )
		{
			offsets = new Dictionary<string, Vector4>();
			foreach( string control in data.Controls )
			{
				if( control.EndsWith( rotationSuffix ) )
				{
					offsets.Add( control, QuaternionToVector4( Quaternion.identity ) );
				}
				else
				{
					offsets.Add( control, Vector4.zero );
				}
			}
		}
		return offsets;
	}

	/****************************************************************************************************/
	private Expression FindExpression( string attr )
	{
		return data.Expressions.Find( expression => expression.Attr == attr );
	}

	/****************************************************************************************************/
	public Dictionary< string, string > GetExpressionNameAttrList()
	{
		Dictionary< string, string > ret = new Dictionary< string, string >();
		foreach( Expression expression in data.Expressions )
		{
			ret.Add( expression.Name, expression.Attr );
		}
		return ret;
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > GetControlValues( string expressionAttr )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>();
		Expression expression = FindExpression( expressionAttr );
		if( expression != null )
		{
			for( int i = 0; i < data.Controls.Count; i++ )
			{
				ret.Add( data.Controls[i], expression.Values[i] );
			}
		}
		return ret;
	}

	/****************************************************************************************************/
	public bool SetControlValues( string expressionAttr, Dictionary< string, Vector4 > values )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression == null )
		{
			return false;
		}

		foreach( KeyValuePair< string, Vector4 > kvp in values )
		{
			int i = data.Controls.FindIndex( str => str == kvp.Key );
			if( i >= 0 )
			{
				expression.Values[i] = kvp.Value;
			}
		}
		return true;
	}

	/****************************************************************************************************/
	public List< string > GetControlList()
	{
		return data.Controls;
	}

	/****************************************************************************************************/
	public bool InUse( string expressionAttr )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression == null )
		{
			return false;
		}
		
		return expression.InUse;
	}

	/****************************************************************************************************/
	public void SetInUse( string expressionAttr, bool value )
	{
		Expression expression = FindExpression( expressionAttr );
		if( expression != null )
		{
			expression.InUse = value;
		}
	}

	/****************************************************************************************************/
	public void AddControls( List< string > controls )
	{
		Vector4 defaultRotationValue = new Vector4( 0, 0, 0, 1 );
		foreach( string control in controls )
		{
			if( !data.Controls.Contains( control ) )
			{
				data.Controls.Add( control );
				if( control.EndsWith( translationSuffix ) )
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( Vector4.zero );
					}
				}
				else if( control.EndsWith( rotationSuffix ) )
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( defaultRotationValue );
					}
				}
				else
				{
					foreach( Expression expression in data.Expressions )
					{
						expression.Values.Add( Vector4.zero );
					}
				}
			}
		}
	}

	/****************************************************************************************************/
	public void RemoveControls( List< string > controls )
	{
		foreach( string control in controls )
		{
			int index = data.Controls.IndexOf( control );
			if( index >= 0 )
			{
				data.Controls.RemoveAt( index );
				foreach( Expression expression in data.Expressions )
				{
					expression.Values.RemoveAt( index );
				}
			}
		}
	}

	/****************************************************************************************************/
	public Dictionary< string, Vector4 > ConstructRigValues( Dictionary< string, float > expressionValues, Dictionary< string, Vector4 > offsets )
	{
		Dictionary< string, Vector4 > ret = new Dictionary<string, Vector4>( offsets );

		// set ret values
		foreach( KeyValuePair< string, float > expressionValue in expressionValues )
		{
			float t = expressionValue.Value;

			if( InUse( expressionValue.Key ) )
			{
				Dictionary< string, Vector4 > controlValues = GetControlValues( expressionValue.Key );
				if( controlValues.Count > 0 )
				{
					foreach( KeyValuePair< string, Vector4 > controlValue in controlValues )
					{
						if( float.IsNaN( controlValue.Value.y ) )
						{
							Vector4 result = ret[controlValue.Key];
							result.x += controlValue.Value.x * t;
							ret[controlValue.Key] = result;
						}
						else if( float.IsNaN( controlValue.Value.w ) )
						{
							ret[controlValue.Key] = ret[controlValue.Key] + ( controlValue.Value - offsets[controlValue.Key] ) * t;
						}
						else
						{
							Quaternion diff = Vector4ToQuaternion( controlValue.Value ) * Quaternion.Inverse( Vector4ToQuaternion( offsets[controlValue.Key] ) );
							ret[controlValue.Key] = QuaternionToVector4( Quaternion.Slerp( Quaternion.identity, diff, t ) * Vector4ToQuaternion( ret[controlValue.Key] ) );
						}
					}
				}
			}
		}

		return ret;
	}
	
	/****************************************************************************************************/
	private Vector4 QuaternionToVector4( Quaternion value )
	{
		return new Vector4( value.x, value.y, value.z, value.w );
	}

	/****************************************************************************************************/
	private Quaternion Vector4ToQuaternion( Vector4 value )
	{
		return new Quaternion( value.x, value.y, value.z, value.w );
	}
}
