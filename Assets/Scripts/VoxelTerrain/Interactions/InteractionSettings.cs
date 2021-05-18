using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct InteractionSettings
{
#pragma warning disable 0649
    [SerializeField, Tooltip("Radius of single mouse interaction"), Range(0, 3)] private int _mouseSize;
    [SerializeField, Tooltip("Distance of cube area on X Axis")] private float _cubeXDistance;
    [SerializeField, Tooltip("Distance of cube area on Z Axis")] private float _cubeZDistance;
    [SerializeField, Tooltip("Radius for circle mode")] private float _circleRadius;
    [SerializeField, Tooltip("Radius for Sphere mode")] private float _sphereRadius;
    [SerializeField, Tooltip("Height for cubing and circle modes above ground")] private float _height;
    [SerializeField, Tooltip("Dig depth for cubic and circle modes below ground")] private float _dig;
#pragma warning restore 0649

    public int MouseSize => _mouseSize;
    public float CubeXDistance => _cubeXDistance;
    public float CubeZDistance => _cubeZDistance;
    public float CircleRadius => _circleRadius;
    public float SphereRadius => _sphereRadius;
    public float Height => _height;
    public float Dig => _dig;
}
