//---------------------------------------------------------------------------------------------------------
//	Copyright © 2018 The Darkprogramer aka (Eon Van Wyk) @thedarkprogr on twitter (xDPx)
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace some calls to java.Float methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------
using System;

internal static class Float
{
    public static int floatToRawIntBits(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);

        int result = BitConverter.ToInt32(bytes, 0);

        return result;
    }

    public static float intBitsToFloat(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        float f = BitConverter.ToSingle(bytes, 0);
        return f;
    }
}