﻿using KompasCore.Cards;
using System;
using System.Linq;
using UnityEngine;

namespace KompasCore.Effects
{
    [Serializable]
    public class SpaceRestriction
    {
        public Subeffect Subeffect { get; private set; }

        //adjacency
        public const string AdjacentToThisCard = "Adjacent to Source";
        public const string AdjacentToWithRestriction = "Adjacent to a Card that Fits Restriction";
        public const string AdjacentToTarget = "Adjacent to Target";
        public const string ConnectedToSourceBy = "Connected to Source by Cards Fitting Restriction";
        public const string InAOE = "In AOE";
        public const string NotInAOE = "Not In AOE";
        public const string LimitAdjacentCardsFittingRestriction = "Limit Number of Adjacent Cards Fitting Restriction";

        //distance
        public const string DistanceX = "Distance to Source == X";
        public const string DistanceToTargetX = "Distance to Target == X";
        public const string DistanceToTargetC = "Distance to Target == Constant";
        public const string DistanceToTargetLTEC = "Distance to Target <= Constant";
        public const string FurtherFromSourceThanTarget = "Further from Source than Target";
        public const string TowardsSourceFromTarget = "Towards Source from Target";

        //misc
        public const string CanPlayTarget = "Can Play Target to This Space";
        public const string CanMoveTarget = "Can Move Target to This Space";
        public const string Empty = "Empty";
        public const string CardHereFitsRestriction = "Card Here Fits Restriction";
        public const string OnTargetsDiagonal = "On Target's Diagonal";
        public const string OnEdge = "On Edge of Board";

        public string[] spaceRestrictions;
        public CardRestriction adjacencyRestriction = new CardRestriction();
        public CardRestriction limitAdjacencyRestriction = new CardRestriction();
        public int adjacencyLimit;
        public CardRestriction connectednessRestriction = new CardRestriction();
        public CardRestriction hereFitsRestriction = new CardRestriction();

        public int constant;

        public string blurb = "";

        public void Initialize(Subeffect subeffect)
        {
            this.Subeffect = subeffect;
            adjacencyRestriction.Initialize(subeffect);
            connectednessRestriction.Initialize(subeffect);
            limitAdjacencyRestriction.Initialize(subeffect);
            hereFitsRestriction.Initialize(subeffect);
        }

        private bool RestrictionValid(string restriction, int x, int y)
        {
            switch (restriction)
            {
                //adjacency
                case AdjacentToThisCard:        return Subeffect.Source.IsAdjacentTo(x, y);
                case AdjacentToWithRestriction: return Subeffect.Game.boardCtrl.CardsAdjacentTo(x, y).Any(c => adjacencyRestriction.Evaluate(c));
                case AdjacentToTarget:          return Subeffect.Target.IsAdjacentTo(x, y);
                case ConnectedToSourceBy:       return Subeffect.Game.boardCtrl.ShortestPath(Subeffect.Source, x, y, connectednessRestriction) < 50;
                case InAOE:                     return Subeffect.Source.SpaceInAOE(x, y);
                case NotInAOE:                  return !Subeffect.Source.SpaceInAOE(x, y);
                case LimitAdjacentCardsFittingRestriction:
                    return Subeffect.Game.boardCtrl.CardsAdjacentTo(x, y).Where(c => limitAdjacencyRestriction.Evaluate(c)).Count() <= adjacencyLimit;

                //distance
                case DistanceX:                   return Subeffect.Source.DistanceTo(x, y) == Subeffect.Effect.X;
                case DistanceToTargetX:           return Subeffect.Target.DistanceTo(x, y) == Subeffect.Effect.X;
                case DistanceToTargetC:           return Subeffect.Target.DistanceTo(x, y) == constant;
                case DistanceToTargetLTEC:        return Subeffect.Target.DistanceTo(x, y) <= constant;
                case FurtherFromSourceThanTarget: return Subeffect.Source.DistanceTo(x, y) > Subeffect.Source.DistanceTo(Subeffect.Target);
                case TowardsSourceFromTarget:     return Subeffect.Source.DistanceTo(x, y) < Subeffect.Source.DistanceTo(Subeffect.Target);

                //misc
                case CanPlayTarget: return Subeffect.Target.PlayRestriction.EvaluateEffectPlay(x, y, Subeffect.Effect);
                case CanMoveTarget: return Subeffect.Target.MovementRestriction.EvaluateEffectMove(x, y);
                case Empty: return Subeffect.Effect.Game.boardCtrl.GetCardAt(x, y) == null;
                case CardHereFitsRestriction: return hereFitsRestriction.Evaluate(Subeffect.Effect.Game.boardCtrl.GetCardAt(x, y));
                case OnTargetsDiagonal: return Subeffect.Target.OnMyDiagonal((x, y));
                case OnEdge: return x == 0 || x == 6 || y == 0 || y == 6;
                default: throw new ArgumentException($"Invalid space restriction {restriction}", "restriction");
            }
        }

        public bool Evaluate((int x, int y) space) => Evaluate(space.x, space.y);

        public bool Evaluate(int x, int y)
        {
            //Debug.Log($"Space restriction for {Subeffect?.Source?.CardName} evaluating {x}, {y}");
            if (!Subeffect.Effect.Game.boardCtrl.ValidIndices(x, y)) return false;

            return spaceRestrictions.All(r => RestrictionValid(r, x, y));
        }
    }
}