﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    //constants
    //all cards should be on layer 9
    public const int layerMask = 1 << 9;

    //raycasting variables
    private Ray mouseRay;
    private RaycastHit raycastHit;
    private Vector3 rayIntersectBoard;
    //game objects hit
    private GameObject gameObjectHit;
    private GameObject lastHit;
    private KompasObject kompasObjectHit;
    private Card cardHit;
    private DeckController deckHit;
    private HandController handHit;

    //raycasting
    public Ray GetMouseRay()
    {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        return mouseRay;
    }
    public RaycastHit GetRaycastHit()
    {
        Physics.Raycast(mouseRay, out raycastHit, float.PositiveInfinity, layerMask);
        return raycastHit;
    }

    //ray intersection
    public Vector3 GetRayIntersectBoard(Ray mouseRay)
    {
        rayIntersectBoard = new Vector3(
            (2f - 1 * mouseRay.origin.y) * mouseRay.direction.x / mouseRay.direction.y,     //x
            2,                                                                              //y. 2 because 2 is where cards should hover
            (2f - 1 * mouseRay.origin.y) * mouseRay.direction.z / mouseRay.direction.y);    //z
        return rayIntersectBoard;
    }
    public Vector3 GetRayIntersectBoard() { return GetRayIntersectBoard(GetMouseRay()); }

    //methods to call during update
    //helper method
    private void ClearHits()
    {
        gameObjectHit = null;
        cardHit = null;
        deckHit = null;
        handHit = null;
    }

    public void DragBeforeRaycast()
    {
        rayIntersectBoard = GetRayIntersectBoard(mouseRay);
        //if mouse button is held down and we're currently dragging a card, then keep dragging the card
        //if for some reason i end up letting people drag other stuff, i can change this to be nested ifs, but for now, this is more readable
        if (Input.GetMouseButton(0) && cardHit != null)
        {
            cardHit.OnDrag(rayIntersectBoard);
        }
        //if the mouse button was just lifted and we havent yet ended the drag, deal with that
        else if (Input.GetMouseButtonUp(0) && cardHit != null)
        {
            cardHit.OnDragEnd(rayIntersectBoard);
            cardHit = null;
        }
    }

    public void NormalClickObject()
    {
        //to get game object
        if (raycastHit.transform != null) gameObjectHit = raycastHit.transform.gameObject;
        else gameObjectHit = null;

        //if (Input.GetMouseButtonDown(0)) Debug.Log("Hit null?" + (gameObjectHit == null));

        if (gameObjectHit != null) kompasObjectHit = gameObjectHit.GetComponent<KompasObject>();
        else kompasObjectHit = null;

        //if (Input.GetMouseButtonDown(0)) Debug.Log("Object clicked: " + (kompasObjectHit == null));

        if (kompasObjectHit == null)
        {
            ClearHits();
            Game.mainGame.uiCtrl.StopHovering();
            //if we clicked on nothing, select nothing
            if(Input.GetMouseButtonDown(0)) Game.mainGame.uiCtrl.SelectCard(null, Game.TargetMode.NoTargeting, true);
        }
        else
        {
            //if click, do click, otherwise do hover. drag is taken care of before this
            if (Input.GetMouseButtonDown(0)) kompasObjectHit.OnClick();
            else kompasObjectHit.OnHover();

            if (kompasObjectHit is Card) cardHit = kompasObjectHit as Card;
            else if (kompasObjectHit is DeckController) deckHit = kompasObjectHit as DeckController;
            else if (kompasObjectHit is HandController) handHit = kompasObjectHit as HandController;
        }
    }


}
