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

        private TileType _tileType;
        public bool IsInventoryTile;
        public GameManager _gameManager;
        private GameObject _gameScreen;
        private Vector2 _dragOffset;
        private Vector2 _startDragLocation;
        private GameObject _inventory;
        public GridPoint GridPoint = GridPoint.Empty;
        private GameObject _gridBoard;

        private Orientation Orientation = Orientation.Up;


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

        private TileType _lastKnownTile;
        public void InitializeInInventory(TileType type)
        {
            GetGameObject(_lastKnownTile).SetActive(false);
            GetGameObject(type).gameObject.SetActive(true);
            _inventory = transform.parent.gameObject;
            _lastKnownTile = type;
        }

        private GameObject GetGameObject(TileType type)  => type switch
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
        /// Rotates clockwise.
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
            _startDragLocation = transform.position;
            gameObject.transform.SetParent(_gameScreen.transform);
            _dragOffset = (Vector2)transform.position - eventData.position;
            v($"Begin drag with offset {_dragOffset}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (!_gameManager.IsValidGridLocation(eventData.position + _dragOffset))
            {
                v("Invalid drop location!");
                //gameObject.transform.position = _startDragLocation;
                gameObject.transform.DOMove(_startDragLocation, 0.2f).SetEase(Ease.OutExpo).Play();
                gameObject.transform.SetParent(_inventory.transform);
                return;
            }
                    
            GridPoint = _gameManager.GetGridPoint(eventData.position + _dragOffset);
            Vector2 newPosition = _gameManager.GetTileLocation(GridPoint);
            gameObject.transform.position = newPosition;
            gameObject.transform.SetParent(_gridBoard.transform);
            IsInventoryTile = false;
            v($"Successful drop on grid: {eventData.position + _dragOffset} at location {GridPoint}");
        }
    }
}
