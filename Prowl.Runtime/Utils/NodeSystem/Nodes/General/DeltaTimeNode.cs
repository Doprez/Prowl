﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

namespace Prowl.Runtime.NodeSystem;

[Node("General/Delta Time")]
public class DeltaTimeNode : Node
{
    public override bool ShowTitle => false;
    public override string Title => "Delta Time";
    public override float Width => 100;

    [Output, SerializeIgnore] public double DeltaTime;
    [Output, SerializeIgnore] public float DeltaTimeF;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(DeltaTime))
            return DeltaTime;
        return DeltaTimeF;
    }
}
