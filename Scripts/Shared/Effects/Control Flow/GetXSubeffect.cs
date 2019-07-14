﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetXSubeffect : Subeffect
{
    public override void Resolve()
    {
        parent.serverGame.serverNetworkCtrl.GetXForEffect(parent.serverGame, parent.effectController, parent.thisCard,
            System.Array.IndexOf(parent.thisCard.Effects, parent), parent.effectIndex);
    }
}