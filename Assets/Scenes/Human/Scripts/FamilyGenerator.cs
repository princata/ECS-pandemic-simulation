using HumanStatusEnum;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using System;

public struct FamilyInfo
{
    public int familyKey;
    public HumanStatus age;
    public Vector3Int homePosition;
    public int sectionKey;
}

public class FamilyGenerator
{
    public static int numberofInfects;
    public static int currentHMK;
    public static int familyCounter = 0;
    public static int currentFamily = -1;
    public static int templateCounter = 0;
   // public static int studentCounter = 0;
   // public static int workerCounter = 0;
   // public static int retiredCounter = 0;
    public static int countKey = 0;
    public static int countMember = 0;
    public static Vector3Int lastHomePosition;
    public static TemplateInfo templateInfos;
    public static NativeList<Vector3Int> houses;
    public static NativeArray<Vector3Int> OAhouses;

    public void SetHouses(List<Vector3Int> home, NativeArray<Vector3Int> OAhome)
    {

        houses = new NativeList<Vector3Int>(home.Count, Allocator.Temp);
        foreach (var hom in home)
            houses.Add(hom);

        OAhouses = OAhome;
    }

    public void SetTemplateInfo(TemplateInfo t)
    {
        templateInfos = t;
    }

    public void PrintTemplateDebug()
    {
       for(int i = 0; i< templateInfos.templateTotal.Length;i++)
            Debug.Log(templateInfos.templateTotal[i]);
      
       
    }

    public FamilyInfo GetFamilyAndAgeDetail()
    {
        FamilyInfo info = new FamilyInfo();

        if (currentFamily != familyCounter)
        {
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            if (templateCounter == (templateInfos.templates.Length - 1) && familyCounter % 2 == 0) //ogni family counter pari piazzo due anziani nelle case di riposo
                lastHomePosition = OAhouses[UnityEngine.Random.Range(0, OAhouses.Length)];
            else
            {
                int index = UnityEngine.Random.Range(0, houses.Length);
                lastHomePosition = houses.ElementAt(index);
               // houses.RemoveAtSwapBack(index);
            }


        }


        if (templateInfos.templateTotal[templateCounter] <= 0)//change template when the total number of families for the current template are implemented
            templateCounter++;
        if (templateCounter >= templateInfos.templates.Length)//if there are more agents, the cycle start again, adding a family for each type
            templateCounter = 0;
        
        if (countMember < templateInfos.nComponents[templateCounter])
        {

            switch (templateInfos.templates[templateCounter].ToString()[countMember])//analysing each component of the family
            {
                case '1': //students
                    info.age = HumanStatus.Student;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;

                    break;

                case '2'://workers
                    info.age = HumanStatus.Worker;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;
                    break;

                case '3'://retired
                    info.age = HumanStatus.Retired;
                    info.familyKey = familyCounter;
                    info.homePosition = lastHomePosition;
                    currentFamily = familyCounter;
                    info.sectionKey = currentHMK;

                    break;

            }
            countMember++;
        } 

        if(templateCounter < templateInfos.templates.Length)
            templateInfos.templateTotal[templateCounter]--;
        familyCounter++;
        countMember = 0; //start again with another family
  
       
        return info;
    }


    public void Disposing()
    {

        houses.Dispose();      
        OAhouses.Dispose();
     
    }

}
