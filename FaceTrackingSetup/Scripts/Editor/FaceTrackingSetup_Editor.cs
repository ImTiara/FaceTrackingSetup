using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

using Object = UnityEngine.Object;

namespace ImTiara.FaceTrackingSetup
{
#pragma warning disable IDE0090 // Use 'new(...)'
    [CustomEditor(typeof(FaceTrackingSetup))]
    public sealed class FaceTrackingSetup_Editor : Editor
    {
        public static string[] blendShapes = new string[0];

        public static readonly Color red = new Color(1.0f, 0.6f, 0.6f);
        public static readonly Color yellow = new Color(1.0f, 1.0f, 0.6f);
        public static readonly Color green = new Color(0.6f, 1.0f, 0.6f);

        public static string[] filterKeywords = new string[0];
        public static string filterString = "";

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, "FTS Change");

            FaceTrackingSetup fts = (FaceTrackingSetup)target;

            bool outputExists = Directory.Exists(fts.outputPath);

            if (GUILayout.Button(outputExists ? fts.outputPath : "Set Output Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Output Folder", "Assets/", "");
                if (path == "") return;

                path = "Assets/" + path.Split(new string[] { "Assets/" }, StringSplitOptions.None)[1];

                fts.outputPath = path;

                Save(fts);
            }
            if (!outputExists) EditorGUILayout.HelpBox("An output folder must be set.", MessageType.Error);

            GUILayout.Space(10);

            fts.expressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField("VRC Parameters", fts.expressionParameters, typeof(VRCExpressionParameters), false);
            if (fts.expressionParameters == null)
            {
                EditorGUILayout.HelpBox("VRC Parameters are required.", MessageType.Error, false);
                GUILayout.Space(10);
            }

            fts.fx = (RuntimeAnimatorController)EditorGUILayout.ObjectField("FX Controller", fts.fx, typeof(RuntimeAnimatorController), false);
            if (fts.fx == null)
            {
                EditorGUILayout.HelpBox("FX controller is required.", MessageType.Error, false);
                GUILayout.Space(10);
            }

            GUI.enabled = outputExists && fts.expressionParameters != null && fts.fx != null;

            GUILayout.Space(10);

            fts.additive = (RuntimeAnimatorController)EditorGUILayout.ObjectField(new GUIContent("Additive Controller", "Only required if you use eye tracking that require eye bones instead of blendshapes."), fts.additive, typeof(RuntimeAnimatorController), false);
            fts.faceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", fts.faceMesh, typeof(SkinnedMeshRenderer), true);

            if (fts.faceMesh != null && blendShapes.Length != fts.faceMesh.sharedMesh.blendShapeCount + 1)
            {
                List<string> tempList = new List<string>() { FaceTrackingSetup.NONE };
                for (int i = 0; i < fts.faceMesh.sharedMesh.blendShapeCount; i++)
                {
                    tempList.Add(fts.faceMesh.sharedMesh.GetBlendShapeName(i));
                }
                
                blendShapes = tempList.ToArray();
            }

            GUILayout.Space(10);

            fts.isShowingSettings = EditorGUILayout.Foldout(fts.isShowingSettings, "Base Settings", true);
            if (fts.isShowingSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Window");

                fts.fxWriteDefaults = EditorGUILayout.Toggle("FX Write Defaults", fts.fxWriteDefaults);
                fts.additiveWriteDefaults = EditorGUILayout.Toggle("Additive Write Defaults", fts.additiveWriteDefaults);

                GUILayout.Space(10);

                GUILayout.BeginHorizontal();
                fts.createEyeTrackingToggle = EditorGUILayout.Toggle("Eye Tracking Toggle", fts.createEyeTrackingToggle);
                if (GUILayout.Button("Copy Parameter")) EditorGUIUtility.systemCopyBuffer = FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                fts.createMouthTrackingToggle = EditorGUILayout.Toggle("Mouth Tracking Toggle", fts.createMouthTrackingToggle);
                if (GUILayout.Button("Copy Parameter")) EditorGUIUtility.systemCopyBuffer = FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER;
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }
            
            GUILayout.Space(20);

            fts.isShowingEyeTrackingSettings = EditorGUILayout.Foldout(fts.isShowingEyeTrackingSettings, "Eye Tracking", true);
            if (fts.isShowingEyeTrackingSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Window");

                fts.enableEyeTracking = EditorGUILayout.Toggle("Enable Eye Tracking", fts.enableEyeTracking);

                GUILayout.Space(10);

                GUI.enabled = fts.enableEyeTracking;
                
                fts.eyeTrackingMode = (FaceTrackingSetup.EyeTrackingMode)EditorGUILayout.Popup("Mode", (int)fts.eyeTrackingMode, new string[2] { "Eye Bones", "Blendshapes" });
                switch(fts.eyeTrackingMode)
                {
                    case FaceTrackingSetup.EyeTrackingMode.EyeBone:
                        if (fts.additive != null)
                        {
                            fts.isAdvancedEyeTrackingSettings = EditorGUILayout.Toggle("Advanced Mode", fts.isAdvancedEyeTrackingSettings);

                            GUILayout.Space(10);

                            if (fts.isAdvancedEyeTrackingSettings)
                            {
                                fts.eyeLeftLeft = EditorGUILayout.Slider("Left Eye Left", fts.eyeLeftLeft, -20, 20);
                                fts.eyeLeftRight = EditorGUILayout.Slider("Left Eye Right", fts.eyeLeftRight, -20, 20);
                                fts.eyeLeftUp = EditorGUILayout.Slider("Left Eye Up", fts.eyeLeftUp, -20, 20);
                                fts.eyeLeftDown = EditorGUILayout.Slider("Left Eye Down", fts.eyeLeftDown, -20, 20);

                                GUILayout.Space(10);

                                fts.Eye_Right_Left = EditorGUILayout.Slider("Right Eye Left", fts.Eye_Right_Left, -20, 20);
                                fts.Eye_Right_Right = EditorGUILayout.Slider("Right Eye Right", fts.Eye_Right_Right, -20, 20);
                                fts.Eye_Right_Up = EditorGUILayout.Slider("Right Eye Up", fts.Eye_Right_Up, -20, 20);
                                fts.Eye_Right_Down = EditorGUILayout.Slider("Right Eye Down", fts.Eye_Right_Down, -20, 20);
                            }
                            else
                            {
                                fts.eyeLeftLeft = EditorGUILayout.Slider("Eye Left", fts.eyeLeftLeft, -20, 20);
                                fts.eyeLeftRight = -fts.eyeLeftLeft;
                                GUI.enabled = false;
                                EditorGUILayout.Slider("Eye Right", fts.eyeLeftRight, -20, 20);
                                GUI.enabled = true;

                                fts.eyeLeftUp = EditorGUILayout.Slider("Eye Up", fts.eyeLeftUp, -20, 20);
                                fts.eyeLeftDown = EditorGUILayout.Slider("Eye Down", fts.eyeLeftDown, -20, 20);

                                fts.Eye_Right_Left = -fts.eyeLeftLeft;
                                fts.Eye_Right_Right = -fts.eyeLeftRight;
                                fts.Eye_Right_Up = fts.eyeLeftUp;
                                fts.Eye_Right_Down = fts.eyeLeftDown;
                            }
                        }
                        else
                        {
                            fts.additive = (RuntimeAnimatorController)EditorGUILayout.ObjectField(new GUIContent("Additive Controller", "Only required if you use eye tracking that require eye bones instead of blendshapes."), fts.additive, typeof(RuntimeAnimatorController), false);
                            EditorGUILayout.HelpBox("Additive controller is required for bone-based eye tracking.", MessageType.Warning);
                        }
                        break;
                    case FaceTrackingSetup.EyeTrackingMode.BlendShape:
                        if (fts.faceMesh != null)
                        {
                            int selectedLeftLeftShapekeyIndex = 0;
                            int selectedLeftRightShapekeyIndex = 0;
                            int selectedLeftUpShapekeyIndex = 0;
                            int selectedLeftDownShapekeyIndex = 0;

                            int selectedRightLeftShapekeyIndex = 0;
                            int selectedRightRightShapekeyIndex = 0;
                            int selectedRightUpShapekeyIndex = 0;
                            int selectedRightDownShapekeyIndex = 0;

                            for (int i = 0; i < blendShapes.Length; i++)
                            {
                                string name = blendShapes[i];

                                if (name == fts.leftleftShapeKey) selectedLeftLeftShapekeyIndex = i;
                                if (name == fts.leftrightShapeKey) selectedLeftRightShapekeyIndex = i;
                                if (name == fts.leftupShapeKey) selectedLeftUpShapekeyIndex = i;
                                if (name == fts.leftdownShapeKey) selectedLeftDownShapekeyIndex = i;

                                if (name == fts.rightleftShapeKey) selectedRightLeftShapekeyIndex = i;
                                if (name == fts.rightrightShapeKey) selectedRightRightShapekeyIndex = i;
                                if (name == fts.rightupShapeKey) selectedRightUpShapekeyIndex = i;
                                if (name == fts.rightdownShapeKey) selectedRightDownShapekeyIndex = i;
                            }

                            GUILayout.Space(10);

                            GUI.backgroundColor = yellow;
                            if (GUILayout.Button("Auto Setup From ARKit"))
                            {
                                fts.leftleftShapeKey = "eyeLookOutLeft";
                                fts.leftrightShapeKey = "eyeLookInLeft";
                                fts.leftupShapeKey = "eyeLookUpLeft";
                                fts.leftdownShapeKey = "eyeLookDownLeft";
                                
                                fts.rightleftShapeKey = "eyeLookInRight";
                                fts.rightrightShapeKey = "eyeLookOutRight";
                                fts.rightupShapeKey = "eyeLookUpRight";
                                fts.rightdownShapeKey = "eyeLookDownRight";

                                return;
                            }
                            GUI.backgroundColor = Color.white;

                            GUILayout.Space(10);

                            StringListSearchProvider.DrawSelectButton("Left Eye - Left", blendShapes, (a, b, _) =>
                            {
                                fts.leftleftShapeKey = a;
                                selectedLeftLeftShapekeyIndex = b;
                            }, selectedLeftLeftShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Left Eye - Right", blendShapes, (a, b, _) =>
                            {
                                fts.leftrightShapeKey = a;
                                selectedLeftRightShapekeyIndex = b;
                            }, selectedLeftRightShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Left Eye - Up", blendShapes, (a, b, _) =>
                            {
                                fts.leftupShapeKey = a;
                                selectedLeftUpShapekeyIndex = b;
                            }, selectedLeftUpShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Left Eye - Down", blendShapes, (a, b, _) =>
                            {
                                fts.leftdownShapeKey = a;
                                selectedLeftDownShapekeyIndex = b;
                            }, selectedLeftDownShapekeyIndex);


                            GUILayout.Space(10);

                            StringListSearchProvider.DrawSelectButton("Right Eye - Left", blendShapes, (a, b, _) =>
                            {
                                fts.rightleftShapeKey = a;
                                selectedRightLeftShapekeyIndex = b;
                            }, selectedRightLeftShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Right Eye - Right", blendShapes, (a, b, _) =>
                            {
                                fts.rightrightShapeKey = a;
                                selectedRightRightShapekeyIndex = b;
                            }, selectedRightRightShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Right Eye - Up", blendShapes, (a, b, _) =>
                            {
                                fts.rightupShapeKey = a;
                                selectedRightUpShapekeyIndex = b;
                            }, selectedRightUpShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Right Eye - Down", blendShapes, (a, b, _) =>
                            {
                                fts.rightdownShapeKey = a;
                                selectedRightDownShapekeyIndex = b;
                            }, selectedRightDownShapekeyIndex);

                            GUILayout.Space(10);

                            fts.eyeLeftLeftShapeKeyWeight = EditorGUILayout.Slider("Left Left Weight", fts.eyeLeftLeftShapeKeyWeight, 0, 100);
                            fts.eyeLeftRightShapeKeyWeight = EditorGUILayout.Slider("Left Right Weight", fts.eyeLeftRightShapeKeyWeight, 0, 100);
                            fts.eyeLeftUpShapeKeyWeight = EditorGUILayout.Slider("Left Up Weight", fts.eyeLeftUpShapeKeyWeight, 0, 100);
                            fts.eyeLeftDownShapeKeyWeight = EditorGUILayout.Slider("Left Down Weight", fts.eyeLeftDownShapeKeyWeight, 0, 100);

                            GUILayout.Space(10);

                            fts.eyeRightLeftShapeKeyWeight = EditorGUILayout.Slider("Right Left Weight", fts.eyeRightLeftShapeKeyWeight, 0, 100);
                            fts.eyeRightRightShapeKeyWeight = EditorGUILayout.Slider("Right Right Weight", fts.eyeRightRightShapeKeyWeight, 0, 100);
                            fts.eyeRightUpShapeKeyWeight = EditorGUILayout.Slider("Right Up Weight", fts.eyeRightUpShapeKeyWeight, 0, 100);
                            fts.eyeRightDownShapeKeyWeight = EditorGUILayout.Slider("Right Down Weight", fts.eyeRightDownShapeKeyWeight, 0, 100);
                        }
                        else
                        {
                            fts.faceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", fts.faceMesh, typeof(SkinnedMeshRenderer), true);
                            EditorGUILayout.HelpBox("Select a face mesh to continue.", MessageType.Warning);
                        }
                        break;
                }

                GUILayout.Space(10);

                GUI.backgroundColor = red;
                if (GUILayout.Button("Reset Eye Settings"))
                {
                    if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to reset the eye settings to the default values?", "Reset", "Cancel"))
                    {
                        fts.isAdvancedEyeTrackingSettings = false;

                        fts.eyeLeftLeft = FaceTrackingSetup.LEFT_LEFT;
                        fts.eyeLeftRight = FaceTrackingSetup.LEFT_RIGHT;
                        fts.eyeLeftUp = FaceTrackingSetup.LEFT_UP;
                        fts.eyeLeftDown = FaceTrackingSetup.LEFT_DOWN;

                        fts.Eye_Right_Left = FaceTrackingSetup.RIGHT_LEFT;
                        fts.Eye_Right_Right = FaceTrackingSetup.RIGHT_RIGHT;
                        fts.Eye_Right_Up = FaceTrackingSetup.RIGHT_UP;
                        fts.Eye_Right_Down = FaceTrackingSetup.RIGHT_DOWN;

                        Save(fts);
                    }
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();

                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            fts.isShowingBlinkSettings = EditorGUILayout.Foldout(fts.isShowingBlinkSettings, "Eye Blink", true);
            if (fts.isShowingBlinkSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Window");

                fts.enableBlinking = EditorGUILayout.Toggle("Enable Eye Blink", fts.enableBlinking);

                GUILayout.Space(10);

                GUI.enabled = fts.enableBlinking;

                fts.blinkingMode = (FaceTrackingSetup.BlinkingMode)EditorGUILayout.Popup("Mode", (int)fts.blinkingMode, new string[2] { "Blendshape", "Animation" });
                switch(fts.blinkingMode)
                {
                    case FaceTrackingSetup.BlinkingMode.BlendShape:
                        if (fts.faceMesh != null)
                        {
                            int selectedLeftBlinkShapekeyIndex = 0;
                            int selectedRightBlinkShapekeyIndex = 0;

                            int selectedLeftWideShapekeyIndex = 0;
                            int selectedRightWideShapekeyIndex = 0;

                            for (int i = 0; i < blendShapes.Length; i++)
                            {
                                string name = blendShapes[i];

                                if (name == fts.leftBlinkShapeKey) selectedLeftBlinkShapekeyIndex = i;
                                if (name == fts.rightBlinkShapeKey) selectedRightBlinkShapekeyIndex = i;
                                
                                if (name == fts.leftWideShapeKey) selectedLeftWideShapekeyIndex = i;
                                if (name == fts.rightWideShapeKey) selectedRightWideShapekeyIndex = i;
                            }

                            GUILayout.Space(10);

                            StringListSearchProvider.DrawSelectButton("Left Blink", blendShapes, (a, b, _) =>
                            {
                                fts.leftBlinkShapeKey = a;
                                selectedLeftBlinkShapekeyIndex = b;
                            }, selectedLeftBlinkShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Right Blink", blendShapes, (a, b, _) =>
                            {
                                fts.rightBlinkShapeKey = a;
                                selectedRightBlinkShapekeyIndex = b;
                            }, selectedRightBlinkShapekeyIndex);

                            GUILayout.Space(10);

                            StringListSearchProvider.DrawSelectButton("Left Wide", blendShapes, (a, b, _) =>
                            {
                                fts.leftWideShapeKey = a;
                                selectedLeftWideShapekeyIndex = b;
                            }, selectedLeftWideShapekeyIndex);

                            StringListSearchProvider.DrawSelectButton("Right Wide", blendShapes, (a, b, _) =>
                            {
                                fts.rightWideShapeKey = a;
                                selectedRightWideShapekeyIndex = b;
                            }, selectedRightWideShapekeyIndex);
                        }
                        else
                        {
                            fts.faceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", fts.faceMesh, typeof(SkinnedMeshRenderer), true);
                            EditorGUILayout.HelpBox("Select a face mesh to continue.", MessageType.Warning);
                        }
                        break;
                    case FaceTrackingSetup.BlinkingMode.Animation:
                        fts.leftBlinkAnimation = (AnimationClip)EditorGUILayout.ObjectField("Left Blink", fts.leftBlinkAnimation, typeof(AnimationClip), false);
                        fts.rightBlinkAnimation = (AnimationClip)EditorGUILayout.ObjectField("Right Blink", fts.rightBlinkAnimation, typeof(AnimationClip), false);

                        GUILayout.Space(10);

                        fts.leftWideAnimation = (AnimationClip)EditorGUILayout.ObjectField("Left Wide", fts.leftWideAnimation, typeof(AnimationClip), false);
                        fts.rightWideAnimation = (AnimationClip)EditorGUILayout.ObjectField("Right Wide", fts.rightWideAnimation, typeof(AnimationClip), false);
                        
                        GUILayout.Space(10);

                        fts.leftIdleAnimation = (AnimationClip)EditorGUILayout.ObjectField("Left Idle", fts.leftIdleAnimation, typeof(AnimationClip), false);
                        fts.rightIdleAnimation = (AnimationClip)EditorGUILayout.ObjectField("Right Idle", fts.rightIdleAnimation, typeof(AnimationClip), false);
                        break;
                }

                GUILayout.Space(10);

                fts.blinkingThreshold_Blink = EditorGUILayout.Slider("Blink", fts.blinkingThreshold_Blink, 0, 1);
                fts.blinkingThreshold_Normal = EditorGUILayout.Slider("Normal", fts.blinkingThreshold_Normal, 0, 1);
                fts.blinkingThreshold_Wide = EditorGUILayout.Slider("Wide", fts.blinkingThreshold_Wide, 0, 1);

                GUILayout.Space(10);

                GUI.backgroundColor = red;
                if (GUILayout.Button("Reset Blink Settings"))
                {
                    if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to reset the blink settings to the default values?", "Reset", "Cancel"))
                    {
                        fts.leftBlinkShapeKey = FaceTrackingSetup.NONE;
                        fts.rightBlinkShapeKey = FaceTrackingSetup.NONE;
                        fts.rightWideShapeKey = FaceTrackingSetup.NONE;
                        fts.leftWideShapeKey = FaceTrackingSetup.NONE;

                        fts.blinkingThreshold_Blink = FaceTrackingSetup.BLINKING_BLINK;
                        fts.blinkingThreshold_Normal = FaceTrackingSetup.BLINKING_NORMAL;
                        fts.blinkingThreshold_Wide = FaceTrackingSetup.BLINKING_WIDE;

                        fts.leftBlinkAnimation = null;
                        fts.rightBlinkAnimation = null;
                        fts.leftWideAnimation = null;
                        fts.rightWideAnimation = null;
                        fts.leftIdleAnimation = null;
                        fts.rightIdleAnimation = null;

                        Save(fts);
                    }
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();

                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            fts.isShowingEyeDilationSettings = EditorGUILayout.Foldout(fts.isShowingEyeDilationSettings, "Pupil Dilation", true);
            if (fts.isShowingEyeDilationSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Window");

                fts.enablePupils = EditorGUILayout.Toggle("Enable Pupil Dilation", fts.enablePupils);

                GUILayout.Space(10);

                if (fts.faceMesh != null)
                {
                    GUI.enabled = fts.enablePupils;

                    GUILayout.Space(10);

                    int selectedConstrictedShapekeyIndex = 0;
                    int selectedDilatedShapekeyIndex = 0;

                    for (int i = 0; i < blendShapes.Length; i++)
                    {
                        string name = blendShapes[i];

                        if (name == fts.constrictedShapeKey) selectedConstrictedShapekeyIndex = i;
                        if (name == fts.dilatedShapeKey) selectedDilatedShapekeyIndex = i;
                    }

                    StringListSearchProvider.DrawSelectButton("Constricted", blendShapes, (a, b, _) =>
                    {
                        fts.constrictedShapeKey = a;
                        selectedConstrictedShapekeyIndex = b;
                    }, selectedConstrictedShapekeyIndex);

                    StringListSearchProvider.DrawSelectButton("Dilated", blendShapes, (a, b, _) =>
                    {
                        fts.dilatedShapeKey = a;
                        selectedDilatedShapekeyIndex = b;
                    }, selectedDilatedShapekeyIndex);

                    GUILayout.Space(10);

                    fts.pupilsThreshold_Constricted = EditorGUILayout.Slider("Constricted", fts.pupilsThreshold_Constricted, 0, 1);
                    fts.pupilsThreshold_Normal = EditorGUILayout.Slider("Normal", fts.pupilsThreshold_Normal, 0, 1);
                    fts.pupilsThreshold_Dilated = EditorGUILayout.Slider("Dilated", fts.pupilsThreshold_Dilated, 0, 1);
                }
                else
                {
                    fts.faceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", fts.faceMesh, typeof(SkinnedMeshRenderer), true);
                    EditorGUILayout.HelpBox("Select a face mesh to continue.", MessageType.Warning);
                }

                GUI.enabled = fts.enablePupils;

                GUI.backgroundColor = red;
                if (GUILayout.Button("Reset Pupil Settings"))
                {
                    if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to reset the pupil settings to the default values?", "Reset", "Cancel"))
                    {
                        fts.constrictedShapeKey = FaceTrackingSetup.NONE;
                        fts.dilatedShapeKey = FaceTrackingSetup.NONE;

                        fts.pupilsThreshold_Constricted = FaceTrackingSetup.PUPILS_CONSTRICTED;
                        fts.pupilsThreshold_Normal = FaceTrackingSetup.PUPILS_NORMAL;
                        fts.pupilsThreshold_Dilated = FaceTrackingSetup.PUPILS_DILATED;

                        Save(fts);
                    }
                }
                GUI.backgroundColor = Color.white;

                GUILayout.EndVertical();

                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            fts.isShowingMouthSettings = EditorGUILayout.Foldout(fts.isShowingMouthSettings, "Mouth Tracking", true);
            if (fts.isShowingMouthSettings)
            {
                EditorGUI.indentLevel++;

                GUILayout.BeginVertical("Window");

                fts.mouthEnable = EditorGUILayout.Toggle("Enable Mouth Tracking", fts.mouthEnable);

                GUILayout.Space(10);

                if (fts.faceMesh != null)
                {
                    GUI.enabled = fts.mouthEnable;

                    GUI.backgroundColor = yellow;
                    if (GUILayout.Button("Auto Setup From ARKit"))
                    {
                        if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to run the auto setup? This will reset you current mouth tracking settings.\n\nThis feature is experimental and will be tweaked in the future.", "Setup", "Cancel"))
                        {
                            fts.mouthAffectors = new FaceTrackingSetup.MouthAffector[37];
                            Save(fts);

                            for (int i = 0; i < FaceTrackingSetup.mouthParameterNames.Length; i++)
                            {
                                switch (FaceTrackingSetup.mouthParameterNames[i])
                                {
                                    case "JawRight": fts.mouthAffectors[i].Add("jawRight"); break;
                                    case "JawLeft": fts.mouthAffectors[i].Add("jawLeft"); break;
                                    case "JawForward": fts.mouthAffectors[i].Add("jawForward"); break;
                                    case "JawOpen": fts.mouthAffectors[i].Add("jawOpen"); break;
                                    case "MouthApeShape": fts.mouthAffectors[i].Add(""); break;
                                    case "MouthUpperRight": fts.mouthAffectors[i].Add("mouthUpperUpRight"); break;
                                    case "MouthUpperLeft": fts.mouthAffectors[i].Add("mouthUpperUpLeft"); break;
                                    case "MouthLowerRight": fts.mouthAffectors[i].Add("mouthFrownRight"); break;
                                    case "MouthLowerLeft": fts.mouthAffectors[i].Add("mouthFrownLeft"); break;
                                    case "MouthUpperOverturn": fts.mouthAffectors[i].Add(""); break;
                                    case "MouthLowerOverturn": fts.mouthAffectors[i].Add(""); break;
                                    case "MouthPout": fts.mouthAffectors[i].Add("mouthPucker"); break;
                                    case "MouthSmileRight":
                                        fts.mouthAffectors[i].Add("mouthSmileRight");
                                        fts.mouthAffectors[i].Add("eyeSquintRight");
                                        break;
                                    case "MouthSmileLeft":
                                        fts.mouthAffectors[i].Add("mouthSmileLeft");
                                        fts.mouthAffectors[i].Add("eyeSquintLeft");
                                        break;
                                    case "MouthSadRight": fts.mouthAffectors[i].Add("mouthFrownRight"); break;
                                    case "MouthSadLeft": fts.mouthAffectors[i].Add("mouthFrownLeft"); break;
                                    case "CheekPuffRight": fts.mouthAffectors[i].Add(""); break;
                                    case "CheekPuffLeft": fts.mouthAffectors[i].Add(""); break;
                                    case "CheekSuck": fts.mouthAffectors[i].Add(""); break;
                                    case "MouthUpperUpRight": fts.mouthAffectors[i].Add("mouthUpperUpRight"); break;
                                    case "MouthUpperUpLeft": fts.mouthAffectors[i].Add("mouthUpperUpLeft"); break;
                                    case "MouthLowerDownRight": fts.mouthAffectors[i].Add("mouthLowerDownRight"); break;
                                    case "MouthLowerDownLeft": fts.mouthAffectors[i].Add("mouthLowerDownLeft"); break;
                                    case "MouthUpperInside": fts.mouthAffectors[i].Add("mouthShrugLower"); break;
                                    case "MouthLowerInside": fts.mouthAffectors[i].Add(""); break;
                                    case "MouthLowerOverlay": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueLongStep1": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueLongStep2": fts.mouthAffectors[i].Add("tongueOut"); break;
                                    case "TongueDown": fts.mouthAffectors[i].Add("tongueCurlDown"); break;
                                    case "TongueUp": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueRight": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueLeft": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueRoll": fts.mouthAffectors[i].Add("tongueCurlUp"); break;
                                    case "TongueUpLeftMorph": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueUpRightMorph": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueDownLeftMorph": fts.mouthAffectors[i].Add(""); break;
                                    case "TongueDownRightMorph": fts.mouthAffectors[i].Add(""); break;
                                }
                            }

                            Save(fts);
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    GUI.backgroundColor = red;
                    if (GUILayout.Button("Reset Mouth Tracking Settings"))
                    {
                        if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to reset the mouth settings to the default values?", "Reset", "Cancel"))
                        {
                            fts.mouthAffectors = new FaceTrackingSetup.MouthAffector[37];
                            
                            Save(fts);
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                    filterString = EditorGUILayout.TextField("Filter", filterString);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        filterString = "";
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();

                    if (filterString != "") filterKeywords = filterString.Split(' ');

                    GUILayout.Space(10);

                    for (int i = 0; i < FaceTrackingSetup.mouthParameterNames.Length; i++)
                    {
                        bool shouldShow = true;
                        if (filterString != "")
                        {
                            shouldShow = true;
                            foreach (var keyword in filterKeywords)
                            {
                                if (!FaceTrackingSetup.mouthParameterNames[i].ToLower().Contains(keyword.ToLower()))
                                {
                                    shouldShow = false;
                                }
                            }
                            if (!shouldShow) continue;
                        }

                        GUILayout.BeginVertical("Window");

                        GUILayout.BeginHorizontal("Box");
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(FaceTrackingSetup.mouthParameterNames[i]);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(10);

                        fts.mouthAffectors[i].type = (FaceTrackingSetup.MouthAffector.Type)EditorGUILayout.EnumPopup("Parameter Type", fts.mouthAffectors[i].type);

                        GUILayout.Space(10);

                        for (int i2 = 0; i2 < fts.mouthAffectors[i].affectedBlendshapes.Count; i2++)
                        {
                            GUI.backgroundColor = Color.gray;
                            GUILayout.BeginVertical("Window");
                            GUI.backgroundColor = Color.white;

                            for (int i3 = 0; i3 < blendShapes.Length; i3++)
                            {
                                if (blendShapes[i3] == fts.mouthAffectors[i].affectedBlendshapes[i2].blendShape)
                                {
                                    fts.mouthAffectors[i].affectedBlendshapes[i2].selectedBSIndex = i3;
                                }
                            }

                            StringListSearchProvider.DrawSelectButton("Blendshape", blendShapes, (a, b, indexes) =>
                            {
                                fts.mouthAffectors[indexes[0]].affectedBlendshapes[indexes[1]].blendShape = a;
                                fts.mouthAffectors[indexes[0]].affectedBlendshapes[indexes[1]].selectedBSIndex = b;
                            }, fts.mouthAffectors[i].affectedBlendshapes[i2].selectedBSIndex, i, i2);

                            fts.mouthAffectors[i].affectedBlendshapes[i2].weight = EditorGUILayout.Slider("Weight", fts.mouthAffectors[i].affectedBlendshapes[i2].weight, 0, 100);
                            GUI.backgroundColor = red;
                            if (GUILayout.Button("Remove"))
                            {
                                fts.mouthAffectors[i].affectedBlendshapes.RemoveAt(i2);
                                return;
                            }
                            GUI.backgroundColor = Color.white;
                            GUILayout.EndVertical();
                            GUILayout.Space(10);
                        }

                        GUI.backgroundColor = green;
                        if (GUILayout.Button("Add Affected Blendshape"))
                        {
                            fts.mouthAffectors[i].affectedBlendshapes.Add(new FaceTrackingSetup.MouthAffector.AffectedBlendshape());
                            return;
                        }
                        GUI.backgroundColor = Color.white;

                        GUILayout.EndVertical();

                        GUILayout.Space(50);
                    }

                    GUI.enabled = true;
                }
                else
                {
                    fts.faceMesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Face Mesh", fts.faceMesh, typeof(SkinnedMeshRenderer), true);
                    EditorGUILayout.HelpBox("Select a face mesh to continue.", MessageType.Warning);
                }

                //EditorGUILayout.HelpBox("Coming soon", MessageType.Info);

                GUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUI.backgroundColor = green;
            if (GUILayout.Button("Generate", GUILayout.Height(50)))
            {
                if (EditorUtility.DisplayDialog("Face Tracking Setup", "Confirm setup?\n\nThis will clear existing face tracking stuff from this avatar.", "Generate", "Cancel"))
                {
                    Setup();
                }
            }
            GUI.backgroundColor = Color.white;
            
            GUI.enabled = true;

            GUILayout.Space(10);

            GUI.enabled = fts.expressionParameters != null && fts.fx != null;

            GUI.backgroundColor = red;
            if (GUILayout.Button("Remove", GUILayout.Height(50)))
            {
                if (EditorUtility.DisplayDialog("Face Tracking Setup", "Are you sure you want to remove the face tracking setup?", "Remove", "Cancel"))
                {
                    RemoveAll();

                    Save(fts, fts.expressionParameters, fts.additive, fts.fx);
                }
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        #region Creators
        public void Setup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Face Tracking Setup - Initiating", "Starting setup process", 0.0f);

                if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;

                RemoveAll();

                FaceTrackingSetup fts = (FaceTrackingSetup)target;

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Initiating", "Loading animator controllers", 0.0f);
                var additive = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fts.additive));
                var fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fts.fx));

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Initiating", "Creating 'do nothing' animation", 0.5f);

                AnimationClip _do_nothing = new AnimationClip();
                _do_nothing.SetCurve("_", typeof(GameObject), "_", AnimationCurve.EaseInOut(0, 0, 0, 0));
                AssetDatabase.CreateAsset(_do_nothing, $"{fts.outputPath}/_Do_Nothing.anim");

                if (fts.createEyeTrackingToggle)
                {
                    AddAnimatorParameter(fx, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, AnimatorControllerParameterType.Bool, true);
                    AddVRCParameter(fts.expressionParameters, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, VRCExpressionParameters.ValueType.Bool, 1, true);
                }

                if (fts.createMouthTrackingToggle)
                {
                    AddAnimatorParameter(fx, FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER, AnimatorControllerParameterType.Bool, true);
                    AddVRCParameter(fts.expressionParameters, FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER, VRCExpressionParameters.ValueType.Bool, 1, true);
                }
                
                EditorUtility.DisplayProgressBar("Face Tracking Setup - Initiating", "Done", 1.0f);

                CreateEyeTracking(fts, additive, fx, _do_nothing);
                CreateEyeBlink(fts, fx, _do_nothing);
                CreateEyeDilation(fts, fx, _do_nothing);
                CreateMouthTracking(fts, fx);

                EditorUtility.DisplayProgressBar("Face Tracking Setup", "Saving", 1.0f);

                Save(fts, fts.expressionParameters, fts.additive, fts.fx);

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Face Tracking Setup", "Tracking created!", "Okay");
            }
            catch (Exception e)
            {
                Debug.LogError("Error creating face tracking: " + e);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Face Tracking Setup", "An error occured while creating the face tracking.\n\nCheck the console for further details.", "Okay");
            }
        }

        public static void CreateEyeTracking(FaceTrackingSetup fts, AnimatorController additive, AnimatorController fx, AnimationClip _do_nothing)
        {
            if (fts.enableEyeTracking)
            {
                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating folders", 0.0f);
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Look")) AssetDatabase.CreateFolder($"{fts.outputPath}", "Look");
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Look/Left")) AssetDatabase.CreateFolder($"{fts.outputPath}/Look", "Left");
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Look/Right")) AssetDatabase.CreateFolder($"{fts.outputPath}/Look", "Right");

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating animator layers", 0.1f);

                AnimatorControllerLayer leftEyeLayer = null;
                AnimatorControllerLayer rightEyeLayer = null;

                AnimationClip eyeLeftLeft = new AnimationClip();
                AnimationClip eyeLeftRight = new AnimationClip();
                AnimationClip eyeLeftUp = new AnimationClip();
                AnimationClip eyeLeftDown = new AnimationClip();

                AnimationClip eyeRightLeft = new AnimationClip();
                AnimationClip eyeRightRight = new AnimationClip();
                AnimationClip eyeRightUp = new AnimationClip();
                AnimationClip eyeRightDown = new AnimationClip();

                switch (fts.eyeTrackingMode)
                {
                    case FaceTrackingSetup.EyeTrackingMode.EyeBone:
                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating layers", 0.2f);
                        leftEyeLayer = AddLayer(additive, "Left Eye", 1.0f);
                        rightEyeLayer = AddLayer(additive, "Right Eye", 0.5f);

                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating animations", 0.3f);
                        eyeLeftLeft.SetCurve("", typeof(Animator), "Left Eye In-Out", AnimationCurve.Constant(0, 0, fts.eyeLeftLeft));
                        eyeLeftRight.SetCurve("", typeof(Animator), "Left Eye In-Out", AnimationCurve.Constant(0, 0, fts.eyeLeftRight));
                        eyeLeftUp.SetCurve("", typeof(Animator), "Left Eye Down-Up", AnimationCurve.Constant(0, 0, fts.eyeLeftUp));
                        eyeLeftDown.SetCurve("", typeof(Animator), "Left Eye Down-Up", AnimationCurve.Constant(0, 0, fts.eyeLeftDown));

                        eyeRightLeft.SetCurve("", typeof(Animator), "Right Eye In-Out", AnimationCurve.Constant(0, 0, fts.Eye_Right_Left));
                        eyeRightRight.SetCurve("", typeof(Animator), "Right Eye In-Out", AnimationCurve.Constant(0, 0, fts.Eye_Right_Right));
                        eyeRightUp.SetCurve("", typeof(Animator), "Right Eye Down-Up", AnimationCurve.Constant(0, 0, fts.Eye_Right_Up));
                        eyeRightDown.SetCurve("", typeof(Animator), "Right Eye Down-Up", AnimationCurve.Constant(0, 0, fts.Eye_Right_Down));

                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating animator parameters", 0.4f);
                        AddAnimatorParameter(additive, "LeftEyeX", AnimatorControllerParameterType.Float);
                        AddAnimatorParameter(additive, "RightEyeX", AnimatorControllerParameterType.Float);
                        AddAnimatorParameter(additive, "EyesY", AnimatorControllerParameterType.Float);
                        break;
                    case FaceTrackingSetup.EyeTrackingMode.BlendShape:
                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating layers", 0.2f);
                        leftEyeLayer = AddLayer(fx, "Left Eye", 1.0f);
                        rightEyeLayer = AddLayer(fx, "Right Eye", 1.0f);

                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating animations", 0.3f);
                        eyeLeftLeft.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftleftShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeLeftLeftShapeKeyWeight));
                        eyeLeftRight.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftrightShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeLeftRightShapeKeyWeight));
                        eyeLeftUp.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftupShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeLeftUpShapeKeyWeight));
                        eyeLeftDown.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftdownShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeLeftDownShapeKeyWeight));

                        eyeRightLeft.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightleftShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeRightLeftShapeKeyWeight));
                        eyeRightRight.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightrightShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeRightRightShapeKeyWeight));
                        eyeRightUp.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightupShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeRightUpShapeKeyWeight));
                        eyeRightDown.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightdownShapeKey}", AnimationCurve.Constant(0, 0, fts.eyeRightDownShapeKeyWeight));

                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating animator parameters", 0.4f);
                        AddAnimatorParameter(fx, "LeftEyeX", AnimatorControllerParameterType.Float);
                        AddAnimatorParameter(fx, "RightEyeX", AnimatorControllerParameterType.Float);
                        AddAnimatorParameter(fx, "EyesY", AnimatorControllerParameterType.Float);
                        break;
                }

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating assets", 0.5f);
                AssetDatabase.CreateAsset(eyeLeftLeft, $"{fts.outputPath}/Look/Left/Eye_Left_Left.anim");
                AssetDatabase.CreateAsset(eyeLeftRight, $"{fts.outputPath}/Look/Left/Eye_Left_Right.anim");
                AssetDatabase.CreateAsset(eyeLeftUp, $"{fts.outputPath}/Look/Left/Eye_Left_Up.anim");
                AssetDatabase.CreateAsset(eyeLeftDown, $"{fts.outputPath}/Look/Left/Eye_Left_Down.anim");

                AssetDatabase.CreateAsset(eyeRightLeft, $"{fts.outputPath}/Look/Right/Eye_Right_Left.anim");
                AssetDatabase.CreateAsset(eyeRightRight, $"{fts.outputPath}/Look/Right/Eye_Right_Right.anim");
                AssetDatabase.CreateAsset(eyeRightUp, $"{fts.outputPath}/Look/Right/Eye_Right_Up.anim");
                AssetDatabase.CreateAsset(eyeRightDown, $"{fts.outputPath}/Look/Right/Eye_Right_Down.anim");

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating vrc parameters", 0.6f);
                AddVRCParameter(fts.expressionParameters, "LeftEyeX", VRCExpressionParameters.ValueType.Float, 0);
                AddVRCParameter(fts.expressionParameters, "RightEyeX", VRCExpressionParameters.ValueType.Float, 0);
                AddVRCParameter(fts.expressionParameters, "EyesY", VRCExpressionParameters.ValueType.Float, 0);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating look blendtrees (left)", 0.7f);

                BlendTree leftEyeBlendTree = new BlendTree() { name = "Left Eye", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "LeftEyeX", blendParameterY = "EyesY" };
                leftEyeBlendTree.AddChild(_do_nothing, new Vector2(0, 0));
                leftEyeBlendTree.AddChild(eyeLeftRight, new Vector2(1, 0));
                leftEyeBlendTree.AddChild(eyeLeftLeft, new Vector2(-1, 0));
                leftEyeBlendTree.AddChild(eyeLeftUp, new Vector2(0, 1));
                leftEyeBlendTree.AddChild(eyeLeftDown, new Vector2(0, -1));
                leftEyeBlendTree.hideFlags = HideFlags.HideInHierarchy;
                var leftEyeState = leftEyeLayer.stateMachine.AddState("Left Eye");
                leftEyeState.motion = leftEyeBlendTree;
                leftEyeState.timeParameterActive = false;

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Tracking", "Creating look blendtrees (right)", 0.8f);
                BlendTree rightEyeBlendTree = new BlendTree() { name = "Right Eye", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "RightEyeX", blendParameterY = "EyesY" };
                rightEyeBlendTree.AddChild(_do_nothing, new Vector2(0, 0));
                rightEyeBlendTree.AddChild(eyeRightRight, new Vector2(1, 0));
                rightEyeBlendTree.AddChild(eyeRightLeft, new Vector2(-1, 0));
                rightEyeBlendTree.AddChild(eyeRightUp, new Vector2(0, 1));
                rightEyeBlendTree.AddChild(eyeRightDown, new Vector2(0, -1));
                rightEyeBlendTree.hideFlags = HideFlags.HideInHierarchy;
                var rightEyeState = rightEyeLayer.stateMachine.AddState("Right Eye");
                rightEyeState.motion = rightEyeBlendTree;

                rightEyeState.timeParameterActive = false;
               

                switch(fts.eyeTrackingMode)
                {
                    case FaceTrackingSetup.EyeTrackingMode.EyeBone:
                        leftEyeState.writeDefaultValues = fts.additiveWriteDefaults;
                        rightEyeState.writeDefaultValues = fts.additiveWriteDefaults;
                        
                        AssetDatabase.AddObjectToAsset(leftEyeBlendTree, AssetDatabase.GetAssetPath(additive));
                        AssetDatabase.AddObjectToAsset(rightEyeBlendTree, AssetDatabase.GetAssetPath(additive));
                        break;
                    case FaceTrackingSetup.EyeTrackingMode.BlendShape:
                        leftEyeState.writeDefaultValues = fts.fxWriteDefaults;
                        rightEyeState.writeDefaultValues = fts.fxWriteDefaults;
                        
                        AssetDatabase.AddObjectToAsset(leftEyeBlendTree, AssetDatabase.GetAssetPath(fx));
                        AssetDatabase.AddObjectToAsset(rightEyeBlendTree, AssetDatabase.GetAssetPath(fx));
                        break;
                }
                
                if (fts.createEyeTrackingToggle)
                {
                    CreateToggle(leftEyeLayer, leftEyeState, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, fts.eyeTrackingMode == FaceTrackingSetup.EyeTrackingMode.EyeBone ? fts.additiveWriteDefaults : fts.fxWriteDefaults);
                    CreateToggle(rightEyeLayer, rightEyeState, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, fts.eyeTrackingMode == FaceTrackingSetup.EyeTrackingMode.EyeBone ? fts.additiveWriteDefaults : fts.fxWriteDefaults);
                }

                EditorUtility.ClearProgressBar();
            }
        }

        public static void CreateEyeBlink(FaceTrackingSetup fts, AnimatorController fx, AnimationClip _do_nothing)
        {
            if (fts.enableBlinking)
            {
                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating folders", 0.0f);
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Blinking")) AssetDatabase.CreateFolder($"{fts.outputPath}", "Blinking");

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating animator layers", 0.0f);
                var leftEyeLidExpandedLayer = AddLayer(fx, "Left Eye Lid Expanded", 1.0f);
                var rightEyeLidExpandedLayer = AddLayer(fx, "Right Eye Lid Expanded", 1.0f);

                AnimationClip eyeLeftBlink = null;
                AnimationClip eyeRightBlink = null;
                AnimationClip eyeLeftWide = null;
                AnimationClip eyeRightWide = null;
                AnimationClip eyeLeftIdle = null;
                AnimationClip eyeRightIdle = null;


                switch (fts.blinkingMode)
                {
                    case FaceTrackingSetup.BlinkingMode.BlendShape:
                        EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating blink animations", 0.0f);

                        eyeLeftBlink = new AnimationClip();
                        eyeRightBlink = new AnimationClip();
                        eyeLeftWide = new AnimationClip();
                        eyeRightWide = new AnimationClip();
                        eyeLeftIdle = new AnimationClip();
                        eyeRightIdle = new AnimationClip();

                        eyeLeftBlink.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftWideShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        eyeLeftBlink.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftBlinkShapeKey}", AnimationCurve.Constant(0, 0, 100));
                        AssetDatabase.CreateAsset(eyeLeftBlink, $"{fts.outputPath}/Blinking/Eye_Left_Blink.anim");

                        eyeRightBlink.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightWideShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        eyeRightBlink.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightBlinkShapeKey}", AnimationCurve.Constant(0, 0, 100));
                        AssetDatabase.CreateAsset(eyeRightBlink, $"{fts.outputPath}/Blinking/Eye_Right_Blink.anim");

                        eyeLeftWide.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftWideShapeKey}", AnimationCurve.Constant(0, 0, 100));
                        eyeLeftWide.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftBlinkShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        AssetDatabase.CreateAsset(eyeLeftWide, $"{fts.outputPath}/Blinking/Eye_Left_Wide.anim");

                        eyeRightWide.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightWideShapeKey}", AnimationCurve.Constant(0, 0, 100));
                        eyeRightWide.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightBlinkShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        AssetDatabase.CreateAsset(eyeRightWide, $"{fts.outputPath}/Blinking/Eye_Right_Wide.anim");

                        eyeLeftIdle.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftWideShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        eyeLeftIdle.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.leftBlinkShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        AssetDatabase.CreateAsset(eyeLeftIdle, $"{fts.outputPath}/Blinking/Eye_Left_Idle.anim");

                        eyeRightIdle.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightWideShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        eyeRightIdle.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.rightBlinkShapeKey}", AnimationCurve.Constant(0, 0, 0));
                        AssetDatabase.CreateAsset(eyeRightIdle, $"{fts.outputPath}/Blinking/Eye_Right_Idle.anim");



                        break;
                    case FaceTrackingSetup.BlinkingMode.Animation:
                        eyeLeftBlink = fts.leftBlinkAnimation;
                        eyeRightBlink = fts.rightBlinkAnimation;
                        eyeLeftWide = fts.leftWideAnimation;
                        eyeRightWide = fts.rightWideAnimation;
                        eyeLeftIdle = fts.leftIdleAnimation;
                        eyeRightIdle = fts.rightIdleAnimation;
                        break;
                }

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating vrc parameters", 0.0f);
                AddVRCParameter(fts.expressionParameters, "LeftEyeLidExpanded", VRCExpressionParameters.ValueType.Float, fts.blinkingThreshold_Normal);
                AddVRCParameter(fts.expressionParameters, "RightEyeLidExpanded", VRCExpressionParameters.ValueType.Float, fts.blinkingThreshold_Normal);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating animator parameters", 0.0f);
                AddAnimatorParameter(fx, "LeftEyeLidExpanded", AnimatorControllerParameterType.Float, fts.blinkingThreshold_Normal);
                AddAnimatorParameter(fx, "RightEyeLidExpanded", AnimatorControllerParameterType.Float, fts.blinkingThreshold_Normal);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Eye Blink", "Creating blink blendtrees", 0.0f);

                BlendTree leftEyeLidBlendTree = new BlendTree
                {
                    name = "Eye Lid",
                    blendParameter = "LeftEyeLidExpanded",
                    useAutomaticThresholds = false,
                    hideFlags = HideFlags.HideInHierarchy
                };
                var leftEyeLidState = leftEyeLidExpandedLayer.stateMachine.AddState("Eye Lid");
                leftEyeLidState.motion = leftEyeLidBlendTree;
                leftEyeLidState.writeDefaultValues = fts.fxWriteDefaults;
                leftEyeLidState.timeParameterActive = false;
                AssetDatabase.AddObjectToAsset(leftEyeLidBlendTree, AssetDatabase.GetAssetPath(fx));

                BlendTree rightEyeLidBlendTree = new BlendTree
                {
                    name = "Eye Lid",
                    blendParameter = "RightEyeLidExpanded",
                    useAutomaticThresholds = false,
                    hideFlags = HideFlags.HideInHierarchy
                };
                var rightEyeLidState = rightEyeLidExpandedLayer.stateMachine.AddState("Eye Lid");
                rightEyeLidState.motion = rightEyeLidBlendTree;
                rightEyeLidState.writeDefaultValues = fts.fxWriteDefaults;
                rightEyeLidState.timeParameterActive = false;
                AssetDatabase.AddObjectToAsset(rightEyeLidBlendTree, AssetDatabase.GetAssetPath(fx));
        
                switch(fts.blinkingMode)
                {
                    case FaceTrackingSetup.BlinkingMode.BlendShape:
                        leftEyeLidBlendTree.AddChild(fts.leftBlinkShapeKey != FaceTrackingSetup.NONE ? eyeLeftBlink : _do_nothing, fts.blinkingThreshold_Blink);
                        leftEyeLidBlendTree.AddChild(eyeLeftIdle, fts.blinkingThreshold_Normal);
                        leftEyeLidBlendTree.AddChild(fts.leftWideShapeKey != FaceTrackingSetup.NONE ? eyeLeftWide : _do_nothing, fts.blinkingThreshold_Wide);

                        rightEyeLidBlendTree.AddChild(fts.rightBlinkShapeKey != FaceTrackingSetup.NONE ? eyeRightBlink : _do_nothing, fts.blinkingThreshold_Blink);
                        rightEyeLidBlendTree.AddChild(eyeRightIdle, fts.blinkingThreshold_Normal);
                        rightEyeLidBlendTree.AddChild(fts.rightWideShapeKey != FaceTrackingSetup.NONE ? eyeRightWide : _do_nothing, fts.blinkingThreshold_Wide);
                        break;
                    case FaceTrackingSetup.BlinkingMode.Animation:
                        leftEyeLidBlendTree.AddChild(eyeLeftBlink, fts.blinkingThreshold_Blink);
                        leftEyeLidBlendTree.AddChild(eyeLeftIdle, fts.blinkingThreshold_Normal);
                        leftEyeLidBlendTree.AddChild(eyeLeftWide, fts.blinkingThreshold_Wide);

                        rightEyeLidBlendTree.AddChild(eyeRightBlink, fts.blinkingThreshold_Blink);
                        rightEyeLidBlendTree.AddChild(eyeRightIdle, fts.blinkingThreshold_Normal);
                        rightEyeLidBlendTree.AddChild(eyeRightWide, fts.blinkingThreshold_Wide);
                        break;
                }

                if (fts.createEyeTrackingToggle)
                {
                    CreateToggle(leftEyeLidExpandedLayer, leftEyeLidState, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, fts.fxWriteDefaults);
                    CreateToggle(rightEyeLidExpandedLayer, rightEyeLidState, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, fts.fxWriteDefaults);
                }

                EditorUtility.ClearProgressBar();
            }
        }

        public static void CreateEyeDilation(FaceTrackingSetup fts, AnimatorController fx, AnimationClip _do_nothing)
        {
            if (fts.enablePupils)
            {
                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating folders", 0.0f);
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Pupils")) AssetDatabase.CreateFolder($"{fts.outputPath}", "Pupils");

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating animator layers", 0.0f);
                var eyeDilationLayer = AddLayer(fx, "Pupil Dilation", 1.0f);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating vrc parameter", 0.0f);
                AddVRCParameter(fts.expressionParameters, "EyesDilation", VRCExpressionParameters.ValueType.Float, fts.pupilsThreshold_Normal);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating animator parameter", 0.0f);
                AddAnimatorParameter(fx, "EyesDilation", AnimatorControllerParameterType.Float, fts.pupilsThreshold_Normal);

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating pupil animations", 0.0f);
                AnimationClip eyeDilated = new AnimationClip();
                AnimationClip eyeNormal = new AnimationClip();
                AnimationClip eyeConstricted = new AnimationClip();

                eyeDilated.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.dilatedShapeKey}", AnimationCurve.Constant(0, 0, 100));
                eyeDilated.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.constrictedShapeKey}", AnimationCurve.Constant(0, 0, 0));
                AssetDatabase.CreateAsset(eyeDilated, $"{fts.outputPath}/Pupils/Eye_Dilated.anim");

                eyeNormal.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.dilatedShapeKey}", AnimationCurve.Constant(0, 0, 0));
                eyeNormal.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.constrictedShapeKey}", AnimationCurve.Constant(0, 0, 0));
                AssetDatabase.CreateAsset(eyeNormal, $"{fts.outputPath}/Pupils/Eye_Normal.anim");

                eyeConstricted.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.dilatedShapeKey}", AnimationCurve.Constant(0, 0, 0));
                eyeConstricted.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{fts.constrictedShapeKey}", AnimationCurve.Constant(0, 0, 100));
                AssetDatabase.CreateAsset(eyeConstricted, $"{fts.outputPath}/Pupils/Eye_Constricted.anim");

                EditorUtility.DisplayProgressBar("Face Tracking Setup - Pupil Dilation", "Creating pupil blendtree", 0.0f);

                BlendTree blendTree = new BlendTree() { name = "Pupil", blendParameter = "EyesDilation", useAutomaticThresholds = false };
                blendTree.AddChild(fts.constrictedShapeKey != FaceTrackingSetup.NONE ? eyeConstricted : _do_nothing, fts.pupilsThreshold_Constricted);
                blendTree.AddChild(eyeNormal, fts.pupilsThreshold_Normal);
                blendTree.AddChild(fts.dilatedShapeKey != FaceTrackingSetup.NONE ? eyeDilated : _do_nothing, fts.pupilsThreshold_Dilated);
                blendTree.hideFlags = HideFlags.HideInHierarchy;
                var state = eyeDilationLayer.stateMachine.AddState("Pupil");
                state.motion = blendTree;
                state.writeDefaultValues = fts.fxWriteDefaults;
                state.timeParameterActive = false;
                AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(fx));

                if (fts.createEyeTrackingToggle)
                {
                    CreateToggle(eyeDilationLayer, state, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, fts.fxWriteDefaults);
                }

                EditorUtility.ClearProgressBar();
            }
        }

        public static void CreateMouthTracking(FaceTrackingSetup fts, AnimatorController fx)
        {
            if (fts.mouthEnable)
            {
                if (!AssetDatabase.IsValidFolder($"{fts.outputPath}/Mouth")) AssetDatabase.CreateFolder($"{fts.outputPath}", "Mouth");

                for (int i = 0; i < FaceTrackingSetup.mouthParameterNames.Length; i++)
                {
                    var trackedMouth = fts.mouthAffectors[i];

                    int count = trackedMouth.affectedBlendshapes.Count;
                    if (count == 0) continue;

                    AnimationClip clip = new AnimationClip();
                    AnimationClip clipIdle = new AnimationClip();
                    for (int i2 = 0; i2 < count; i2++)
                    {
                        if (trackedMouth.affectedBlendshapes[i2].blendShape.Length == 0 || trackedMouth.affectedBlendshapes[i2].blendShape == FaceTrackingSetup.NONE) continue;
                        
                        clipIdle.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{trackedMouth.affectedBlendshapes[i2].blendShape}", AnimationCurve.Constant(0, 0, 0));
                        clip.SetCurve(fts.faceMesh.name, typeof(SkinnedMeshRenderer), $"blendShape.{trackedMouth.affectedBlendshapes[i2].blendShape}", AnimationCurve.Constant(0, 0, trackedMouth.affectedBlendshapes[i2].weight));
                    }
                    
                    AssetDatabase.CreateAsset(clip, $"{fts.outputPath}/Mouth/{FaceTrackingSetup.mouthParameterNames[i]}.anim");
                    AssetDatabase.CreateAsset(clipIdle, $"{fts.outputPath}/Mouth/{FaceTrackingSetup.mouthParameterNames[i]}_Idle.anim");

                    switch (trackedMouth.type)
                    {
                        case FaceTrackingSetup.MouthAffector.Type.Float:
                            var layer = AddLayer(fx, FaceTrackingSetup.mouthParameterNames[i], 1.0f);

                            AddVRCParameter(fts.expressionParameters, FaceTrackingSetup.mouthParameterNames[i], VRCExpressionParameters.ValueType.Float, 0.0f);
                            AddAnimatorParameter(fx, FaceTrackingSetup.mouthParameterNames[i], AnimatorControllerParameterType.Float, 0.0f);

                            BlendTree blendTree = new BlendTree() { name = FaceTrackingSetup.mouthParameterNames[i], blendParameter = FaceTrackingSetup.mouthParameterNames[i], useAutomaticThresholds = false };
                            blendTree.AddChild(clipIdle, 0.0f);
                            blendTree.AddChild(clip, 1.0f);
                            blendTree.hideFlags = HideFlags.HideInHierarchy;
                            var state = layer.stateMachine.AddState(FaceTrackingSetup.mouthParameterNames[i]);
                            state.motion = blendTree;
                            state.writeDefaultValues = fts.fxWriteDefaults;
                            state.timeParameterActive = false;
                            AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(fx));

                            if (fts.createEyeTrackingToggle)
                            {
                                CreateToggle(layer, state, FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER, fts.fxWriteDefaults);
                            }
                            break;
                            
                            /// TODO: Add binary support
                    }
                }

                EditorUtility.ClearProgressBar();
            }
        }
        #endregion
        
        #region Removers
        public void RemoveAll()
        {
            EditorUtility.DisplayProgressBar("Face Tracking Setup", "Removing existing setup", 0.0f);
            
            FaceTrackingSetup fts = (FaceTrackingSetup)target;
            var additive = fts.additive != null ? AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fts.additive)) : null;
            var fx = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fts.fx));
            
            RemoveEyeTracking(fts, additive, fx);
            RemoveEyeBlink(fts, fx);
            RemoveEyeDilation(fts, fx);
            RemoveMouthTracking(fts, fx);

            RemoveAnimatorParameters(fx, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER);
            RemoveVRCParameters(fts.expressionParameters, FaceTrackingSetup.EYE_TRACKING_TOGGLE_PARAMETER, FaceTrackingSetup.MOUTH_TRACKING_TOGGLE_PARAMETER);

            EditorUtility.DisplayProgressBar("Face Tracking Setup", "Removing existing setup", 1.0f);
            
            EditorUtility.ClearProgressBar();
        }

        public static void RemoveEyeTracking(FaceTrackingSetup fts, AnimatorController additive, AnimatorController fx)
        {
            if (additive != null)
            {
                RemoveLayers(additive, "Left Eye", "Right Eye");
                RemoveAnimatorParameters(additive, "LeftEyeX", "RightEyeX", "EyesY");
            }
            
            RemoveLayers(fx, "Left Eye", "Right Eye");
            RemoveAnimatorParameters(fx, "LeftEyeX", "RightEyeX", "EyesY");
            RemoveVRCParameters(fts.expressionParameters, "LeftEyeX", "RightEyeX", "EyesY");
        }

        public static void RemoveEyeBlink(FaceTrackingSetup fts, AnimatorController fx)
        {
            RemoveLayers(fx, "Left Eye Lid Expanded", "Right Eye Lid Expanded");
            RemoveAnimatorParameters(fx, "LeftEyeLidExpanded", "RightEyeLidExpanded");
            RemoveVRCParameters(fts.expressionParameters, "LeftEyeLidExpanded", "RightEyeLidExpanded");
        }

        public static void RemoveEyeDilation(FaceTrackingSetup fts, AnimatorController fx)
        {
            RemoveLayers(fx, "Pupil Dilation");
            RemoveAnimatorParameters(fx, "EyesDilation");
            RemoveVRCParameters(fts.expressionParameters, "EyesDilation");
        }

        public static void RemoveMouthTracking(FaceTrackingSetup fts, AnimatorController fx)
        {
            RemoveLayers(fx, FaceTrackingSetup.mouthParameterNames);
            RemoveAnimatorParameters(fx, FaceTrackingSetup.mouthParameterNames);
            RemoveVRCParameters(fts.expressionParameters, FaceTrackingSetup.mouthParameterNames);
        }
        #endregion

        #region Helpers
        public static AnimatorControllerLayer AddLayer(AnimatorController controller, string name, float weight)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = name,
                defaultWeight = weight,
                stateMachine = new AnimatorStateMachine
                {
                    name = name,
                    hideFlags = HideFlags.HideInHierarchy,
                }
            };
                
            AssetDatabase.AddObjectToAsset(layer.stateMachine, AssetDatabase.GetAssetPath(controller));
            controller.AddLayer(layer);
            
            return layer;
        }

        public static void RemoveLayers(AnimatorController controller, params string[] names)
        {
            foreach (var name in names)
            {
                for (int i = 0; i < controller.layers.Length; i++)
                {
                    if (controller.layers[i].name == name)
                    {
                        controller.RemoveLayer(i);
                    }
                }
            }
        }

        public static void AddAnimatorParameter(AnimatorController controller, string name, AnimatorControllerParameterType valueType, object defaultValue = null)
        {
            var x = controller.parameters.ToList();

            var parameter = new AnimatorControllerParameter
            {
                name = name,
                type = valueType,
            };

            if (defaultValue != null)
            {
                switch (valueType)
                {
                    case AnimatorControllerParameterType.Float: parameter.defaultFloat = (float)defaultValue; break;
                    case AnimatorControllerParameterType.Int: parameter.defaultInt = (int)defaultValue; break;
                    case AnimatorControllerParameterType.Bool: parameter.defaultBool = (bool)defaultValue; break;
                }
            }

            x.Add(parameter);

            controller.parameters = x.ToArray();
        }

        public static void RemoveAnimatorParameters(AnimatorController controller, params string[] names)
        {
            var x = controller.parameters.ToList();

            foreach (var name in names)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i].name == name)
                    {
                        x.RemoveAt(i);
                    }
                }
            }

            controller.parameters = x.ToArray();
        }

        public static void RemoveVRCParameters(VRCExpressionParameters vrcParameters, params string[] names)
        {
            var x = vrcParameters.parameters.ToList();

            foreach (var name in names)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    if (x[i].name == name)
                    {
                        x.RemoveAt(i);
                    }
                }
            }

            vrcParameters.parameters = x.ToArray();
        }

        public static void AddVRCParameter(VRCExpressionParameters vrcParameters, string name, VRCExpressionParameters.ValueType valueType, float defaultValue, bool saved = false)
        {
            var _parameters = vrcParameters.parameters.ToList();
            var parameter = new VRCExpressionParameters.Parameter
            {
                name = name,
                valueType = valueType,
                defaultValue = defaultValue,
                saved = saved
            };

            _parameters.Add(parameter);

            vrcParameters.parameters = _parameters.ToArray();
        }

        public static void CreateToggle(AnimatorControllerLayer layer, AnimatorState animatorState, string parameter, bool writeDefaults)
        {
            var disable = layer.stateMachine.AddState("Disable");
            disable.writeDefaultValues = writeDefaults;
            var onToDisable = animatorState.AddTransition(disable);
            var disableToOn = disable.AddTransition(animatorState);

            onToDisable.hasExitTime = false;
            onToDisable.duration = 0.0f;
            onToDisable.AddCondition(AnimatorConditionMode.IfNot, 0, parameter);

            disableToOn.hasExitTime = false;
            disableToOn.duration = 0.0f;
            disableToOn.AddCondition(AnimatorConditionMode.If, 0, parameter);
        }

        public static void Save(params Object[] @objects)
        {
            AssetDatabase.Refresh();
            
            foreach (var @object in @objects)
            {
                if (@object != null)
                {
                    EditorUtility.SetDirty(@object);
                }
            }
            
            AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        }
        #endregion
    }
#pragma warning restore IDE0090 // Use 'new(...)'
}