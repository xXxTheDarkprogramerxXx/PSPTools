//---------------------------------------------------------------------------------------------------------
//	Copyright © 2018 The Darkprogramer aka (Eon Van Wyk) @thedarkprogr on twitter (xDPx)
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace some calls to java.pattern methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------
using System.Text.RegularExpressions;

internal class Pattern
{
    public static Regex compile(string value)
    {
        Regex rx = new Regex(value);
        return rx;
    }
}