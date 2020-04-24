﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellCard : Card
{

    public enum SpellType { Simple, Enchant, Augment, Terraform, Delayed };

    private int c;
    private SpellType spellType;
    private string subtext;
    private bool fast;

    public int C
    {
        get { return c; }
        set { c = value; }
    }
    public string Subtext
    {
        get { return subtext; }
        set { subtext = value; }
    }
    public SpellType SpellSubtype { get { return spellType; } }

    public override int Cost { get { return C; } }

    //get data
    public SerializableSpellCard GetSerializableVersion()
    {
        SerializableSpellCard serializableSpell = new SerializableSpellCard
        {
            cardName = CardName,
            effText = EffText,
            spellType = spellType,
            subtext = subtext,
            c = c,

            location = location,
            owner = ControllerIndex,
            BoardX = boardX,
            BoardY = boardY,
            subtypeText = SubtypeText
        };
        return serializableSpell;
    }

    //set data
    public override void SetInfo(SerializableCard serializedCard, Game game, Player owner)
    {
        if (!(serializedCard is SerializableSpellCard serializedSpell)) return;

        c = serializedSpell.c;
        subtext = serializedSpell.subtext;
        spellType = serializedSpell.spellType;
        fast = serializedSpell.fast;

        base.SetInfo(serializedCard, game, owner);
    }

    //game mechanics
    public override void MoveTo(int toX, int toY)
    {
        boardX = toX;
        boardY = toY;

        /* for setting where the gameobject is, it would be x and z, except that the quad is turned 90 degrees
         * so we change the local x and y. the z coordinate also therefore needs to be negative
         * to show the card above the game board on the screen. */
        transform.localPosition = new Vector3(GridIndexToPos(toX), GridIndexToPos(toY), -0.03f);
        if (ControllerIndex == 0) transform.localEulerAngles = new Vector3(0, 0, 90);
        else transform.localEulerAngles = new Vector3(0, 0, 270);

    }
}
