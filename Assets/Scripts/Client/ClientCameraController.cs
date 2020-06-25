﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCameraController : MonoBehaviour
{
    public const float ZoomFactorBase = 0.5f;
    public const float PanFactorBase = 0.4f;
    public const float RotationFactorBase = 1f;

    public float ZoomFactor => Mathf.Log10(transform.position.y) * ZoomFactorBase;
    public float PanFactor => Mathf.Log10(transform.position.y) * PanFactorBase;
    public float RotationAngle => Mathf.Log10(transform.position.y) * RotationFactorBase;

    public Vector3 Down     => PanFactor * Vector3.down;
    public Vector3 Up       => PanFactor * Vector3.up;
    public Vector3 Forward  => ZoomFactor * Vector3.forward;
    public Vector3 Backward => ZoomFactor * Vector3.back;
    public Vector3 Left     => PanFactor * Vector3.left;
    public Vector3 Right    => PanFactor * Vector3.right;

    public void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.F) && transform.position.y > 1) transform.Translate(Forward);
        if (Input.GetKey(KeyCode.R) && transform.position.y < 20) transform.Translate(Backward);

        if (Input.GetKey(KeyCode.W)) transform.Translate(Up);
        if (Input.GetKey(KeyCode.S)) transform.Translate(Down);
        if (Input.GetKey(KeyCode.A)) transform.Translate(Left);
        if (Input.GetKey(KeyCode.D)) transform.Translate(Right);

        if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.back, RotationAngle);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.forward, RotationAngle);

        if (Input.GetKeyUp(KeyCode.Space)) transform.eulerAngles = new Vector3(90f, 0f, 0f);
    }
}