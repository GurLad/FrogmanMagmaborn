using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SaveMode { Slot, Global }
namespace UnityUtils
{

    /*
     * TODO: 
     * -Fix Appeand & HasKey
     * -Add SaveAll & LoadAll
     * -Add auto-save to files (inefficent, but for lazy people)
     * -Auto save & load when changing slot/starting the game
     * -Add File SaveFileType (after making sure everything works in PlayerPrefs mode)
     */ 


    public enum SaveFileType { PlayerPrefs, File }

    public static class SavedData
    {
        /// <summary>
        /// You must change this through code, as those files are created when the game launches.
        /// </summary>
        public const SaveFileType DEFAULT_GLOBAL_FILES_SAVE_TYPE = SaveFileType.PlayerPrefs;
        /// <summary>
        /// The amount of available save slots. You should probably change this in the code.
        /// </summary>
        public static int MaxNumSlots = 5;
        /// <summary>
        /// If a given key doesn't exist:
        /// <para>- If SaveDefaultData is true, save the default data to the given key (like FilesController).</para>
        /// <para>- If SaveDefaultData is false, return the default data and do nothing (like PlayerPrefs).</para>
        /// </summary>
        public static bool SaveDefaultData = false;
        /// <summary>
        /// Set the current slot. Automatically saves the current one, then loads the next one. If you don't want it to save, simply LoadAll first.
        /// </summary>
        public static int SaveSlot
        {
            get => saveSlot;
            set
            {
                if (value < 0 || value > MaxNumSlots)
                {
                    throw new Exception("Using an invalid slot (" + value + "). Must be negative and below " + MaxNumSlots);
                }
                saveSlot = value;
            }
        }

        private static int saveSlot;
        private static SaveFile GlobalFile = new SaveFile("GlobalSettings", DEFAULT_GLOBAL_FILES_SAVE_TYPE);
        private static SaveFile SlotFile = new SaveFile("Default", DEFAULT_GLOBAL_FILES_SAVE_TYPE);
        private static Dictionary<string, SaveFile> SaveFiles = new Dictionary<string, SaveFile>();

        /// <summary>
        /// Saves a string, int or float to the default file.
        /// </summary>
        /// <typeparam name="T">The type to save (string, int or float).</typeparam>
        /// <param name="dataName">The filename.</param>
        /// <param name="data">The data to save.</param>
        /// <param name="saveMode">Which default file to save to (global or slot-based).</param>
        public static void Save<T>(string dataName, T data, SaveMode saveMode = SaveMode.Slot)
        {
            if (saveMode == SaveMode.Global)
            {
                GlobalFile.Set(dataName, data);
            }
            else
            {
                SlotFile.Set(dataName, data);
            }
        }
        /// <summary>
        /// Saves a string, int or float to a given file.
        /// </summary>
        /// <typeparam name="T">The type to save (string, int or float).</typeparam>
        /// <param name="file">The slot-based file to save.</param>
        /// <param name="dataName">The filename.</param>
        /// <param name="data">The data to save.</param>
        public static void Save<T>(string file, string dataName, T data)
        {
            if (!SaveFiles.ContainsKey(file))
            {
                throw new Exception("Trying to save to a non-existing file (" + file + "). Make sure to CreateFile first.");
            }
            SaveFiles[file].Set(dataName, data);
        }
        /// <summary>
        /// Loads a string, int or float from the default file.
        /// </summary>
        /// <typeparam name="T">The type to load (string, int or float).</typeparam>
        /// <param name="dataName">The filename.</param>
        /// <param name="data">The defult data to save if the file doesn't exist.</param>
        /// <param name="saveMode">Which default file to load from (global or slot-based).</param>
        public static T Load<T>(string dataName, T defaultValue = default, SaveMode saveMode = SaveMode.Slot)
        {
            if (saveMode == SaveMode.Global)
            {
                return GlobalFile.Get(dataName, defaultValue);
            }
            else
            {
                return SlotFile.Get(dataName, defaultValue);
            }
        }
        /// <summary>
        /// Loads a string, int or float from a given file.
        /// </summary>
        /// <typeparam name="T">The type to load (string, int or float).</typeparam>
        /// <param name="file">The slot-based file to load from.</param>
        /// <param name="dataName">The filename.</param>
        /// <param name="data">The defult data to save if the file doesn't exist.</param>
        public static T Load<T>(string file, string dataName, T defaultValue = default)
        {
            if (!SaveFiles.ContainsKey(file))
            {
                throw new Exception("Trying to load from a non-existing file (" + file + "). Make sure to CreateFile first.");
            }
            return SaveFiles[file].Get(dataName, defaultValue);
        }
        /// <summary>
        /// Appeands a string, int or float.
        /// </summary>
        /// <typeparam name="T">The type to appeand (string, int or float).</typeparam>
        /// <param name="dataName">The filename.</param>
        /// <param name="data">The data to appeand.</param>
        public static void Appeand<T>(string dataName, T data, SaveMode saveMode = SaveMode.Slot)
        {
            Type selectedType = typeof(T);
            if (selectedType == typeof(string))
            {
                Save(dataName, Load<T>(dataName).ToString() + data);
            }
            else if (selectedType == typeof(int))
            {
                Save(dataName, Convert.ToInt32(Load<T>(dataName)) + Convert.ToInt32(data));
            }
            else if (selectedType == typeof(float))
            {
                Save(dataName, (float)Convert.ToDouble(Load<T>(dataName)) + (float)Convert.ToDouble(data));
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }
        /// <summary>
        /// Returns whether key dataName exists.
        /// </summary>
        /// <param name="dataName">The name of the key.</param>
        /// <param name="saveMode">The save mode</param>
        /// <returns></returns>
        public static bool HasKey(string dataName, SaveMode saveMode = SaveMode.Slot)
        {
            dataName = (saveMode != SaveMode.Global ? SaveSlot.ToString() : "") + dataName;
            return PlayerPrefs.HasKey(dataName);
        }
    }

    // I know that writing multiple classes in the same file is a bad practice, but Unity requires so many script files anyway, I'd rather reduce that amount.

    internal class SaveFile
    {
        public string Name { get; }
        public SaveFileType Type { get; set; }
        public Dictionary<string, string> StringValues { get; } = new Dictionary<string, string>();
        public Dictionary<string, int> IntValues { get; } = new Dictionary<string, int>();
        public Dictionary<string, float> FloatValues { get; } = new Dictionary<string, float>();
        public bool Autosave { get; set; } = false;

        public SaveFile(string name, SaveFileType type)
        {
            Name = name;
            Type = type;
        }

        public void Save(int slot)
        {
            switch (Type)
            {
                case SaveFileType.PlayerPrefs:
                    PlayerPrefsSaveDictionary(StringValues, PlayerPrefs.SetString, slot);
                    PlayerPrefsSaveDictionary(IntValues, PlayerPrefs.SetInt, slot);
                    PlayerPrefsSaveDictionary(FloatValues, PlayerPrefs.SetFloat, slot);
                    break;
                case SaveFileType.File:
                    break;
                default:
                    break;
            }
        }

        private void PlayerPrefsSaveDictionary<T>(Dictionary<string, T> dictionary, Action<string, T> saveFunction, int slot)
        {
            string allKeys = "";
            foreach (string key in dictionary.Keys)
            {
                saveFunction(slot + Name + key, dictionary[key]);
                allKeys += key + ";";
            }
            PlayerPrefs.SetString("AllKeys" + slot + Name + typeof(T).ToString(), allKeys.Substring(0, allKeys.Length - 1));
        }

        public void Load(int slot)
        {
            switch (Type)
            {
                case SaveFileType.PlayerPrefs:
                    PlayerPrefsLoadDictionary(StringValues, PlayerPrefs.GetString, slot);
                    PlayerPrefsLoadDictionary(IntValues, PlayerPrefs.GetInt, slot);
                    PlayerPrefsLoadDictionary(FloatValues, PlayerPrefs.GetFloat, slot);
                    break;
                case SaveFileType.File:
                    break;
                default:
                    break;
            }
        }

        private void PlayerPrefsLoadDictionary<T>(Dictionary<string, T> dictionary, Func<string, T> loadFunction, int slot)
        {
            dictionary.Clear();
            string[] allKeys = PlayerPrefs.GetString("AllKeys" + slot + Name + typeof(T).ToString()).Split(';');
            foreach (string key in allKeys)
            {
                dictionary.Add(key, loadFunction(slot + Name + key));
            }
        }

        public void Set<T>(string dataName, T data)
        {
            Type selectedType = typeof(T);
            if (selectedType == typeof(string))
            {
                Set(StringValues, dataName, data.ToString());
            }
            else if (selectedType == typeof(int))
            {
                Set(IntValues, dataName, Convert.ToInt32(data));
            }
            else if (selectedType == typeof(float))
            {
                Set(FloatValues, dataName, (float)Convert.ToDouble(data));
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        private void Set<T>(Dictionary<string, T> dictionary, string dataName, T data)
        {
            if (!dictionary.ContainsKey(dataName))
            {
                dictionary.Add(dataName, data);
            }
            else
            {
                dictionary[dataName] = data;
            }
        }

        public T Get<T>(string dataName, T defaultValue = default)
        {
            Type selectedType = typeof(T);
            if (selectedType == typeof(string))
            {
                return (T)Convert.ChangeType(Get(StringValues, dataName, defaultValue.ToString()), typeof(T));
            }
            else if (selectedType == typeof(int))
            {
                return (T)Convert.ChangeType(Get(IntValues, dataName, Convert.ToInt32(defaultValue)), typeof(T));
            }
            else if (selectedType == typeof(float))
            {
                return (T)Convert.ChangeType(Get(FloatValues, dataName, (float)Convert.ToDouble(defaultValue)), typeof(T));
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        private T Get<T>(Dictionary<string, T> dictionary, string dataName, T defaultData)
        {
            if (!dictionary.ContainsKey(dataName))
            {
                if (SavedData.SaveDefaultData) // Should anyone change that
                {
                    dictionary.Add(dataName, defaultData);
                }
                return defaultData;
            }
            else
            {
                return dictionary[dataName];
            }
        }

        public void Appeand<T>(string dataName, T data, SaveMode saveMode = SaveMode.Slot)
        {
            Type selectedType = typeof(T);
            if (selectedType == typeof(string))
            {
                Set(dataName, Get<T>(dataName).ToString() + data);
            }
            else if (selectedType == typeof(int))
            {
                Set(dataName, Convert.ToInt32(Get<T>(dataName)) + Convert.ToInt32(data));
            }
            else if (selectedType == typeof(float))
            {
                Set(dataName, (float)Convert.ToDouble(Get<T>(dataName)) + (float)Convert.ToDouble(data));
            }
            else
            {
                throw new Exception("Unsupported type");
            }
        }

        public bool HasKey(string dataName, SaveMode saveMode = SaveMode.Slot)
        {
            return StringValues.ContainsKey(dataName) || IntValues.ContainsKey(dataName) || FloatValues.ContainsKey(dataName);
        }
    }
}