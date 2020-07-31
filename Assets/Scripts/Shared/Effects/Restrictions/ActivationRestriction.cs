﻿using Boo.Lang;
using KompasCore.Cards;
using KompasServer.GameCore;
using System.Linq;
using UnityEngine;

namespace KompasCore.Effects
{
    [System.Serializable]
    public class ActivationRestriction
    {
        public Effect Effect { get; private set; }
        public GameCard Card => Effect.Source;

        public const string TimesPerTurn = "Max Times Per Turn";
        public const string TimesPerRound = "Max Times Per Round";
        public const string FriendlyTurn = "Friendly Turn";
        public const string EnemyTurn = "Enemy Turn";
        public const string InPlay = "In Play";
        public const string Location = "Location";
        //public const string StackEmpty = "Stack Empty";
        public const string ControllerActivates = "Controller Activates";
        public const string NotNegated = "Not Negated";

        public const string Default = "Default";
        public static readonly string[] DefaultRestrictions =
        {
            "Controller Activates", "Not Negated", "In Play"
        };

        public int maxTimes = 1;
        public int location = (int) CardLocation.Field;

        public List<string> activationRestrictions = new List<string>{ "Default" };

        public void Initialize(Effect eff)
        {
            Effect = eff;
            if (activationRestrictions.Contains("Default")) activationRestrictions.AddRange(DefaultRestrictions);
            Debug.Log($"Initializing activation restriction for {Card.CardName} with restrictions: {string.Join(", ", activationRestrictions)}");
        }

        private bool RestrictionValid(string r, Player activator)
        {
            switch (r)
            {
                case Default: return true;
                case TimesPerTurn: return Effect.TimesUsedThisTurn < maxTimes;
                case TimesPerRound: return Effect.TimesUsedThisRound < maxTimes;
                case FriendlyTurn: return Effect.Game.TurnPlayer == activator;
                case EnemyTurn: return Effect.Game.TurnPlayer != activator;
                case InPlay: return Effect.Source.Location == CardLocation.Field;
                case Location: return Effect.Source.Location == (CardLocation)location;
                //case StackEmpty: return Effect.Game.CurrStackEntry == null; //TODO make client game actually track current stack entry
                case ControllerActivates: return activator == Card.Controller;
                case NotNegated: return !Effect.Negated;
                default:
                    Debug.LogError($"You forgot to check for {r} in Activation Restriction switch");
                    return false;
            }
        }

        public bool Evaluate(Player activator)
            => activationRestrictions.All(r => RestrictionValid(r, activator));
    }
}