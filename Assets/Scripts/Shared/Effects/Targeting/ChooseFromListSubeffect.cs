﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChooseFromListSubeffect : Subeffect
{
    public CardRestriction CardRestriction;
    public int MaxCanChoose = -1;

    protected List<Card> potentialTargets;

    protected void RequestTargets()
    {
        parent.EffectController.ServerNotifier.GetChoicesFromList(potentialTargets, MaxCanChoose);
    }

    public override void Initialize()
    {
        base.Initialize();
        CardRestriction.subeffect = this;
    }

    public override void Resolve()
    {
        potentialTargets = new List<Card>();

        //get all cards that fulfill the cardrestriction
        foreach(KeyValuePair<int, Card> pair in ServerGame.cards)
        {
            if (CardRestriction.Evaluate(pair.Value))
            {
                potentialTargets.Add(pair.Value);
            }
        }

        RequestTargets();
    }

    public virtual bool AddListIfLegal(IEnumerable<Card> choices)
    {
        //check that there are no elements in choices that aren't in potential targets
        //also check that, if a maximum number to choose has been specified, that many have been chosen
        if ((MaxCanChoose > 0 && choices.Count() > MaxCanChoose) ||
            choices.Intersect(potentialTargets).Count() != choices.Count())
        {
            RequestTargets();
            return false;
        }

        //add all cards in the chosen list to targets
        parent.targets.AddRange(choices);
        //everything's cool
        parent.ResolveNextSubeffect();
        return true;
    }

}
