﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/* On the generic type functions:
 * Generic types are a bad idea here, as only three types are supported. However, copying & pasting the save & load functions 12 times is bad too. Here are the options:
 * -Current (generic types with if)
 * -Overloading (tons of copy-pastes, although could be one-liners)
 * -A basic class with casting overrides for string/int/float (most elegant, but weird & harder to discern int vs. float in editor)
 */


public enum SaveFileType { PlayerPrefs, File }
public enum SaveMode { Slot, Global }

public static class SavedData
{
    private static string _prefix = "";
    public static string Prefix // To prevent moddable builds from modifying the game's saved data
    {
        get
        {
#if MODDABLE_BUILD && !UNITY_EDITOR
            return "Mod" + _prefix;
#else
            return _prefix;
#endif
        }
        set
        {
            _prefix = value;
        }
    }
    /// <summary>
    /// You must change this through code, as those files are created when the game launches.
    /// </summary>
    public const SaveFileType DEFAULT_GLOBAL_FILES_SAVE_TYPE = SaveFileType.File;
    /// <summary>
    /// You must change this through code, as those files are created when the game launches.
    /// </summary>
    public const bool DEFAULT_FILES_AUTO_SAVE = false;
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
    /// <para><b>Changing slots might close and open new files, based on the ones in that slot - use carefully.</b> I recommand always having the same files in every slot.</para>
    /// </summary>
    public static int SaveSlot
    {
        get => saveSlot;
        set
        {
            if (value < 0 || value > MaxNumSlots)
            {
                throw new SavedDataException("Using an invalid slot (" + value + "). Must be positive and below " + MaxNumSlots);
            }
            if (value != saveSlot)
            {
                SaveAll(SaveMode.Slot, saveSlot);
                saveSlot = value;
                LoadAll(SaveMode.Slot, saveSlot);
            }
        }
    }

    private static int saveSlot = 0;
    private static SaveFile GlobalFile;
    private static SaveFile SlotFile;
    private static Dictionary<string, SaveFile> SaveFiles = new Dictionary<string, SaveFile>();

    /// <summary>
    /// Initializes the GlobalFile and a default SlotFile.
    /// This function should only ever be called once - before any other SavedData action is taken, but after loading the prefix.
    /// </summary>
    public static void InitFiles()
    {
        if (GlobalFile != null)
        {
            throw new SavedDataException("GlobalFile already exists!");
        }
        GlobalFile = new SaveFile("GlobalSettings", DEFAULT_GLOBAL_FILES_SAVE_TYPE, -1);
        SlotFile = new SaveFile("Default", SaveFileType.File, 0);
    }
    /// <summary>
    /// Creates a new file. <b>Use this only when creating new slots, and make sure to always have the same files in each slot.</b>
    /// <para>While SavedData can handle different slots having different files, it would be a pain to maintain, as changing slots closes all previous files.</para>
    /// </summary>
    /// <param name="fileName">The filename.</param>
    /// <param name="type">The type of the new file. Use PlayerPrefs for things like settings, and File for more complex things like player stats.</param>
    /// <param name="saveSlot">Creates a file based on one from a different slot. <b>Does not change the current slot - the file will be saved to it.</b></param>
    public static void CreateFile(string fileName, SaveFileType type, int saveSlot = -1, bool autoSave = DEFAULT_FILES_AUTO_SAVE)
    {
        if (fileName.Contains(',') || fileName.Contains(';') || fileName == "")
        {
            throw new SavedDataException("File name cannot be empty or contain ',' and ';' (" + fileName + ").");
        }
        if (SaveFiles.ContainsKey(fileName))
        {
            if (SaveFiles[fileName].Type != type)
            {
                throw new SavedDataException("Conflicting with an existing file: trying to create a " + type + " type SaveFile named " + fileName + ", while one typed " + SaveFiles[fileName].Type + " already exists.");
            }
            return;
        }
        saveSlot = saveSlot >= 0 ? saveSlot : SaveSlot;
        SaveFiles.Add(fileName, new SaveFile(fileName, type, saveSlot, autoSave));
    }
    /// <summary>
    /// Saves a string, int or float to the default file.
    /// </summary>
    /// <typeparam name="T">The type to save (string, int or float).</typeparam>
    /// <param name="dataName">The filename.</param>
    /// <param name="data">The data to save.</param>
    /// <param name="saveMode">Which default file to save to (global or slot-based).</param>
    public static void Save<T>(string dataName, T data, SaveMode saveMode = SaveMode.Slot)
    {
        CheckDatanameValid(dataName);
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
            throw new SavedDataException("Trying to save to a non-existing file (" + file + "). Make sure to CreateFile first.");
        }
        CheckDatanameValid(dataName);
        SaveFiles[file].Set(dataName, data);
    }
    /// <summary>
    /// Loads a string, int or float from the default file.
    /// </summary>
    /// <typeparam name="T">The type to load (string, int or float).</typeparam>
    /// <param name="dataName">The filename.</param>
    /// <param name="defaultValue">The defult data to save if the file doesn't exist.</param>
    /// <param name="saveMode">Which default file to load from (global or slot-based).</param>
    public static T Load<T>(string dataName, T defaultValue = default, SaveMode saveMode = SaveMode.Slot)
    {
        CheckDatanameValid(dataName);
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
    /// <param name="defaultValue">The defult data to save if the file doesn't exist.</param>
    public static T Load<T>(string file, string dataName, T defaultValue = default)
    {
        if (!SaveFiles.ContainsKey(file))
        {
            throw new SavedDataException("Trying to load from a non-existing file (" + file + "). Make sure to CreateFile first.");
        }
        CheckDatanameValid(dataName);
        return SaveFiles[file].Get(dataName, defaultValue);
    }
    /// <summary>
    /// Appends a string, int or float in the default file.
    /// </summary>
    /// <typeparam name="T">The type to append (string, int or float).</typeparam>
    /// <param name="dataName">The filename.</param>
    /// <param name="data">The data to append.</param>
    public static void Append<T>(string dataName, T data, SaveMode saveMode = SaveMode.Slot)
    {
        if (saveMode == SaveMode.Global)
        {
            GlobalFile.Append(dataName, data);
        }
        else
        {
            SlotFile.Append(dataName, data);
        }
    }
    /// <summary>
    /// Appends a string, int or float in a given file.
    /// </summary>
    /// <typeparam name="T">The type to append (string, int or float).</typeparam>
    /// <param name="file">The slot-based file to load from.</param>
    /// <param name="dataName">The filename.</param>
    /// <param name="data">The data to append.</param>
    public static void Append<T>(string file, string dataName, T data)
    {
        if (!SaveFiles.ContainsKey(file))
        {
            throw new SavedDataException("Trying to append a non-existing file (" + file + "). Make sure to CreateFile first.");
        }
        SaveFiles[file].Append(dataName, data);
    }
    /// <summary>
    /// Returns whether key dataName exists in the default file.
    /// </summary>
    /// <param name="dataName">The name of the key.</param>
    /// <param name="saveMode">The save mode</param>
    /// <returns></returns>
    public static bool HasKey(string dataName, SaveMode saveMode = SaveMode.Slot)
    {
        if (saveMode == SaveMode.Global)
        {
            return GlobalFile.HasKey(dataName);
        }
        else
        {
            return SlotFile.HasKey(dataName);
        }
    }
    /// <summary>
    /// Returns whether key dataName exists in the given file.
    /// </summary>
    /// <param name="file">The filename.</param>
    /// <param name="dataName">The name of the key.</param>
    /// <returns></returns>
    public static bool HasKey(string file, string dataName)
    {
        if (!SaveFiles.ContainsKey(file))
        {
            throw new SavedDataException("Trying to check key in a non-existing file (" + file + "). Make sure to CreateFile first.");
        }
        return SaveFiles[file].HasKey(dataName);
    }
    /// <summary>
    /// Saves all data, using each file's save mode. Use on checkpoints, save &amp; quit etc.
    /// <para>Called automatically when changing slots.</para>
    /// </summary>
    /// <param name="saveMode">Global to only save the global file (controls etc.), Slot to save the current slot.</param>
    /// <param name="saveSlot">Specify a save slot other than the currently selected one. For save data duplicating mostly.</param>
    public static void SaveAll(SaveMode saveMode = SaveMode.Slot, int saveSlot = -1, bool forceSave = false)
    {
        if (saveMode == SaveMode.Global)
        {
            GlobalFile.Save(-1);
        }
        else
        {
            saveSlot = saveSlot >= 0 ? saveSlot : SaveSlot;
            SlotFile.Save(saveSlot, forceSave);
            PlayerPrefs.SetString(Prefix + "AllFiles" + saveSlot, string.Join(";", SaveFiles.Values));
            foreach (SaveFile file in SaveFiles.Values)
            {
                file.Save(saveSlot, forceSave);
            }
        }
    }
    /// <summary>
    /// Loads all data, closing &amp; saving new files as appropriate. Use at the beggining of the game, when loading a previous save etc.
    /// <para>Called automatically when changing slots.</para>
    /// </summary>
    /// <param name="saveMode">Global to only load the global file (controls etc.), Slot to load the current slot.</param>
    /// <param name="saveSlot">Specify a save slot other than the currently selected one. For save data duplicating mostly.</param>
    public static void LoadAll(SaveMode saveMode = SaveMode.Slot, int saveSlot = -1)
    {
        if (saveMode == SaveMode.Global)
        {
            GlobalFile.Load(-1);
        }
        else
        {
            saveSlot = saveSlot >= 0 ? saveSlot : SaveSlot;
            SlotFile.Load(saveSlot);
            // Find the target files
            string[] temp = PlayerPrefs.GetString(Prefix + "AllFiles" + saveSlot).Split(';');
            List<string[]> files = new List<string[]>();
            foreach (var item in temp)
            {
                files.Add(item.Split(','));
            }
            // Remove unnecessery files from this slot, and load similar ones.
            foreach (string key in SaveFiles.Keys.ToList())
            {
                if (files.Find(a => a[0] == key) == null)
                {
                    SaveFiles.Remove(key);
                }
                else
                {
                    SaveFiles[key].Load(saveSlot);
                }
            }
            // Create all files from this slot, and load them.
            foreach (string[] file in files)
            {
                if (file[0] != "" && !SaveFiles.ContainsKey(file[0]))
                {
                    CreateFile(file[0], (SaveFileType)int.Parse(file[1]), saveSlot);
                }
            }
        }
    }
    /// <summary>
    /// Deletes all data from all open files. This does not save the deleted data - you need to call SaveAll in order to actually delete everything.
    /// </summary>
    /// <param name="saveMode">Global to only delete the global file (controls etc.), Slot to delete the current slot.</param>
    /// <param name="saveSlot">Specify a save slot other than the currently selected one. For save data duplicating mostly.</param>
    public static void DeleteAll(SaveMode saveMode = SaveMode.Slot, int saveSlot = -1)
    {
        if (saveMode == SaveMode.Global)
        {
            GlobalFile.Delete(-1);
        }
        else
        {
            saveSlot = saveSlot >= 0 ? saveSlot : SaveSlot;
            SlotFile.Delete(saveSlot);
            PlayerPrefs.SetString(Prefix + "AllFiles" + saveSlot, string.Join(";", SaveFiles.Values));
            foreach (SaveFile file in SaveFiles.Values)
            {
                file.Delete(saveSlot);
            }
        }
    }
    /// <summary>
    /// Frogman Magmaborn specific - creates the files all save slots have.
    /// </summary>
    public static void CreateSaveSlotFiles()
    {
        CreateFile("Knowledge", SaveFileType.File);
        CreateFile("ConversationData", SaveFileType.File);
        CreateFile("Statistics", SaveFileType.File);
        CreateFile("SuspendData", SaveFileType.File);
        CreateFile("Log", SaveFileType.File);
        CreateFile("Achievements", SaveFileType.File);
    }

    private static void CheckDatanameValid(string dataName)
    {
        if (dataName.Contains(":"))
        {
            throw new SavedDataException("Data name cannot contain ':' (" + dataName + ").");
        }
    }

    public class SavedDataException : Exception
    {
        public SavedDataException(string what) : base("SavedData error: " + what) { }
    }

    private class SaveFile
    {
        public string Name { get; }
        public SaveFileType Type { get; }
        public Dictionary<string, string> StringValues { get; } = new Dictionary<string, string>();
        public Dictionary<string, int> IntValues { get; } = new Dictionary<string, int>();
        public Dictionary<string, float> FloatValues { get; } = new Dictionary<string, float>();
        public bool AutoSave { get; }
        private bool dataChanged; // More efficient in SaveAll

        public SaveFile(string name, SaveFileType type, int slot, bool autoSave = SavedData.DEFAULT_FILES_AUTO_SAVE)
        {
            Name = name;
            Type = type;
            Load(slot);
            AutoSave = autoSave;
        }

        public void Save(int slot, bool forceSave = false)
        {
            if (!dataChanged && !forceSave)
            {
                return;
            }
            switch (Type)
            {
                case SaveFileType.PlayerPrefs:
                    PlayerPrefsSaveDictionary(StringValues, PlayerPrefs.SetString, slot);
                    PlayerPrefsSaveDictionary(IntValues, PlayerPrefs.SetInt, slot);
                    PlayerPrefsSaveDictionary(FloatValues, PlayerPrefs.SetFloat, slot);
                    break;
                case SaveFileType.File:
                    if (!System.IO.Directory.Exists(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot))
                    {
                        System.IO.Directory.CreateDirectory(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot);
                    }
                    FileSaveDictionary(StringValues, slot);
                    FileSaveDictionary(IntValues, slot);
                    FileSaveDictionary(FloatValues, slot);
                    break;
                default:
                    break;
            }
            dataChanged = false;
        }

        private void PlayerPrefsSaveDictionary<T>(Dictionary<string, T> dictionary, Action<string, T> saveFunction, int slot)
        {
            string allKeys = "";
            foreach (string key in dictionary.Keys)
            {
                saveFunction(Prefix + slot + Name + key, dictionary[key]);
                allKeys += key + ";";
            }
            if (allKeys.Length > 0)
            {
                PlayerPrefs.SetString(Prefix + "AllKeys" + slot + Name + typeof(T).Name, allKeys.Substring(0, allKeys.Length - 1));
            }
        }

        private void FileSaveDictionary<T>(Dictionary<string, T> dictionary, int slot)
        {
            // Dictionary ToString
            string result = "";
            foreach (string key in dictionary.Keys)
            {
                result += key + ":" + dictionary[key] + '\r';
            }
            // Save file
            string path = Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot + "/" + Name + "Type" + typeof(T).Name + "s.data";
            if (result.Length > 0)
            {
                System.IO.File.WriteAllText(path, result.Substring(0, result.Length - 1));
            }
            else if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
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
                    if (!System.IO.Directory.Exists(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot))
                    {
                        System.IO.Directory.CreateDirectory(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot);
                    }
                    FileLoadDictionary(StringValues, slot);
                    FileLoadDictionary(IntValues, slot);
                    FileLoadDictionary(FloatValues, slot);
                    break;
                default:
                    break;
            }
            dataChanged = false;
        }

        private void PlayerPrefsLoadDictionary<T>(Dictionary<string, T> dictionary, Func<string, T> loadFunction, int slot)
        {
            dictionary.Clear();
            string[] allKeys = PlayerPrefs.GetString(Prefix + "AllKeys" + slot + Name + typeof(T).Name).Split(';');
            foreach (string key in allKeys)
            {
                if (key == "")
                {
                    continue;
                }
                dictionary.Add(key, loadFunction(Prefix + slot + Name + key));
            }
        }

        private void FileLoadDictionary<T>(Dictionary<string, T> dictionary, int slot)
        {
            dictionary.Clear();
            // Load file
            if (!System.IO.File.Exists(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot + "/" + Name + "Type" + typeof(T).Name + "s.data"))
            {
                return;
            }
            string result = System.IO.File.ReadAllText(Application.persistentDataPath + (Prefix != "" ? ("/" + Prefix) : "") + "/Slot" + slot + "/" + Name + "Type" + typeof(T).Name + "s.data");
            // Dictionary FromString
            if (result == "")
            {
                return;
            }
            string[] pairs = result.Split('\r');
            foreach (string pair in pairs)
            {
                string[] parts = pair.Split(':');
                Type selectedType = typeof(T);
                if (selectedType == typeof(string))
                {
                    Set(StringValues, parts[0], string.Join(":", parts, 1, parts.Length - 1));
                }
                else if (selectedType == typeof(int))
                {
                    Set(IntValues, parts[0], Convert.ToInt32(parts[1]));
                }
                else if (selectedType == typeof(float))
                {
                    Set(FloatValues, parts[0], (float)Convert.ToDouble(parts[1]));
                }
            }
        }

        public void Delete(int slot)
        {
            StringValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            dataChanged = true;
        }

        public void Set<T>(string dataName, T data)
        {
            Type selectedType = typeof(T);
            if (selectedType == typeof(string))
            {
                if (data.ToString().Contains('\r'))
                {
                    throw new SavedDataException("Saving data with " + @"'\r'" + " is currently not supported (" + data.ToString() + ").");
                }
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
                throw new SavedDataException("Unsupported type");
            }
            if (AutoSave)
            {
                Save(SavedData.GlobalFile == this ? -1 : SavedData.SaveSlot);
            }
        }

        private void Set<T>(Dictionary<string, T> dictionary, string dataName, T data)
        {
            dataChanged = true;
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
                return (T)Convert.ChangeType(Get(StringValues, dataName, defaultValue?.ToString() ?? ""), typeof(T));
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
                throw new SavedDataException("Unsupported type");
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

        public void Append<T>(string dataName, T data)
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
                throw new SavedDataException("Unsupported type");
            }
        }

        public bool HasKey(string dataName)
        {
            return StringValues.ContainsKey(dataName) || IntValues.ContainsKey(dataName) || FloatValues.ContainsKey(dataName);
        }

        public override string ToString()
        {
            return Name + "," + (int)Type;
        }
    }
}