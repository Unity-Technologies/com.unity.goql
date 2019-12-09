using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.GoQL
{
    public class GoQLWindow : EditorWindow
    {
        string query = string.Empty;
        GoQLExecutor goqlMachine;
        List<object> instructions;
        GameObject[] selection;

        [MenuItem("Window/General/GoQL")]
        static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<GoQLWindow>();
            window.Show();
        }

        // Update is called once per frame
        void Update()
        {
            instructions = Parser.Parse(query);
            if (goqlMachine == null)
            {
                goqlMachine = new GoQLExecutor();
            }
            selection = goqlMachine.Execute(instructions);
            // if (goqlMachine.selection != null)
            //     Selection.objects = goqlMachine.selection.ToArray();
        }

        void OnGUI()
        {
            query = EditorGUILayout.TextField(query);
            if (goqlMachine.Error != string.Empty)
            {
                EditorGUILayout.HelpBox(goqlMachine.Error, MessageType.Error);
            }
            if (instructions != null)
                foreach (var i in instructions)
                {
                    // GUILayout.Label(i.ToString());
                }
            GUILayout.BeginVertical("box");
            if (selection != null)
            {
                foreach (var i in selection)
                    if (i != null) GUILayout.Label(i.name);
                Selection.objects = selection;
                selection = null;
            }

            GUILayout.EndVertical();
        }
    }
}