using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    public float Speed;
    public float TransitionSpeed;
    public Transform Camera;
    public GameObject Frogman;
    public Palette[] BaseBackgrounds;
    private bool finishedMove;
    private PaletteTransition transition;
    private int lastCheckedCurrent;
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        for (int i = 0; i < BaseBackgrounds.Length; i++)
        {
            PaletteController.Current.BackgroundPalettes[i].CopyFrom(BaseBackgrounds[i]);
        }
    }
    private void Update()
    {
        if (!finishedMove)
        {
            Camera.position -= new Vector3(0, Speed * Time.deltaTime, 0);
            if (Camera.position.y <= transform.position.y)
            {
                Camera.position = new Vector3(Camera.position.x, transform.position.y, Camera.position.z);
                finishedMove = true;
                Palette palette = new Palette(PaletteController.Current.SpritePalettes[0]);
                palette[3] = palette[2];
                transition = PaletteController.Current.TransitionTo(false, 0, palette, TransitionSpeed);
                Frogman.SetActive(true);
            }
        }
        else
        {
            if (transition == null)
            {
                Destroy(this);
                ConversationPlayer.Current.Resume();
                return;
            }
            if (lastCheckedCurrent != transition.Current)
            {
                Frogman.GetComponent<PalettedSprite>().UpdatePalette();
                lastCheckedCurrent = transition.Current;
            }
        }
    }
}
