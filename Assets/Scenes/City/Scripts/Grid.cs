﻿/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using TileMapEnum;
using Unity.Collections;
using UnityEngine;

//Class of the city map
public class Grid<TGridObject>
{

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    //params
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    //constructor with generics
    public Grid(int width, int height, float cellSize, Vector3 originPosition, string[,] array2Dmap, Func<int, Grid<TGridObject>, int, int, TGridObject> createGridObject) //qui posso modificare il costruttore del gridobject
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        //populate the map, setting fixed roads and the other elements randomly
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var floors = array2Dmap[y, x].Split(' ');
                string tms = "";
                int i = 0;
                foreach(var floor in floors)
                {
                   
                   if(i < 8) //Massimo 8 piani perchè al massimo posso rappresentare 8 char(tile) da 4 bit su un int32
                    tms += int.Parse(floor).ToString("X"); //conversione del singolo elemento anche solo di un piano a int per poi portarlo all'hex
                    i++;
                }
                
                gridArray[x, y] = createGridObject(int.Parse(tms, System.Globalization.NumberStyles.HexNumber), this, x, y); //inserito nella griglia numero int da decodificare
                
            }
        }

        //debug show the cell borders, set to false since colors work
        bool showDebug = false;
        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.red, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.red, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.red, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.red, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    //some getters and setters
    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetOrigin()
    {
        return originPosition;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);

    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        //check if position is inside the grid
        if (x >= originPosition.x && y >= originPosition.y && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= originPosition.x && y >= originPosition.y && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    //create a NativeArray of grid elements, so that we can deal with it in our entities/jobs
    //way to have a copy without passing the object
    public NativeArray<int> GetGridByValue(Func<TGridObject, int> convert)//probabilmente bisogna inserire nativemultihashmap
    {
        NativeArray<int> grid = new NativeArray<int>(GetWidth() * GetHeight(), Allocator.Persistent);
        for (int i = 0; i < GetWidth(); i++)
        {
            for (int j = 0; j < GetHeight(); j++)
            {
                grid[i + j * GetWidth()] = convert(gridArray[i, j]);
               
            }
        }
        return grid;
    }

}
