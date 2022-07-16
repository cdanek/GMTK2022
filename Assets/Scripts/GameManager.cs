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
        public int RollsLeft;
        public Dictionary<GridPoint, Tile> GridContents;
        public TextMeshProUGUI RollsLeftText;
        public TextMeshProUGUI LevelText;
        public GameObject InventoryLayoutContainer;        
        public AudioSource _musicSource;
        public AudioSource _sfxSource;
        public AudioSource _meowSource; // use this for pitch bending so we don't have to worry about it messing with other sounds.
        public AudioClip BackgroundMusic;
        public AudioClip DiceRoll;
        public AudioClip Meow;
        public AudioClip Barking;
        public AudioClip Cancel;
        public AudioClip PlayCard;
        public AudioClip Win;
        public int Level = 1;
        public GameObject MusicCancelIcon;
        public GameObject FXCancelIcon;
        
        public GameObject CutScene;
        public TextMeshProUGUI CutSceneText;
        public List<GameObject> CutSceneImages;
        public Image Fader;


        private int maxId = 0;
        private List<Tile> InventoryTiles;
        //private int StartingCats = 3; (currently based on level)
        private int StartingRolls = 15;
        private GridPoint _cheesePoint;
        private Tile _cheeseTile;
        private int GridWidth = 8;
        private int CatChance = 30;

        private Vector3 cutsceneOffscreen = new Vector3(3000, 450, 0);
        private Vector3 cutsceneOnscreen = new Vector3(800, 450, 0);

        private void Awake()
        {
            GridContents = new Dictionary<GridPoint, Tile>();
            InventoryTiles = new List<Tile>();
            LocalAssert();
            StartNewLevel();
            StartMusic();
            Fader.color = Color.black;
            CutScene.SetActive(true);
            Fader.gameObject.SetActive(true);
            HideAllCutsceneImages();
            CutSceneImages[0].SetActive(true);
            CutSceneText.text = GetCutsceneText(1);
        }

        private void StartMusic()
        {
            _musicSource.clip = BackgroundMusic;
            _musicSource.volume = 1;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        private void LocalAssert()
        {
            Assert.IsNotNull(InventoryLayoutContainer);
            Assert.IsNotNull(RollsLeftText);
            Assert.IsNotNull(TilePrefab);
            Assert.IsNotNull(Grid);
            Assert.IsNotNull(BackgroundMusic);
            Assert.IsNotNull(DiceRoll);
        }

        public void OnCutsceneButtonClicked()
        {
            StartCoroutine(HideCutsceneCoroutine());
        }

        private void HideAllCutsceneImages()
        {
            foreach (GameObject go in CutSceneImages)
            {
                go.SetActive(false);
            }
        }

        /// <summary>
        /// Does the cutscenes between scenes - can go to the beginning scene if the game is over.
        /// </summary>
        /// <param name="which"></param>
        /// <returns></returns>
        private IEnumerator DoCutscene(int which)
        {
            Sequence seq = DOTween.Sequence();

            // set text and images
            Fader.gameObject.SetActive(true);
            CutScene.SetActive(true);
            HideAllCutsceneImages();
            Fader.color = Color.clear;
            CutScene.transform.position = cutsceneOffscreen;
            CutSceneText.text = GetCutsceneText(which);

            if (which == 1) CutSceneImages[0].gameObject.SetActive(true);
            else if (which == 2) CutSceneImages[1].gameObject.SetActive(true);
            else if (which == 3) CutSceneImages[2].gameObject.SetActive(true);
            else if (which == 4) CutSceneImages[3].gameObject.SetActive(true);
            else CutSceneImages[4].gameObject.SetActive(true); 

            // fade in fader
            seq.Append(Fader.DOColor(Color.black, 1f)).SetEase(Ease.Linear);

            // slide in cutscene
            seq.Insert(0.5f, CutScene.transform.DOMove(cutsceneOnscreen, 0.5f)).SetEase(Ease.InQuint);

            yield return seq.Play().WaitForCompletion();

            // start the next level while the cutscene is up
            if (which != 1) StartNewLevel();
        }

        private string GetCutsceneText(int level) => level switch
        {
            1 => "You are George IV, the king of a small mouse kingdom in northeast Wales. Your goal? Provide your subjects with as much cheese as possible. You were given 2 Mice Dice as a birth gift, but you lost one on your 8th birthday, for which your father has never forgiven you. Thus, armed with but one Mouse Douse - you must build a path for your subjects to the fabled Swiss Alps. Good luck!",
            2 => "He built a great house.",
            3 => "Then he ate a louse.",
            4 => "He wore a blouse.",
            5 => "Once there was a great mouse.",
            6 => "Once there was a great mouse.",
            _ => "Amazing.",
        };

        private IEnumerator HideCutsceneCoroutine()
        {
            float duration = 1f;
            float halfDuration = 0.5f;
            Sequence seq = DOTween.Sequence();

            v($"Start position:{CutScene.transform.position}");

            seq.Append(CutScene.transform.DOMove(cutsceneOffscreen, duration)).SetEase(Ease.OutQuint);
            seq.Insert(halfDuration, Fader.DOColor(Color.clear, halfDuration)).SetEase(Ease.Linear);
            yield return seq.Play().WaitForCompletion();
            
            Fader.color = Color.black;
            CutScene.transform.position = cutsceneOnscreen; // reset
            Fader.gameObject.SetActive(false);
            CutScene.SetActive(false);
        }

        public void CreateInventoryTile(TileType type)
        {
            Tile newTile = Instantiate(TilePrefab, InventoryLayoutContainer.transform);
            newTile.InitializeInInventory(type, InventoryLayoutContainer, ++maxId);
            InventoryTiles.Add(newTile);
        }

        private void PlayMeow()
        {
            _meowSource.pitch = NumberUtils.NextFloat(0.8f, 1.2f);
            _meowSource.PlayOneShot(Meow);
        }

        private void PlayBarking()
        {
            _meowSource.pitch = NumberUtils.NextFloat(0.8f, 1.2f);
            _meowSource.PlayOneShot(Barking);
        }

        private void StartNewLevel()
        {

            i("Starting a new game.");
            foreach (Tile inventoryTile in InventoryTiles)
            {
                Destroy(inventoryTile.gameObject);
            }
            InventoryTiles.Clear();
            maxId++;


            foreach (GridPoint key in GridContents.Keys)
            {
                Destroy(GridContents[key].gameObject);
            }
            GridContents.Clear();
            RollsLeft = StartingRolls;
            UpdateTextElements();

            // Goal
            int x = NumberUtils.Next(GridWidth);
            _cheesePoint = new GridPoint(x, GridWidth-1);
            _cheeseTile = Instantiate(TilePrefab, Grid.gameObject.transform);
            _cheeseTile.InitializeOnGrid(TileType.Cheese, _cheesePoint, Grid.gameObject, ++maxId);
            GridContents[_cheesePoint] = _cheeseTile;

            // Starting Cross
            x = NumberUtils.Next(GridWidth);
            GridPoint startingGridPoint = new GridPoint(x, 0);
            Tile startingCrossTile = Instantiate(TilePrefab, Grid.gameObject.transform);
            startingCrossTile.InitializeOnGrid(TileType.Cross, startingGridPoint, Grid.gameObject, ++maxId);
            GridContents[startingGridPoint] = startingCrossTile;

            //cats!
            for (int i = 0; i < Level + 3; i++)
            {
                GridPoint catpoint = new GridPoint(NumberUtils.Next(GridWidth), NumberUtils.Next(GridWidth));
                if (GetTile(catpoint) != null) // try again
                {
                    i--;
                    continue;
                }
                Tile catTile = Instantiate(TilePrefab, Grid.gameObject.transform);
                catTile.InitializeOnGrid(TileType.Cat, catpoint, Grid.gameObject, ++maxId);
                GridContents[catpoint] = catTile;
            }

            //freebie in inventory
            TileType type = GetRandomInventoryTile();
            CreateInventoryTile(type);

        }

        public bool CheckForWin()
        {
            if (IsConnectedToCheesePoint())
            {
                e("Winner!");
                Level++;
                StartCoroutine(DoCutscene(Level));
                return true;
            }
            return false;
        }

        public bool IsConnectedToCheesePoint()
        {
            //_cheesePoint, _cheeseTile
            List<GridPoint> candidates = new List<GridPoint>()
            {
                _cheesePoint.Up(),
                _cheesePoint.Down(),
                _cheesePoint.Left(),
                _cheesePoint.Right(),
            };
            foreach (GridPoint gp in candidates)
            {
                if (!IsLegalGridPoint(gp)) continue;
                if (!GridContents.TryGetValue(gp, out Tile tile)) continue;
                if (IsPathTile(tile))
                {
                    foreach (GridPoint exit in Tile.GetExits(tile.TileType, tile.Orientation, tile.GridPoint))
                    {
                        if (exit == _cheesePoint) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the GridPoint exists on the grid - does not check for tiles there: use GetTile(gridPoint) to check for existence of a tile.
        /// </summary>
        /// <param name="gp"></param>
        /// <returns></returns>
        public bool IsLegalGridPoint(GridPoint gp)
        {
            if (gp.X < 0) return false;
            if (gp.X > GridWidth-1) return false;
            if (gp.Y < 0) return false;
            if (gp.Y > GridWidth-1) return false;
            return true;
        }

        /// <summary>
        /// Rolls. Destroys a random inventory if they're full.
        /// </summary>
        public void OnRollClicked()
        {
            RollsLeft--;
            bool didDestroyInventoryTile = false;
            if (InventoryTiles.Count == 4)
            {
                didDestroyInventoryTile = true;
                DestroyRandomInventoryTile();
            }

            if (NumberUtils.Next(100) < CatChance)
            {
                GridPoint catpoint = TryFindCatPoint();
                if (catpoint != GridPoint.Empty)
                {
                    Tile catTile = Instantiate(TilePrefab, Grid.gameObject.transform);
                    catTile.InitializeOnGrid(TileType.Cat, catpoint, Grid.gameObject, ++maxId);
                    GridContents[catpoint] = catTile;
                    PlayMeow();
                    return;
                }
            }

            TileType newTileType = GetRandomRollTile();
            //i($"Rolled a {newTileType}.");
            CreateInventoryTile(newTileType);
            UpdateTextElements();
            _sfxSource.PlayOneShot(didDestroyInventoryTile ? Cancel : DiceRoll);
        }

        private bool IsPathTile(Tile tile)
        {
            if (tile.TileType == TileType.Cross) return true;
            if (tile.TileType == TileType.LeftTurn) return true;
            if (tile.TileType == TileType.RightTurn) return true;
            if (tile.TileType == TileType.Tee) return true;
            if (tile.TileType == TileType.Straight) return true;

            return false;
        }


        /// <summary>
        /// returns an empty gridpoint next to a path tile - GridPoint.Empty if it couldn't find one
        /// </summary>
        /// <returns></returns>
        private GridPoint TryFindCatPoint()
        {
            List<GridPoint> allPoints = new List<GridPoint>();
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridWidth; y++)
                {
                    allPoints.Add(new GridPoint(x, y));
                }
            }

            List<GridPoint> candidatePoints = new List<GridPoint>();
            foreach (GridPoint gp in allPoints)
            {
                if (GridContents.ContainsKey(gp.Up()) && IsPathTile(GridContents[gp.Up()]) && !candidatePoints.Contains(gp))
                {
                    candidatePoints.Add(gp);
                    continue;
                }
                if (GridContents.ContainsKey(gp.Down()) && IsPathTile(GridContents[gp.Down()]) && !candidatePoints.Contains(gp))
                {
                    candidatePoints.Add(gp);
                    continue;
                }
                if (GridContents.ContainsKey(gp.Right()) && IsPathTile(GridContents[gp.Right()]) && !candidatePoints.Contains(gp))
                {
                    candidatePoints.Add(gp);
                    continue;
                }
                if (GridContents.ContainsKey(gp.Left()) && IsPathTile(GridContents[gp.Left()]) && !candidatePoints.Contains(gp))
                {
                    candidatePoints.Add(gp);
                    continue;
                }
            }
            //v($"Candidate points:{candidatePoints.Count}");

            if (candidatePoints.Count == 0) return GridPoint.Empty;

            List<GridPoint> candidatePoints2 = new List<GridPoint>();
            foreach (GridPoint gp in candidatePoints)
            {
                if (!GridContents.ContainsKey(gp))
                {
                    //v($"Space at {gp} is empty. Adding to cp2");
                    candidatePoints2.Add(gp);
                }
                else
                {
                    //v($"Space at {gp} is filled: {GridContents[gp].TileType}. Skipping.");
                }
            }
            //v($"Candidate points 2:{candidatePoints.Count}");
            if (candidatePoints2.Count == 0) return GridPoint.Empty;


            return candidatePoints2[NumberUtils.Next(candidatePoints2.Count)];
        }

        private void UpdateTextElements()
        {
            LevelText.text = $"Level: {Level}";
            RollsLeftText.text = $"Rolls Left: {RollsLeft}";
        }

        public void DropTile(Tile tile, GridPoint gp, int id)
        {
            GridContents[gp] = tile;
            Tile removedFromInventoryTile = InventoryTiles.Where(x => x.Id == id).FirstOrDefault();
            if (removedFromInventoryTile == null)
            {
                e($"Couldn't find an inventory tile with that ID: {id}");
                return;
            }
            InventoryTiles.Remove(removedFromInventoryTile);

            bool didWin = CheckForWin();
            if (didWin)
            {
                _sfxSource.PlayOneShot(Win);
            }
            else
            {
                _sfxSource.PlayOneShot(PlayCard);
            }
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
                _ => TileType.Tee,
                //_ => TileType.Star,
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

        public void OnResignClicked()
        {
            Level = 1;
            StartNewLevel();
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
        
        public void DestroyRandomInventoryTile()
        {
            int inventoryTilesCount = InventoryTiles.Count;
            if (inventoryTilesCount == 0)
            {
                e("Trying to destroy an inventory tile with no inventory tiles to destroy.");
                return;
            }
            Tile destroyInventoryTile = InventoryTiles[NumberUtils.Next(inventoryTilesCount)];
            InventoryTiles.Remove(destroyInventoryTile);

            //Destroy(destroyInventoryTile.gameObject);
            StartCoroutine(DestroyTileFromInventoryCoroutine(destroyInventoryTile));
        }

        private IEnumerator DestroyTileFromInventoryCoroutine(Tile destroyTile)
        {
            Vector2 destination = UnityEngine.Random.insideUnitCircle.normalized * 1600 + (Vector2)destroyTile.transform.position;
            destroyTile.transform.SetParent(gameObject.transform);
            Sequence s = DOTween.Sequence();
            
            s.Append(destroyTile.transform.DOScale(new Vector3(2, 2, 2), .5f).SetEase(Ease.Linear));
            
            s.Insert(0.5f, destroyTile.transform.DOMove(destination, .5f).SetEase(Ease.InExpo));
            s.Insert(0.5f, destroyTile.transform.DOScale(Vector3.zero, .5f));
            
            s.SetAutoKill(false).Play();
            
            float rotationTimeScaled = 0;
            while (!s.IsComplete())
            {
                rotationTimeScaled += 400 * Time.deltaTime;
                Vector3 rotationAmount = new Vector3(0, 0, rotationTimeScaled);
                destroyTile.transform.rotation = destroyTile.transform.rotation = Quaternion.Euler(rotationAmount);
                yield return null; // s.WaitForCompletion();
            }
            s.Kill();
            Destroy(destroyTile.gameObject);
        }


        private bool _musicEnabled = true;
        private bool _fxEnabled = true;

        public void OnMusicToggleClicked()
        {
            _musicEnabled = !_musicEnabled;
            MusicCancelIcon.SetActive(!_musicEnabled);
            _musicSource.enabled = _musicEnabled;

            if (!_musicEnabled)
            {
                _musicSource.Stop();
            }
            else
            {
                _musicSource.Play(); // don't need to set clip/info here, it's done in Start()
            }
        }

        public void OnFXToggleClicked()
        {
            _fxEnabled = !_fxEnabled;
            FXCancelIcon.SetActive(!_fxEnabled);
            if (!_fxEnabled)
            {
                _meowSource.Stop();
                _sfxSource.Stop();
            }
            _meowSource.enabled = _fxEnabled;
            _sfxSource.enabled = _fxEnabled;
        }




    }
}
