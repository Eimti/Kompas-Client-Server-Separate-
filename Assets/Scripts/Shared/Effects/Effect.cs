﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effects will only be resolved on server. Clients will just get to know what effects they can use
/// </summary>
public class Effect : IStackable
{
    //if this effect is resolving on a server, this is the server game it's resolving on
    public ServerGame serverGame;

    //card that this is the effect of. to be set at initialization
    public Card thisCard;
    public Card Source { get { return thisCard; } }

    //who owns the effect. TODO set when a player activates an effect
    public int effectControllerIndex;
    public ServerPlayer EffectController { get { return serverGame.ServerPlayers[effectControllerIndex]; } }
    public Player Controller { get { return EffectController; } }

    //current subeffect that's resolving
    public int subeffectIndex;

    public int EffectIndex { get { return System.Array.IndexOf(thisCard.Effects, this); } }

    //checked by effect resolution to see if it should start resolving the next effect on the stack
    public bool doneResolving = false;

    public Subeffect OnImpossible = null;

    private Subeffect[] subeffects;
    public Subeffect[] Subeffects { get => subeffects; }

    private Trigger trigger;
    public Trigger Trigger { get => trigger; }

    public List<Card> targets;
    public List<Vector2Int> coords;

    public bool Negated { get; protected set; }

    /// <summary>
    /// X value as listed on cards
    /// </summary>
    public int X = 0;

    private int timesUsedThisTurn;
    /// <summary>
    /// The number of times this effect has been used this turn
    /// </summary>
    public int TimesUsedThisTurn { get => timesUsedThisTurn; }

    private int? maxTimesCanUsePerTurn;
    /// <summary>
    /// The maximum number of times this effect can be used in a turn
    /// </summary>
    public int? MaxTimesCanUsePerTurn { get => maxTimesCanUsePerTurn; }

    //get the currently resolving subeffect
    public Subeffect CurrSubeffect { get { return subeffects[subeffectIndex]; } }

    public Effect(SerializableEffect se, Card thisCard, int controller)
    {
        this.thisCard = thisCard;
        this.serverGame = thisCard.serverGame;
        subeffects = new Subeffect[se.subeffects.Length];
        targets = new List<Card>();
        coords = new List<Vector2Int>();

        if (!string.IsNullOrEmpty(se.trigger))
        {
            try
            {
                trigger = Trigger.FromJson(se.triggerCondition, se.trigger, this);
            }
            catch(System.ArgumentException)
            {
                Debug.LogError($"Failed to load trigger of type {se.triggerCondition} from json {se.trigger}");
                throw;
            }
        }

        for (int i = 0; i < se.subeffectTypes.Length; i++)
        {
            try
            {
                subeffects[i] = thisCard.game.SubeffectFactory.FromJson(se.subeffectTypes[i], se.subeffects[i], this, i);
            }
            catch (System.ArgumentException)
            {
                Debug.LogError($"Failed to load subeffect of type {se.subeffectTypes[i]} from json {se.subeffects[i]}");
                throw;
            }
        }

        maxTimesCanUsePerTurn = se.maxTimesCanUsePerTurn;
        timesUsedThisTurn = 0;
    }

    public void ResetForTurn()
    {
        timesUsedThisTurn = 0;
    }

    public bool CanUse()
    {
        return timesUsedThisTurn < maxTimesCanUsePerTurn;
    }

    public void PushToStack(int controller)
    {
        serverGame.PushToStack(this, controller);
    }

    public void StartResolution()
    {
        thisCard.game.CurrEffect = this;
        if (Negated) EffectImpossible();
        else ResolveSubeffect(0);
    }

    public void ResolveNextSubeffect()
    {
        ResolveSubeffect(subeffectIndex + 1);
    }

    public void ResolveSubeffect(int index)
    {
        if(index >= subeffects.Length)
        {
            FinishResolution();
            return;
        }

        Debug.Log($"Resolving subeffect of type {subeffects[index].GetType()}");
        subeffectIndex = index;
        subeffects[index].Resolve();
    }

    /// <summary>
    /// If the effect finishes resolving, this method is called.
    /// Any function can also call this effect to finish resolution early.
    /// </summary>
    private void FinishResolution()
    {
        doneResolving = true;
        subeffectIndex = 0;
        X = 0;
        targets.Clear();
        OnImpossible = null;
        EffectController.ServerNotifier.AcceptTarget();
        EffectController.ServerNotifier.NotifyBothPutBack();
        serverGame.FinishStackEntryResolution();
    }

    /// <summary>
    /// Cancels resolution of the effect, 
    /// or, if there is something pending if the effect becomes impossible, resolves that
    /// </summary>
    public void EffectImpossible()
    {
        Debug.Log($"Effect of {thisCard.CardName} is being declared impossible");
        if (OnImpossible == null) FinishResolution();
        else
        {
            subeffectIndex = OnImpossible.SubeffIndex;
            OnImpossible.OnImpossible();
        }
    }

    public void DeclineAnotherTarget()
    {
        OnImpossible?.OnImpossible();
    }

    public void Negate()
    {
        Negated = true;
    }
}
