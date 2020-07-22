﻿using KompasCore.Cards;
using KompasCore.Effects;
using KompasServer.GameCore;

namespace KompasServer.Effects
{
    /// <summary>
    /// Does nothing when created. When resolves, annihilates its target
    /// </summary>
    public class HangingAnnihilationEffect : HangingEffect
    {
        private readonly GameCard target;

        public HangingAnnihilationEffect(ServerGame serverGame, TriggerRestriction triggerRestriction, string endCondition,
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
}