﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public abstract class Card : CardBase {
    public Game game;
    public CardController cardCtrl;

    //stats
    public int Negations { get; private set; } = 0;
    public bool Negated {
        get => Negations > 0;
        set {
            if (value) Negations++;
            else Negations--;

            foreach (var e in Effects) e.Negated = value;
        }
    }
    public int Activations { get; private set; } = 0;
    public bool Activated
    {
        get => Activations > 0;
        set
        {
            if (value) Activations++;
            else Activations--;
        }
    }
    public abstract int Cost { get; }
    public abstract string StatsString { get; }
    public virtual bool Summoned { get => false; }

    //positioning
    public int BoardX { get; protected set; }
    public int BoardY { get; protected set; }
    public (int, int) Position { get => (BoardX, BoardY); }

    //movement
    public int SpacesMoved { get; protected set; }
    public virtual int SpacesCanMove { get => 0; }
    public MovementRestriction MovementRestriction { get; private set; }

    //attacking
    public AttackRestriction AttackRestriction { get; private set; }

    //playing
    public PlayRestriction PlayRestriction { get; private set; }

    //controller/owners
    public Player Controller { get; protected set; }
    public int ControllerIndex { get { return Controller.index; } }
    public Player Owner { get; private set; }
    public int OwnerIndex { get => Owner.index; }

    //misc
    public CardLocation Location { get; private set; }
    public int ID { get; private set; }
    public Effect[] Effects { get; private set; }
    public List<AugmentCard> Augments { get; private set; } = new List<AugmentCard>();
    public int TurnsOnBoard { get; private set; }
    
    public int IndexInList
    {
        get
        {
            switch (Location)
            {
                case CardLocation.Deck:
                    return Controller.deckCtrl.IndexOf(this);
                case CardLocation.Discard:
                    return Controller.discardCtrl.IndexOf(this);
                case CardLocation.Field:
                    return BoardX * 7 + BoardY;
                case CardLocation.Hand:
                    return Controller.handCtrl.IndexOf(this);
                default:
                    Debug.LogError($"Tried to ask for card index when in location {Location}");
                    return -1;
            }
        }
    }

    //set data
    /// <summary>
    /// Updates the location variable, sets active or not, and sets the parent transform.
    /// </summary>
    /// <param name="location">Where the card is going</param>
    public void SetLocation(CardLocation location)
    {
        Debug.Log($"Attempting to move {CardName} from {this.Location} to {location}");

        //set the card's location variable
        this.Location = location;
        cardCtrl.SetPhysicalLocation(location);
    }

    /// <summary>
    /// Set image by stored card file name
    /// </summary>
    public void SetImage()
    {
        cardCtrl.SetImage(CardName);
    }

    public virtual void SetInfo(SerializableCard serializedCard, Game game, Player owner, Effect[] effects, int id)
    {
        base.SetInfo(serializedCard);

        this.Augments = new List<AugmentCard>();
        this.game = game;
        this.Owner = owner;
        this.ID = id;
        ChangeController(owner);

        this.Effects = effects;

        SpacesMoved = 0;
        MovementRestriction = serializedCard.MovementRestriction ?? new MovementRestriction();
        MovementRestriction.SetInfo(this);
        AttackRestriction = serializedCard.AttackRestriction ?? new AttackRestriction();
        AttackRestriction.SetInfo(this);
        PlayRestriction = serializedCard.PlayRestriction ?? new PlayRestriction();
        PlayRestriction.SetInfo(this);
    }

    #region distance/adjacency
    public int DistanceTo(int x, int y)
    {
        if (Location != CardLocation.Field) return -1;
        return Mathf.Abs(x - BoardX) > Mathf.Abs(y - BoardY) ? Mathf.Abs(x - BoardX) : Mathf.Abs(y - BoardY);
        /* equivalent to
         * if (Mathf.Abs(card.X - X) > Mathf.Abs(card.Y - Y)) return Mathf.Abs(card.X - X);
         * else return Mathf.Abs(card.Y - Y);
         * is card.X - X > card.Y - Y? If so, return card.X -X, otherwise return card.Y - Y
        */
    }
    public int DistanceTo(Card card) => DistanceTo(card.BoardX, card.BoardY);
    public bool WithinSlots(int numSlots, Card card)
    {
        if (card == null) return false;
        return DistanceTo(card) <= numSlots;
    }
    public bool WithinSlots(int numSlots, int x, int y) => DistanceTo(x, y) <= numSlots;
    public bool IsAdjacentTo(Card card) => DistanceTo(card) == 1;
    public bool IsAdjacentTo(int x, int y) => DistanceTo(x, y) == 1;
    public virtual bool CardInAOE(Card c) => false;
    public virtual bool SpaceInAOE(int x, int y) => false;
    #endregion distance/adjacency

    //misc mechanics methods
    public virtual void ChangeController(Player newController)
    {
        Controller = newController;
        cardCtrl.SetRotation();
    }

    /// <summary>
    /// Sets this card's x and y values and updates its transform
    /// </summary>
    public virtual void MoveTo(int toX, int toY, bool playerInitiated)
    {
        if(playerInitiated)
            SpacesMoved += System.Math.Abs(BoardX - toX) + System.Math.Abs(toY - toY);

        BoardX = toX;
        BoardY = toY;

        /* for setting where the gameobject is, it would be x and z, except that the quad is turned 90 degrees
         * so we change the local x and y. the z coordinate also therefore needs to be negative
         * to show the card above the game board on the screen. */
        transform.localPosition = BoardController.GridIndicesFromPos(toX, toY);
        ChangeController(Controller);
        foreach (AugmentCard aug in Augments) aug.MoveTo(toX, toY, false);
    }

    public void PutBack()
    {
        Debug.Log($"Putting back {CardName} to {Location}");

        cardCtrl.SetPhysicalLocation(Location);
    }

    /// <summary>
    /// Resets any of the card's values that might be different from their originals.
    /// Should be called when cards move out the discard, or into the hand, deck, or annihilation
    /// </summary>
    public virtual void ResetCard() { }

    /// <summary>
    /// Resets anything that needs to be reset for the start of the turn.
    /// </summary>
    public virtual void ResetForTurn(Player turnPlayer)
    {
        foreach(Effect eff in Effects)
        {
            eff.ResetForTurn(turnPlayer);
        }

        SpacesMoved = 0;
        TurnsOnBoard++;
    }

    #region augments
    public void AddAugment(AugmentCard augment)
    {
        if (augment == null) return;
        Augments.Add(augment);
        augment.AugmentedCard = this;
    }
    public bool HasAugment(AugmentCard augment) { return Augments.Contains(augment); }
    public void RemoveAugment(AugmentCard augment)
    {
        Augments.Remove(augment);
        augment.AugmentedCard = null;
    }
    public void RemoveAugmentAt(int index)
    {
        AugmentCard aug = Augments[index];
        Augments.RemoveAt(index);
        aug.AugmentedCard = null;
    }
    #endregion augments
}
