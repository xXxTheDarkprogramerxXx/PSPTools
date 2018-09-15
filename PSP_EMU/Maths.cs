//---------------------------------------------------------------------------------------------------------
//	Copyright © 2018 The Darkprogramer aka (Eon Van Wyk) @thedarkprogr on twitter (xDPx)
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace some calls to java.math methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------
using System;

internal static class Maths
{
    public static double cbrt(float value)
    {
        return Math.Pow(Convert.ToDouble(value), 0.3333333333333333);
    }
}