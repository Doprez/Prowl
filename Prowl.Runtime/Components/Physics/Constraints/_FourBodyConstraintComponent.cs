﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using BepuPhysics.Constraints;

namespace Prowl.Runtime;

public abstract class FourBodyConstraintComponent<T> : ConstraintComponent<T> where T : unmanaged, IConstraintDescription<T>, IFourBodyConstraintDescription<T>
{
    [ShowInInspector]
    public Rigidbody? A
    {
        get => this[0];
        set => this[0] = value;
    }

    [ShowInInspector]
    public Rigidbody? B
    {
        get => this[1];
        set => this[1] = value;
    }

    [ShowInInspector]
    public Rigidbody? C
    {
        get => this[2];
        set => this[2] = value;
    }

    [ShowInInspector]
    public Rigidbody? D
    {
        get => this[3];
        set => this[3] = value;
    }

    public FourBodyConstraintComponent() : base(4) { }
}
