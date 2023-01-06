using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class NodesBB
{
    private static int[,] tab;
    private static string filePath = "nodes";

    private static void LoadMatrix()
    {
        if (tab != null)
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

        InitializeMatrix(n);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');

            for (int j = i; j < columns.Length; j++)
            {
                try
                {
                    tab[i, j] = int.Parse(columns[j]);
                }
                catch
                {
                    Debug.LogError("unable to parse element at (" + i + ", " + j + ")! Element equals '" + columns[j] + "'.");
                    break;
                }
            }
        }

        Debug.Log("Matrix loaded with size " + n);
    }

    private static void InitializeMatrix(int matrixSize)
    {
        tab = new int[matrixSize, 3];

        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                tab[i, j] = -1;
            }
        }
    }


    public static int2 GetXYfromID(int ID)
    {

        LoadMatrix();

        int2 tmp = new int2(tab[ID, 1], tab[ID, 2]);

        if (tmp.x < 0 || tmp.x > 949 || tmp.y < 0 || tmp.y > 941)
        {
            Debug.LogError("Wrong values in trying to get XY from backbone ID!");
            return -1;
        }
        else
            return tmp;
    }
}
