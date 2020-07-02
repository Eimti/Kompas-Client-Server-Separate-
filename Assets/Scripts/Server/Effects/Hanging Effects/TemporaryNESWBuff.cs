﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryNESWBuff : HangingEffect
{
    private readonly GameCard buffRecipient;
    private readonly int nBuff = 0;
    private readonly int eBuff = 0;
    private readonly int sBuff = 0;
    private readonly int wBuff = 0;

    private bool ended = false;

    public TemporaryNESWBuff(ServerGame game, TriggerRestriction triggerRestriction, TriggerCondition EndCondition, 
        GameCard buffRecipient, int nBuff, int eBuff, int sBuff, int wBuff) 
        : base(game, triggerRestriction, EndCondition)
    {
        this.buffRecipient = buffRecipient ?? throw new System.ArgumentNullException("Null characcter card in temporary nesw buff");
        this.nBuff = nBuff;
        this.eBuff = eBuff;
        this.sBuff = sBuff;
        this.wBuff = wBuff;

        buffRecipient.AddToCharStats(nBuff, eBuff, sBuff, wBuff);
    }

    protected override void Resolve()
    {
        if (!ended)
        {
            buffRecipient.AddToCharStats(-1 * nBuff, -1 * eBuff, -1 * sBuff, -1 * wBuff);
            ended = true;
        }
    }
}
