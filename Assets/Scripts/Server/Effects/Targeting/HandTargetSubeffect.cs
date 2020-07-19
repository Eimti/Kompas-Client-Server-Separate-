﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTargetSubeffect : CardTargetSubeffect
{
    public override bool Resolve()
    {
        //check first that there exist valid targets. if there exist no valid targets, finish resolution here
        if (!ThisCard.Game.ExistsCardTarget(cardRestriction))
        {
            Debug.Log("No target exists for " + ThisCard.CardName + " effect");
            return ServerEffect.EffectImpossible();
        }

        //ask the client that is this effect's controller for a target. 
        //give the card if whose effect it is, the index of the effect, and the index of the subeffect
        //since only the server resolves effects, this should never be called for a client. 
        EffectController.ServerNotifier.GetHandTarget(ThisCard, this);

        //then wait for the network controller to call the continue method
        return false;
    }

    public override bool AddTargetIfLegal(GameCard card)
    {
        if (base.AddTargetIfLegal(card)) return true;

        EffectController.ServerNotifier.GetHandTarget(ThisCard, this);
        return false;
    }
}
