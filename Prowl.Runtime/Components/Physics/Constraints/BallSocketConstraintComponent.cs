﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using BepuPhysics.Constraints;

namespace Prowl.Runtime;


[AddComponentMenu($"{Icons.FontAwesome6.HillRockslide}  Physics/{Icons.FontAwesome6.Joint}  Constraints/{Icons.FontAwesome6.Circle}  Ball Socket")]
public sealed class BallSocketConstraintComponent : TwoBodyConstraintComponent<BallSocket>
{
    [SerializeField, HideInInspector] private Vector3 _localOffsetA;
    [SerializeField, HideInInspector] private Vector3 _localOffsetB;
    [SerializeField, HideInInspector] private float _springFrequency = 35;
    [SerializeField, HideInInspector] private float _springDampingRatio = 5;

    [ShowInInspector]
    public Vector3 LocalOffsetA
    {
        get => _localOffsetA;
        set
        {
            _localOffsetA = value;
            ConstraintData?.TryUpdateDescription();
        }
    }

    [ShowInInspector]
    public Vector3 LocalOffsetB
    {
        get => _localOffsetB;
        set
        {
            _localOffsetB = value;
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

    internal override BallSocket CreateConstraint()
    {
        return new BallSocket
        {
            LocalOffsetA = _localOffsetA,
            LocalOffsetB = _localOffsetB,
            SpringSettings = new SpringSettings(_springFrequency, _springDampingRatio)
        };
    }
}
