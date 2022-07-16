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
    public class Inventory : BetterMonoBehaviour
    {
        public List<Tile> Tiles;
        public Tile TilePrefab;
        public GameObject LayoutContainer;

        private void Awake()
        {
            LocalAssert();
            Tiles = new List<Tile>();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(TilePrefab);
            Assert.IsNotNull(LayoutContainer);
        }

        public void CreateTile(TileType type)
        {
            Tile newTile = Instantiate(TilePrefab, LayoutContainer.transform);
            newTile.InitializeInInventory(type, LayoutContainer);
        }

    }
}
