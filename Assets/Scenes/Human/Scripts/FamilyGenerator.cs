using HumanStatusEnum;
using Unity.Collections;
using UnityEngine;

public struct FamilyInfo
{
    public int familyKey;
    public int numberOfMembers;
    public HumanStatus age;
    public Vector2Int homePosition;
}

public class FamilyGenerator
{
    public static int familyCounter = 0;
    public static int currentFamily = -1;
    public static int templateCounter = 0;
    public static int studentCounter = 0;
    public static int workerCounter = 0;
    public static int retiredCounter = 0;
    public static Vector2Int lastHomePosition;

    public static NativeArray<Vector2Int> houses;
    public static NativeArray<Vector2Int> OAhouses;

    public void SetHouses(NativeArray<Vector2Int> home , NativeArray<Vector2Int> OAhome)
    {
        houses = home;
        OAhouses = OAhome;
    }


    public FamilyInfo GetFamilyAndAgeDetail()
    {
        FamilyInfo info = new FamilyInfo();

        if (currentFamily != familyCounter)
        {
            if (templateCounter == 4 && familyCounter % 2 == 0) //ogni family counter pari piazzo due anziani nelle case di riposo
                lastHomePosition = OAhouses[Random.Range(0, OAhouses.Length)];
            else
                lastHomePosition = houses[UnityEngine.Random.Range(0, houses.Length)];
        }

        switch (templateCounter)
        {
            case 0:
                //Template 0 (2 students , 2 workers)
                if (studentCounter < 2)
                {
                    studentCounter++;
                    info.age = HumanStatus.Student;
                    info.numberOfMembers = 4;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                else if (workerCounter < 2)
                {
                    workerCounter++;
                    info.age = HumanStatus.Worker;
                    info.numberOfMembers = 4;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                if (studentCounter >= 2 && workerCounter >= 2)
                {
                    templateCounter++;
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

            case 1:
                //Template 1 (1 student , 2 workers , 2 retired)

                if (studentCounter < 1)
                {
                    studentCounter++;
                    info.age = HumanStatus.Student;
                    info.numberOfMembers = 5;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                else if (workerCounter < 2)
                {
                    workerCounter++;
                    info.age = HumanStatus.Worker;
                    info.numberOfMembers = 5;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                else if (retiredCounter < 2)
                {
                    retiredCounter++;
                    info.age = HumanStatus.Retired;
                    info.numberOfMembers = 5;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }

                if (studentCounter >= 1 && workerCounter >= 2 && retiredCounter >= 2)
                {
                    templateCounter++;
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

            case 2:
                //Template 2 (3 students)

                if (studentCounter < 3)
                {
                    studentCounter++;
                    info.age = HumanStatus.Student;
                    info.numberOfMembers = 3;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                if (studentCounter >= 3)
                {
                    templateCounter++;
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

            case 3:
                //Template 3 (2 workers)

                if (workerCounter < 2)
                {
                    workerCounter++;
                    info.age = HumanStatus.Worker;
                    info.numberOfMembers = 2;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                if (workerCounter >= 2)
                {
                    templateCounter++;
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

            case 4:
                //Template 4 (2 retired)

                if (retiredCounter < 2)
                {
                    retiredCounter++;
                    info.age = HumanStatus.Retired;
                    info.numberOfMembers = 2;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                }
                if (retiredCounter >= 2)
                {
                    templateCounter = 0; //ciclo ricomincia
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

        }

        return info;
    }

}
