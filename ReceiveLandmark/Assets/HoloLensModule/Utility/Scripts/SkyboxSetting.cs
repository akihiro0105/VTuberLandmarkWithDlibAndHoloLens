﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA;
#endif

namespace HoloLensModule.Utility
{
    public class SkyboxSetting : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
#if UNITY_2017_2_OR_NEWER
            Camera.main.clearFlags = (HolographicSettings.IsDisplayOpaque == true) ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
#endif
        }
    }
}
