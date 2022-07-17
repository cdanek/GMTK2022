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
    public class Tile : BetterMonoBehaviour, IDragHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
    {
        public GameObject Cat1;
        public GameObject Cat2;
        public GameObject Cat3;
        public GameObject Cat4;
        public GameObject Cheese;
        public GameObject Cheese2;
        public GameObject Cheese3;
        public GameObject Cheese4;
        public GameObject Cheese5;
        public GameObject Cross;
        public GameObject Empty;
        public GameObject LeftTurn;
        public GameObject RightTurn;
        public GameObject Straight;
        public GameObject Tee;
        public TileType TileType;
        public GameManager _gameManager;
        public GridPoint GridPoint = GridPoint.Empty;
        public bool IsInventoryTile;
        public Orientation Orientation = Orientation.Up;
        public int Id;
        public bool IsFogged = true;

        private GameObject _gameScreen;
        private Vector2 _dragOffset;
        private GameObject _inventoryContainer;
        private GameObject _gridBoard;
        private bool _isDragging;

        

        private void Awake()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _gridBoard = FindObjectOfType<GridBoard>().gameObject;
            LocalAssert();
            _gameScreen = _gameManager.gameObject;
            IsInventoryTile = false;
            _isDragging = false;
            IsFogged = true;
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(_gameManager);
            Assert.IsNotNull(Cheese);
            Assert.IsNotNull(Cross);
            Assert.IsNotNull(Empty);
            Assert.IsNotNull(LeftTurn);
            Assert.IsNotNull(RightTurn);
            Assert.IsNotNull(Straight);
            Assert.IsNotNull(Tee);
        }

        public void InitializeInInventory(TileType type, GameObject inventoryContainer, int id)
        {
            TileType = type;
            GetGameObject(TileType).gameObject.SetActive(true);
            IsInventoryTile = true;
            _inventoryContainer = inventoryContainer;
            Id = id;
            IsFogged = false;
            //v($"Created tile of type {type} in inventory with ID{id}");
        }

        /// <summary>
        /// Items on grid are fogged by default. Call UnFog to show them.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="gp"></param>
        /// <param name="gridContainer"></param>
        /// <param name="id"></param>
        public void InitializeOnGrid(TileType type, GridPoint gp, GameObject gridContainer, int id)
        {
            TileType = type;
            GridPoint = gp;
            gameObject.transform.position = _gameManager.GetTileLocation(gp);
            Id = id;
            GetGameObject(TileType.Empty).gameObject.SetActive(true); // show the fog tile.
            //GetGameObject(TileType).gameObject.SetActive(true);
            //v($"Created tile of type {type} on grid at {gp} with ID{id}");
        }

        /// <summary>
        /// Unfogs and returns true if this has a real tile (type != empty). Delete it elsewhere.
        /// </summary>
        public bool UnFog()
        {
            GetGameObject(TileType.Empty).gameObject.SetActive(false); // hide the fog tile.
            if (TileType != TileType.Empty && IsFogged == true) GetGameObject(TileType).gameObject.SetActive(true); // set active if currently fogged
            IsFogged = false;
            return (TileType != TileType.Empty);
        }

        private GameObject GetGameObject(TileType type) => type switch
        {
            TileType.Cat => GetRandomCatGO(),
            TileType.Cheese => GetRandomCheeseGO(),
            TileType.Cross => Cross,
            TileType.Empty => Empty,
            TileType.LeftTurn => LeftTurn,
            TileType.RightTurn => RightTurn,
            TileType.Straight => Straight,
            TileType.Tee => Tee,
            _ => Empty,
        };


        private GameObject GetRandomCheeseGO() => NumberUtils.Next(5) switch
        {
            0 => Cheese,
            1 => Cheese2,
            2 => Cheese3,
            3 => Cheese4,
            _ => Cheese5,
        };

        private GameObject GetRandomCatGO() => NumberUtils.Next(4) switch
        {
            0 => Cat1,
            1 => Cat2,
            2 => Cat3,
            _ => Cat4,
        };

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            //v($"OnDrag: {eventData.position}");
            gameObject.transform.position = eventData.position + _dragOffset;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging) return;
            if (!IsInventoryTile) return;
            //v("Tile rotated.");
            Rotate();
        }

        /// <summary>
        ///  Returns a list of exits. Does not check validity.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="orientation"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<GridPoint> GetExits(TileType type, Orientation orientation, GridPoint location)
        {
            //v($"Getting exits for a {type} tile with {orientation} orientation at {location}.");
            List<GridPoint> noExits = new List<GridPoint>();
            List<GridPoint> allExits = new List<GridPoint>()
            {
                location.Up(),
                location.Down(),
                location.Left(),
                location.Right(),
            };
            if (type == TileType.Empty)
            {
                //v("None.");
                return noExits;
            }
            if (type == TileType.Cat)
            {
                //v("None.");
                return noExits;
            }
            if (type == TileType.Star)
            {
                //v("None.");
                return noExits;
            }
            if (type == TileType.Cheese)
            {
                //v("All of them.");
                return noExits;
            }
            if (type == TileType.Cross) return allExits;

            if (type == TileType.Straight)
            {
                // Straight piece - up/down
                if (orientation == Orientation.Up || orientation == Orientation.Down)
                {
                    return new List<GridPoint>()
                    {
                        location.Up(),
                        location.Down(),
                    };
                }
                // straight piece - left/right
                return new List<GridPoint>()
                {
                        location.Right(),
                        location.Left(),
                };
            }

            if (type == TileType.LeftTurn)
            {
                //   -+
                //    |
                if (orientation == Orientation.Up)
                {
                    return new List<GridPoint>()
                    {
                        location.Down(),
                        location.Left(),
                    };

                }
                //    |
                //   -+
                if (orientation == Orientation.Left)
                {
                    return new List<GridPoint>()
                    {
                        location.Left(),
                        location.Up(),
                    };

                }
                //   |
                //   +-
                if (orientation == Orientation.Down)
                {
                    return new List<GridPoint>()
                    {
                        location.Right(),
                        location.Up(),
                    };

                }
                //   +-
                //   |
                if (orientation == Orientation.Right)
                {
                    return new List<GridPoint>()
                    {
                        location.Right(),
                        location.Down(),
                    };
                }
            }

            if (type == TileType.RightTurn)
            {
                //   +-
                //   |
                if (orientation == Orientation.Up)
                {
                    return new List<GridPoint>()
                    {
                        location.Down(),
                        location.Right(),
                    };

                }
                //  -+
                //   |
                if (orientation == Orientation.Left)
                {
                    return new List<GridPoint>()
                    {
                        location.Left(),
                        location.Down(),
                    };

                }
                //   |
                //  -+
                if (orientation == Orientation.Down)
                {
                    return new List<GridPoint>()
                    {
                        location.Left(),
                        location.Up(),
                    };

                }
                //   |
                //   +-
                if (orientation == Orientation.Right)
                {
                    return new List<GridPoint>()
                    {
                        location.Right(),
                        location.Up(),
                    };
                }
            }

            if (type == TileType.Tee)
            {
                //   |
                //   +-
                //   |
                if (orientation == Orientation.Up)
                {
                    return new List<GridPoint>()
                    {
                        location.Up(),
                        location.Down(),
                        location.Right(),
                    };

                }
                //  -+-
                //   |
                if (orientation == Orientation.Left)
                {
                    return new List<GridPoint>()
                    {
                        location.Right(),
                        location.Left(),
                        location.Down(),
                    };

                }
                //   |
                //  -+
                //   |
                if (orientation == Orientation.Down)
                {
                    return new List<GridPoint>()
                    {
                        location.Left(),
                        location.Up(),
                        location.Down(),
                    };

                }
                //    |
                //   -+-
                if (orientation == Orientation.Right)
                {
                    return new List<GridPoint>()
                    {
                        location.Left(),
                        location.Right(),
                        location.Up(),
                    };
                }
            }

            throw new NotImplementedException();

        }

        /// <summary>
        /// Rotates clockwise. Up -> Left -> Down -> Right -> Up
        /// </summary>
        public void Rotate()
        {
            Orientation--;
            if ((int)Orientation == -1) Orientation = Orientation.Left;
            transform.DORotateQuaternion(Rotation, 0.2f).SetEase(Ease.OutExpo).Play();
        }

        private Quaternion Rotation => Orientation switch
        {
            Orientation.Up => Quaternion.Euler(new(0, 0, 0)),
            Orientation.Right => Quaternion.Euler(new(0, 0, 90)),
            Orientation.Down => Quaternion.Euler(new(0, 0, 180)),
            Orientation.Left => Quaternion.Euler(new(0, 0, 270)),
            _ => Quaternion.Euler(new(0, 270, 0)), // doesn't matter
        };

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInventoryTile) return;
            _isDragging = true;
            gameObject.transform.SetParent(_gameScreen.transform);
            _dragOffset = (Vector2)transform.position - eventData.position;
            //v($"Begin drag with offset {_dragOffset}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (!_gameManager.IsValidGridLocation(eventData.position + _dragOffset, this))
            {
                //v("Invalid drop location!");
                gameObject.transform.SetParent(_inventoryContainer.transform); // this is going to set the destination 
                return;
            }
                    
            GridPoint = _gameManager.GetGridPoint(eventData.position + _dragOffset);
            Vector2 newPosition = _gameManager.GetTileLocation(GridPoint);
            gameObject.transform.position = newPosition;
            gameObject.transform.SetParent(_gridBoard.transform);
            IsInventoryTile = false;
            _gameManager.DropTile(this, GridPoint, Id);
            i($"Successful drop on grid: {eventData.position + _dragOffset} at location {GridPoint}");
        }
    }
}
