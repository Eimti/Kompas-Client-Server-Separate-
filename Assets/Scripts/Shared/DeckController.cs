﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DeckController : MonoBehaviour
{
    public const string BLANK_CARD_PATH = "Card Jsons/Blank Card";

    public Game game;

    //one of these for each player
    public Player Owner;

    //rng for shuffling
    private static readonly System.Random rng = new System.Random();

    public abstract IEnumerable<GameCard> Deck { get; }

    public abstract int IndexOf(GameCard card);
    public int DeckSize => Deck.Count();
    public abstract void InsertAt(GameCard card, int index);
    public abstract void RemoveAt(int index);

    /// <summary>
    /// Gets the card at the designated index.
    /// </summary>
    /// <param name="index">Index of the card to get</param>
    /// <param name="remove">Whether or not to remove the card</param>
    /// <param name="shuffle">Whether or not to shuffle the deck after getting the card</param>
    /// <returns></returns>
    public GameCard CardAt(int index, bool remove, bool shuffle = false)
    {
        if (index > Deck.Count()) return null;
        GameCard card = Deck.ElementAt(index);
        if (remove) RemoveAt(index);
        if (shuffle) Shuffle();
        return card;
    }

    private void AddCard(GameCard card)
    {
        card.Location = CardLocation.Deck;
        card.Controller = Owner;
    }

    //adding and removing cards
    public void PushTopdeck(GameCard card)
    {
        InsertAt(card, 0);
        AddCard(card);
    }

    public void PushBottomdeck(GameCard card)
    {
        Deck.Append(card);
        AddCard(card);
    }

    public void ShuffleIn(GameCard card)
    {
        Deck.Add(card);
        Shuffle();
        AddCard(card);
    }

    public GameCard PopTopdeck()
    {
        if (Deck.Count == 0) return null;

        GameCard card = Deck[0];
        Deck.RemoveAt(0);
        return card;
    }

    public GameCard PopBottomdeck()
    {
        if (Deck.Count == 0) return null;

        GameCard card = Deck[Deck.Count - 1];
        Deck.RemoveAt(Deck.Count - 1);
        return card;
    }

    public GameCard RemoveCardWithName(string name)
    {
        GameCard toReturn;
        for(int i = 0; i < Deck.Count; i++)
        {
            if (Deck[i].CardName.Equals(name))
            {
                toReturn = Deck[i];
                Deck.RemoveAt(i);
                return toReturn;
            }
        }
        return null;
    }

    /// <summary>
    /// Random access remove from deck
    /// </summary>
    public void RemoveFromDeck(GameCard card)
    {
        Deck.Remove(card);
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

    /// <summary>
    /// Checks if a card exists that fits the given restriction
    /// </summary>
    /// <param name="cardRestriction"></param>
    /// <returns></returns>
    public bool Exists(CardRestriction cardRestriction)
    {
        foreach(GameCard c in Deck)
        {
            if (c != null && cardRestriction.Evaluate(c)) return true;
        }

        return false;
    }

    public List<GameCard> CardsThatFitRestriction(CardRestriction cardRestriction)
    {
        List<GameCard> cards = new List<GameCard>();
        foreach(GameCard c in Deck)
        {
            if (c != null && cardRestriction.Evaluate(c))
                cards.Add(c);
        }
        return cards;
    }
}
