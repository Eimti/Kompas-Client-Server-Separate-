﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Subeffect
{
    [System.NonSerialized] public Effect parent;

    public ServerGame ServerGame { get { return parent.serverGame; } }

    /// <summary>
    /// parent resolve method. at the end, needs to call resolve subeffect in parent
    /// if it's an if, it does a specific index
    /// otherwise, it does currentIndex + 1
    /// </summary>
    public abstract void Resolve();

    /// <summary>
    /// Called by restrictions that have found a valid target to add to the list
    /// </summary>
    /// <param name="card"></param>
    public void ContinueResolutionWith(Card card)
    {
        parent.targets.Add(card);
        parent.ResolveSubeffect(parent.subeffectIndex + 1);
    }

    public virtual void Initialize()
    {

    }

    /// <summary>
    /// The index in the Effect.targets array for which target this effect uses.
    /// If positive, just an index.
    /// If negative, it's Effect.targets.Count + targetIndex (aka that many back from the end)
    /// </summary>
    public int TargetIndex;

    public Card Target
    {
        get
        {
            return TargetIndex < 0 ?
                parent.targets[parent.targets.Count + TargetIndex] :
                parent.targets[TargetIndex];
        }
    }

    public static Subeffect FromJson(SubeffectType seType, string subeffJson, Effect parent)
    {
        Debug.Log("Creating subeffect from json of type " + seType + " json " + subeffJson);

        Subeffect toReturn = null;

        switch (seType)
        {
            case SubeffectType.TargetCardOnBoard:
                toReturn = JsonUtility.FromJson<BoardTargetSubeffect>(subeffJson);
                break;
            case SubeffectType.DeckTarget:
                toReturn = JsonUtility.FromJson<DeckTargetSubeffect>(subeffJson);
                break;
            case SubeffectType.DiscardTarget:
                toReturn = JsonUtility.FromJson<DiscardTargetSubeffect>(subeffJson);
                break;
            case SubeffectType.HandTarget:
                toReturn = JsonUtility.FromJson<HandTargetSubeffect>(subeffJson);
                break;
            case SubeffectType.TargetThis:
                toReturn = JsonUtility.FromJson<TargetThisSubeffect>(subeffJson);
                break;
            case SubeffectType.TargetThisSpace:
                toReturn = JsonUtility.FromJson<TargetThisSpaceSubeffect>(subeffJson);
                break;
            case SubeffectType.ChangeNESW:
                toReturn = JsonUtility.FromJson<ChangeNESWSubeffect>(subeffJson);
                break;
            case SubeffectType.AddPips:
                toReturn = JsonUtility.FromJson<AddPipsSubeffect>(subeffJson);
                break;
            case SubeffectType.SetXByBoardCount:
                toReturn = JsonUtility.FromJson<SetXBoardRestrictionSubeffect>(subeffJson);
                break;
            case SubeffectType.SpaceTarget:
                toReturn = JsonUtility.FromJson<SpaceTargetSubeffect>(subeffJson);
                Debug.Log("Toreturn is null? " + (toReturn == null) + " here");
                Debug.Log("Is the restriction null tho " + ((toReturn as SpaceTargetSubeffect).spaceRestriction == null));
                break;
            case SubeffectType.PayPips:
                toReturn = JsonUtility.FromJson<PayPipsSubeffect>(subeffJson);
                break;
            case SubeffectType.SetXByTargetS:
                toReturn = JsonUtility.FromJson<SetXTargetSSubeffect>(subeffJson);
                break;
            case SubeffectType.SetXByTargetCost:
                throw new NotImplementedException();
            case SubeffectType.PlayCard:
                toReturn = JsonUtility.FromJson<PlaySubeffect>(subeffJson);
                break;
            case SubeffectType.PayPipsByTargetCost:
                toReturn = JsonUtility.FromJson<PayPipsTargetCostSubeffect>(subeffJson);
                break;
            case SubeffectType.DiscardCard:
                toReturn = JsonUtility.FromJson<DiscardSubeffect>(subeffJson);
                break;
            case SubeffectType.ReshuffleCard:
                toReturn = JsonUtility.FromJson<ReshuffleSubeffect>(subeffJson);
                break;
            case SubeffectType.RehandCard:
                toReturn = JsonUtility.FromJson<RehandSubeffect>(subeffJson);
                break;
            case SubeffectType.XTimesLoop:
                toReturn = JsonUtility.FromJson<XTimesSubeffect>(subeffJson);
                break;
            default:
                Debug.LogError("Unrecognized effect type enum for loading effect in effect constructor");
                break;
        }

        if (toReturn != null)
        {
            Debug.Log($"Finishing setup for new effect of type {seType}");
            toReturn.parent = parent;
            toReturn.Initialize();
        }

        return toReturn;
    }
}