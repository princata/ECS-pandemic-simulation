using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClosestBB
{
    private static int[,] backmap;
    private static string filePath = "cityMap";
    private static int nRows;
    private static int nCol;

    public static void LoadMatrix()
    {
        if (backmap != null)
        {
            return;
        }

        string ss = "";

        try
        {
            TextAsset SourceFile = (TextAsset)Resources.Load(filePath, typeof(TextAsset));
            ss = SourceFile.text;
        }
        catch
        {
            Debug.LogError("Unable to load file " + filePath);
            return;
        }

        string[] lines = ss.Split('\n');
        int n = lines.Length;
        nRows = n;

        InitializeMatrix(n,lines);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');

            for (int j = 0; j < columns.Length; j++)
            {
                try
                {
                    backmap[i, j] = int.Parse(columns[j]);
                }
                catch
                {
                    Debug.LogError("unable to parse element at (" + i + ", " + j + ")! Element equals '" + columns[j] + "'.");
                    break;
                }
            }
        }

        Debug.Log("ClosestBB loaded with size " + nRows + " " + nCol);
    }

    private static void InitializeMatrix(int matrixSize, string[] lines)
    {
        int maxColumns = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');

            if (columns.Length > maxColumns)
                maxColumns = columns.Length;
        }

        nCol = maxColumns;
        backmap = new int[matrixSize, maxColumns];

        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < maxColumns; j++)
            {
                backmap[i, j] = -1;
            }
        }

    }

    public static int GetClosestBB(int x,int y)
    {
        if(backmap[x,y] < 0)
        {
            Debug.LogError($"x = {x} y = {y} closest bb not exist for these parameters");
        }

        if (x <= nRows && y <= nCol && x >= 0 && y >= 0)
            return backmap[x, y];
        else
        {
            Debug.LogError("Wrong coordinates in trying to get closest backbone node!");
            return -1;
        }

        
    }

}
