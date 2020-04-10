﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummySpaceTargetSubeffect : Subeffect
{
    public SpaceRestriction spaceRestriction;

    public override void Resolve()
    {
        throw new System.NotImplementedException("Dummy Space target subeffect should never resolve");
    }

    public override void Initialize()
    {
        spaceRestriction.Subeffect = this;
    }
}
