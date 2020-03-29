﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSubeffectFactory : ISubeffectFactory
{
    public Subeffect FromJson(SubeffectType seType, string subeffJson, Effect parent, int subeffIndex)
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
            case SubeffectType.ChooseFromList:
                toReturn = JsonUtility.FromJson<ChooseFromListSubeffect>(subeffJson);
                break;
            case SubeffectType.ChooseFromListSaveRest:
                toReturn = JsonUtility.FromJson<ChooseFromListSaveRestSubeffect>(subeffJson);
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
                break;
            case SubeffectType.PayPips:
                toReturn = JsonUtility.FromJson<PayPipsSubeffect>(subeffJson);
                break;
            case SubeffectType.SetXByTargetS:
                toReturn = JsonUtility.FromJson<SetXTargetSSubeffect>(subeffJson);
                break;
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
            case SubeffectType.TTimesLoop:
                toReturn = JsonUtility.FromJson<TTimesSubeffect>(subeffJson);
                break;
            case SubeffectType.WhileHaveTargetsLoop:
                toReturn = JsonUtility.FromJson<LoopWhileHaveTargetsSubeffect>(subeffJson);
                break;
            default:
                Debug.LogError($"Unrecognized effect type enum {seType} for loading effect in effect constructor");
                break;
        }

        if (toReturn != null)
        {
            Debug.Log($"Finishing setup for new effect of type {seType}");
            toReturn.parent = parent;
            toReturn.Initialize();
            toReturn.SubeffIndex = subeffIndex;
        }

        return toReturn;
    }
}
