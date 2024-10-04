﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;

namespace Prowl.Runtime.GUI;

public struct Spacing
{
    public double Left;
    public double Right;
    public double Top;
    public double Bottom;

    public readonly double Horizontal => Left + Right;
    public readonly double Vertical => Top + Bottom;
    public readonly Vector2 TopLeft => new(Left, Top);
    public readonly Vector2 TopRight => new(Right, Top);
    public readonly Vector2 BottomLeft => new(Left, Bottom);
    public readonly Vector2 BottomRight => new(Right, Bottom);

    public Spacing() { }

    public Spacing(double left, double right, double top, double bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public static bool operator ==(Spacing s1, Spacing s2)
    {
        return MathD.ApproximatelyEquals(s1.Left, s2.Left) &&
               MathD.ApproximatelyEquals(s1.Right, s2.Right) &&
               MathD.ApproximatelyEquals(s1.Top, s2.Top) &&
               MathD.ApproximatelyEquals(s1.Bottom, s2.Bottom);
    }

    public static bool operator !=(Spacing s1, Spacing s2)
    {
        return MathD.ApproximatelyEquals(s1.Left, s2.Left) ||
               MathD.ApproximatelyEquals(s1.Right, s2.Right) ||
               MathD.ApproximatelyEquals(s1.Top, s2.Top) ||
               MathD.ApproximatelyEquals(s1.Bottom, s2.Bottom);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Spacing value)
            return false;

        return value.Left == Left && value.Right == Right && value.Top == Top && value.Bottom == Bottom;
    }
}
