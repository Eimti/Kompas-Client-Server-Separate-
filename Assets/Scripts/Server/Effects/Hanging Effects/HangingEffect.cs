﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HangingEffect
{
    public TriggerCondition EndCondition { get; }

    private readonly TriggerRestriction triggerRestriction;
    protected readonly ServerGame serverGame;

    public HangingEffect(ServerGame serverGame, TriggerRestriction triggerRestriction, TriggerCondition endCondition)
    {
        this.serverGame = serverGame ?? throw new System.ArgumentNullException("ServerGame in HangingEffect must not be null");
        this.triggerRestriction = triggerRestriction ?? throw new System.ArgumentNullException("Trigger Restriction in HangingEffect must not be null");
        this.EndCondition = endCondition;
        serverGame.EffectsController.RegisterHangingEffect(EndCondition, this);
    }

    /// <summary>
    /// Called when the trigger for this hanging effect occurs.
    /// If the hanging effect should end (as determined by the ShouldEnd function),
    /// ends the hanging effect (as determined by Resolve)
    /// </summary>
    /// <param name="triggerer">The card that triggered the triggering event</param>
    /// <param name="stackTrigger">The item on the stack that triggered the event</param>
    /// <returns><see langword="true"/> if the hanging effect is now ended, <see langword="false"/> otherwise.</returns>
    public virtual bool EndIfApplicable(Card triggerer, IStackable stackTrigger)
    {
        bool end = ShouldEnd(triggerer, stackTrigger);
        if (end) Resolve();
        return end;
    }

    protected virtual bool ShouldEnd(Card triggerer, IStackable stackTrigger)
    {
        return triggerRestriction.Evaluate(triggerer, stackTrigger, triggerRestriction.Subeffect.EffectController);
    }

    protected abstract void Resolve();
}
