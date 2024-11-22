﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using BepuPhysics.Constraints;

namespace Prowl.Runtime;


[AddComponentMenu($"{Icons.FontAwesome6.HillRockslide}  Physics/{Icons.FontAwesome6.Joint}  Constraints/{Icons.FontAwesome6.ClockRotateLeft}  Angular Swivel Hinge")]
public sealed class AngularSwivelHingeConstraintComponent : TwoBodyConstraintComponent<AngularSwivelHinge>
{
    [SerializeField, HideInInspector] private Vector3 _localSwivelAxisA;
    [SerializeField, HideInInspector] private Vector3 _localHingeAxisB;

    [SerializeField, HideInInspector] private float _springFrequency = 35;
    [SerializeField, HideInInspector] private float _springDampingRatio = 5;

    [ShowInInspector]
    public Vector3 LocalSwivelAxisA
    {
        get
        {
            return _localSwivelAxisA;
        }
        set
        {
            _localSwivelAxisA = value;
            ConstraintData?.TryUpdateDescription();
        }
    }

    [ShowInInspector]
    public Vector3 LocalHingeAxisB
    {
        get
        {
            return _localHingeAxisB;
        }
        set
        {
            _localHingeAxisB = value;
            ConstraintData?.TryUpdateDescription();
        }
    }

    [ShowInInspector]
    public float SpringFrequency
    {
        get
        {
            return _springFrequency;
        }
        set
        {
            _springFrequency = value;
            ConstraintData?.TryUpdateDescription();
        }
    }

    [ShowInInspector]
    public float SpringDampingRatio
    {
        get
        {
            return _springDampingRatio;
        }
        set
        {
            _springDampingRatio = value;
            ConstraintData?.TryUpdateDescription();
        }
    }

    internal override AngularSwivelHinge CreateConstraint()
    {
        return new AngularSwivelHinge
        {
            SpringSettings = new SpringSettings(_springFrequency, _springDampingRatio),
            LocalSwivelAxisA = _localSwivelAxisA,
            LocalHingeAxisB = _localHingeAxisB
        };
    }
}
