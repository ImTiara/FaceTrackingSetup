using System;
using UnityEditor;
using UnityEngine;

namespace ImTiara.FaceTrackingSetup
{
    public sealed class StringListSearchProvider : EditorWindow
    {
        public static StringListSearchProvider CurrentInstance { get; private set; }

        public static readonly float m_WindowWidth = 400;
        public static readonly float m_WindowHeight = 400;

        public static string[] currentItems;
        public static Action<string, int, int[]> currentAction;
        public static int selectedIndex;
        public static int[] indexes;

        private static Vector2 scrollPos;

        private string searchString = "";
        public static string[] searchKeywords;

        public static void DrawSelectButton(string title, string[] items, Action<string, int, int[]> onSelect, int selectedIndex = 0, params int[] indexes)
        {
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(title);
                if (GUILayout.Button(items[selectedIndex], EditorStyles.popup, GUILayout.MaxWidth(200))) Show(title, items, onSelect, selectedIndex, indexes);
                GUILayout.EndHorizontal();
            }
            catch (Exception e) { Debug.LogError("StringListSearchProvider ERROR: " + e); }
        }

        private static void Show(string title, string[] items, Action<string, int, int[]> onSelect, int selectedIndex, params int[] indexes)
        {
            Hide();

            CurrentInstance = (StringListSearchProvider)GetWindow(typeof(StringListSearchProvider), true, title);

            CurrentInstance.minSize = new Vector2(m_WindowWidth, m_WindowHeight - 200);
            CurrentInstance.maxSize = new Vector2(m_WindowWidth, m_WindowHeight + 500);

            currentItems = items;
            currentAction = onSelect;
            StringListSearchProvider.selectedIndex = selectedIndex;
            StringListSearchProvider.indexes = indexes;

            CurrentInstance.Show();

            Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            CurrentInstance.position = new Rect(mousePos, new Vector2(m_WindowWidth, m_WindowHeight));

        }

        private static void Hide()
        {
            GetWindow(typeof(StringListSearchProvider)).Close();

            if (CurrentInstance == null) return;

            currentItems = null;
            currentAction = null;
            selectedIndex = 0;
            indexes = null;

            CurrentInstance = null;
        }

        public void OnGUI()
        {
            try
            {
                if (currentItems == null || currentAction == null)
                {
                    Hide();
                    return;
                }

                scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(position.width), GUILayout.Height(position.height));

                GUILayout.BeginVertical("groupbox");

                GUI.SetNextControlName("filter");
                searchString = EditorGUILayout.TextField(searchString);
                EditorGUI.FocusTextInControl("filter");

                if (searchString != "") searchKeywords = searchString.Split(' ');

                GUILayout.Space(10);

                for (int i = 0; i < currentItems.Length; i++)
                {
                    string value = currentItems[i];

                    bool shouldShow = true;
                    if (searchString != "")
                    {
                        shouldShow = true;
                        foreach (var keyword in searchKeywords)
                        {
                            if (!value.ToLower().Contains(keyword.ToLower()))
                            {
                                shouldShow = false;
                                continue;
                            }
                        }
                    }
                    if (!shouldShow) continue;

                    if (i == selectedIndex) GUI.backgroundColor = FaceTrackingSetup_Editor.green;
                    if (GUILayout.Button(value))
                    {
                        currentAction(value, i, indexes);
                        Hide();
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.EndVertical();

                GUILayout.EndScrollView();
            }
            catch(Exception e) { Debug.LogError("StringListSearchProvider ERROR: " + e); }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnScriptsReloaded() => Hide();
    }
}
