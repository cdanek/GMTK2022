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
        public GameObject Cat;
        public GameObject Cheese;
        public GameObject Cross;
        public GameObject Empty;
        public GameObject LeftTurn;
        public GameObject RightTurn;
        public GameObject Star;
        public GameObject Straight;
        public GameObject Tee;

        public TileType TileType;
        public GameManager _gameManager;
        private GameObject _gameScreen;
        private Vector2 _dragOffset;
        private GameObject _inventoryContainer;
        public GridPoint GridPoint = GridPoint.Empty;
        private GameObject _gridBoard;
        private bool IsInventoryTile;

        public Orientation Orientation = Orientation.Up;


        private void Awake()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _gridBoard = FindObjectOfType<GridBoard>().gameObject;
            LocalAssert();
            _gameScreen = _gameManager.gameObject;
            IsInventoryTile = false;
            _isDragging = false;
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(_gameManager);
            Assert.IsNotNull(Cat);
            Assert.IsNotNull(Cheese);
            Assert.IsNotNull(Cross);
            Assert.IsNotNull(Empty);
            Assert.IsNotNull(LeftTurn);
            Assert.IsNotNull(RightTurn);
            Assert.IsNotNull(Star);
            Assert.IsNotNull(Straight);
            Assert.IsNotNull(Tee);
        }

        //private TileType _lastKnownTile;
        public void InitializeInInventory(TileType type, GameObject inventoryContainer)
        {
            TileType = type;
            GetGameObject(TileType).gameObject.SetActive(true);
            IsInventoryTile = true;
            _inventoryContainer = inventoryContainer;
        }

        public void InitializeOnGrid(TileType type, GridPoint gp, GameObject gridContainer)
        {
            TileType = type;
            GetGameObject(TileType).gameObject.SetActive(true);
            GridPoint = gp;
            gameObject.transform.position = _gameManager.GetTileLocation(gp);
        }

        private GameObject GetGameObject(TileType type) => type switch
        {
            TileType.Cat => Cat,
            TileType.Cheese => Cheese,
            TileType.Cross => Cross,
            TileType.Empty => Empty,
            TileType.LeftTurn => LeftTurn,
            TileType.RightTurn => RightTurn,
            TileType.Star => Star,
            TileType.Straight => Straight,
            TileType.Tee => Tee,
            _ => Empty,
        };

        private bool _isDragging;

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
            v("Tile rotated.");
            Rotate();
        }

        /// <summary>
        ///  Returns true/false if this tile/rotation connects to a given GP. Can handle bad GPs.
        /// </summary>
        /// <param name="gp"></param>
        /// <returns></returns>
        public bool DoesConnectToXXXXXXX(GridPoint potentialSpot, GridPoint targetSpot)
        {
            v($"Checking if a {TileType} tile with rotation {Orientation} at {potentialSpot} would connect to {targetSpot}");

            if (!_gameManager.IsLegalGridPoint(targetSpot))
            {
                v("$The targetspot isn't a legal grid point");
                return false;
            }
            if (!_gameManager.IsLegalGridPoint(potentialSpot))
            {
                v("$The potential/connecting spot isn't a legal grid point");
                return false;
            }
            e("Unknown/TODO type encountered.");
            return true; // todo remove
        }

        //public static bool DoesConnectTo(TileType type, Orientation orientation) => false;

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
            v($"Getting exits for a {type} tile with {orientation} orientation at {location}.");
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
                v("None.");
                return noExits;
            }
            if (type == TileType.Cat)
            {
                v("None.");
                return noExits;
            }
            if (type == TileType.Star)
            {
                v("None.");
                return noExits;
            }
            if (type == TileType.Cheese)
            {
                v("All of them.");
                return allExits;
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
            //transform.rotation = Rotation;
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
            //_startDragLocation = transform.position;
            gameObject.transform.SetParent(_gameScreen.transform);
            _dragOffset = (Vector2)transform.position - eventData.position;
            v($"Begin drag with offset {_dragOffset}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (!_gameManager.IsValidGridLocation(eventData.position + _dragOffset, this))
            {
                v("Invalid drop location!");

                //TODO - Can't animate this with a layout manager - it sets the location on the NEXT frame.
                gameObject.transform.SetParent(_inventoryContainer.transform); // this is going to set the destination 
                //Vector2 startPos = transform.position;
                //Vector2 endPos = transform.position;
                //gameObject.transform.position = startPos;
                //gameObject.transform.DOMove(_startDragLocation, 0.2f).SetEase(Ease.OutExpo).Play();
                //gameObject.transform.DOMove(endPos, 0.2f).SetEase(Ease.OutExpo).Play();
                return;
            }
                    
            GridPoint = _gameManager.GetGridPoint(eventData.position + _dragOffset);
            Vector2 newPosition = _gameManager.GetTileLocation(GridPoint);
            gameObject.transform.position = newPosition;
            gameObject.transform.SetParent(_gridBoard.transform);
            IsInventoryTile = false;
            _gameManager.DropTile(this, GridPoint);
            v($"Successful drop on grid: {eventData.position + _dragOffset} at location {GridPoint}");
        }
    }
}
