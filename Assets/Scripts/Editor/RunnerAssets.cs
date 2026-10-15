using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runner.EditorTools
{
    /// <summary>
    /// Loads every generated asset from disk once the art pass has run, so the prefab and scene
    /// builders can reference them by role instead of by path.
    /// </summary>
    internal static class RunnerAssets
    {
        public static InputActionAsset InputActions => Field(ref _inputActions,
            () => AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/RunnerInput.inputactions"));

        public static RuntimeAnimatorController Controller => Field(ref _controller,
            () => AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/AnimatorControllers/Runner.controller"));

        public static Sprite Sky => Field(ref _sky, () => Load("Environment/bg_sky"));
        public static Sprite FarMountains => Field(ref _far, () => Load("Environment/bg_far"));
        public static Sprite City => Field(ref _city, () => Load("Environment/bg_city"));
        public static Sprite Ridge => Field(ref _ridge, () => Load("Environment/bg_ridge"));
        public static Sprite Road => Field(ref _road, () => Load("Environment/bg_road"));
        public static Sprite Foreground => Field(ref _foreground, () => Load("Environment/bg_foreground"));

        public static Sprite Panel => Field(ref _panel, () => Load("Ui/ui_panel"));
        public static Sprite Button => Field(ref _button, () => Load("Ui/ui_button"));
        public static Sprite RoundedBar => Field(ref _bar, () => Load("Ui/ui_bar"));
        public static Sprite Square => Field(ref _square, () => Load("Ui/ui_square"));
        public static Sprite Heart => Field(ref _heart, () => Load("Ui/ui_heart"));

        public static GameObject Spike => Field(ref _spike, () => Prefab("hazard_spike"));
        public static GameObject Crate => Field(ref _crate, () => Prefab("hazard_crate"));
        public static GameObject Saw => Field(ref _saw, () => Prefab("hazard_saw"));
        public static GameObject Drone => Field(ref _drone, () => Prefab("hazard_drone"));
        public static GameObject Mine => Field(ref _mine, () => Prefab("hazard_mine"));
        public static GameObject Coin => Field(ref _coin, () => Prefab("pickup_coin"));

        public static GameObject[] GroundHazards => new[] { Spike, Crate, Saw };
        public static GameObject[] AirHazards => new[] { Drone, Mine };

        public static Object[] PoolablePrefabs => new Object[]
        {
            Spike, Crate, Saw, Drone, Mine, Coin, RunnerPrefabs.Burst, RunnerPrefabs.Ghost
        };

        /// <summary>Forces a reload; used after the art pass rewrites the files on disk.</summary>
        public static void Invalidate()
        {
            _inputActions = null;
            _controller = null;
            _sky = _far = _city = _ridge = _road = _foreground = null;
            _panel = _button = _bar = _square = _heart = null;
            _spike = _crate = _saw = _drone = _mine = _coin = null;
        }

        private static T Field<T>(ref T slot, System.Func<T> load) where T : Object
        {
            if (slot == null) slot = load();
            return slot;
        }

        private static Sprite Load(string relative) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{SpriteWriter.SpriteRoot}/{relative}.png");

        private static GameObject Prefab(string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{name}.prefab");

        private static InputActionAsset _inputActions;
        private static RuntimeAnimatorController _controller;
        private static Sprite _sky;
        private static Sprite _far;
        private static Sprite _city;
        private static Sprite _ridge;
        private static Sprite _road;
        private static Sprite _foreground;
        private static Sprite _panel;
        private static Sprite _button;
        private static Sprite _bar;
        private static Sprite _square;
        private static Sprite _heart;
        private static GameObject _spike;
        private static GameObject _crate;
        private static GameObject _saw;
        private static GameObject _drone;
        private static GameObject _mine;
        private static GameObject _coin;
    }
}
