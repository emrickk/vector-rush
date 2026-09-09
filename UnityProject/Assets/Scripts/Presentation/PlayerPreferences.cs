using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    public enum PlayerAction
    {
        Throttle,
        Brake,
        SteerLeft,
        SteerRight,
        AirbrakeLeft,
        AirbrakeRight,
        Boost,
        Recover,
        Pause,
        Confirm,
        Restart
    }

    public sealed class MenuFocusController
    {
        readonly string[] actions;
        int selected;
        public string SelectedAction => actions[selected];
        public int SelectedIndex => selected;
        public IReadOnlyList<string> Actions => actions;

        public MenuFocusController(IEnumerable<string> actionIds)
        {
            if (actionIds == null) throw new ArgumentNullException(nameof(actionIds));
            actions = actionIds.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            if (actions.Length == 0) throw new ArgumentException("At least one focus action is required", nameof(actionIds));
            if (actions.Distinct(StringComparer.Ordinal).Count() != actions.Length) throw new ArgumentException("Focus action IDs must be unique", nameof(actionIds));
        }

        public void Move(int direction)
        {
            if (direction == 0) return;
            selected = (selected + Math.Sign(direction) + actions.Length) % actions.Length;
        }

        public string Activate() => SelectedAction;
        public void Select(int index) => selected = Mathf.Clamp(index, 0, actions.Length - 1);
    }

    public sealed class PlayerPreferences
    {
        const int FileVersion = 1;
        static PlayerPreferences current;
        readonly string filePath;
        PreferenceData data;

        public static PlayerPreferences Current
        {
            get
            {
                if (current == null) current = new PlayerPreferences();
                return current;
            }
        }

        public float MasterVolume => data.masterVolume;
        public float MusicVolume => data.musicVolume;
        public float EffectsVolume => data.effectsVolume;
        public float SteeringSensitivity => data.steeringSensitivity;
        public bool ShakeEnabled => data.shakeEnabled;
        public string FilePath => filePath;

        public PlayerPreferences(string directory = null)
        {
            string root = string.IsNullOrWhiteSpace(directory) ? Application.persistentDataPath : Path.GetFullPath(directory);
            filePath = Path.Combine(root, "player-preferences.json");
            data = Defaults();
            Load();
        }

        public void SetVolumes(float master, float music, float effects)
        {
            data.masterVolume = Mathf.Clamp01(FiniteOr(master, 1f));
            data.musicVolume = Mathf.Clamp01(FiniteOr(music, .72f));
            data.effectsVolume = Mathf.Clamp01(FiniteOr(effects, 1f));
        }

        public void SetSteeringSensitivity(float value) => data.steeringSensitivity = Mathf.Clamp(FiniteOr(value, 1f), .5f, 1.5f);
        public void SetShake(bool enabled) => data.shakeEnabled = enabled;

        public KeyCode BindingFor(PlayerAction action)
        {
            BindingData binding = data.bindings.FirstOrDefault(value => value.action == action);
            return binding == null ? DefaultBinding(action) : binding.key;
        }

        public bool TryRebind(PlayerAction action, KeyCode key, out string error)
        {
            if (key == KeyCode.None)
            {
                error = "A required action cannot be unbound";
                return false;
            }
            BindingContext context = ContextFor(action);
            foreach (BindingData binding in data.bindings)
            {
                if (binding.action == action || binding.key != key || (ContextFor(binding.action) & context) == 0) continue;
                error = key + " is already bound to " + ActionLabel(binding.action);
                return false;
            }
            BindingData target = data.bindings.First(value => value.action == action);
            target.key = key;
            error = null;
            return true;
        }

        public bool IsHeld(PlayerAction action)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(BindingFor(action));
#elif ENABLE_INPUT_SYSTEM
            KeyControl control = ResolveInputSystemKey(BindingFor(action));
            return control != null && control.isPressed;
#else
            return false;
#endif
        }

        public bool WasPressedThisFrame(PlayerAction action)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(BindingFor(action));
#elif ENABLE_INPUT_SYSTEM
            KeyControl control = ResolveInputSystemKey(BindingFor(action));
            return control != null && control.wasPressedThisFrame;
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static KeyControl ResolveInputSystemKey(KeyCode code)
        {
            if (Keyboard.current == null) return null;
            string name = code.ToString();
            if (name == "Return") name = "Enter";
            else if (name == "KeypadEnter") name = "NumpadEnter";
            else if (name.StartsWith("Alpha", StringComparison.Ordinal)) name = "Digit" + name.Substring(5);
            if (!Enum.TryParse(name, true, out Key key)) return null;
            return Keyboard.current[key];
        }
#endif

        public void ResetDefaults() => data = Defaults();

        public void Save()
        {
            Normalize(data);
            string directory = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directory);
            string temporary = filePath + ".tmp-" + Guid.NewGuid().ToString("N");
            string backup = filePath + ".backup-" + Guid.NewGuid().ToString("N");
            try
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(data, false));
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(filePath))
                {
                    File.Replace(temporary, filePath, backup);
                    File.Delete(backup);
                }
                else File.Move(temporary, filePath);
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
                if (File.Exists(backup)) File.Delete(backup);
            }
        }

        void Load()
        {
            if (!File.Exists(filePath)) return;
            try
            {
                PreferenceData loaded = JsonUtility.FromJson<PreferenceData>(File.ReadAllText(filePath));
                if (loaded == null || loaded.version != FileVersion || loaded.bindings == null) return;
                if (!HasEveryActionExactlyOnce(loaded.bindings) || HasConflicts(loaded.bindings)) return;
                Normalize(loaded);
                data = loaded;
            }
            catch (Exception error)
            {
                Debug.LogWarning("Player preferences are malformed and were ignored without modifying the file: " + error.Message);
            }
        }

        static PreferenceData Defaults()
        {
            return new PreferenceData
            {
                version = FileVersion,
                masterVolume = 1f,
                musicVolume = .72f,
                effectsVolume = 1f,
                steeringSensitivity = 1f,
                shakeEnabled = true,
                bindings = Enum.GetValues(typeof(PlayerAction)).Cast<PlayerAction>()
                    .Select(action => new BindingData { action = action, key = DefaultBinding(action) }).ToArray()
            };
        }

        static void Normalize(PreferenceData value)
        {
            value.version = FileVersion;
            value.masterVolume = Mathf.Clamp01(FiniteOr(value.masterVolume, 1f));
            value.musicVolume = Mathf.Clamp01(FiniteOr(value.musicVolume, .72f));
            value.effectsVolume = Mathf.Clamp01(FiniteOr(value.effectsVolume, 1f));
            value.steeringSensitivity = Mathf.Clamp(FiniteOr(value.steeringSensitivity, 1f), .5f, 1.5f);
        }

        static bool HasEveryActionExactlyOnce(IEnumerable<BindingData> bindings)
        {
            PlayerAction[] expected = Enum.GetValues(typeof(PlayerAction)).Cast<PlayerAction>().ToArray();
            PlayerAction[] actual = bindings.Where(value => value != null).Select(value => value.action).ToArray();
            return actual.Length == expected.Length && actual.Distinct().Count() == expected.Length && expected.All(actual.Contains);
        }

        static bool HasConflicts(BindingData[] bindings)
        {
            for (int i = 0; i < bindings.Length; i++)
                for (int j = i + 1; j < bindings.Length; j++)
                    if (bindings[i].key == bindings[j].key && (ContextFor(bindings[i].action) & ContextFor(bindings[j].action)) != 0) return true;
            return false;
        }

        static KeyCode DefaultBinding(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Throttle: return KeyCode.W;
                case PlayerAction.Brake: return KeyCode.S;
                case PlayerAction.SteerLeft: return KeyCode.A;
                case PlayerAction.SteerRight: return KeyCode.D;
                case PlayerAction.AirbrakeLeft: return KeyCode.Q;
                case PlayerAction.AirbrakeRight: return KeyCode.E;
                case PlayerAction.Boost: return KeyCode.Space;
                case PlayerAction.Recover: return KeyCode.R;
                case PlayerAction.Pause: return KeyCode.Escape;
                case PlayerAction.Confirm: return KeyCode.Return;
                case PlayerAction.Restart: return KeyCode.R;
                default: throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        static BindingContext ContextFor(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Confirm:
                case PlayerAction.Restart: return BindingContext.Menu;
                case PlayerAction.Pause: return BindingContext.Gameplay | BindingContext.Menu;
                default: return BindingContext.Gameplay;
            }
        }

        public static string ActionLabel(PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.SteerLeft: return "Steer Left";
                case PlayerAction.SteerRight: return "Steer Right";
                case PlayerAction.AirbrakeLeft: return "Left Airbrake";
                case PlayerAction.AirbrakeRight: return "Right Airbrake";
                default: return action.ToString();
            }
        }

        static float FiniteOr(float value, float fallback) => float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;

        [Flags]
        enum BindingContext { Gameplay = 1, Menu = 2 }

        [Serializable]
        sealed class PreferenceData
        {
            public int version;
            public float masterVolume;
            public float musicVolume;
            public float effectsVolume;
            public float steeringSensitivity;
            public bool shakeEnabled;
            public BindingData[] bindings;
        }

        [Serializable]
        sealed class BindingData
        {
            public PlayerAction action;
            public KeyCode key;
        }
    }
}
