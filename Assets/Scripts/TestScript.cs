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
    public class TestScript : BetterMonoBehaviour, IPointerDownHandler
    {

        public HitText HitTextPrefab;


        public void OnPointerDown(PointerEventData eventData)
        {
        }

        private void Awake()
        {
            Assert.IsNotNull(HitTextPrefab);
        }




    }
}
