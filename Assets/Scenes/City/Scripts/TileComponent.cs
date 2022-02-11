using Unity.Entities;
public struct TileComponent : IComponentData
{
    public TileMapEnum.TileMapSprite currentTile;
    public int currentFloor;
}
//just an enumerator for the different cell types
namespace TileMapEnum
{
    public enum TileMapSprite
    {
        Pub = 1,
        Park = 2,
        Office = 3,
        RoadVertical = 4,
        RoadHorizontal = 5,
        OAhome = 6,
        RoadCrossing = 7,
        Supermarket = 8,
        Home = 9,
        Hospital = 10,
        Gym = 11,
        School = 12,
        Impassable = 13,
        Walkable = 14
    }
}
