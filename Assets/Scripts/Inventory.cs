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
        public GameObject RootGameObject;

        private void Awake()
        {
            LocalAssert();
            Tiles = new List<Tile>();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(RootGameObject);
            Assert.IsNotNull(TilePrefab);
            Assert.IsNotNull(LayoutContainer);
        }

        public void CreateTile(TileType type)
        {
            Tile newTile = Instantiate(TilePrefab, LayoutContainer.transform);
            newTile.InitializeInInventory(type, LayoutContainer);
            Tiles.Add(newTile);
        }

        // We don't keep track of tiles all that well - Didn't want to make a callback from Tile to inventory.
        // Just find one and delete it.
        public void DestroyRandomTile()
        {
            List<Tile> candidates = new List<Tile>();
            foreach (Tile t in Tiles) if (t.IsInventoryTile) candidates.Add(t);
            int count = candidates.Count;
            if (count == 0)
            {
                e("DestroyRandomTile called with no valid tiles in inventory.");
                return;
            }
            int index = NumberUtils.Next(count);

            Tile destroyThis = candidates[index];
            Tiles.Remove(destroyThis);
            StartCoroutine(DestroyTileFromInventoryCoroutine(destroyThis));
        }

        //TODO: needs some love
        private IEnumerator DestroyTileFromInventoryCoroutine(Tile destroyTile)
        {
            Vector2 destination = UnityEngine.Random.insideUnitCircle.normalized * 900 + (Vector2)destroyTile.transform.position;
            destroyTile.transform.SetParent(RootGameObject.transform);
            yield return destroyTile.transform.DOMove(destination, .5f).SetEase(Ease.OutExpo).Play().WaitForCompletion();
            Destroy(destroyTile.gameObject);
        }

    }
}
