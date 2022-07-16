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

    /// <summary>
    /// Renderer for the grid board.
    /// Grid will be center-anchored, but (0,0) will be bottom left.
    /// </summary>
    public class GridBoard : BetterMonoBehaviour
    {
        private readonly float halfWidth = 375f;
        private readonly float halfHeight = 375f;
        private readonly float tileWidth = 150f;
        private readonly float tileHeight = 150f;
        private readonly float halfTileWidth = 75f;
        private readonly float halfTileHeight = 75f;

        private float LeftX => transform.position.x - halfWidth;
        private float RightX => transform.position.x + halfWidth;
        private float TopY => transform.position.y + halfHeight;
        private float BottomY => transform.position.y - halfHeight;



        private void Awake()
        {
            LocalAssert();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(new object());
        }

        public GridPoint GetGridPoint(Vector2 location)
        {
            int x = (int)((location.x - transform.position.x + halfWidth) / tileWidth);
            int y = (int)((location.y - transform.position.y + halfHeight) / tileHeight);
            v($"Location is ({x}, {y})");
            return new GridPoint(x, y);
        }

        public bool IsOnGrid(Vector2 location)
        {
            v($"Checking location {location} against gridX ({LeftX}-{RightX}) and gridY ({BottomY}-{TopY})");

            if (location.x <= LeftX) return false;
            if (location.x >= RightX) return false;
            if (location.y <= BottomY) return false;
            if (location.y >= TopY) return false;

            return true;
        }


        // Center of the grid point
        public Vector2 GetTileLocation(GridPoint gp)
        {
            float x = LeftX + (tileWidth * gp.X) + halfTileWidth;
            float y = BottomY + (tileHeight * gp.Y) + halfTileHeight;
            return new Vector2(x, y);
        }


    }
}
