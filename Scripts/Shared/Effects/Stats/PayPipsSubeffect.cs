﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayPipsSubeffect : Subeffect
{
    public int xMultiplier;
    public int xDivisor;
    public int modifier;

    public override void Resolve()
    {
        int toPay = parent.X * xMultiplier / xDivisor + modifier;
        if(parent.EffectController.pips < toPay)
        {
            parent.EffectImpossible();
            return;
        }

        parent.EffectController.pips -= toPay;
        ServerGame?.serverNotifier.NotifySetPips(parent.EffectController, parent.EffectController.pips);
        parent.ResolveNextSubeffect();
    }
}
