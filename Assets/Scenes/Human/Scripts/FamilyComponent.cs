using Unity.Entities;
using UnityEngine;

public struct FamilyComponent : IComponentData
{
    public int familyKey;
    public int numberOfMembers;

    //home
    public Vector2Int homePosition;
    //characteristics

}
