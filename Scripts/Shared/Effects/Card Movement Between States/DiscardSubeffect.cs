﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardSubeffect : CardChangeStateSubeffect
{
    public override void Resolve()
    {
        Debug.Log("Resolving discard subeffect");
        ServerGame?.serverNotifier.NotifyDiscard(Target);
        Target.Discard();
        parent.ResolveNextSubeffect();
    }
}
