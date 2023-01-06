using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PathMatrix
{
    private static int[,] pred;
    private static string filePath = "paths";

    private static void LoadMatrix()
    {
        if (pred != null)
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
                    pred[i, j] = int.Parse(columns[j]);
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
        pred = new int[matrixSize, matrixSize];

        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < matrixSize; j++)
            {
                pred[i, j] = -1;
            }
        }
    }

    public static List<int> GetPath(int startID, int endID)
    {
        LoadMatrix();

        bool reversePath = false;

        
        if (startID > endID)
        {
            int temp = startID;
            startID = endID;
            endID = temp;
            reversePath = true;
        }
        

        List<int> path = new List<int>();

        if (pred[startID, endID] < 0)
        {
            Debug.LogError("Path Unavailable!");
            return null;
        }

        path.Add(endID);

        do
        {
            endID = pred[startID, endID];
            path.Insert(0, endID);
        }
        while (endID != startID);

        if (reversePath)
        {
            path.Reverse();
        }
        
        //Enable this to print full path in console
        /* 
        string debugString = "Returned Path = [";
        for (int n = 0; n < path.Count; n++)
        {
            debugString += path[n];
            if (n < path.Count - 1)
            {
                debugString += ", ";
            }
        }
        debugString += "]";
        Debug.Log(debugString);
        */

        return path;
    }
}
