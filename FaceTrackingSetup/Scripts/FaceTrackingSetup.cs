using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace ImTiara.FaceTrackingSetup
{
#pragma warning disable IDE0090 // Use 'new(...)'
    public sealed class FaceTrackingSetup : MonoBehaviour
    {
        public const string VERSION = "1.0.1";

        public const string NONE = "-none-";

        public string outputPath;

        public VRCExpressionParameters expressionParameters;
        public RuntimeAnimatorController additive;

        public RuntimeAnimatorController fx;

        public SkinnedMeshRenderer faceMesh;

        public bool additiveWriteDefaults = true;
        public bool fxWriteDefaults = true;
        public bool createEyeTrackingToggle;
        public bool createMouthTrackingToggle;

        public const string EYE_TRACKING_TOGGLE_PARAMETER = "enableEyeTracking";
        public const string MOUTH_TRACKING_TOGGLE_PARAMETER = "enableMouthTracking";

        public bool isShowingSettings, isShowingEyeTrackingSettings, isShowingBlinkSettings, isShowingEyeDilationSettings, isShowingMouthSettings;

        #region Eye Tracking
        public const float LEFT_LEFT = 5.85f;
        public const float LEFT_RIGHT = -5.85f;
        public const float LEFT_UP = 8.0f;
        public const float LEFT_DOWN = -10.0f;

        public const float RIGHT_LEFT = -5.85f;
        public const float RIGHT_RIGHT = 5.85f;
        public const float RIGHT_UP = 8.0f;
        public const float RIGHT_DOWN = -10.0f;
        
        public bool enableEyeTracking;

        public EyeTrackingMode eyeTrackingMode;
        public bool isAdvancedEyeTrackingSettings;

        public float eyeLeftLeft = LEFT_LEFT;
        public float eyeLeftRight = LEFT_RIGHT;
        public float eyeLeftUp = LEFT_UP;
        public float eyeLeftDown = LEFT_DOWN;

        public float Eye_Right_Left = RIGHT_LEFT;
        public float Eye_Right_Right = RIGHT_RIGHT;
        public float Eye_Right_Up = RIGHT_UP;
        public float Eye_Right_Down = RIGHT_DOWN;

        public float eyeLeftLeftShapeKeyWeight = 100;
        public float eyeLeftRightShapeKeyWeight = 100;
        public float eyeLeftUpShapeKeyWeight = 100;
        public float eyeLeftDownShapeKeyWeight = 100;

        public float eyeRightLeftShapeKeyWeight = 100;
        public float eyeRightRightShapeKeyWeight = 100;
        public float eyeRightUpShapeKeyWeight = 100;
        public float eyeRightDownShapeKeyWeight = 100;

        public string leftleftShapeKey = NONE;
        public string leftrightShapeKey = NONE;
        public string leftupShapeKey = NONE;
        public string leftdownShapeKey = NONE;

        public string rightleftShapeKey = NONE;
        public string rightrightShapeKey = NONE;
        public string rightupShapeKey = NONE;
        public string rightdownShapeKey = NONE;
        
        public enum EyeTrackingMode
        {
            EyeBone,
            BlendShape
        }
        #endregion

        #region Blinking
        public const float BLINKING_BLINK = 0.1f;
        public const float BLINKING_NORMAL = 0.8f;
        public const float BLINKING_WIDE = 1.0f;

        public bool enableBlinking;
        public BlinkingMode blinkingMode;

        public string leftBlinkShapeKey = NONE;
        public string rightBlinkShapeKey = NONE;
        public string leftWideShapeKey = NONE;
        public string rightWideShapeKey = NONE;

        public AnimationClip leftBlinkAnimation;
        public AnimationClip rightBlinkAnimation;
        public AnimationClip leftWideAnimation;
        public AnimationClip rightWideAnimation;

        public float blinkingThreshold_Blink = BLINKING_BLINK;
        public float blinkingThreshold_Normal = BLINKING_NORMAL;
        public float blinkingThreshold_Wide = BLINKING_WIDE;

        public enum BlinkingMode
        {
            BlendShape,
            Animation
        }
        #endregion

        #region Pupils
        public const float PUPILS_CONSTRICTED = 0.15f;
        public const float PUPILS_NORMAL = 0.6f;
        public const float PUPILS_DILATED = 0.85f;
        
        public bool enablePupils;
        public string constrictedShapeKey = NONE;
        public string dilatedShapeKey = NONE;

        public float pupilsThreshold_Constricted = PUPILS_CONSTRICTED;
        public float pupilsThreshold_Normal = PUPILS_NORMAL;
        public float pupilsThreshold_Dilated = PUPILS_DILATED;
        #endregion

        #region Mouth
        public bool mouthEnable;

        public MouthAffector[] mouthAffectors = new MouthAffector[37];

        public static readonly string[] mouthParameterNames =
        {
            "JawRight",
            "JawLeft",
            "JawForward",
            "JawOpen",
            "MouthApeShape",
            "MouthUpperRight",
            "MouthUpperLeft",
            "MouthLowerRight",
            "MouthLowerLeft",
            "MouthUpperOverturn",
            "MouthLowerOverturn",
            "MouthPout",
            "MouthSmileRight",
            "MouthSmileLeft",
            "MouthSadRight",
            "MouthSadLeft",
            "CheekPuffRight",
            "CheekPuffLeft",
            "CheekSuck",
            "MouthUpperUpRight",
            "MouthUpperUpLeft",
            "MouthLowerDownRight",
            "MouthLowerDownLeft",
            "MouthUpperInside",
            "MouthLowerInside",
            "MouthLowerOverlay",
            "TongueLongStep1",
            "TongueLongStep2",
            "TongueDown",
            "TongueUp",
            "TongueRight",
            "TongueLeft",
            "TongueRoll",
            "TongueUpLeftMorph",
            "TongueUpRightMorph",
            "TongueDownLeftMorph",
            "TongueDownRightMorph"
        };
        
        [Serializable]
        public sealed class MouthAffector
        {
            public Type type = Type.Float;
            
            public List<AffectedBlendshape> affectedBlendshapes = new List<AffectedBlendshape>();

            public void Add(string blendShape)
            {
                if (blendShape == "") return;
                affectedBlendshapes.Add(new AffectedBlendshape(blendShape));
            }

            public enum Type
            {
                Float
            }

            [Serializable]
            public sealed class AffectedBlendshape
            {
                public string blendShape = NONE;
                public float weight = 100;
                public int selectedBSIndex = 0;

                public AffectedBlendshape(string blendShape = NONE)
                {
                    this.blendShape = blendShape;
                }
            }
        }
        #endregion
    }
#pragma warning restore IDE0090 // Use 'new(...)'
}