using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorFlowWindow : EditorWindow
{
    [MenuItem("Window/EditorFlow Window")]
    static void Init()
    {
        EditorFlowWindow window = GetWindow<EditorFlowWindow>();
        window.Show();
    }

    Action currentMainSelection;

    Action currentScriptableObjectSelection;
    Action currentScriptSelection;
    Action currentScriptableObjectDefinitionSelection;
    Action currentScriptableObjectInstanceSelection;

    string[] scriptableObjectAbstractDefinitionNames;
    string[] scriptableObjectClassDefinitionNames;

    string[] scriptableObjectAbstractInstanceNames;
    string[] scriptableObjectClassInstanceNames;

    string[] abstractClassNames;
    string[] classNames;
    string[] interfaceNames;
    string[] monoBehaviourNames;
    string[] scriptableObjectTypeNames;
    string[] staticNames;
    string[] structNames;

    int currentAssemblyIndex;
    int previousAssemblyIndex;

    Action<string> UserChangesSelectedAssembly;

    Assembly currentlySelectedAssembly;

    Assembly[] projectAssemblies;
    string[] projectAssemblyNames;

    const string scriptableObjectsFolderRoute = "Assets/ScriptableObjects/";

    const string scriptableObjectsInstancesRoute = scriptableObjectsFolderRoute + "Instances/";
    const string scriptableObjectsAbstractInstancesRoute = scriptableObjectsInstancesRoute + "Abstracts/";
    const string scriptableObjectsBaseInstancesRoute = scriptableObjectsInstancesRoute + "ScriptableObjects/";

    const string scriptableObjectsDefinitionsRoute = scriptableObjectsFolderRoute + "Definitions/";
    const string scriptableObjectsDefinitionsAbstractsRoute = scriptableObjectsDefinitionsRoute + "Abstracts/";
    const string scriptableObjectsDefinitionsClassesRoute = scriptableObjectsDefinitionsRoute + "Classes/";

    Color defaultBackgroundColor;

    Color scriptableObjectsColor;
    Color scriptableObjectsSelectedColor = Color.red;
    bool scriptableObjectsIsSelected = false;

    Color scriptsColor;
    Color scriptsSelectedColor = Color.blue;
    bool scriptsIsSelected = false;

    Color templatesColor;
    Color templatesSelectedColor = Color.green;
    //bool templatesIsSelected = false;

    Color scriptableObjectsDefinitionsColor;
    Color scriptableObjectsDefinitionsSelectedColor = Color.red;
    //bool scriptableObjectsDefinitionsIsSelected = false;

    Color scriptableObjectsInstancesColor;
    Color scriptableObjectsInstancesSelectedColor = Color.red;
    //bool scriptableObjectsInstancesIsSelected = false;

    bool scriptableObjectsDefinitionsAbstractsIsSelected = false;
    bool scriptableObjectsDefinitionsClassIsSelected = false;
    bool scriptableObjectsInstancesAbstractsIsSelected = false;
    bool scriptableObjectsInstancesScriptableObjectsIsSelected = false;

    bool scriptsAbstractsIsSelected = false;
    bool scriptsClassIsSelected = false;
    bool scriptsInterfaceIsSelected = false;
    bool scriptsMonoBehaviourIsSelected = false;
    bool scriptableObjectIsSelected = false;
    bool scriptsStaticIsSelected = false;
    bool scriptsStructIsSelected = false;

    string inputFieldValue;

    //bool assemblySelectorIsOpen = false;

    string currentlySelectedOutputRoute;
    string newDirectoryInputValue;

    void OnEnable()
    {
        scriptableObjectsColor = new Color(1f, 0.7f, 0.7f, 1f);
        scriptsColor = new Color(0.7f, 0.7f, 1f, 1f);
        templatesColor = new Color(0.7f, 1f, 0.7f, 1f);
        scriptableObjectsDefinitionsColor = new Color(1f, 0.7f, 0.7f, 1f);
        scriptableObjectsInstancesColor = new Color(1f, 0.7f, 0.7f, 1f);

        try
        {
            SetActiveAssembly("Assembly-CSharp");
            projectAssemblies = new Assembly[] { currentlySelectedAssembly };
            projectAssemblyNames = new string[] { currentlySelectedAssembly.GetName().Name };
        }
        catch { }

        UserChangesSelectedAssembly = SetActiveAssembly;

        currentScriptableObjectSelection = ScriptableObjectsDefinitionsOutput;

        defaultBackgroundColor = GUI.backgroundColor;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = scriptableObjectsIsSelected ? scriptableObjectsSelectedColor : scriptableObjectsColor;
        if (GUILayout.Button("ScriptableObjects")) { currentMainSelection = ScriptableObjectsOutput; ToggleMainSelectionActiveColor("ScriptableObjects"); }
        GUI.backgroundColor = scriptsIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Scripts")) { currentMainSelection = ScriptsOutput; ToggleMainSelectionActiveColor("Scripts"); }
        //GUI.backgroundColor = templatesIsSelected ? templatesSelectedColor : templatesColor;
        //if (GUILayout.Button("Templates")) { currentMainSelection = TemplatesOutput; ToggleMainSelectionActiveColor("Templates"); }
        EditorGUILayout.EndHorizontal();
        
        GUI.backgroundColor = defaultBackgroundColor;
        currentMainSelection?.Invoke();

        if(Selection.activeObject != default)
        {
            currentlySelectedOutputRoute = AssetDatabase.GetAssetPath(Selection.activeObject);
        }
        EditorGUILayout.LabelField(currentlySelectedOutputRoute);

        EditorGUILayout.BeginHorizontal();
        newDirectoryInputValue = EditorGUILayout.TextField("New Directory", newDirectoryInputValue);
        if (GUILayout.Button("Add New Directory"))
        {
            string routePath = currentlySelectedOutputRoute + "/" + newDirectoryInputValue;
            Directory.CreateDirectory(routePath);

            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndHorizontal();

        Repaint();

        //assemblySelectorIsOpen = EditorGUILayout.Foldout(assemblySelectorIsOpen, currentlySelectedAssembly.GetName().Name);
        //if(assemblySelectorIsOpen)
        //{
        //    AssemblySelector();
        //}

        EditorGUILayout.EndVertical();       
    }

    void ToggleMainSelectionActiveColor(string name)
    {
        switch (name)
        {
            case "ScriptableObjects":
                scriptableObjectsIsSelected = true;
                scriptsIsSelected = false;
                //templatesIsSelected = false;
                //currentlySelectedOutputRoute = scriptableObjectsFolderRoute;
                break;
            case "Scripts":
                scriptableObjectsIsSelected = false;
                scriptsIsSelected = true;
                //templatesIsSelected = false;
                //currentlySelectedOutputRoute = scriptsFolderRoute;
                break;
            case "Templates":
                scriptableObjectsIsSelected = false;
                scriptsIsSelected = false;
                //templatesIsSelected = true;
                //currentlySelectedOutputRoute = scriptableObjectsFolderRoute;
                break;
        }
    }

    string scriptableObjectsInputValue;

    void ScriptableObjectsOutput()
    {
        EditorGUILayout.BeginHorizontal();
        //GUI.backgroundColor = scriptableObjectsDefinitionsIsSelected ? scriptableObjectsDefinitionsSelectedColor : scriptableObjectsDefinitionsColor;
        //if (GUILayout.Button("Definitions")) { currentScriptableObjectSelection = ScriptableObjectsDefinitionsOutput; ToggleScriptableObjectsActiveColor("Definitions"); }
        //GUI.backgroundColor = scriptableObjectsInstancesIsSelected ? scriptableObjectsInstancesSelectedColor : scriptableObjectsInstancesColor;
        //if (GUILayout.Button("Instances")) { currentScriptableObjectSelection = ScriptableObjectsInstancesOutput; ToggleScriptableObjectsActiveColor("Instances");}
        EditorGUILayout.EndHorizontal();

        if(currentlySelectedAssembly == null || currentlySelectedAssembly == default)
        {
            EditorGUILayout.LabelField("No Assembly found.");
        }
        else
        {
            ScriptableObjectClassDefinitionsOutput();
        }


        EditorGUILayout.BeginHorizontal();
        scriptableObjectsInputValue = EditorGUILayout.TextField(scriptableObjectsInputValue);
        if (GUILayout.Button("+"))
        {
            string[] nameSegments = new string[0];

            if (scriptableObjectsInputValue.Contains("+"))
            {
                nameSegments = scriptableObjectsInputValue.Split('+');
            }

            if (nameSegments.Length > 0)
            {
                for (int i = 0; i < nameSegments.Length; i++)
                {
                    string buildRoute = currentlySelectedOutputRoute + "/" + nameSegments[i] + ".asset";
                    ScriptableObject newSO = CreateInstance(currentScriptableObjectSelectionName);
                    AssetDatabase.CreateAsset(newSO, buildRoute);
                }
            }
            else
            {
                string buildRoute = currentlySelectedOutputRoute + "/" + scriptableObjectsInputValue + ".asset";
                ScriptableObject newSO = CreateInstance(currentScriptableObjectSelectionName);
                AssetDatabase.CreateAsset(newSO, buildRoute);
            }
            AssetDatabase.Refresh();
        }
        EditorGUILayout.EndHorizontal();
    }

    void ToggleScriptableObjectsActiveColor(string name)
    {
        switch (name)
        {
            case "Definitions":
                //scriptableObjectsDefinitionsIsSelected = true;
                //scriptableObjectsInstancesIsSelected = false;
                break;
            case "Instances":
                //scriptableObjectsDefinitionsIsSelected = false;
                //scriptableObjectsInstancesIsSelected = true;
                break;            
        }
    }

    string scriptsOutputInputValue;    
    string selectedScriptType;

    bool leftControlPressedOnScriptField = false;
    bool enterPressedOnScriptField = false;

    void ScriptsOutput()
    {
        GUI.backgroundColor = scriptsColor;
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = scriptsAbstractsIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Abstract")) { currentScriptSelection = ScriptsAbstractsOutput; ToggleScriptsSelectionActiveColor("Abstract");}
        GUI.backgroundColor = scriptsClassIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Class")) { currentScriptSelection = ScriptsClassesOutput; ToggleScriptsSelectionActiveColor("Class");}
        GUI.backgroundColor = scriptsInterfaceIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Interface")) { currentScriptSelection = ScriptsInterfaceOutput; ToggleScriptsSelectionActiveColor("Interface");}
        GUI.backgroundColor = scriptsMonoBehaviourIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("MonoBehaviour")) { currentScriptSelection = ScriptsMonoBehaviourOutput; ToggleScriptsSelectionActiveColor("MonoBehaviour");}
        GUI.backgroundColor = scriptableObjectIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("ScriptableObject")) { currentScriptSelection = ScriptsScriptableObjectOutput; ToggleScriptsSelectionActiveColor("ScriptableObject"); }
        GUI.backgroundColor = scriptsStaticIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Static")) { currentScriptSelection = ScriptsStaticsOutput; ToggleScriptsSelectionActiveColor("Static");}
        GUI.backgroundColor = scriptsStructIsSelected ? scriptsSelectedColor : scriptsColor;
        if (GUILayout.Button("Struct")) { currentScriptSelection = ScriptsStructsOutput; ToggleScriptsSelectionActiveColor("Struct");}
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = defaultBackgroundColor;
        //currentScriptSelection?.Invoke();

        EditorGUILayout.BeginHorizontal();
        scriptsOutputInputValue = EditorGUILayout.TextField(scriptsOutputInputValue);

        if (Event.current.type == EventType.KeyDown)
        {
            if(Event.current.keyCode == KeyCode.LeftControl)
            {
                leftControlPressedOnScriptField = true;
            }            
        }
        if(Event.current.type == EventType.KeyUp)
        {            
            if(Event.current.keyCode == KeyCode.LeftControl)
            {                
                leftControlPressedOnScriptField = false;
            }
            if (Event.current.keyCode == KeyCode.Return)
            {                
                enterPressedOnScriptField = true;
            }
        }

        if (enterPressedOnScriptField == true || GUILayout.Button("+"))
        {
            enterPressedOnScriptField = false;

            string[] paths;
            string[] inputSegments;

            if (scriptsOutputInputValue.Contains(" + "))
            {
                inputSegments = scriptsOutputInputValue.Split(" + ");
            }
            else
            {
                inputSegments = new string[] { scriptsOutputInputValue };
            }
            paths = new string[inputSegments.Length];
            for (int i = 0; i < inputSegments.Length; i++)
            {
                string scriptName;
                if (inputSegments[i].Contains(" : ")) { scriptName = inputSegments[i].Split(" : ")[0]; }
                else { scriptName = inputSegments[i]; }

                paths[i] = currentlySelectedOutputRoute + "/" + scriptName + ".cs";
                string[] newScript = new string[0];

                switch (selectedScriptType)
                {
                    case "abstract":
                        newScript = new string[]
                        {
                            "public abstract class " + inputSegments[i],
                            "{",
                            "}"
                        };
                        break;
                    case "class":                        
                        newScript = new string[]
                        {
                            "public class " + inputSegments[i],
                            "{",
                            "}"
                        };
                        break;
                    case "interface":
                        newScript = new string[]
                        {
                            "public interface " + inputSegments[i],
                            "{",
                            "}"
                        };
                        break;
                    case "MonoBehaviour":
                        newScript = new string[]
                        {
                            "using UnityEngine;",
                            "",
                            "public class " + inputSegments[i] + " : MonoBehaviour",
                            "{",
                            "}"
                        };
                        break;
                    case "ScriptableObject":
                        newScript = new string[]
                        {
                            "using UnityEngine;",
                            "",
                            "public class " + inputSegments[i] + " : ScriptableObject",
                            "{",
                            "}"
                        };
                        break;
                    case "static":
                        newScript = new string[]
                        {
                            "public static class " + inputSegments[i],
                            "{",
                            "}"
                        };
                        break;
                    case "struct":
                        newScript = new string[]
                        {
                            "public struct " + inputSegments[i],
                            "{",
                            "}"
                        };
                        break;
                }

                File.WriteAllLines(paths[i], newScript);
            }

            AssetDatabase.Refresh();

            if (leftControlPressedOnScriptField)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    MonoScript scriptObject = (MonoScript)AssetDatabase.LoadAssetAtPath(paths[i], typeof(MonoScript));
                    if (scriptObject != null)
                    {
                        AssetDatabase.OpenAsset(scriptObject);
                    }
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void ToggleScriptsSelectionActiveColor(string name)
    {
        switch (name)
        {
            case "Abstract":
                scriptsAbstractsIsSelected = true;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = false;
                selectedScriptType = "abstract";                
                break;
            case "Class":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = true;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = false;
                selectedScriptType = "class";                
                break;
            case "Interface":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = true;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = false;
                selectedScriptType = "interface";                
                break;
            case "MonoBehaviour":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = true;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = false;
                selectedScriptType = "MonoBehaviour";                
                break;
            case "ScriptableObject":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = true;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = false;
                selectedScriptType = "ScriptableObject";                
                break;
            case "Static":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = true;
                scriptsStructIsSelected = false;
                selectedScriptType = "static";                
                break;
            case "Struct":
                scriptsAbstractsIsSelected = false;
                scriptsClassIsSelected = false;
                scriptsInterfaceIsSelected = false;
                scriptsMonoBehaviourIsSelected = false;
                scriptableObjectIsSelected = false;
                scriptsStaticIsSelected = false;
                scriptsStructIsSelected = true;
                selectedScriptType = "struct";                
                break;
        }
    }

    string templatesTextArea;
    int selectedTemplateIndex;
    string[] templateOptionNames;
    string[] templateValues;
    string templatesInputValue;

    void TemplatesOutput()
    {
        templatesTextArea = EditorGUILayout.TextArea(templatesTextArea);

        if(templateOptionNames.Length > 0)
        {
            selectedTemplateIndex = EditorGUILayout.Popup("Active template", selectedTemplateIndex, templateOptionNames);
        }
        else
        {
            EditorGUILayout.LabelField("No templates yet generated");
        }

        EditorGUILayout.BeginHorizontal();        
        templatesInputValue = EditorGUILayout.TextField(templatesInputValue);
        if (GUILayout.Button("+"))
        {
            if(templateOptionNames?.Contains(templatesInputValue) == false)
            {
                int newLength = templateOptionNames.Length + 1;
                int lastIndex = templateOptionNames.Length;

                string[] newTemplateOptionNames = new string[newLength];
                string[] newTemplateValues = new string[newLength];

                newTemplateOptionNames[lastIndex] = templatesInputValue;
                newTemplateValues[lastIndex] = templatesTextArea;

                templateOptionNames = newTemplateOptionNames;
                templateValues = newTemplateValues;
            }
            else
            {
                Debug.Log("Cannot add template of the same name.");
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void ScriptableObjectsDefinitionsOutput()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = scriptableObjectsDefinitionsAbstractsIsSelected ? scriptableObjectsDefinitionsSelectedColor : scriptableObjectsDefinitionsColor;
        if (GUILayout.Button("Abstracts")) { currentScriptableObjectDefinitionSelection = ScriptableObjectAbstractDefinitionsOutput; ToggleScriptableObjectsDefinitionsActiveColor("Abstract"); }
        GUI.backgroundColor = scriptableObjectsDefinitionsClassIsSelected ? scriptableObjectsDefinitionsSelectedColor : scriptableObjectsDefinitionsColor;
        if (GUILayout.Button("Classes")) { currentScriptableObjectDefinitionSelection = ScriptableObjectClassDefinitionsOutput; ToggleScriptableObjectsDefinitionsActiveColor("Class"); }
        EditorGUILayout.EndHorizontal();

        currentScriptableObjectDefinitionSelection?.Invoke();
    }
    void ScriptableObjectsInstancesOutput()
    {
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = scriptableObjectsInstancesAbstractsIsSelected ? scriptableObjectsDefinitionsSelectedColor : scriptableObjectsDefinitionsColor;
        if (GUILayout.Button("Abstracts")) { currentScriptableObjectInstanceSelection = ScriptableObjectAbstractInstancesOutput; ToggleScriptableObjectsInstancesActiveColor("Abstracts"); }
        GUI.backgroundColor = scriptableObjectsInstancesScriptableObjectsIsSelected ? scriptableObjectsDefinitionsSelectedColor : scriptableObjectsDefinitionsColor;
        if (GUILayout.Button("ScriptableObjects")) { currentScriptableObjectInstanceSelection = ScriptableObjectClassInstancesOutput; ToggleScriptableObjectsInstancesActiveColor("ScriptableObjects");}
        EditorGUILayout.EndHorizontal();

        currentScriptableObjectInstanceSelection?.Invoke();
    }

    void ToggleScriptableObjectsDefinitionsActiveColor(string name)
    {
        switch (name)
        {
            case "Abstract":
                scriptableObjectsDefinitionsAbstractsIsSelected = true;
                scriptableObjectsDefinitionsClassIsSelected = false;
                break;
            case "Class":
                scriptableObjectsDefinitionsAbstractsIsSelected = false;
                scriptableObjectsDefinitionsClassIsSelected = true;
                break;
        }
    }
    void ToggleScriptableObjectsInstancesActiveColor(string name)
    {
        switch (name)
        {
            case "Abstracts":
                scriptableObjectsInstancesAbstractsIsSelected = true;
                scriptableObjectsInstancesScriptableObjectsIsSelected = false;
                break;
            case "ScriptableObjects":
                scriptableObjectsInstancesAbstractsIsSelected = false;
                scriptableObjectsInstancesScriptableObjectsIsSelected = true;
                break;
        }
    }

    string currentScriptableObjectSelectionName;

    void ScriptableObjectAbstractDefinitionsOutput()
    {
        if(scriptableObjectAbstractDefinitionNames.Length > 0)
        {
            GUI.backgroundColor = defaultBackgroundColor;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < scriptableObjectAbstractDefinitionNames.Length; i++)
            {
                if (GUILayout.Button(scriptableObjectAbstractDefinitionNames[i]))
                {
                    currentScriptableObjectSelectionName = scriptableObjectAbstractDefinitionNames[i];
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No abstract ScriptableObject definitions in Assembly");
        }
    }
    void ScriptableObjectClassDefinitionsOutput()
    {
        if (scriptableObjectClassDefinitionNames.Length > 0)
        {
            GUI.backgroundColor = defaultBackgroundColor;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < scriptableObjectClassDefinitionNames.Length; i++)
            {
                if (GUILayout.Button(scriptableObjectClassDefinitionNames[i]))
                {
                    currentScriptableObjectSelectionName = scriptableObjectClassDefinitionNames[i];
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No ScriptableObject definitions in Assembly");
        }
    }

    void ScriptableObjectAbstractInstancesOutput()
    {
        if (scriptableObjectAbstractInstanceNames.Length > 0)
        {
            GUI.backgroundColor = defaultBackgroundColor;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < scriptableObjectAbstractInstanceNames.Length; i++)
            {
                if (GUILayout.Button(scriptableObjectAbstractInstanceNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No abstract ScriptableObject definitions in Assembly");
        }
    }
    void ScriptableObjectClassInstancesOutput()
    {
        if (scriptableObjectClassInstanceNames.Length > 0)
        {
            GUI.backgroundColor = defaultBackgroundColor;
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < scriptableObjectClassInstanceNames.Length; i++)
            {
                if (GUILayout.Button(scriptableObjectClassInstanceNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No ScriptableObject definitions in Assembly");
        }
    }

    void ScriptsAbstractsOutput()
    {
        if (abstractClassNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < abstractClassNames.Length; i++)
            {
                if (GUILayout.Button(abstractClassNames[i]))
                {
                    
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No abstract class types in Assembly.");
        }
    }
    void ScriptsClassesOutput()
    {
        if(classNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < classNames.Length; i++)
            {
                if (GUILayout.Button(classNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No class types in Assembly.");
        }
    }
    void ScriptsInterfaceOutput()
    {
        if (interfaceNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < interfaceNames.Length; i++)
            {
                if (GUILayout.Button(interfaceNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No interface types in Assembly.");
        }
    }
    void ScriptsMonoBehaviourOutput()
    {
        if (monoBehaviourNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < monoBehaviourNames.Length; i++)
            {
                if (GUILayout.Button(monoBehaviourNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No MonoBehaviour types in Assembly.");
        }
    }
    void ScriptsScriptableObjectOutput()
    {
        if (scriptableObjectTypeNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < scriptableObjectTypeNames.Length; i++)
            {
                if (GUILayout.Button(scriptableObjectTypeNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No ScriptableObject types in Assembly.");
        }
    }
    void ScriptsStaticsOutput()
    {
        if(staticNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < staticNames.Length; i++)
            {
                if (GUILayout.Button(staticNames[i]))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No static types in Assembly.");
        }
    }
    void ScriptsStructsOutput()
    {
        if(structNames.Length > 0)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < structNames.Length; i++)
            {
                if (GUILayout.Button(structNames[i]))
                {
                
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("No struct types in Assembly.");
        }
    }



    void SetActiveAssembly(string name)
    {
        currentlySelectedAssembly = Assembly.Load(new AssemblyName(name));
        Type[] currentAssemblyTypes = currentlySelectedAssembly.GetTypes();

        //scriptableObjectAbstractDefinitionNames = currentAssemblyTypes.Where(t => t.IsAbstract && t.BaseType == typeof(ScriptableObject)).Select(t => t.Name).ToArray();
        scriptableObjectClassDefinitionNames = currentAssemblyTypes.Where(t => t.IsAbstract == false && typeof(ScriptableObject).IsAssignableFrom(t)).Select(t => t.Name).ToArray();

        //scriptableObjectAbstractInstanceNames = GatherScriptableObjectNamesAtRoute(scriptableObjectsAbstractInstancesRoute);
        //scriptableObjectClassInstanceNames = GatherScriptableObjectNamesAtRoute(scriptableObjectsBaseInstancesRoute);

        abstractClassNames = currentAssemblyTypes.Where(t => t.IsAbstract && t.IsClass && t.IsSealed == false && typeof(ScriptableObject).IsAssignableFrom(t) == false).Select(t => t.Name).ToArray();
        classNames = currentAssemblyTypes.Where(t => t.IsAbstract == false && t.IsClass && typeof(ScriptableObject).IsAssignableFrom(t) == false && t.BaseType != typeof(MonoBehaviour)).Select(t => t.Name).ToArray();
        interfaceNames = currentAssemblyTypes.Where(t => t.IsInterface).Select(t => t.Name).ToArray();
        monoBehaviourNames = currentAssemblyTypes.Where(t => t.IsClass && t.BaseType == typeof(MonoBehaviour)).Select(t => t.Name).ToArray();
        scriptableObjectTypeNames = currentAssemblyTypes.Where(t => typeof(ScriptableObject).IsAssignableFrom(t)).Select(t => t.Name).ToArray();
        staticNames = currentAssemblyTypes.Where(t => t.IsAbstract && t.IsSealed).Select(t => t.Name).ToArray();
        structNames = currentAssemblyTypes.Where(t => t.IsValueType).Select(t => t.Name).ToArray();       
    }

    string[] GatherScriptableObjectNamesAtRoute(string route)
    {
        string[] fileResults = Directory.GetFiles(route);
        List<string> nonMetaResults = new List<string>(fileResults);

        for (int i = 0; i < nonMetaResults.Count; i++)
        {
            if (nonMetaResults[i].Contains(".meta"))
            {
                nonMetaResults.RemoveAt(i);
            }
        }

        string[] names = nonMetaResults.ToArray();

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = names[i].Remove(0, route.Length);
            names[i] = names[i].Replace(".asset", "");
        }

        return names;
    }

    //bool mainAssemblyHasScripts = false;
    //bool editorAssemblyHasScripts = false;


    //void AssemblySelector()
    //{        
    //    if(Assembly.Load(new AssemblyName("Assembly-CSharp")) != null){
    //        Assembly mainAssembly = Assembly.Load(new AssemblyName("Assembly-CSharp"));
    //        mainAssemblyHasScripts = true;
    //    }
    //    else { mainAssemblyHasScripts = false; }

    //    if (Assembly.Load(new AssemblyName("Assembly-CSharp-Editor")) != null)
    //    {
    //        Assembly editorAssembly = Assembly.Load(new AssemblyName("Assembly-CSharp-Editor"));
    //        editorAssemblyHasScripts = true;
    //    }
    //    else { editorAssemblyHasScripts = false; }

    //    string[] userDefinedAssembliesGUIDs = AssetDatabase.FindAssets("t: AssemblyDefinition");
    //    Assembly[] userDefinedAssemblies = new Assembly[userDefinedAssembliesGUIDs.Length];

    //    for (int i = 0; i < userDefinedAssembliesGUIDs.Length; i++)
    //    {
    //        string currentAssetPath = AssetDatabase.GUIDToAssetPath(userDefinedAssembliesGUIDs[i]);
    //        userDefinedAssemblies[i] = Assembly.LoadFile(currentAssetPath);
    //    }
    //}
}