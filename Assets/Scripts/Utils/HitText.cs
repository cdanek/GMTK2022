//using Unity;
//using UnityEngine.Assertions;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using DG.Tweening;

//namespace KaimiraGames.GameJam
//{
//    public class HitText : BetterMonoBehaviour
//    {
//        public Color StartColor;
//        public Color EndColor;
//        public TextMeshProUGUI Text;

//        private void Awake()
//        {
//            LocalAssert();
//        }

//        private void LocalAssert()
//        {
//            Assert.IsNotNull(Text);
//        }

//        public void ShowAndDestroy(string text, float duration)
//        {
//            gameObject.SetActive(true); // in case the a renderer is using a disabled prefab in the scene for color customization etc
//            Text.text = text;
//            Text.color = StartColor;
//            Vector2 newPos = new(transform.position.x + NumberUtils.Next(-30, 30), transform.position.y + NumberUtils.Next(50, 70));
//            Sequence seq = DOTween.Sequence();
//            seq.Insert(0, transform.DOMove(newPos, duration));
//            seq.Insert(0, Text.DOColor(EndColor, duration));
//            seq.AppendCallback(() => Destroy(gameObject));
//            seq.Play();
//        }

//    }
//}
