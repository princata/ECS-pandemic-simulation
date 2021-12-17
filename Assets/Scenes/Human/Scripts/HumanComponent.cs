using HumanStatusEnum;
using Unity.Entities;
using UnityEngine;

public struct HumanComponent : IComponentData
{
    // human needs
    public float hunger;
    public float sportivity;
    public float sociality;
    public float fatigue;
    public float grocery;
    public float work;
    public float need4vax;
    public float firstDoseTime;
    public float immunityTime;
    // social behavior
    public float socialResposibility;
    public float jobEssentiality;
    //home
    public Vector2Int homePosition;
    public Vector2Int officePosition;
    //characteristics
    public HumanStatus age;
    public bool PROvax;
    public int vaccinations;
    //family
    public int familyKey;
    public int numberOfMembers;

}
