//----------------------------------------------------------------------------------------
//	Copyright © 2007 - 2018 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class includes methods to convert Java rectangular arrays (jagged arrays
//	with inner arrays of the same length).
//----------------------------------------------------------------------------------------
internal static class RectangularArrays
{
    public static int[][][] ReturnRectangularIntArray(int size1, int size2, int size3)
    {
        int[][][] newArray = new int[size1][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new int[size2][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new int[size3];
                }
            }
        }

        return newArray;
    }

    public static int[][] ReturnRectangularIntArray(int size1, int size2)
    {
        int[][] newArray = new int[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new int[size2];
        }

        return newArray;
    }

    public static short[][] ReturnRectangularShortArray(int size1, int size2)
    {
        short[][] newArray = new short[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new short[size2];
        }

        return newArray;
    }

    public static Ih264_qpel_mc_func[][] ReturnRectangularIh264_qpel_mc_funcArray(int size1, int size2)
    {
        Ih264_qpel_mc_func[][] newArray = new Ih264_qpel_mc_func[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new Ih264_qpel_mc_func[size2];
        }

        return newArray;
    }

    public static long[][][] ReturnRectangularLongArray(int size1, int size2, int size3)
    {
        long[][][] newArray = new long[size1][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new long[size2][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new long[size3];
                }
            }
        }

        return newArray;
    }

    public static int[][][][] ReturnRectangularIntArray(int size1, int size2, int size3, int size4)
    {
        int[][][][] newArray = new int[size1][][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new int[size2][][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new int[size3][];
                    if (size4 > -1)
                    {
                        for (int array3 = 0; array3 < size3; array3++)
                        {
                            newArray[array1][array2][array3] = new int[size4];
                        }
                    }
                }
            }
        }

        return newArray;
    }

    public static AVFrame[][] ReturnRectangularAVFrameArray(int size1, int size2)
    {
        AVFrame[][] newArray = new AVFrame[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new AVFrame[size2];
        }

        return newArray;
    }

    public static short[][][] ReturnRectangularShortArray(int size1, int size2, int size3)
    {
        short[][][] newArray = new short[size1][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new short[size2][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new short[size3];
                }
            }
        }

        return newArray;
    }

    public static JTextField[][][] ReturnRectangularJTextFieldArray(int size1, int size2, int size3)
    {
        JTextField[][][] newArray = new JTextField[size1][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new JTextField[size2][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new JTextField[size3];
                }
            }
        }

        return newArray;
    }

    public static float[][] ReturnRectangularFloatArray(int size1, int size2)
    {
        float[][] newArray = new float[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new float[size2];
        }

        return newArray;
    }

    public static VertexState[][] ReturnRectangularVertexStateArray(int size1, int size2)
    {
        VertexState[][] newArray = new VertexState[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new VertexState[size2];
        }

        return newArray;
    }

    public static float[][][] ReturnRectangularFloatArray(int size1, int size2, int size3)
    {
        float[][][] newArray = new float[size1][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new float[size2][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new float[size3];
                }
            }
        }

        return newArray;
    }

    public static float[][][][] ReturnRectangularFloatArray(int size1, int size2, int size3, int size4)
    {
        float[][][][] newArray = new float[size1][][][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new float[size2][][];
            if (size3 > -1)
            {
                for (int array2 = 0; array2 < size2; array2++)
                {
                    newArray[array1][array2] = new float[size3][];
                    if (size4 > -1)
                    {
                        for (int array3 = 0; array3 < size3; array3++)
                        {
                            newArray[array1][array2][array3] = new float[size4];
                        }
                    }
                }
            }
        }

        return newArray;
    }

    public static ChannelElement[][] ReturnRectangularChannelElementArray(int size1, int size2)
    {
        ChannelElement[][] newArray = new ChannelElement[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new ChannelElement[size2];
        }

        return newArray;
    }

    public static bool[][] ReturnRectangularBoolArray(int size1, int size2)
    {
        bool[][] newArray = new bool[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new bool[size2];
        }

        return newArray;
    }

    public static AtracGainInfo[][] ReturnRectangularAtracGainInfoArray(int size1, int size2)
    {
        AtracGainInfo[][] newArray = new AtracGainInfo[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new AtracGainInfo[size2];
        }

        return newArray;
    }

    public static WavesData[][] ReturnRectangularWavesDataArray(int size1, int size2)
    {
        WavesData[][] newArray = new WavesData[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new WavesData[size2];
        }

        return newArray;
    }

    public static Granule[][] ReturnRectangularGranuleArray(int size1, int size2)
    {
        Granule[][] newArray = new Granule[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new Granule[size2];
        }

        return newArray;
    }

    public static sbyte[][] ReturnRectangularSbyteArray(int size1, int size2)
    {
        sbyte[][] newArray = new sbyte[size1][];
        for (int array1 = 0; array1 < size1; array1++)
        {
            newArray[array1] = new sbyte[size2];
        }

        return newArray;
    }
}