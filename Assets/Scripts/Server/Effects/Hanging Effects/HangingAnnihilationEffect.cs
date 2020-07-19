﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Does nothing when created. When resolves, annihilates its target
/// </summary>
public class HangingAnnihilationEffect : HangingEffect
{
    private readonly GameCard target;

    public HangingAnnihilationEffect (ServerGame serverGame, TriggerRestriction triggerRestriction, string endCondition,
        GameCard target)
        : base(serverGame, triggerRestriction, endCondition)
    {
        this.target = target;
    }

    protected override void Resolve()
    {
        serverGame.annihilationCtrl.Annihilate(target);
    }
}
