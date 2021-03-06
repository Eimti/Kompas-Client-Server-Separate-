﻿using System.Collections.Generic;
using KompasCore.Effects;
using KompasCore.Cards;
using System.Threading.Tasks;

namespace KompasServer.Effects
{
    [System.Serializable]
    public abstract class TemporaryCardChangeSubeffect : ServerSubeffect
    {
        public TriggerRestriction triggerRestriction;
        public string endCondition;
        public virtual bool ContinueResolution => true;

        public string fallOffCondition = Trigger.Remove;
        public string[] fallOffRestrictions =
        {
            TriggerRestriction.ThisCardTriggered,
            TriggerRestriction.ThisCardInPlay,
        };

        protected TriggerRestriction CreateFallOffRestriction(GameCard card)
        {
            //conditions for falling off
            var triggerRest = new TriggerRestriction() { triggerRestrictions = fallOffRestrictions };
            triggerRest.Initialize(Game, card, thisTrigger: null, effect: Effect);
            return triggerRest;
        }

        public override void Initialize(ServerEffect eff, int subeffIndex)
        {
            base.Initialize(eff, subeffIndex);
            triggerRestriction = triggerRestriction ?? new TriggerRestriction();
            triggerRestriction.Initialize(Game, ThisCard, thisTrigger: null, effect: Effect);
        }

        public override Task<ResolutionInfo> Resolve()
        {
            //create the hanging effects, of which there can be multiple
            var effectsApplied = CreateHangingEffects();

            //each of the effects needs to be registered, and registered for how it could fall off
            foreach (var eff in effectsApplied)
            {
                ServerGame.EffectsController.RegisterHangingEffect(endCondition, eff);
                if(!string.IsNullOrEmpty(fallOffCondition))
                    ServerGame.EffectsController.RegisterHangingEffectFallOff(fallOffCondition, eff.FallOffRestriction, eff);
            }

            //after all that's done, make it do the next subeffect
            if (ContinueResolution) return Task.FromResult(ResolutionInfo.Next);
            else return Task.FromResult(ResolutionInfo.End(EndOnPurpose));
        }

        protected abstract IEnumerable<HangingEffect> CreateHangingEffects();
    }
}