﻿using System.Linq;

namespace KompasServer.Effects
{
    public class TargetAllSubeffect : CardTargetSubeffect
    {
        public override bool Resolve()
        {
            var targets = ServerGame.Cards.Where(c => cardRestriction.Evaluate(c)).ToArray();
            //check what targets there are now, before you add them, to not mess with NotAlreadyTarget restriction
            //because Linq executes lazily, it would otherwise add the targets, then re-execute the query and not find any
            foreach (var t in targets) Effect.AddTarget(t);

            if (targets.Any()) return ServerEffect.ResolveNextSubeffect();
            else return ServerEffect.EffectImpossible();
        }
    }

}