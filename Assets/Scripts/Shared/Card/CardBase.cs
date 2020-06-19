﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardBase : MonoBehaviour
{
    public char CardType { get; private set; }
    public string CardName { get; private set; }
    public string EffText { get; private set; }
    public string SubtypeText { get; private set; }
    public string[] Subtypes { get; private set; }

    public Sprite detailedSprite;
    public Sprite simpleSprite;

    protected void SetInfo(SerializableCard card)
    {
        CardType = card.cardType;
        CardName = card.cardName;
        EffText = card.effText;
        SubtypeText = card.subtypeText;
        Subtypes = card.subtypes;

        detailedSprite = Resources.Load<Sprite>("Detailed Sprites/" + CardName);
        simpleSprite = Resources.Load<Sprite>("Simple Sprites/" + CardName);
    }
}
