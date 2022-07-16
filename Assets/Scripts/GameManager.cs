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
        public int RollsLeft;
        public Dictionary<GridPoint, Tile> GridContents;
        public TextMeshProUGUI RollsLeftText;

        private int StartingCats = 3;
        private int StartingRolls = 12;
        private int _currentInventory = 0;

        private void Awake()
        {
            GridContents = new Dictionary<GridPoint, Tile>();
            LocalAssert();
            StartNewGame();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(RollsLeftText);
            Assert.IsNotNull(TilePrefab);
            Assert.IsNotNull(Grid);
            Assert.IsNotNull(Inventory);
        }


        private void StartNewGame()
        {
            i("Starting a new game.");

            foreach (GridPoint key in GridContents.Keys)
            {
                Destroy(GridContents[key].gameObject);
            }
            GridContents.Clear();
            _currentInventory = 0;
            RollsLeft = StartingRolls;
            UpdateRollText();

            // Goal
            _cheesePoint = new GridPoint(4, 4);
            _cheeseTile = Instantiate(TilePrefab, Grid.gameObject.transform);
            _cheeseTile.InitializeOnGrid(TileType.Cheese, _cheesePoint, Grid.gameObject);
            GridContents[_cheesePoint] = _cheeseTile;

            // Starting Cross
            GridPoint gp3 = new GridPoint(2, 0);
            Tile tile2 = Instantiate(TilePrefab, Grid.gameObject.transform);
            tile2.InitializeOnGrid(TileType.Cross, gp3, Grid.gameObject);
            GridContents[gp3] = tile2;

            //cats!
            for (int i = 0; i < StartingCats; i++)
            {
                GridPoint catpoint = new GridPoint(NumberUtils.Next(5), NumberUtils.Next(5));
                if (GetTile(catpoint) != null) // try again
                {
                    i--;
                    continue;
                }
                Tile catTile = Instantiate(TilePrefab, Grid.gameObject.transform);
                catTile.InitializeOnGrid(TileType.Cat, catpoint, Grid.gameObject);
                GridContents[catpoint] = catTile;
            }

            //freebie in inventory
            TileType type = GetRandomInventoryTile();
            Inventory.CreateTile(type);
            _currentInventory++;

        }

        private GridPoint _cheesePoint;
        private Tile _cheeseTile;

        public void CheckForWin()
        {
            if (IsConnected(_cheesePoint, _cheeseTile))
            {
                e("Winner!");
                StartNewGame();
            }
        }

        /// <summary>
        /// Returns true if the GridPoint exists on the grid - does not check for tiles there: use GetTile(gridPoint) to check for existence of a tile.
        /// </summary>
        /// <param name="gp"></param>
        /// <returns></returns>
        public bool IsLegalGridPoint(GridPoint gp)
        {
            if (gp.X < 0) return false;
            if (gp.X > 4) return false;
            if (gp.Y < 0) return false;
            if (gp.Y > 4) return false;
            return true;
        }

        /// <summary>
        /// Rolls. Destroys a random inventory if they're full.
        /// </summary>
        public void OnRollClicked()
        {
            RollsLeft--;
            if (_currentInventory == 4)
            {
                _currentInventory--;
                Inventory.DestroyRandomTile();
            }
            _currentInventory++;
            TileType newTileType = GetRandomRollTile();
            i($"Rolled a {newTileType}.");
            Inventory.CreateTile(newTileType);
            UpdateRollText();
        }

        private void UpdateRollText()
        {
            RollsLeftText.text = $"Rolls Left: {RollsLeft}";
        }

        public void DropTile(Tile tile, GridPoint gp)
        {
            GridContents[gp] = tile;
            _currentInventory--;
            CheckForWin();
        }


        public TileType GetRandomRollTile()
        {
            int result = NumberUtils.Next(6);
            return result switch
            {
                0 => TileType.LeftTurn,
                1 => TileType.RightTurn,
                2 => TileType.Straight,
                3 => TileType.Cross,
                4 => TileType.Tee,
                _ => TileType.Star,
            };
        }

        public TileType GetRandomInventoryTile()
        {
            int result = NumberUtils.Next(5);
            return result switch
            {
                0 => TileType.LeftTurn,
                1 => TileType.RightTurn,
                2 => TileType.Straight,
                3 => TileType.Cross,
                _ => TileType.Tee,
            };
        }

        public void OnRestartClicked()
        {
            StartNewGame();
        }


        private bool IsConnected(GridPoint gp, Tile tile)
        {
            bool foundOneConnector = false;
            List<GridPoint> connectingExits = Tile.GetExits(tile.TileType, tile.Orientation, gp);
            //v($"Checking {gp} with type:{tile.TileType} and orientation:{tile.Orientation} for valid connections at: {connectingExits.GetString()}");
            foreach (GridPoint potentialTargetGP in connectingExits)
            {
                if (IsLegalGridPoint(potentialTargetGP))
                {
                    //v($"{potentialTargetGP} is a legal GP, checking...");
                    Tile targetConnector = GetTile(potentialTargetGP);
                    if (targetConnector != null)
                    {
                        //v("There's something here.");
                        List<GridPoint> targetsExits = Tile.GetExits(targetConnector.TileType, targetConnector.Orientation, potentialTargetGP);
                        v($"Checking {potentialTargetGP} with type:{targetConnector.TileType} and orientation:{targetConnector.Orientation} for valid connections at: {targetsExits.GetString()}");
                        if (targetsExits.Contains(gp))
                        {
                            //i("Found a valid connector!");
                            foundOneConnector = true;
                            break;
                        }
                        else
                        {
                            //v($"Our drop point isn't connected. Skipping.");
                        }
                    }
                    else
                    {
                        //v($"Nothing's there. Skipping.");
                    }
                }
                else
                {
                    //v($"{potentialTargetGP} isn't a legal GP, skipping.");
                }
            }

            if (!foundOneConnector)
            {
                //e("Couldn't find a connecting tile.");
                return false;
            }

            return foundOneConnector;
        }

        public bool IsValidGridLocation(Vector2 position, Tile draggedTile)
        {
            if (!IsOnGrid(position)) return false;

            GridPoint gp = GetGridPoint(position);

            if (GridContents.ContainsKey(gp))
            {
                e($"Can't drop there - something else is there: {GridContents[gp]}");
                return false; // grid space is occupied
            }

            // if the dropped item connects to another
            return IsConnected(gp, draggedTile);
        }

        public bool IsOnGrid(Vector2 position) => Grid.IsOnGrid(position);

        public GridPoint GetGridPoint(Vector2 position) => Grid.GetGridPoint(position);

        public Vector2 GetTileLocation(GridPoint gp) => Grid.GetTileLocation(gp);

        public Tile GetTile(GridPoint at)
        {
            bool foundOne = GridContents.TryGetValue(at, out Tile ret);
            return foundOne ? ret : null;
        }

    }
}
