using Runner.Core;
using Runner.Fx;
using Runner.Input;
using Runner.Player;
using Runner.UI;
using Runner.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UGuiImage = UnityEngine.UI.Image;
using UGuiText = UnityEngine.UI.Text;
using UGuiButton = UnityEngine.UI.Button;

namespace Runner.EditorTools
{
    /// <summary>
    /// Assembles the playable scene: the parallax rig, the road, the runner, the systems that own
    /// them, and the three UI roots. Everything is generated, so one menu command reproduces the
    /// whole game.
    /// </summary>
    internal static class RunnerSceneBuilder
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = SceneFolder + "/Game.unity";
        private const float GroundTop = -2.6f;
        private const int Tiles = 3;

        private static Font _font;
        private static WorldController _world;

        public static void Build()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            _world = null;
            var camera = BuildCamera();
            var player = BuildPlayer();
            var world = BuildWorld();
            var rig = camera.GetComponent<CameraRig>();
            rig.SetTarget(player.transform);

            BuildSystems(world, player, rig);
            BuildUi();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.625f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Palette.Night;
            camera.nearClipPlane = -100f;
            camera.farClipPlane = 100f;

            go.AddComponent<AudioListener>();

            var rig = go.AddComponent<CameraRig>();
            Wire.On(rig).Set("yOffset", 0.55f).Set("followLerp", 5f).Apply();
            return go;
        }

        private static PlayerCharacter BuildPlayer()
        {
            var go = new GameObject("Player");
            go.transform.position = new Vector3(-5f, GroundTop, 0f);

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 3.6f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.6f, 1.45f);
            collider.offset = new Vector2(0f, 0.76f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = RunnerCharacterArt.Run[0];
            renderer.sortingOrder = SortOrder.Player;

            var animator = go.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.runtimeAnimatorController = RunnerAssets.Controller;

            var character = go.AddComponent<PlayerCharacter>();
            var health = go.AddComponent<PlayerHealth>();
            var driver = go.AddComponent<PlayerAnimationDriver>();

            Wire.On(health).Set("player", character).Set("animator", animator).Set("visual", renderer).Apply();
            Wire.On(driver).Set("player", character).Set("animator", animator).Apply();

            return character;
        }

        private static WorldController BuildWorld()
        {
            var go = new GameObject("World");
            var world = go.AddComponent<WorldController>();
            var spawner = go.AddComponent<RunnerSpawner>();
            _world = world;

            Wire.On(world)
                .Set("groundTopY", GroundTop)
                .Set("airLaneY", 1.2f)
                .Set("spawnX", 13f)
                .Set("despawnX", -13.5f)
                .Set("viewLeftX", -10f)
                .Apply();

            Wire.On(spawner)
                .Set("world", world)
                .Refs("groundHazards", RunnerAssets.GroundHazards)
                .Refs("airHazards", RunnerAssets.AirHazards)
                .Set("coinPrefab", RunnerAssets.Coin)
                .Apply();

            BuildSky(go.transform);
            Band(go.transform, "Far", RunnerAssets.FarMountains, 0.12f, SortOrder.FarLayer, -2.35f);
            Band(go.transform, "City", RunnerAssets.City, 0.3f, SortOrder.MidLayer, -2.5f);
            Band(go.transform, "Ridge", RunnerAssets.Ridge, 0.55f, SortOrder.NearLayer, GroundTop);
            Band(go.transform, "Road", RunnerAssets.Road, 1f, SortOrder.Ground, GroundTop - 4f);
            Band(go.transform, "Foreground", RunnerAssets.Foreground, 1.35f, SortOrder.Foreground, -5.62f);

            var ground = new GameObject("Ground");
            ground.transform.SetParent(go.transform, false);
            ground.transform.localPosition = new Vector3(0f, GroundTop - 2f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(60f, 4f);

            return world;
        }

        private static void BuildSky(Transform parent)
        {
            var go = new GameObject("Sky");
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = RunnerAssets.Sky;
            renderer.sortingOrder = SortOrder.Sky;
        }

        private static void Band(Transform parent, string name, Sprite sprite, float factor, int sorting, float baseY)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"RunnerSceneBuilder: sprite for '{name}' is missing.");
                return;
            }

            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            var width = sprite.bounds.size.x;
            for (var i = 0; i < Tiles; i++)
            {
                var tile = new GameObject($"tile{i}");
                tile.transform.SetParent(root.transform, false);
                tile.transform.localPosition = new Vector3((i - 1) * width, baseY, 0f);

                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sorting;
            }

            var layer = root.AddComponent<ParallaxLayer>();
            Wire.On(layer).Set("world", _world).Set("speedFactor", factor).Apply();
        }

        private static void BuildSystems(WorldController world, PlayerCharacter player, CameraRig rig)
        {
            var go = new GameObject("Systems");

            var input = go.AddComponent<GameInputManager>();
            Wire.On(input).Set("actions", RunnerAssets.InputActions).Apply();

            var score = go.AddComponent<ScoreManager>();
            Wire.On(score).Set("world", world).Apply();

            var pool = go.AddComponent<ObjectPool>();
            Wire.On(pool).Refs("prefabs", RunnerAssets.PoolablePrefabs).Set("prewarmPerPrefab", 8).Apply();

            var effects = go.AddComponent<EffectFactory>();
            Wire.On(effects)
                .Set("world", world)
                .Set("pool", pool)
                .Set("cameraRig", rig)
                .Set("shardSprite", RunnerPropArt.Shard)
                .Set("burstPrefab", RunnerPrefabs.Burst)
                .Set("ghostPrefab", RunnerPrefabs.Ghost)
                .Apply();

            var health = player.GetComponent<PlayerHealth>();
            var driver = player.GetComponent<PlayerAnimationDriver>();
            var spawner = world.GetComponent<RunnerSpawner>();

            // The spawner is built with the world, before the pool exists, so its pool link lands here.
            Wire.On(spawner).Set("pool", pool).Apply();

            Wire.On(player)
                .Set("input", input)
                .Set("world", world)
                .Set("health", health)
                .Set("score", score)
                .Apply();

            Wire.On(driver).Set("world", world).Apply();

            var game = go.AddComponent<GameManager>();
            Wire.On(game)
                .Set("input", input)
                .Set("world", world)
                .Set("spawner", spawner)
                .Set("score", score)
                .Set("player", player)
                .Set("health", health)
                .Set("animationDriver", driver)
                .Set("effects", effects)
                .Apply();
        }

        private static void BuildUi()
        {
            var systems = GameObject.Find("Systems");
            var score = systems.GetComponent<ScoreManager>();
            var game = systems.GetComponent<GameManager>();
            var health = Object.FindAnyObjectByType<PlayerHealth>();

            var canvasGo = new GameObject("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            var uiInput = events.AddComponent<InputSystemUIInputModule>();
            uiInput.actionsAsset = RunnerAssets.InputActions;

            BuildHud(canvasGo.transform, game, score, health);
            BuildMainMenu(canvasGo.transform, game, score);
            BuildPauseMenu(canvasGo.transform, game, score);
            BuildGameOver(canvasGo.transform, game, score);
        }

        private static void BuildHud(Transform parent, GameManager game, ScoreManager score, PlayerHealth health)
        {
            var root = PanelRoot(parent, "HUD", out var group);
            var hudGroup = group;

            var bar = Img(root, "HealthBar", RunnerAssets.RoundedBar, new Vector2(0f, 1f),
                new Vector2(28f, -28f), new Vector2(250f, 34f), Palette.WithAlpha(Palette.Night, 0.85f));

            var fill = Img(bar.rectTransform, "Fill", RunnerAssets.Square, new Vector2(0f, 0.5f),
                new Vector2(11f, 0f), new Vector2(228f, 20f), Palette.Cyan);
            fill.type = UGuiImage.Type.Filled;
            fill.fillMethod = UGuiImage.FillMethod.Horizontal;
            fill.fillOrigin = (int)UGuiImage.OriginHorizontal.Left;

            var pips = new Object[3];
            for (var i = 0; i < pips.Length; i++)
            {
                pips[i] = Img(root, $"Heart{i}", RunnerAssets.Heart, new Vector2(0f, 1f),
                    new Vector2(30f + i * 34f, -74f), new Vector2(26f, 24f), Color.white);
            }

            var label = Label(root, "Score", "0", new Vector2(0.5f, 1f), new Vector2(0f, -40f), 48,
                Palette.White, TextAnchor.MiddleCenter);
            var best = Label(root, "Best", "BEST  0", new Vector2(1f, 1f), new Vector2(-28f, -34f), 24,
                Palette.Amber, TextAnchor.UpperRight);
            var pickup = Label(root, "Pickup", string.Empty, new Vector2(0.5f, 1f), new Vector2(0f, -104f), 26,
                Palette.Amber, TextAnchor.MiddleCenter);

            var hud = root.gameObject.AddComponent<HudController>();
            Wire.On(hud)
                .Set("game", game)
                .Set("score", score)
                .Set("health", health)
                .Set("scoreLabel", label)
                .Set("highScoreLabel", best)
                .Set("pickupLabel", pickup)
                .Set("healthFill", fill)
                .Refs("healthPips", pips)
                .Set("group", group)
                .Apply();
        }

        private static void BuildMainMenu(Transform parent, GameManager game, ScoreManager score)
        {
            var root = PanelRoot(parent, "MainMenu", out var group, 0.78f);

            Label(root, "Title", "NEON RUN", new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), 104,
                Palette.White, TextAnchor.MiddleCenter, Palette.HotPink);
            Label(root, "Subtitle", "AN ENDLESS RUNNER BUILT IN UNITY", new Vector2(0.5f, 0.5f),
                new Vector2(0f, 82f), 24, Palette.IceBlue, TextAnchor.MiddleCenter);

            var best = Label(root, "Best", "BEST  0", new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), 28,
                Palette.Amber, TextAnchor.MiddleCenter);
            var runs = Label(root, "Runs", "FIRST RUN", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), 20,
                Palette.WithAlpha(Palette.White, 0.7f), TextAnchor.MiddleCenter);

            var start = Button(root, "Start", "RUN", new Vector2(0.5f, 0.5f), new Vector2(0f, -96f),
                new Vector2(300f, 64f), 32);
            var quit = Button(root, "Quit", "QUIT", new Vector2(0.5f, 0.5f), new Vector2(0f, -172f),
                new Vector2(220f, 46f), 22);

            var controls = Label(root, "Controls", "SPACE  JUMP      SHIFT  DASH      ESC  PAUSE",
                new Vector2(0.5f, 0f), new Vector2(0f, 44f), 20,
                Palette.WithAlpha(Palette.White, 0.75f), TextAnchor.MiddleCenter);

            var menu = root.gameObject.AddComponent<MainMenuController>();
            Wire.On(menu)
                .Set("startButton", start)
                .Set("quitButton", quit)
                .Set("highScoreLabel", best)
                .Set("runCountLabel", runs)
                .Set("controlsLabel", controls)
                .Set("score", score)
                .Set("group", group)
                .Apply();
        }

        private static void BuildPauseMenu(Transform parent, GameManager game, ScoreManager score)
        {
            var root = PanelRoot(parent, "PauseMenu", out var group, 0.66f);
            Dimmer(root, Palette.WithAlpha(Palette.Night, 0.78f));

            Label(root, "Title", "PAUSED", new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), 68,
                Palette.White, TextAnchor.MiddleCenter, Palette.IceBlue);
            var snapshot = Label(root, "Snapshot", "SCORE  0", new Vector2(0.5f, 0.5f), new Vector2(0f, 72f),
                26, Palette.Amber, TextAnchor.MiddleCenter);

            var resume = Button(root, "Resume", "RESUME", new Vector2(0.5f, 0.5f), new Vector2(0f, -10f),
                new Vector2(320f, 58f), 28);
            var restart = Button(root, "Restart", "RESTART", new Vector2(0.5f, 0.5f), new Vector2(0f, -80f),
                new Vector2(320f, 58f), 28);
            var menu = Button(root, "Menu", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0f, -150f),
                new Vector2(320f, 58f), 28);

            var pause = root.gameObject.AddComponent<PauseMenuController>();
            Wire.On(pause)
                .Set("resumeButton", resume)
                .Set("restartButton", restart)
                .Set("menuButton", menu)
                .Set("snapshotLabel", snapshot)
                .Set("score", score)
                .Set("group", group)
                .Apply();
        }

        private static void BuildGameOver(Transform parent, GameManager game, ScoreManager score)
        {
            var root = PanelRoot(parent, "GameOver", out var group, 0.72f);
            Dimmer(root, Palette.WithAlpha(Palette.Night, 0.72f));

            Label(root, "Title", "GAME OVER", new Vector2(0.5f, 0.5f), new Vector2(0f, 156f), 72,
                Palette.Damage, TextAnchor.MiddleCenter);
            var final = Label(root, "Final", "0", new Vector2(0.5f, 0.5f), new Vector2(0f, 62f), 80,
                Palette.White, TextAnchor.MiddleCenter);
            var best = Label(root, "Best", "BEST  0", new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), 26,
                Palette.Amber, TextAnchor.MiddleCenter);
            var record = Label(root, "Record", "NEW RECORD", new Vector2(0.5f, 0.5f), new Vector2(0f, -58f),
                26, Palette.HotPink, TextAnchor.MiddleCenter);

            var restart = Button(root, "Restart", "RUN AGAIN", new Vector2(0.5f, 0.5f), new Vector2(0f, -130f),
                new Vector2(320f, 58f), 28);
            var menu = Button(root, "Menu", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0f, -198f),
                new Vector2(320f, 58f), 28);

            var over = root.gameObject.AddComponent<GameOverPanel>();
            Wire.On(over)
                .Set("restartButton", restart)
                .Set("menuButton", menu)
                .Set("finalScoreLabel", final)
                .Set("bestScoreLabel", best)
                .Set("recordLabel", record)
                .Set("score", score)
                .Set("group", group)
                .Apply();
        }

        private static RectTransform PanelRoot(Transform parent, string name, out CanvasGroup group, float alpha = 1f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            group = go.GetComponent<CanvasGroup>();
            group.alpha = alpha;
            return rect;
        }

        /// Stretched solid sprite behind a panel's content: the world keeps scrolling into view
        /// otherwise, and amber text over the sun is unreadable.
        private static UGuiImage Dimmer(RectTransform parent, Color color)
        {
            var image = Img(parent, "Dimmer", RunnerAssets.Square, Vector2.zero, Vector2.zero, Vector2.one, color);
            var rect = image.rectTransform;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private static UGuiImage Img(RectTransform parent, string name, Sprite sprite, Vector2 anchor,
            Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UGuiImage));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<UGuiImage>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static UGuiText Label(RectTransform parent, string name, string text, Vector2 anchor,
            Vector2 position, int fontSize, Color color, TextAnchor alignment, Color? glow = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UGuiText));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(900f, fontSize * 2f);
            rect.anchoredPosition = position;

            var label = go.GetComponent<UGuiText>();
            label.font = _font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            if (!glow.HasValue) return label;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = glow.Value;
            shadow.effectDistance = new Vector2(3f, -3f);
            return label;
        }

        private static UGuiButton Button(RectTransform parent, string name, string label, Vector2 anchor,
            Vector2 position, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UGuiImage), typeof(UGuiButton));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<UGuiImage>();
            image.sprite = RunnerAssets.Button;
            image.type = UGuiImage.Type.Sliced;

            var button = go.GetComponent<UGuiButton>();
            button.targetGraphic = image;
            button.colors = new ColorBlock
            {
                normalColor = Palette.WithAlpha(Palette.Violet, 0.95f),
                highlightedColor = Palette.WithAlpha(Palette.RoadLine, 1f),
                selectedColor = Palette.WithAlpha(Palette.RoadLine, 1f),
                pressedColor = Palette.WithAlpha(Palette.DeepPurple, 1f),
                disabledColor = Palette.WithAlpha(Palette.Night, 0.6f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };

            image.color = button.colors.normalColor;

            var text = Label(rect, "Label", label, new Vector2(0.5f, 0.5f), Vector2.zero, fontSize,
                Palette.White, TextAnchor.MiddleCenter);
            text.raycastTarget = false;

            return button;
        }
    }
}
