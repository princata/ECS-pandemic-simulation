using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class NodesBB
{
   // private static int[,] tab;
    public static NativeHashMap<int, int2> tab;
    private static string filePath = "nodes";

    public static void LoadMatrix()
    {
        if (tab.IsCreated)
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
           
            try
            {
                tab.Add(i, new int2(int.Parse(columns[1]), int.Parse(columns[2])));
            }
            catch
            {
                Debug.LogError("unable to parse element at (" + i + ")!");
                break;
            }
            
        }

        Debug.Log("Matrix loaded with size " + n);
    }

    private static void InitializeMatrix(int matrixSize)
    {
        tab = new NativeHashMap<int, int2>(matrixSize, Allocator.Persistent);

    }


    public static int2 GetXYfromID(int ID)
    {
        if (!tab.ContainsKey(ID))
        {
            Debug.LogError($" id = {ID} not present in file nodes");
            return -1;
        }

        int2 tmp = tab[ID];

        if (tmp.x < 0 || tmp.x > 949 || tmp.y < 0 || tmp.y > 941)
        {
            Debug.LogError("Wrong values in trying to get XY from backbone ID!");
            return -1;
        }
        else
            return tmp;
    }
}
