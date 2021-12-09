using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextFile
{
    public string Text { get; }
    public string Name { get; }

    public TextFile(string text, string name)
    {
        Text = text;
        Name = name;
    }

    public TextFile(TextAsset textAsset)
    {
        Text = textAsset.text;
        Name = textAsset.name;
    }
}
