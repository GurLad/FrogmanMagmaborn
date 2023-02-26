using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemSFXController : MonoBehaviour
{
    public enum Type { MenuSelect, MenuMove, MenuCancel, LongSelect, LongMove, LongCancel, UnitSelect, UnitCancel, UnitForbidden, CursorMove }

    private static SystemSFXController systemSFXController;
    public List<AudioClip> SFXList;

    public void Init()
    {
        systemSFXController = this;
    }

    public static void Play(Type type)
    {
        SoundController.PlaySound(systemSFXController.SFXList[(int)type]);
    }
}
