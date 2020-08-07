﻿using System.Collections.Generic;
using UnityEngine;
using KompasCore.Effects;
using KompasServer.GameCore;
using System.Linq;

namespace KompasServer.Effects
{
    public class ServerEffectsController : MonoBehaviour
    {
        public readonly object triggerStackLock = new object();
        public readonly object responseLock = new object();

        public ServerGame ServerGame;

        protected ServerEffectStack stack = new ServerEffectStack();

        public Stack<(ServerTrigger, ActivationContext, ServerPlayer)> OptionalTriggersToAsk
            = new Stack<(ServerTrigger, ActivationContext, ServerPlayer)>();

        //trigger map
        protected Dictionary<string, List<ServerTrigger>> triggerMap = new Dictionary<string, List<ServerTrigger>>();
        protected Dictionary<string, List<HangingEffect>> hangingEffectMap = new Dictionary<string, List<HangingEffect>>();
        protected Dictionary<string, List<(HangingEffect, TriggerRestriction)>> hangingEffectFallOffMap
            = new Dictionary<string, List<(HangingEffect, TriggerRestriction)>>();

        public IServerStackable CurrStackEntry 
        { 
            get; 
            private set; 
        }

        public void Start()
        {
            foreach (var c in Trigger.TriggerConditions)
            {
                triggerMap.Add(c, new List<ServerTrigger>());
                hangingEffectMap.Add(c, new List<HangingEffect>());
                hangingEffectFallOffMap.Add(c, new List<(HangingEffect, TriggerRestriction)>());
            }
        }

        #region the stack
        public void PushToStack(IServerStackable eff, ActivationContext context)
        {
            ResetPassingPriority();
            stack.Push((eff, context));
        }

        public void PushToStack(ServerEffect eff, ServerPlayer controller, ActivationContext context)
        {
            eff.PushedToStack(ServerGame, controller);
            PushToStack(eff, context);
        }

        public IServerStackable CancelStackEntry(int index)
        {
            return stack.Cancel(index);
        }

        public void ResolveNextStackEntry()
        {
            var (stackable, context) = stack.Pop();
            Debug.Log($"Resolving next stack entry: {stackable}, {context}");
            if (stackable == null)
            {
                ServerGame.TurnServerPlayer.ServerNotifier.DiscardSimples();
                ServerGame.boardCtrl.DiscardSimples();
                ServerGame.ServerPlayers.First().ServerNotifier.StackEmpty();
            }
            else
            {
                foreach (var p in ServerGame.ServerPlayers) p.ServerNotifier.RequestNoResponse();
                CurrStackEntry = stackable;
                stackable.StartResolution(context);
            }
        }

        /// <summary>
        /// The last effect that resolved is now done.
        /// </summary>
        public void FinishStackEntryResolution()
        {
            CurrStackEntry = null;
            CheckForResponse();
        }

        public void ResetPassingPriority()
        {
            foreach (var player in ServerGame.ServerPlayers)
            {
                player.passedPriority = false;
            }
        }

        public void OptionalTriggerAnswered(bool answer)
        {
            lock (triggerStackLock)
            {
                if (OptionalTriggersToAsk.Count == 0)
                {
                    Debug.LogError($"Tried to answer about a trigger when there weren't any triggers to answer about.");
                    return;
                }

                var (t, context, controller) = OptionalTriggersToAsk.Pop();
                if (answer) t.OverrideTrigger(context, controller);
                CheckForResponse();
            }
        }

        public void CheckForResponse(bool reset = true)
        {
            if (CurrStackEntry != null)
            {
                Debug.Log($"Tried to check for response while {CurrStackEntry?.Source?.CardName} is resolving");
                return;
            }

            if (reset) ResetPassingPriority();

            if (OptionalTriggersToAsk.Count > 0)
            {
                //then ask the respective player about that trigger.
                lock (triggerStackLock)
                {
                    var (t, context, controller) = OptionalTriggersToAsk.Peek();
                    controller.ServerNotifier.AskForTrigger(t, context.X, context.Card, context.Stackable, context.Triggerer);
                }
                //if the player chooses to trigger it, it will be removed from the list
            }
            //check if responses exist. if not, resolve
            else
            {
                lock (responseLock)
                {
                    //TODO if any player can activate effects, do ServerGame.Players.Any() the entire expression
                    var players = ServerGame.Cards.Where(c => c.Effects.Any(e => e.ActivationRestriction.Evaluate(c.Controller)))
                        .Select(c => c.Controller).Distinct().Where(p => !p.passedPriority);

                    //TODO figure out a way to not resolve the deferred execution of Linq twice
                    if (players.Any()) foreach (var p in players) ServerGame.ServerPlayers[p.index].ServerNotifier.RequestResponse();
                    //if neither player has anything to do, resolve the stack
                    else ResolveNextStackEntry();
                }
            }
        }
        #endregion the stack

        #region triggers
        public void RegisterTrigger(string condition, ServerTrigger trigger)
        {
            Debug.Log($"Registering a new trigger from card {trigger.effToTrigger.Source.CardName} to condition {condition}");
            List<ServerTrigger> triggers = triggerMap[condition];
            if (triggers == null)
            {
                triggers = new List<ServerTrigger>();
                triggerMap.Add(condition, triggers);
            }
            triggers.Add(trigger);
        }

        public void RegisterHangingEffect(string condition, HangingEffect hangingEff)
        {
            Debug.Log($"Registering a new hanging effect to condition {condition}");
            List<HangingEffect> hangingEffs = hangingEffectMap[condition];
            hangingEffs.Add(hangingEff);
        }

        public void RegisterHangingEffectFallOff(string condition, TriggerRestriction restriction, HangingEffect hangingEff)
        {
            Debug.Log($"Registering a new hanging effect to condition {condition}");
            var hangingEffs = hangingEffectFallOffMap[condition];
            hangingEffs.Add((hangingEff, restriction));
        }

        public void TriggerForCondition(string condition, params ActivationContext[] contexts)
        {
            foreach (var c in contexts) TriggerForCondition(condition, c);
        }

        public void TriggerForCondition(string condition, ActivationContext context)
        {
            Debug.Log($"Attempting to trigger {condition}, with context {context}");
            List<HangingEffect> toRemove = new List<HangingEffect>();
            foreach (HangingEffect t in hangingEffectMap[condition])
            {
                if (t.EndIfApplicable(context))
                {
                    toRemove.Add(t);
                }
            }
            foreach (var t in toRemove)
            {
                hangingEffectMap[condition].Remove(t);
            }

            var fallOffToRemove = new List<(HangingEffect, TriggerRestriction)>();
            foreach (var (eff, fallOffRestriction) in hangingEffectFallOffMap[condition])
            {
                if (fallOffRestriction.Evaluate(context))
                {
                    fallOffToRemove.Add((eff, fallOffRestriction));
                }
            }
            foreach (var (eff, fallOffRestriction) in fallOffToRemove)
            {
                hangingEffectMap[eff.EndCondition].Remove(eff);
                hangingEffectFallOffMap[condition].Remove((eff, fallOffRestriction));
            }

            foreach (ServerTrigger t in triggerMap[condition])
            {
                t.TriggerIfValid(context);
            }
        }

        /// <summary>
        /// Adds this trigger to the list that, once a stack entry resolves,
        /// asks the player whose trigger it is if they actually want to trigger that effect
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="x"></param>
        public void AskForTrigger(ServerTrigger trigger, ActivationContext context, ServerPlayer controller)
        {
            Debug.Log($"Asking about trigger for effect of card {trigger.effToTrigger.Source.CardName}");
            lock (triggerStackLock)
            {
                OptionalTriggersToAsk.Push((trigger, context, controller));
            }
        }
        #endregion triggers
    }
}