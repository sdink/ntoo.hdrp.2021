using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class LiveCharacterSetupEditor : EditorWindow
{
	[Serializable]
	class SerializeData
	{
		public bool selectControlSelectAll;
		public bool sortSelectControl;
		public bool addedControlSelectAll;
		public bool sortAddedControl;
		public List< string > controlSelectionKeys;
		public List< bool > controlSelectionValues;
		public List< string > addedControlKeys;
		public List< bool > addedControlValues;
		public SerializeData()
		{
			controlSelectionKeys = new List<string>();
			controlSelectionValues = new List<bool>();
			addedControlKeys = new List<string>();
			addedControlValues = new List<bool>();
		}
	}

    BaseFacewareExpressions baseLiveExpressions;
	LiveCharacterSetup charSetupData = new LiveCharacterSetup();
	SerializeData serializeData = new SerializeData();

	Dictionary< string, bool > controlSelection = new Dictionary<string, bool>();
	Dictionary< string, bool > addedControls = new Dictionary<string, bool>();

	List< UnityEngine.Object > sceneControlObjectList = new List<UnityEngine.Object>();

    public LiveCharacterSetupFile currentCharSetupFile;

	bool selectControlSelectAll = false;
	bool sortSelectControl = false;
	bool addedControlSelectAll = false;
	bool sortAddedControl = false;

	Vector2 mainScrollPos = Vector2.zero;
	Vector2 expressionScrollPos;
	Vector2 selectedControlScrollPos;
	Vector2 addedControlScrollPos;

	GUIStyle titleStyle;
	GUIStyle headingStyle ;
	GUIStyle scrollviewBgStyle ;
	GUIStyle exprSetBgStyle ;
	GUIStyle exprBgStyle ;

	Texture titleIcon;
	Texture yesIcon;
	Texture noIcon;

	/****************************************************************************************************/
	[MenuItem( "Tools/Faceware/Character Setup" )]
	/****************************************************************************************************/
	public static void ShowWindow()
	{
        EditorWindow charSetupWindow = EditorWindow.GetWindow(typeof(LiveCharacterSetupEditor), false, "Faceware");
        charSetupWindow.position = new Rect(0, 0, 530, 920);
    }

    public bool CreateNewCharacterSetupFile()
    {
        bool success = false;
        //GET FILE PATH FROM USER
        string filePath = EditorUtility.SaveFilePanelInProject("Where do you want to create your new character setup file?", "New Live Character Setup", "asset", "");

        if(filePath != "")
        {
            LiveCharacterSetupFile newCharSetupFile = ScriptableObject.CreateInstance<LiveCharacterSetupFile>();

            currentCharSetupFile = newCharSetupFile;
            currentCharSetupFile.Expressions = new List<Expression>();

            //Deep copy base expressions into new file expressions to prevent base file from being modified
            foreach(Expression exp in baseLiveExpressions.BaseExpressions)
            {
                currentCharSetupFile.Expressions.Add(new Expression(exp.Name, exp.Desc, exp.Attr, false));
            }

            AssetDatabase.CreateAsset(newCharSetupFile, filePath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = newCharSetupFile;

            success = true;
        }

        return success;
    }

	/****************************************************************************************************/
	void Awake()
	{
		string iconPath = Utility.CombinePath( "Assets", "Faceware", "Scripts", "Editor", "Icons" );

		titleIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "LiveClient_DeviceHeader.png" ), typeof( Texture ) );
		yesIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "Ok_16.png" ), typeof( Texture ) );
		noIcon = ( Texture )AssetDatabase.LoadAssetAtPath( Path.Combine( iconPath, "No_16.png" ), typeof( Texture ) );

        string baseFacewareExpressionsPath = Utility.CombinePath("Assets", "Faceware", "Scripts", "Editor", "BaseFacewareExpressions.asset");
        baseLiveExpressions = (BaseFacewareExpressions)AssetDatabase.LoadAssetAtPath(baseFacewareExpressionsPath, typeof(BaseFacewareExpressions));
        charSetupData.Init(new List<Expression>(), new List<string>());
        currentCharSetupFile = null;
    }

	/****************************************************************************************************/
	public void OnEnable()
	{
		charSetupData.ReportError += delegate(string title, string message)
		{
			EditorUtility.DisplayDialog( title, message, "OK" );
		};

		selectControlSelectAll = serializeData.selectControlSelectAll;
		sortSelectControl = serializeData.sortSelectControl;
		addedControlSelectAll = serializeData.addedControlSelectAll;
		sortAddedControl = serializeData.sortAddedControl;
		controlSelection.Clear();
		for( int i = 0; i < serializeData.controlSelectionKeys.Count; i++ )
		{
			controlSelection.Add( serializeData.controlSelectionKeys[i], serializeData.controlSelectionValues[i] );
		}
		addedControls.Clear();
		for( int i = 0; i < serializeData.addedControlKeys.Count; i++ )
		{
			addedControls.Add( serializeData.addedControlKeys[i], serializeData.addedControlValues[i] );
		}
	}

	/****************************************************************************************************/
	public void OnDisable()
	{
		serializeData.selectControlSelectAll = selectControlSelectAll;
		serializeData.sortSelectControl = sortSelectControl;
		serializeData.addedControlSelectAll = addedControlSelectAll;
		serializeData.sortAddedControl = sortAddedControl;
		serializeData.controlSelectionKeys = new List<string>( controlSelection.Keys );
		serializeData.controlSelectionValues = new List<bool>( controlSelection.Values );
		serializeData.addedControlKeys = new List<string>( addedControls.Keys );
		serializeData.addedControlValues = new List<bool>( addedControls.Values );
	}

	/****************************************************************************************************/
	public void OnDestroy()
	{
        if(currentCharSetupFile != null)
        {
            if (EditorUtility.DisplayDialog("Save Character Setup File", "Would you like to save your Character Setup file before exiting?", "Yes", "No"))
            {
                EditorUtility.SetDirty(currentCharSetupFile);
                AssetDatabase.SaveAssets();
            }
        }
	}

	/****************************************************************************************************/
	public void OnGUI()
	{
		//Define styles
		titleStyle = new GUIStyle (EditorStyles.label);
		titleStyle.fontStyle = FontStyle.Bold;
		titleStyle.fontSize = 12;
		titleStyle.alignment = TextAnchor.UpperCenter;
		
		GUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label(titleIcon, GUILayout.MinWidth(1));
            GUILayout.FlexibleSpace();
        }
		GUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Version "  + LiveHelpers.CurrentVersion(), titleStyle );
		
		titleStyle.alignment = TextAnchor.LowerLeft;

		headingStyle = new GUIStyle (EditorStyles.label);
		headingStyle.fontStyle = FontStyle.Bold;
		headingStyle.fontSize = 11;
		headingStyle.alignment = TextAnchor.LowerLeft;

        //Draw UI
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);
		{
            CreateFileGUI();
			CreateControlSetupGUI();
			CreateExpressionSetGUI();
			CreateSaveGUI();
			CreateHelpGUI();
		}
		EditorGUILayout.EndScrollView();
	}

	/****************************************************************************************************/
	private void CreateFileGUI()
	{
		EditorGUILayout.BeginVertical("Box"); //Root layout group
		{
			EditorGUILayout.LabelField("Faceware Live Client for Unity - Character Setup" , titleStyle );

            EditorGUI.BeginChangeCheck();
            currentCharSetupFile = (LiveCharacterSetupFile)EditorGUILayout.ObjectField("Character Setup File: ", currentCharSetupFile, typeof(LiveCharacterSetupFile), false);
            if(EditorGUI.EndChangeCheck())
            {
                if(currentCharSetupFile == null)
                {
                    charSetupData.Init(new List<Expression>(), new List<string>());
                }
                else
                {
                    charSetupData.Init(currentCharSetupFile.Expressions, currentCharSetupFile.Controls);
                    
                    //Check for root object
                    if(currentCharSetupFile.rootObjectName != null)
                    {
                        GameObject rootObject = GameObject.Find(currentCharSetupFile.rootObjectName);
                        if(rootObject != null)
                        {
                            //We have it
                            currentCharSetupFile.rootObject = rootObject;
                            ValidateControls();
                        }
                        else
                        {
                            //Don't have it in scene, prompt user
                            EditorUtility.DisplayDialog("Unable to find character!", "The character " + currentCharSetupFile.rootObjectName + " could not be found in the scene!", "Ok");
                        }
                    }
                }

                controlSelection.Clear();
                addedControls.Clear();
                sceneControlObjectList.Clear();
                InitAddedControls();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true)); //Buttons' control group
			{
				GUILayoutOption buttonWidth = GUILayout.Width( 162 );
                GUILayout.FlexibleSpace();
				if( GUILayout.Button( "New", buttonWidth ) )
				{
                    bool success = CreateNewCharacterSetupFile();
                    if(success)
                    {
                        charSetupData.Init(currentCharSetupFile.Expressions, currentCharSetupFile.Controls);
                        controlSelection.Clear();
                        addedControls.Clear();
                        sceneControlObjectList.Clear();
                    }
                }
                GUILayout.FlexibleSpace();
                if ( GUILayout.Button( "Save", buttonWidth ) )
                {
                    EditorUtility.SetDirty(currentCharSetupFile);
                    AssetDatabase.SaveAssets();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End buttons' control group
            CreateSpace(1);
        }
		EditorGUILayout.EndVertical();
		CreateSpace (1);
	}
	
	/****************************************************************************************************/
	private void CreateControlSetupGUI()
	{
        //Root control group
		EditorGUILayout.BeginVertical("Box");
		{
			EditorGUILayout.LabelField( "Step 1: Control Setup", titleStyle);
            EditorGUILayout.BeginHorizontal(); //Get Selected scene objects control group
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Get Controls From Selected Object", GUILayout.Width(494)))
                {
                    InitSelectControlList();
                    sortSelectControl = false;
                    selectControlSelectAll = false;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            CreateSpace (1);
			EditorGUILayout.BeginHorizontal();
			{
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(); //Selected objects region
				{
					EditorGUILayout.LabelField( "Selected Objects:", headingStyle );
                    EditorGUILayout.BeginHorizontal(); //Add controls button region
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Add Controls", GUILayout.MaxWidth(240)))
                        {
                            AddControls();
                            if(currentCharSetupFile != null)
                                sceneControlObjectList = LiveUnityInterface.GetControls(currentCharSetupFile.rootObject, charSetupData.GetControlList());
                            sortAddedControl = false;
                            addedControlSelectAll = false;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                    bool checkSelectAll;
					bool checkSort;
					EditorGUILayout.BeginHorizontal(); //Toggle controls region for selected objects
					{
						checkSelectAll = GUILayout.Toggle( selectControlSelectAll, "Select All" );
                        GUILayout.FlexibleSpace();
                        checkSort = GUILayout.Toggle( sortSelectControl, "Sort A-Z" );
					}
					EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical("Box");//Selected controls scroll area region
                    {
						selectedControlScrollPos = EditorGUILayout.BeginScrollView( selectedControlScrollPos, false, false, GUILayout.MinHeight(150) );
						PopulateControlScrollView( controlSelection, selectControlSelectAll != checkSelectAll, checkSelectAll, checkSort );
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndVertical();

					sortSelectControl = checkSort;
					selectControlSelectAll = checkSelectAll;
				}
				EditorGUILayout.EndVertical(); //End Selected objects region
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(); //Added controls region
				{
					EditorGUILayout.LabelField( "Added Controls:", headingStyle );
                    EditorGUILayout.BeginHorizontal(); //Remove controls button region
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove Controls", GUILayout.MaxWidth(240)))
                        {
                            RemoveControls();
                            if(currentCharSetupFile != null)
                                sceneControlObjectList = LiveUnityInterface.GetControls(currentCharSetupFile.rootObject, charSetupData.GetControlList());
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

					bool checkSelectAll;
					bool checkSort;
					EditorGUILayout.BeginHorizontal(); //Toggle controls region for added objects
                    {
						checkSelectAll = GUILayout.Toggle( addedControlSelectAll, "Select All" );
                        GUILayout.FlexibleSpace();
                        checkSort = GUILayout.Toggle( sortAddedControl, "Sort A-Z" );
					}
					EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical("Box"); //Added controls scroll area region
                    {
						addedControlScrollPos = EditorGUILayout.BeginScrollView( addedControlScrollPos, false, false, GUILayout.MinHeight(150));
						PopulateControlScrollView( addedControls, addedControlSelectAll != checkSelectAll, checkSelectAll, checkSort );
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndVertical(); //End added controls scroll region

					sortAddedControl = checkSort;
					addedControlSelectAll = checkSelectAll;
				}
				EditorGUILayout.EndVertical(); //End added controls region
                GUILayout.FlexibleSpace();
            }
			EditorGUILayout.EndHorizontal();
            CreateSpace(1);
        }
		EditorGUILayout.EndVertical();
		CreateSpace (1);
	}

	/****************************************************************************************************/
	private void PopulateControlScrollView( Dictionary< string, bool > controls, bool checkAllChanged, bool checkAll, bool sort )
	{
		EditorGUILayout.BeginVertical();
		{
			if( checkAllChanged )
			{
				if( sort )
				{
					SortedList< string, bool > display = new SortedList< string, bool >( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( checkAll, kvp.Key );
					}
				}
				else
				{
					Dictionary<string, bool> display = new Dictionary<string, bool>( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( checkAll, kvp.Key );
					}
				}
			}
			else
			{
				if( sort )
				{
					SortedList< string, bool > display = new SortedList< string, bool >( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( kvp.Value, kvp.Key );
					}
				}
				else
				{
					Dictionary<string, bool> display = new Dictionary<string, bool>( controls );
					foreach( KeyValuePair< string, bool > kvp in display )
					{
						controls[kvp.Key] = GUILayout.Toggle( kvp.Value, kvp.Key );
					}
				}
			}
		}
		EditorGUILayout.EndVertical();
	}

	/****************************************************************************************************/
	private void CreateExpressionSetGUI()
	{
		EditorGUILayout.BeginVertical("Box"); //Root layout group
		{
			EditorGUILayout.LabelField( "Step 2: Expression Set", titleStyle);
            EditorGUILayout.BeginHorizontal("Box"); //Scroll view container region 
			{
                expressionScrollPos = GUILayout.BeginScrollView( expressionScrollPos, false, true, GUILayout.Height( 300 ) ); //Scroll view region
				EditorGUILayout.BeginVertical();
				GUILayoutOption buttonWidth = GUILayout.Width( 95 );
				foreach( KeyValuePair< string, string > kvp in charSetupData.GetExpressionNameAttrList() ) //Create controls within scroll view
				{
					EditorGUILayout.BeginHorizontal(); //Individual control region
					{
						bool hasData = charSetupData.InUse( kvp.Value );
						GUILayout.Label( hasData ? yesIcon : noIcon, GUILayout.Width( 20 ) );
						EditorGUILayout.LabelField( kvp.Key, GUILayout.Width( 205 ) );
                        GUILayout.FlexibleSpace();
                        if ( GUILayout.Button( hasData ? "Update Pose" : "Save Pose", buttonWidth ) )
						{
							bool updateExpression = true;
							if( hasData )
							{
								updateExpression = EditorUtility.DisplayDialog( "Update Pose", "Are you sure you want to overwrite this Pose?", "Yes", "No" );
							}
							if( updateExpression )
							{
								List< string > controls = charSetupData.GetControlList();
								if( controls.Count > 0 )
								{
									charSetupData.SetControlValues( kvp.Value, LiveUnityInterface.GetControlValues(currentCharSetupFile.rootObject, controls) );
									charSetupData.SetInUse( kvp.Value, true );
                                    currentCharSetupFile.Expressions = charSetupData.data.Expressions;
                                    EditorUtility.SetDirty(currentCharSetupFile);
                                    AssetDatabase.SaveAssets();
								}
							}
						}
						if( GUILayout.Button( "Show Saved", buttonWidth ) )
						{
							Undo.RecordObjects( sceneControlObjectList.ToArray(), ( "Set '" + kvp.Key + "' Expression" ) );
							LiveUnityInterface.ApplyControlValues(currentCharSetupFile.rootObject, charSetupData.GetControlValues( kvp.Value ) );
						}
					}
					EditorGUILayout.EndHorizontal(); //End of individual control region
				}
                GUILayout.EndScrollView(); //End scroll view region
                GUILayout.FlexibleSpace();
            }
			EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            CreateSpace(1);
            EditorGUILayout.BeginHorizontal(); //Reset to neutral button region
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset Character to 'Neutral'", GUILayout.Width(496)))
                {
                    if (currentCharSetupFile != null)
                        LiveUnityInterface.ApplyControlValues(currentCharSetupFile.rootObject, charSetupData.GetControlValues("neutral"));
                    else
                        ShowNoSetupFileMsg();
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End neutral button region
        }
        CreateSpace(1);
        EditorGUILayout.EndVertical();
		CreateSpace (1);
	}

	/****************************************************************************************************/
	private void CreateSaveGUI()
	{
		EditorGUILayout.BeginVertical("Box"); //Root layout group
		{
			EditorGUILayout.LabelField( "Step 3: Save Your Character Setup File and Apply it to Character", titleStyle);
            EditorGUILayout.BeginHorizontal(); //Save as button region
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save and Apply to Character...", GUILayout.Width(496)))
                {
                    if(currentCharSetupFile != null)
                    {
                        if (currentCharSetupFile.rootObject != null)
                        {
                            LiveClient liveScript = currentCharSetupFile.rootObject.GetComponent<LiveClient>();

                            if (liveScript == null)
                            {
                                liveScript = currentCharSetupFile.rootObject.AddComponent<LiveClient>();
                                liveScript.ExpressionSetFile = currentCharSetupFile;
                            }
                            else
                            {
                                liveScript.ExpressionSetFile = currentCharSetupFile;
                            }

                            Selection.activeGameObject = currentCharSetupFile.rootObject;

                            EditorUtility.SetDirty(currentCharSetupFile);
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Character Setup Error", "Please click 'Get Controls From Selected Object' with your character selected at least once before attempting to apply!", "Ok");
                        }
                    }
                    else
                    {
                        ShowNoSetupFileMsg();
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal(); //End save as button region
		}
        CreateSpace(1);
        EditorGUILayout.EndVertical();
        CreateSpace(1);
    }
	/****************************************************************************************************/
	private void CreateHelpGUI()
	{
		EditorGUILayout.BeginVertical("Box"); //Root layout group
		{
			EditorGUILayout.LabelField( "Need Help?", titleStyle);
			EditorGUILayout.BeginHorizontal(); //Top two help buttons region
			{
                GUILayout.FlexibleSpace();
                GUILayoutOption buttonWidth = GUILayout.Width( 246 );
				if( GUILayout.Button( "Live Client for Unity - User Guide", buttonWidth ) )
				{
					System.Diagnostics.Process.Start("http://support.facewaretech.com/live-client-for-unity");
				}
				if( GUILayout.Button( "Visit www.facewaretech.com", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://www.facewaretech.com/" );
				}
                GUILayout.FlexibleSpace();
            }
			EditorGUILayout.EndHorizontal(); //End top two help buttons region
			EditorGUILayout.BeginHorizontal(); //30 day trial button region
			{
                GUILayout.FlexibleSpace();
                GUILayoutOption buttonWidth = GUILayout.Width( 496 );
				if( GUILayout.Button( "Get Your 30-Day Free Trial of Faceware Live Server", buttonWidth ) )
				{
					System.Diagnostics.Process.Start( "http://facewaretech.com/products/software/free-trial/" );
				}
                GUILayout.FlexibleSpace();
            }
			EditorGUILayout.EndHorizontal(); //End 30 day trial button region
            CreateSpace(1);
        }
		EditorGUILayout.EndVertical();
	}

	/****************************************************************************************************/
	private void InitSelectControlList()
	{
		List< string > controlList = new List<string>();
		if(Selection.gameObjects.Length == 1)
		{
            if(currentCharSetupFile != null)
            {
                bool validateCurrentControls = false;
                GameObject selectedGameobject = Selection.gameObjects[0];
                if (currentCharSetupFile.rootObject != null)
                {
                    if(selectedGameobject != currentCharSetupFile.rootObject)
                    {
                        //Prompt before continueing with yes or cancel
                        if (!EditorUtility.DisplayDialog("Override associated character?", "This character setup file was made with " + currentCharSetupFile.rootObjectName + ". Do you wish to override this with your current selection?\n\nIf controls associated to " + currentCharSetupFile.rootObjectName + " do not exist on your new selection they will be removed.", "Yes", "Cancel"))
                        {
                            return;
                        }
                        else
                        {
                            validateCurrentControls = true;
                        }
                    }
                }

                currentCharSetupFile.rootObject = selectedGameobject;
                currentCharSetupFile.rootObjectName = currentCharSetupFile.rootObject.name;
                List<GameObject> selectedChildren = new List<GameObject>();
                currentCharSetupFile.rootObject.transform.ReturnAllChildren(selectedChildren);

                foreach (GameObject selected in selectedChildren)
                {
                    List<string> newControls = LiveUnityInterface.GetControls(selected);
                    foreach (string ctrl in newControls)
                    {
                        if (!controlList.Contains(ctrl))
                        {
                            controlList.Add(ctrl);
                        }
                    }
                }

                if (validateCurrentControls)
                {
                    ValidateControls();
                }

                controlSelection.Clear();
                foreach (string control in controlList)
                {
                    controlSelection.Add(control, false);
                }
            }
            else
            {
                ShowNoSetupFileMsg();        
            }
        }
        else
        {
            //Prompt nothing is selected
            EditorUtility.DisplayDialog("Invalid Selection!", "Please select a single root game object and try again.", "Ok");
        }
	}
    /****************************************************************************************************/
    private void ValidateControls()
    {
        List<string> missingControls = LiveUnityInterface.ValidateControls(currentCharSetupFile.rootObject, currentCharSetupFile.Controls);
        string removedControlsString = "";
        string fullRemovedControlsString = "";

        if (missingControls.Count > 0)
        {
            int removedControls = 0;
            //Remove from controls list and expression values
            for (int i = currentCharSetupFile.Controls.Count - 1; i >= 0; i--)
            {
                if (missingControls.Contains(currentCharSetupFile.Controls[i]))
                {
                    foreach (Expression exp in currentCharSetupFile.Expressions)
                    {
                        if (i < exp.Values.Count - 1)
                        {
                            exp.Values.RemoveAt(i);
                        }
                    }

                    if (removedControls < 11)
                        removedControlsString += "\n -" + currentCharSetupFile.Controls[i];
                    else if (removedControls == 12)
                        removedControlsString += "\n...See console for full list of removed controls...";

                    fullRemovedControlsString += "\n -" + currentCharSetupFile.Controls[i];
                    currentCharSetupFile.Controls.RemoveAt(i);

                    removedControls++;
                }
            }

            //Refresh added controls list
            InitAddedControls();
            //Display warnings
            Debug.LogWarning("The following controls were missing and have been removed from the character setup file. However, changes will not be applied until you hit save in character setup. Click this log message to see the list below: \n" + fullRemovedControlsString);
            EditorUtility.DisplayDialog("Missing Controls", "The following controls were missing and have been removed from the character setup file. However, changes will not be applied until you hit save.\n" + removedControlsString, "Ok");
        }

        charSetupData.UpdateData(currentCharSetupFile.Expressions, currentCharSetupFile.Controls);
    }

    /****************************************************************************************************/
    private void InitAddedControls()
	{
		addedControls.Clear();
		foreach( string control in charSetupData.GetControlList() )
		{
			addedControls.Add( control, false );
		}
	}

	/****************************************************************************************************/
	private void AddControls()
	{
        if(currentCharSetupFile != null)
        {
            List<string> newControls = new List<string>();
            foreach (KeyValuePair<string, bool> kvp in controlSelection)
            {
                if (kvp.Value)
                {
                    if (!addedControls.ContainsKey(kvp.Key))
                    {
                        newControls.Add(kvp.Key);
                        addedControls.Add(kvp.Key, false);
                    }
                }
            }
            charSetupData.AddControls(newControls);
            EditorUtility.SetDirty(currentCharSetupFile);
            currentCharSetupFile.Controls = charSetupData.data.Controls;
            AssetDatabase.SaveAssets();
        }
        else
        {
            ShowNoSetupFileMsg();
        }
    }

	/****************************************************************************************************/
	private void RemoveControls()
	{
        if(currentCharSetupFile != null)
        {
            List<string> controls = new List<string>();
            foreach (KeyValuePair<string, bool> kvp in addedControls)
            {
                if (kvp.Value)
                {
                    controls.Add(kvp.Key);
                }
            }
            foreach (string control in controls)
            {
                addedControls.Remove(control);
            }
            EditorUtility.SetDirty(currentCharSetupFile);
            charSetupData.RemoveControls(controls);
            currentCharSetupFile.Controls = charSetupData.data.Controls;
            AssetDatabase.SaveAssets();
        }
        else
        {
            ShowNoSetupFileMsg();
        }
    }

	/****************************************************************************************************/
	private void CreateSpace(int numSpaces)
	{
		for(int i = 0; i < numSpaces; i++)
			EditorGUILayout.Space() ;
	}
    
    private void ShowNoSetupFileMsg()
    {
        EditorUtility.DisplayDialog("No Setup File", "You currently aren't working on a character setup file. Please create a new one or load one.", "Ok");
    }
}


