using HumanStatusEnum;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

public struct FamilyInfo
{
    public int familyKey;
    public HumanStatus age;
    public Vector3Int homePosition;
    public int sectionKey;
}

public class FamilyGenerator
{
    public static int currentHMK;
    public static int familyCounter = 0;
    public static int currentFamily = -1;
    public static int templateCounter = 0;
    public static int studentCounter = 0;
    public static int workerCounter = 0;
    public static int retiredCounter = 0;
    public static int countKey = 0;
    public static Vector3Int lastHomePosition;
    public static TemplateInfo templateInfos;
    public static NativeMultiHashMap<int,Vector3Int> houses;
    public static NativeList<Vector3Int> localHouses;
    public static NativeArray<int> keys;
    public static NativeList<int> keyS;
    public static NativeArray<Vector3Int> OAhouses;

    public void SetHouses(NativeMultiHashMap<int, Vector3Int> home , NativeArray<Vector3Int> OAhome)
    {
        houses = home;
        keys = houses.GetKeyArray(Allocator.Temp);
        keyS = new NativeList<int>(Allocator.Temp);
        foreach( var key in keys)
        {
            if (keyS.Contains(key))
                continue;
            else
                keyS.Add(key);
        }
        OAhouses = OAhome;
        localHouses = new NativeList<Vector3Int>(Allocator.Persistent);
        countKey = keyS[0];
    }

    public void SetTemplateInfo(TemplateInfo t)
    {
        templateInfos = t;
    }

    public void PrintTemplateDebug()
    {
        Debug.Log(templateInfos.template1Total);
        Debug.Log(templateInfos.template2Total);
        Debug.Log(templateInfos.template3Total);
        Debug.Log(templateInfos.template4Total);
        Debug.Log(templateInfos.template5Total);
    }

    public FamilyInfo GetFamilyAndAgeDetail()
    {
        FamilyInfo info = new FamilyInfo();

        if (currentFamily != familyCounter)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            if (templateCounter == 4 && familyCounter % 2 == 0)
            { //ogni family counter pari piazzo due anziani nelle case di riposo
                lastHomePosition = OAhouses[UnityEngine.Random.Range(0, OAhouses.Length)];              
            }
            else
            {
                if (countKey >= keyS.Length)
                    countKey = keyS[0]; //ricomincia il ciclo di assegnazione di ogni famiglia ad un quadrante diverso

                currentHMK = keyS[countKey++];
                
                NativeMultiHashMap<int, Vector3Int>.Enumerator e = houses.GetValuesForKey(currentHMK);
                while (e.MoveNext())
                {
                    localHouses.Add(e.Current); //aggiungo tutte le possibili case di quel quadrante
                }
                int index;
               
                if (localHouses.Length == 1)
                    index = 0;
                else
                    index = UnityEngine.Random.Range(0, localHouses.Length);

                lastHomePosition = localHouses.ElementAt(index);
                localHouses.Clear();
                //info.sectionKey = currentHMK;
                //houses.RemoveAtSwapBack(index);
            }

           
        }

        if(templateInfos.template1Total > 0)
        {
            templateCounter = 0;            
        }
        else if (templateInfos.template2Total > 0)
        {
            templateCounter = 1;
            
        }
        else if (templateInfos.template3Total > 0)
        {
            templateCounter = 2;
           
        }
        else if (templateInfos.template4Total > 0)
        {
            templateCounter = 3;
            
        }
        else if (templateInfos.template5Total > 0)
        {
            templateCounter = 4;
           
        }

       
        switch (templateCounter)
        {
            case 0:
                //Template 0 (2 students , 2 workers)
                if (studentCounter < 2)
                {
                    studentCounter++;
                    info.age = HumanStatus.Student;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                }
                else if (workerCounter < 2)
                {
                    workerCounter++;
                    info.age = HumanStatus.Worker;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                }
                if (studentCounter >= 2 && workerCounter >= 2)
                {
                    templateInfos.template1Total--;
                    // templateCounter++;
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
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                }
                else if (workerCounter < 2)
                {
                    workerCounter++;
                    info.age = HumanStatus.Worker;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                }
                else if (retiredCounter < 2)
                {
                    retiredCounter++;
                    info.age = HumanStatus.Retired;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                }

                if (studentCounter >= 1 && workerCounter >= 2 && retiredCounter >= 2)
                {
                    templateInfos.template2Total--;
                    //templateCounter++;
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
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    info.sectionKey = currentHMK;
                    currentFamily = familyCounter;
                }
                if (studentCounter >= 3)
                {
                    // templateCounter++;
                    templateInfos.template3Total--;
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
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    info.sectionKey = currentHMK;
                    currentFamily = familyCounter;
                }
                if (workerCounter >= 2)
                {
                    // templateCounter++;
                    templateInfos.template4Total--;
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
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    info.sectionKey = GetPositionHashMapKey(lastHomePosition.x, lastHomePosition.y);
                    currentFamily = familyCounter;
                }
                if (retiredCounter >= 2)
                {
                    // templateCounter = 0;
                    templateInfos.template5Total--;
                    familyCounter++;
                    studentCounter = 0;
                    workerCounter = 0;
                    retiredCounter = 0;
                }

                break;

        }

        return info;
    }

    public static int GetPositionHashMapKey(int x, int y)
    {
        return (int)(math.floor(x / Human.conf.sectionSize) + (Human.quadrantYMultiplier * math.floor(y / Human.conf.sectionSize)));
    }

    public void Disposing()
    {
        keys.Dispose();
        keyS.Dispose();
       // houses.Dispose();
        localHouses.Dispose();
        //houses.Dispose();
        OAhouses.Dispose();
    }

}
