﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryNESWBuffSubeffect : TemporaryCardChangeSubeffect
{
    public int nBuff = 0;
    public int eBuff = 0;
    public int sBuff = 0;
    public int wBuff = 0;

    public int nMultiplier = 0;
    public int eMultiplier = 0;
    public int sMultiplier = 0;
    public int wMultiplier = 0;

    protected override IEnumerable<(HangingEffect, GameCard)> CreateHangingEffects()
    {
        var temp = new TemporaryNESWBuff(ServerGame, triggerRestriction, endCondition,
            Target,
            nBuff + Effect.X * nMultiplier,
            eBuff + Effect.X * eMultiplier,
            sBuff + Effect.X * sMultiplier,
            wBuff + Effect.X * wMultiplier);

        return new List<(HangingEffect, GameCard)>() { (temp, Target) };
    }
}
