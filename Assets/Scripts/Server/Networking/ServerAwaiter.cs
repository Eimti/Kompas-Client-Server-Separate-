﻿using KompasCore.Cards;
using KompasCore.Networking;
using KompasServer.Effects;
using KompasServer.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace KompasServer.Networking {
    public class ServerAwaiter : MonoBehaviour
    {
        private const int DefaultDelay = 100;
        private const int TargetCheckDelay = 100;

        public ServerNotifier serverNotifier;
        public ServerNetworkController serverNetCtrl;

        #region awaited values
        /*the following are properties so that later i can override the setter to 
         * also signal a semaphore to awaken the relevant awaiting request. */
        public bool? OptionalTriggerAnswer { get; set; }
        public (int[] cardIds, int[] effIndices, int[] orders)? TriggerOrders { get; set; }

        public int? EffOption { get; set; }
        public int? PlayerXChoice { get; set; }


        public bool DeclineTarget { get; set; }
        public GameCard CardTarget { get; set; }
        public IEnumerable<GameCard> CardListTargets { get; set; }
        public (int, int)? SpaceTarget { get; set; }
        #endregion awaited values

        #region locks
        private readonly object OptionalTriggerLock = new object();
        private readonly object TriggerOrderLock = new object();

        private readonly object EffectOptionsLock = new object();
        private readonly object PlayerChooseXLock = new object();

        private readonly object CardTargetLock = new object();
        private readonly object CardListTargetsLock = new object();
        private readonly object SpaceTargetLock = new object();
        #endregion locks

        #region trigger things
        public async Task<bool> GetOptionalTriggerChoice(ServerTrigger trigger)
        {
            serverNotifier.AskForTrigger(trigger);
            //TODO later use a semaphore or something to signal once packet is available
            while (true)
            {
                lock (OptionalTriggerLock)
                {
                    if (OptionalTriggerAnswer.HasValue)
                    {
                        bool answer = OptionalTriggerAnswer.Value;
                        OptionalTriggerAnswer = null;
                        return answer;
                    }
                }
                
                await Task.Delay(DefaultDelay); 
            }
        }

        public async Task GetTriggerOrder(IEnumerable<ServerTrigger> triggers)
        {
            serverNotifier.GetTriggerOrder(triggers);
            while (true)
            {
                lock (TriggerOrderLock)
                {
                    if (TriggerOrders.HasValue)
                    {
                        (int[] cardIds, int[] effIndices, int[] orders) = TriggerOrders.Value;
                        for (int i = 0; i < effIndices.Length; i++)
                        {
                            //TODO deal with garbage values here
                            var card = serverNetCtrl.sGame.GetCardWithID(cardIds[i]);
                            if (card == null) continue;
                            if (card.Effects.ElementAt(effIndices[i]).Trigger is ServerTrigger trigger)
                                trigger.Order = orders[i];
                        }

                        TriggerOrders = null;
                        return;
                    }
                }
                
                await Task.Delay(DefaultDelay);
            }
        }
        #endregion trigger things

        #region effect flow control
        public async Task<int> GetEffectOption(string cardName, string choiceBlurb, string[] optionBlurbs)
        {
            serverNotifier.ChooseEffectOption(cardName, choiceBlurb, optionBlurbs);
            while (true)
            {
                lock (EffectOptionsLock)
                {
                    if (EffOption.HasValue)
                    {
                        int val = EffOption.Value;
                        EffOption = null;
                        return val;
                    }
                }

                await Task.Delay(DefaultDelay);
            }
        }

        public async Task<int> GetPlayerXValue()
        {
            serverNotifier.GetXForEffect();
            while (true)
            {
                lock (PlayerChooseXLock)
                {
                    if (PlayerXChoice.HasValue)
                    {
                        int val = PlayerXChoice.Value;
                        PlayerXChoice = null;
                        return val;
                    }
                }

                await Task.Delay(DefaultDelay);
            }
        }
        #endregion effect flow control

        #region targeting
        /// <summary>
        /// Gets a card target, waiting until the client sends one or sends that they don't want to choose a target
        /// </summary>
        /// <param name="sourceCardName">The card whose effect is asking for a target</param>
        /// <param name="blurb">The description of the target to get</param>
        /// <param name="ids">The list of card ids of valid targets</param>
        /// <param name="listRestrictionJson">The list resriction, if any</param>
        /// <returns>The card the person chose and false if they chose a target;<br></br>
        /// null and true if they declined to choose a target</returns>
        public async Task<(GameCard target, bool declined)> GetCardTarget
            (string sourceCardName, string blurb, int[] ids, string listRestrictionJson)
        {
            serverNotifier.GetCardTarget(sourceCardName, blurb, ids, listRestrictionJson);
            while (true)
            {
                lock (CardTargetLock)
                {
                    if (CardTarget != null)
                    {
                        var target = CardTarget;
                        CardTarget = null;
                        return (target, false);
                    }
                    else if (DeclineTarget) return (null, true);
                }

                await Task.Delay(TargetCheckDelay);
            }
        }

        /// <summary>
        /// Gets a list of card targets, waiting until the client sends one or sends that they don't want to choose targets
        /// </summary>
        /// <param name="sourceCardName">The card whose effect is asking for targets</param>
        /// <param name="blurb">The description of the targets to get</param>
        /// <param name="ids">The list of card ids of valid targets</param>
        /// <param name="listRestrictionJson">The list resriction, if any</param>
        /// <returns>The cards the person chose and false if they chose targets;<br></br>
        /// null and true if they declined to choose targets</returns>
        public async Task<(IEnumerable<GameCard> chocies, bool declined)> GetCardListTargets
            (string sourceCardName, string blurb, int[] ids, string listRestructionJson)
        {
            serverNotifier.GetCardTarget(sourceCardName, blurb, ids, listRestructionJson);
            while (true)
            {
                lock (CardListTargetsLock)
                {
                    if (CardListTargets != null)
                    {
                        var targets = CardListTargets;
                        CardListTargets = null;
                        return (targets, false);
                    }
                    else if (DeclineTarget) return (null, true);
                }

                await Task.Delay(TargetCheckDelay);
            }
        }

        /// <summary>
        /// Gets a space target, waiting until the client sends one or send that they don't want to choose a space.
        /// </summary>
        /// <param name="cardName">The card whose effect is asking for a space</param>
        /// <param name="blurb">The description of the space to target</param>
        /// <param name="spaces">The list of valid spaces</param>
        /// <returns>The space and false if the player chose a space<br></br>
        /// default and true if the player declined to choose a space</returns>
        public async Task<((int, int) space, bool declined)> GetSpaceTarget
            (string cardName, string blurb, (int, int)[] spaces)
        {
            serverNotifier.GetSpaceTarget(cardName, blurb, spaces);
            while (true)
            {
                lock (SpaceTargetLock)
                {
                    if (SpaceTarget.HasValue)
                    {
                        var space = SpaceTarget.Value;
                        SpaceTarget = null;
                        return (space, false);
                    }
                    else if (DeclineTarget) return (default, true);
                }

                await Task.Delay(TargetCheckDelay);
            }
        }
        #endregion targeting
    }
}