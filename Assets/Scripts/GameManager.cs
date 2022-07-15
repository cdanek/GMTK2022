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
    public class GameManager : BetterMonoBehaviour
    {
        public Tile TilePrefab;
        public GridBoard Grid;
        public Inventory Inventory;


        private void Awake()
        {
            LocalAssert();
            StartNewGame();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(TilePrefab);
            Assert.IsNotNull(Grid);
            Assert.IsNotNull(Inventory);
        }


        public int RollsLeft;
        private void StartNewGame()
        {
            RollsLeft = 15;
            i("Starting a new game.");
        }

        public void OnRollClicked()
        {
            RollsLeft--;
            TileType newTileType = GetRandomRollTile();
            i($"Rolled a {newTileType}.");
            Inventory.CreateTile(newTileType);
        }

        public TileType GetRandomRollTile()
        {
            int result = NumberUtils.Next(6);
            return result switch
            {
                1 => TileType.LeftTurn,
                2 => TileType.RightTurn,
                3 => TileType.Straight,
                4 => TileType.Cross,
                5 => TileType.Tee,
                _ => TileType.Star,
            };
        }

        public bool IsValidGridLocation(Vector2 position)
        {
            if (!IsOnGrid(position)) return false;

            GridPoint gp = GetGridPoint(position);

            // if (IsOccupied) return false;

            return true;
        }

        public bool IsOnGrid(Vector2 position) => Grid.IsOnGrid(position);

        public GridPoint GetGridPoint(Vector2 position) => Grid.GetGridPoint(position);

        public Vector2 GetTileLocation(GridPoint gp) => Grid.GetTileLocation(gp);

        public TileType GetTile(GridPoint at)
        {
            return TileType.Empty;
        }

    }
}
