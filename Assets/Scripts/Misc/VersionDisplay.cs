using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionDisplay : MonoBehaviour
{
    public string Number;
    public Text Text;

    private void Reset()
    {
        Text = GetComponent<Text>();
    }

    private void Start()
    {
        Text.text = "Version " + Number;
#if MODDABLE_BUILD
        Text.text += ", moddable build";
#endif
#if STEAM_BUILD
        Text.text += ", Steam build";
#endif
    }
}
