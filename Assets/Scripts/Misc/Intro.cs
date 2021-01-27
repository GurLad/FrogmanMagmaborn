﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    public float Speed;
    public float TransitionSpeed;
    public Transform Camera;
    public GameObject Frogman;
    private bool finishedMove;
    private PaletteTransition transition;
    private int lastCheckedCurrent;
    private Palette[] baseBackgrounds;
    private void Awake()
    {
        baseBackgrounds = new Palette[2];
        for (int i = 0; i < baseBackgrounds.Length; i++)
        {
            baseBackgrounds[i] = new Palette(PaletteController.Current.BackgroundPalettes[i]);
        }
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        for (int i = 0; i < baseBackgrounds.Length; i++)
        {
            PaletteController.Current.BackgroundPalettes[i] = baseBackgrounds[i];
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
                palette.Colors[3] = palette.Colors[2];
                transition = PaletteController.Current.TransitionTo(false, 0, palette, TransitionSpeed);
                Frogman.SetActive(true);
            }
        }
        else
        {
            if (transition == null)
            {
                Destroy(this);
                ConversationPlayer.Current.Play(ConversationController.Current.SelectConversation());
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