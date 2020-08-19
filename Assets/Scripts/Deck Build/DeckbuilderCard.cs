﻿using KompasCore.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KompasDeckbuilder
{
    public abstract class DeckbuilderCard : CardBase, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
    {
        protected CardSearchController cardSearchController;
        protected DeckbuilderController deckbuildCtrl => cardSearchController.DeckbuilderCtrl;

        protected Image image;
        private bool InDeck;

        public abstract string BlurbString { get; }

        public void Awake()
        {
            image = GetComponent<Image>();
        }

        public virtual void SetInfo(CardSearchController searchCtrl, SerializableCard card, bool inDeck)
        {
            SetInfo(card);
            SetImage(CardName);
            InDeck = inDeck;
            cardSearchController = searchCtrl;
        }

        public void Select()
        {
            cardSearchController.Select(this);
        }

        /// <summary>
        /// Shows this card in the "selected card" area of the deckbuilder
        /// </summary>
        public virtual void Show()
        {
            cardSearchController.CardImage.sprite = detailedSprite;
            cardSearchController.CardNameText.text = CardName;
            cardSearchController.SubtypesText.text = SubtypeText;
            cardSearchController.EffectText.text = EffText;
        }

        public void Unshow()
        {
            cardSearchController.ShowSelectedCard();
        }

        protected void SetImage(string cardFileName)
        {
            detailedSprite = Resources.Load<Sprite>("Detailed Sprites/" + cardFileName);
            simpleSprite = Resources.Load<Sprite>("Simple Sprites/" + cardFileName);
            //check if either is null. if so, log to debug and return
            if (detailedSprite == null || simpleSprite == null)
            {
                Debug.LogError("Could not find sprite with name " + cardFileName);
            }
            image.sprite = simpleSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                deckbuildCtrl.RemoveFromDeck(this);
        }

        public void OnPointerDown(PointerEventData eventData) 
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                deckbuildCtrl.CurrentDrag = this;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"Pointer Up on {CardName}");
            deckbuildCtrl.CurrentDrag = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            deckbuildCtrl.CurrentDrag?.SetIndex(transform.GetSiblingIndex());
        }

        public void SetIndex(int index)
        {
            transform.SetSiblingIndex(index);
            deckbuildCtrl.MoveTo(this, index);
        }

    }
}