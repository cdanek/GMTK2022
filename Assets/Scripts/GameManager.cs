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
//        public AudioClip Barking;
        public AudioClip Cancel;
        public AudioClip PlayCard;
        public AudioClip Win;
        public int Level;
        public GameObject MusicCancelIcon;
        public GameObject FXCancelIcon;
        public GameObject CutScene;
        public TextMeshProUGUI CutSceneText;
        public List<GameObject> CutSceneImages;
        public Image Fader;
        public Button RollsLeftButton;
        public TextMeshProUGUI RestartCampaignButtonText;
        public TextMeshProUGUI CutsceneLevelText;
        private bool _isRestartCampaignSafetyOff = false;
        private List<Tile> InventoryTiles;
        private int maxId = 0;
        private GridPoint _cheesePoint;
        private Tile _cheeseTile;
        private Vector3 cutsceneOffscreen = new Vector3(3000, 450, 0);
        private Vector3 cutsceneOnscreen = new Vector3(800, 450, 0);

        // Game Settings
        private int StartingRolls = 20;
        private int GridWidth = 8;
        private int CatChance = 30;

        private void Awake()
        {
            GridContents = new Dictionary<GridPoint, Tile>();
            InventoryTiles = new List<Tile>();
            LocalAssert();
            StartMusic();
            Fader.color = Color.black;
            CutScene.SetActive(true);
            Fader.gameObject.SetActive(true);
            HideAllCutsceneImages();
            RollsLeftButton.interactable = true;
            if (PlayerPrefs.HasKey("MiceDiceLevel"))
            {
                Level = PlayerPrefs.GetInt("MiceDiceLevel");
                i($"Loading campaign from where we left off.. Level:{Level}");
            }
            else
            {
                e("Couldn't find a mice dice level. Starting campaign fresh.");
                Level = 1;
            }
            CutSceneImages[GetCutsceneImage(Level)].SetActive(true);
            CutSceneText.text = GetCutsceneText(Level);
            CutsceneLevelText.text = $"Level: {Level}";
            StartNewLevel();
            StartCoroutine(SpaceCutsceneTitleCoroutine());
        }

        public void OnRestartCampaignClicked()
        {
            if (_isRestartCampaignSafetyOff)
            {
                Level = 1;
                StartCoroutine(DoCutscene(Level));
                return;
            }
            _isRestartCampaignSafetyOff = true;
            RestartCampaignButtonText.text = "Sure?";
            StartCoroutine(RestartCampaignSafety());
        }

        private IEnumerator RestartCampaignSafety()
        {
            yield return new WaitForSeconds(5f);
            _isRestartCampaignSafetyOff = false;
            RestartCampaignButtonText.text = "Restart";
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
            CutSceneImages[GetCutsceneImage(which)].gameObject.SetActive(true);

            // fade in fader
            seq.Append(Fader.DOColor(Color.black, 1f)).SetEase(Ease.Linear);

            // slide in cutscene
            seq.Insert(0.5f, CutScene.transform.DOMove(cutsceneOnscreen, 0.5f)).SetEase(Ease.InQuint);

            yield return seq.Play().WaitForCompletion();

            CutsceneLevelText.text = $"Level: {Level}";

            StartCoroutine(SpaceCutsceneTitleCoroutine());

            // start the next level while the cutscene is up
            StartNewLevel();

            // ensure roll button is enabled
            RollsLeftButton.interactable = true;
        }

        private IEnumerator SpaceCutsceneTitleCoroutine()
        {
            float startSpacing = 20;
            float endSpacing = 50;
            float duration = 10;

            DateTime startDT = DateTime.Now;
            TimeSpan ts = DateTime.Now - startDT;
            while (ts.TotalSeconds < duration)
            {
                float t = (float)ts.TotalSeconds / duration;

                //https://easings.net/#easeOutBack
                float c1 = 1.70158f;
                float c3 = c1 + 1;
                t = 1f + c3 * (float)Math.Pow(t - 1, 3) + c1 * (float)Math.Pow(t - 1, 2);

                float currentSpacing = Mathf.Lerp(startSpacing, endSpacing, t);
                CutsceneLevelText.characterSpacing = currentSpacing;
                ts = DateTime.Now - startDT;
                yield return null;
            }
            CutsceneLevelText.characterSpacing = endSpacing;
        }

        private string GetCutsceneText(int level) => level switch
        {
            1 => "You are George IV, the king of a small mouse kingdom in northeast Wales. Your goal? Provide your subjects with as much cheese as possible. You were given 2 Mice Dice as a birth gift, but you lost one on your 8th birthday, for which your father has never forgiven you. Thus, armed with but one Mouse Douse - you must build a path for your subjects to the fabled Swiss Alps. Good luck!",
            2 => "Brie is better than Cheddar, but Mozzarella is better than Gouda. No one really knows where Bleu stands, though.",
            3 => "Your illustrious father, George III, may he rest in peace, always warned you to beware the cats. Like, duh. You're a mouse.",
            4 => "As you get further in the campaign, you'll see more and more cats. This is, unfortunately, what happens when landscapes are made of cheese, attracting mice and their dice from all over the realm.",
            5 => "You probably shouldn't have been as surprised as you were that your father was eaten by a cat. As was your brother. And mother. And.. everyone you know, really.",
            6 => "In the distance, you spy the glorious Mount Brie, its' waxen sheath glimmering in the moonlight. You wonder how such a feast could exist, but your mouse brain does not, unfortunately, grasp the concept of industrial dairy production, so you are left without answers.",
            7 => "The morning fog is quite thick today. Smoky almost. Perhaps this biome is a byproduct of the Smoky Cheddar Mountain in the distance? Who knows. Better get to work, though, the light sounds of mewing seem to be getting louder.",
            8 => "Mount Kilimanchego rises in the distance, beckoning you with its' sweet, sweet call. Wait, actually, no, that's just your ring tone. It's your brother Jim - he's in the mousepital, having been, well, eaten whole. You're not exactly sure how they fix that, but you're not a doctor.",
            9 => "Jim didn't make it. Shame - you're down to your last 46 brothers and sisters. It's surprising that you're only up to George the Fourth, but George isn't that common of a mouse name. Henry's dynasty, on the other hand, went all the way up to the 800s.",
            10 => "Sometimes you wonder where all this cheese comes from. Actually, no you don't. Let's eat.",
            11 => "Great news, the base on Muenster Mountain is a whopping 180 cm! Good thing you brought your toothpick ski poles, looks like it's going to be another great day.",
            12 => "Sometimes the crown weighs heavily upon your head. 'We want more cheese!' echo your subjects, almost in a never ending cacophony of mousey noise. HA, just kidding, we all love cheese. Let's get it!",
            13 => "Mew, mew mew, mew mew mew mew. Mew mew? Mew mew. Your efforts at learning the cat language are frustrated by the lack of good teachers.",
            14 => "You've tried to make peace with the cats, but they seem blissfully and ferociously un-sentient.",
            15 => "Sometimes you wonder how a Mouse Douse has the power to create cobbled roads so easily. Was it always this way? What scientific explanation could there be? Is there a god?",
            16 => "Are you still playing?",
            17 => "No, I mean, really. The game is over now.",
            18 => "There's no more text.",
            19 => "That's it, it's over. Go home!",
            20 => "...",
            21 => "Really??",
            22 => "You are the greatest gamer of all time. Will you leave now?",
            23 => "Please go.",
            24 => "I'd like to thank my mother, my father, the academy, and mozzarella.",
            25 => "Hot take: Babybels aren't cheese.",
            26 => "I'm trapped in a clothing factory in Poughkeepsie. Send help!",
            27 => "If you play a game long enough, you'll hear the sound effects in your sleep.",
            28 => "Woof woof! Woof, woof woof, woof. Moo woof moo woof.",
            29 => "Knock knock. Who's there?",
            30 => "The interrupting cow. The interrupt- MOO!!!",
            31 => "That wasn't funny.",
            32 => "It was, a little.",
            33 => "OK, there's no more. Really, this time.",
            _ => "Amazing!",
        };

        private int GetCutsceneImage(int level) => level switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            6 => 5,
            7 => 6,
            8 => 7,
            9 => 8,
            _ => NumberUtils.Next(CutSceneImages.Count),
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

        //private void PlayBarking()
        //{
        //    _meowSource.pitch = NumberUtils.NextFloat(0.8f, 1.2f);
        //    _meowSource.PlayOneShot(Barking);
        //}

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

            RollsLeft = Math.Max(StartingRolls - (Level / 2), 12); // 20->12

            UpdateTextElements();

            // Goal
            int x = NumberUtils.Next(GridWidth);
            _cheesePoint = new GridPoint(x, GridWidth-1 - (NumberUtils.NextBool() ? 1 : 0));
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
            int howManyCats = Math.Min(15, Level + 1);
            for (int i = 0; i < howManyCats; i++)
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
                i("Winner!");
                Level++;
                PlayerPrefs.SetInt("MiceDiceLevel", Level);
                PlayerPrefs.Save();
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
            UpdateTextElements();
            if (RollsLeft == 0) RollsLeftButton.interactable = false;

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
            CreateInventoryTile(newTileType);
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
            RollsLeftText.color = (RollsLeft <= 3) ? Color.red : Color.gray;
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
            StartCoroutine(DoCutscene(Level));
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
