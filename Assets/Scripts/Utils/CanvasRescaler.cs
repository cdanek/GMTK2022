using Unity;
using UnityEngine.Assertions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;
using static KaimiraGames.GameJam.Logging;

namespace KaimiraGames.GameJam
{
    public class CanvasRescaler : BetterMonoBehaviour
    {
        public bool IsWide { get => _isWide; private set => _isWide = value; }

        private int _lastKnownWidth = -1;
        private int _lastKnownHeight = -1;
        private bool _isWide = true;
        private bool _isFirstOrientationEvent = true; // used to force an event raised on the first tick
        private readonly float _magicAspectRatio = 1.77777777777777777f;
        private CanvasScaler _canvasScaler;

        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            SetOrientation();
        }

        private void SetOrientation()
        {
            //v("Setting orientation and canvas scale.");
            _canvasScaler.matchWidthOrHeight = (IsWide ? 1f : 0f);
        }

        public void Update()
        {
            if (_lastKnownWidth != Screen.width || _lastKnownHeight != Screen.height)
            {
                _lastKnownWidth = Screen.width;
                _lastKnownHeight = Screen.height;
                float aspectRatio = ((float)_lastKnownWidth / (float)_lastKnownHeight);
                bool oldIsWide = IsWide;
                IsWide = (aspectRatio > _magicAspectRatio);
                bool didIsWideChange = (oldIsWide != IsWide);

                if (didIsWideChange || _isFirstOrientationEvent)
                {
                    _isFirstOrientationEvent = false;
                    SetOrientation();
                }
            }
        }


    }
}
