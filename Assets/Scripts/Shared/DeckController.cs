﻿using KompasCore.Cards;
using KompasCore.Effects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KompasCore.GameCore
{
    public class DeckController : MonoBehaviour
    {
        public const string BLANK_CARD_PATH = "Card Jsons/Blank Card";

        public Game game;
        public Player Owner;

        //rng for shuffling
        private static readonly System.Random rng = new System.Random();

        public readonly List<GameCard> Deck = new List<GameCard>();

        public int IndexOf(GameCard card) => Deck.IndexOf(card);
        public int DeckSize => Deck.Count;
        public GameCard Topdeck => Deck.FirstOrDefault();
        public GameCard Bottomdeck => Deck.LastOrDefault();

        protected virtual bool AddCard(GameCard card, IStackable stackSrc = null)
        {
            card.Remove(stackSrc);
            card.Location = CardLocation.Deck;
            card.Controller = Owner;
            return true;
        }

        //adding and removing cards
        public virtual bool PushTopdeck(GameCard card, IStackable stackSrc = null)
        {
            AddCard(card, stackSrc);
            Deck.Insert(0, card);
            return true;
        }

        public virtual bool PushBottomdeck(GameCard card, IStackable stackSrc = null)
        {
            AddCard(card, stackSrc);
            Deck.Add(card);
            return true;
        }

        public virtual bool ShuffleIn(GameCard card, IStackable stackSrc = null)
        {
            AddCard(card, stackSrc);
            Deck.Add(card);
            Shuffle();
            return true;
        }

        /// <summary>
        /// Random access remove from deck
        /// </summary>
        public virtual bool RemoveFromDeck(GameCard card)
        {
            if (!Deck.Contains(card)) return false;

            Deck.Remove(card);
            return true;
        }

        //misc
        public void Shuffle()
        {
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                GameCard value = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = value;
            }
        }

        public List<GameCard> CardsThatFitRestriction(CardRestriction cardRestriction)
        {
            List<GameCard> cards = new List<GameCard>();
            foreach (GameCard c in Deck)
            {
                if (c != null && cardRestriction.Evaluate(c))
                    cards.Add(c);
            }
            return cards;
        }
    }
}