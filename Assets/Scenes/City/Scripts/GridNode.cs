using TileMapEnum;

//element of each cell, set the param iswalkable if road in order to perform proper pathfinding
public class GridNode
{

    private Grid<GridNode> grid;
    private int x;
    private int y;

    private bool isWalkable;
    //mettere una label di presenza di Z layer
    private int tiles;//possibilità di inserire un array di tile
    private int firstfloor;
    public GridNode(int tile, Grid<GridNode> grid, int x, int y)
    {
        this.grid = grid;
        this.tiles = tile;
        this.x = x;
        this.y = y;
        this.firstfloor = int.Parse(tile.ToString("X")[0].ToString(), System.Globalization.NumberStyles.HexNumber);

        if (this.firstfloor == (int)TileMapSprite.RoadCrossing ||
            this.firstfloor == (int)TileMapSprite.RoadVertical ||
            this.firstfloor == (int)TileMapSprite.RoadHorizontal ||
            this.firstfloor == (int)TileMapSprite.Park||
            this.firstfloor == (int)TileMapSprite.Walkable)
            isWalkable = true;
        else isWalkable = false;
        
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public int GetTiles()
    {
        return tiles;
    }

    public int GetFirstFloors()
    {
        return firstfloor;
    }

}
