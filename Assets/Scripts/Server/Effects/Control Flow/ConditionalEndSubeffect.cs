﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionalEndSubeffect : ServerSubeffect
{
    public const string XLessThan0 = "X<0";
    public const string XLessThanEqual0 = "X<=0";
    public const string XGreaterThanConst = "X>C";
    public const string XLessThanConst = "X<C";
    public const string NoneFitRestriction = "None Fit Restriction";

    public int C = 0;
    public CardRestriction CardRestriction = new CardRestriction();

    public string Condition;

    public override void Initialize(ServerEffect eff, int subeffIndex)
    {
        base.Initialize(eff, subeffIndex);
        CardRestriction.Subeffect = this;
    }

    public override void Resolve()
    {
        bool end;
        switch (Condition)
        {
            case XLessThan0:
                end = ServerEffect.X < 0;
                break;
            case XLessThanEqual0:
                end = ServerEffect.X <= 0;
                break;
            case XGreaterThanConst:
                end = ServerEffect.X > C;
                break;
            case XLessThanConst:
                end = ServerEffect.X < C;
                break;
            case NoneFitRestriction:
                end = !ServerGame.cards.Any(c => CardRestriction.Evaluate(c.Value));
                break;
            default:
                throw new System.ArgumentException($"Condition {Condition} invalid for conditional end subeffect");
        }

        if (end) ServerEffect.EffectImpossible();
        else ServerEffect.ResolveNextSubeffect();
    }
}