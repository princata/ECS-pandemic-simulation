using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PathMatrix
{
    private static int[,] pred;
    private static string filePath = "paths";

    public static void LoadMatrix()
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

            for (int j = 0; j < columns.Length; j++) //old was j = i
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

        /*
        bool reversePath = false;
        
        if (startID > endID)
        {
            int temp = startID;
            startID = endID;
            endID = temp;
            reversePath = true;
        }
        */
        if(startID < 0 || startID > pred.Length || endID < 0 || endID > pred.Length)
        {
            Debug.LogError("startID = " + startID + "endID = " + endID + "this values are outside of ranges");
            return null;
        }


        List<int> path = new List<int>();

        if (startID == endID)
        {
            path.Add(endID);
            return path;
        }

        if (pred[startID, endID] < 0)
        {
            Debug.LogError("Path Unavailable! Start ID = " + startID + ", endID = " + endID);
            return null;
        }

        path.Add(endID);

       

        do
        {
            try
            {
                //endID = pred[startID, endID];
                int nextEndID = pred[startID, endID];
                if (nextEndID < 0)
                {
                    Debug.LogError("ERROR: Position in table at [" + startID + ", " + endID + "] = " + nextEndID);
                    string pathString = "[";
                    for (int i = 0; i < path.Count; i++)
                    {
                        pathString += path[i]+", ";
                    }
                    pathString += "]";
                    Debug.LogError("Path computed until error: " + pathString);
                    return null;
                }
                else
                {
                    endID = nextEndID;
                }
            }
            catch
            {
                Debug.LogError("Error! Index outside of bounds. startID = " + startID + ", endID = " + endID + ". current Path = " + path.ToString());
                string pathString = "[";
                for (int i = 0; i < path.Count; i++)
                {
                    pathString += path[i] + ", ";
                }
                pathString += "]";
                Debug.LogError("Path computed until error: " + pathString);
                return null;
            }
            path.Insert(0, endID);
        }
        while (endID != startID);

        /*
        if (reversePath)
        {
            path.Reverse();
        }
        */

        //Enable this to print full path in console

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


        return path;
    }

    public static void Test(Text txt)
    {
        LoadMatrix();

        //DEBUGSTUFF
        string matrixString = "";

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                matrixString += pred[i, j]+",";
            }

            matrixString += "\n";
        }

        txt.text = matrixString;
    }
}
