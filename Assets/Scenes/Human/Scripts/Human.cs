using HumanStatusEnum;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System;

public struct TemplateInfo
{
    public int template1Total;
    public int template2Total;
    public int template3Total;
    public int template4Total;
    public int template5Total;
}

public struct TileInfo
{
    public int x;
    public int y;
    public int floor;
    public TileMapEnum.TileMapSprite type;
}

public class Human : MonoBehaviour
{
    public static Human Instance { private set; get; }
    NativeArray<Entity> entityArray;
    public static Configuration conf;
    public bool vaccinationPolicy;
   // public bool large;
    public int template1Percent;
    public int template2Percent;
    public int template3Percent;
    public int template4Percent;
    public int template5Percent;
    public long totalIntensiveCare;
    private static FamilyGenerator famGenerator;
    private static TemplateInfo templateInfo;

    [SerializeField] public Mesh mesh;
    [SerializeField] public Material healthyMaterial;
    [SerializeField] public Material sickMaterial;

    [SerializeField] public Material humanSpriteMaterial;

    //public NativeArray<Vector3Int> houses;
    //public NativeArray<int> NeighbourQuadrants;
    public static NativeMultiHashMap<int, TileInfo> places;
    public static NativeMultiHashMap<int, Pathfinding.PathNode> pathFindingMap;
    public static NativeMultiHashMap<int, Vector3Int> housesMap;

    public const int quadrantYMultiplier = 1000;
    public float quadrantCellSize;
    private void Awake()
    {
        Instance = this;

    }
    private void Start()
    {
        float mean, sigma;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //Archetype (same set of component types) so Unity makes sure they are packed nice and tight in memory for an efficient fetch from the CPU.
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(Translation),
            typeof(MoveSpeedComponent),
            typeof(PathFollow),
            typeof(QuadrantEntity),
            typeof(SpriteSheetAnimation_Data),
            typeof(InfectionComponent),
            typeof(TileComponent)
        );

        //Extract configuration from json file
        conf = Configuration.CreateFromJSON();
        int numberOfInfects = conf.NumberOfInfects;
        quadrantCellSize = conf.SectionSize;
        float tmp = ICUproportion(conf.NumberOfHumans); //CALCOLO ICU IN BASE AI DATI VERI SULLE ICU
        this.totalIntensiveCare = Mathf.RoundToInt(tmp);
        

        vaccinationPolicy = conf.VaccinationPolicy;
        //Time Scale
        Time.timeScale = conf.TimeScale; //DA PARAMETRIZZARE

        entityArray = new NativeArray<Entity>(conf.NumberOfHumans, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        //NeighbourQuadrants = new NativeArray<int>(8, Allocator.Persistent);
        //NeighbourQuadrants[0] = 1; // right
        //NeighbourQuadrants[1] = 1 + quadrantYMultiplier; //right-up
        //NeighbourQuadrants[2] = quadrantYMultiplier; //up
        //NeighbourQuadrants[3] = quadrantYMultiplier - 1; //left up
        //NeighbourQuadrants[4] = -1; //left
        //NeighbourQuadrants[5] = 0 - (1 + quadrantYMultiplier); //left down
        //NeighbourQuadrants[6] = - quadrantYMultiplier; //down
        //NeighbourQuadrants[7] = 0 - (quadrantYMultiplier - 1); //right down
        // Get grid size
        int gridWidth = Testing.Instance.grid.GetWidth();
        int gridHeight = Testing.Instance.grid.GetHeight();

       
        //initialize family generator
        famGenerator = new FamilyGenerator();

        templateInfo = FillTemplateData(conf.numberOfHumans);

        // Get houses and offices from grid
       // List<Vector3Int> housesList = new List<Vector3Int>();
     
        NativeMultiHashMap<int, Vector3Int> officesMap = new NativeMultiHashMap<int, Vector3Int>(gridWidth * gridHeight, Allocator.Temp);
        NativeMultiHashMap<int, Vector3Int> schoolMap = new NativeMultiHashMap<int, Vector3Int>(gridWidth * gridHeight, Allocator.Temp);
        housesMap = new NativeMultiHashMap<int, Vector3Int>(gridWidth * gridHeight, Allocator.Persistent);
        housesMap.Capacity = gridWidth * gridHeight;
        List<Vector3Int> officesList = new List<Vector3Int>();
       
        List<Vector3Int> schoolsList = new List<Vector3Int>();
        List<Vector3Int> OAhomeList = new List<Vector3Int>();
        // var mapGrid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTileType(); });
        var mapGrid = Testing.Instance.grid;
        places = new NativeMultiHashMap<int, TileInfo>(gridWidth * gridHeight, Allocator.Persistent);
        places.Capacity = gridWidth * gridHeight;
        //NativeMultiHashMap<int, TileInfo>.ParallelWriter places2 = places.AsParallelWriter();

        pathFindingMap = new NativeMultiHashMap<int, Pathfinding.PathNode>(gridWidth * gridHeight, Allocator.Persistent);
        //NativeMultiHashMap<int, Pathfinding.PathNode>.ParallelWriter pathnodehashmap = pathFindingMap.AsParallelWriter();
        pathFindingMap.Capacity = gridWidth * gridHeight;
     
       
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)//Inserire controllo piani
            {

                int hashmapkey = GetPositionHashMapKey(i, j);
               
                pathFindingMap.Add(hashmapkey, new Pathfinding.PathNode
                {
                    x = i,
                    y = j,
                    index = CalculateIndex(i, j, (int)quadrantCellSize),

                    gCost = int.MaxValue,

                    isWalkable = mapGrid.GetGridObject(i, j).IsWalkable(),
                    cameFromNodeIndex = -1,

                    //pathNodeArray[index] = pathNode
                }); 

                int f = 0;
                string tiles = mapGrid.GetGridObject(i, j).GetTiles().ToString("X"); //conversione numero in hex
                
                foreach (var floor in tiles) //analisi di ogni char rappresentante un piano
                {

                    if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Home)
                    {
                        housesMap.Add(hashmapkey,new Vector3Int(i, j, f++));
                                            
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Office)
                    {
                       
                        officesMap.Add(hashmapkey, new Vector3Int(i,j,f++));
                      
                        
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.School)
                    {
                       
                         schoolMap.Add(hashmapkey, new Vector3Int(i, j, f++));
                     
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.OAhome)
                    {
                        OAhomeList.Add(new Vector3Int(i, j, f++));
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Pub)
                    {
                        
                        places.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Pub
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Park)
                    {
                        
                        places.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Park
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Supermarket)
                    {
                       
                        places.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Supermarket
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Hospital)
                    {
                       
                        places.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Hospital
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Gym)
                    {
                        
                        places.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Gym
                        });
                    }
                }
            }
        }
       
       // NativeArray<Vector3Int> offices = officesList.ToNativeArray<Vector3Int>(Allocator.Temp);
       // NativeArray<Vector3Int> schools = schoolsList.ToNativeArray<Vector3Int>(Allocator.Temp);
        NativeArray<Vector3Int> OAhouses = OAhomeList.ToNativeArray<Vector3Int>(Allocator.Temp);
        famGenerator.SetHouses(housesMap, OAhouses);
        famGenerator.SetTemplateInfo(templateInfo);
        //houses = housesList.ToNativeArray<Vector3Int>(Allocator.Persistent);
        //famGenerator.PrintTemplateDebug();
        float symptomsProbability = 0f;
        float humanDeathProbability = 0f;
        float socialResponsability = 0f;
        float jobEssentiality = 0f;
        float firstDoseTime = 0f;
        bool PROvax = false;
        //TODO model social responsibility
        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            FamilyInfo familyInfo = famGenerator.GetFamilyAndAgeDetail();
            HumanStatus age = familyInfo.age;
            var homePosition = familyInfo.homePosition;
            var officePosition = Vector3Int.zero;
            var hashmapkeyHome = GetPositionHashMapKey(homePosition.x, homePosition.y);
         //   Debug.Log($"entity {entity.Index} hmkHome {hashmapkeyHome} sectionkey{familyInfo.sectionKey}");
           // var startKey = hashmapkeyHome;
            var found = false;
          //  var count = 0;
            
            socialResponsability = GenerateNormalRandom(0.5f, 0.45f, 0f, 0.99f);

            
            if (socialResponsability > 0.35f)
                PROvax = true;
            else
                PROvax = false;
            

            if (age == HumanStatus.Student) // 5-30 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

                              
                do
                {
                    NativeMultiHashMap<int, Vector3Int>.Enumerator e = schoolMap.GetValuesForKey(hashmapkeyHome);
                    while (e.MoveNext())
                    {
                        schoolsList.Add(e.Current);
                    }
                    if (schoolsList.Count <= 0)//SE NEL QUADRANTE CORRENTE NON C'E' UNA SCUOLA CERCO NEI QUADRANTI ADIACENTI
                    {
                       // do
                       // {
                          //  if(count >= NeighbourQuadrants.Length)//NON HO TROVATO NULLA NEMMENO NEI QUADRANTI ADIACENTI
                           // {
                        Debug.LogError($"The map is incorrectly built, no school is found in a space of {quadrantCellSize}x{quadrantCellSize} times 9. Rebuild your map, otherwise try to increase section size");
                        UnityEditor.EditorApplication.isPlaying = false;
                                
                           // }
                           // hashmapkeyHome = startKey;
                           // hashmapkeyHome += NeighbourQuadrants[count++];
                        //} while (schoolMap.ContainsKey(hashmapkeyHome));//QUESTO CHECK MI PERMETTE DI PRENDERE SOLO I QUADRANTI ADIACENTI VALIDI, CIOE' CHE CADONO ALL'INTERNO DELLA MAPPA
                        
                    }
                    else
                    {
                        if(schoolsList.Count == 1)
                            officePosition = schoolsList[0];
                        else
                            officePosition = schoolsList[UnityEngine.Random.Range(0, schoolsList.Count)];
                        
                        found = true;
                        schoolsList.Clear();
                    }

                } while (!found);


                
                symptomsProbability = GenerateNormalRandom(0.2f, 0.1f, 0f, 1f) * 100; //20% sintomi
                humanDeathProbability = GenerateNormalRandom(0.01f, 0.1f, 0.01f, 1f) * 100; //1% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(10f * 25f * 60f, 20f * 25f * 60f);

                jobEssentiality = 1f;

            }
            else if (age == HumanStatus.Worker) //30-60 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

                do
                {
                    NativeMultiHashMap<int, Vector3Int>.Enumerator e = officesMap.GetValuesForKey(hashmapkeyHome);
                    while (e.MoveNext())
                    {
                        officesList.Add(e.Current);
                    }
                    if (officesList.Count <= 0) //SE NEL QUADRANTE CORRENTE NON C'E' UN UFFICIO CERCO NEI QUADRANTI ADIACENTI
                    {
                      //  do
                       // {
                        //    if (count >= NeighbourQuadrants.Length)
                         //   {
                        Debug.LogError($"The map is incorrectly built, no offices is found in a space of {quadrantCellSize}x{quadrantCellSize} times 9. Rebuild your map, otherwise try to increase section size");
                        UnityEditor.EditorApplication.isPlaying = false;
                                
                         //   }
                         //   hashmapkeyHome = startKey;
                         //   hashmapkeyHome += NeighbourQuadrants[count++];
                       // } while (schoolMap.ContainsKey(hashmapkeyHome));
                       
                    }
                    else
                    {
                        if(officesList.Count == 1)
                            officePosition = officesList[0];
                        else
                            officePosition = officesList[UnityEngine.Random.Range(0, officesList.Count)];

                        found = true;
                        officesList.Clear();
                    }

                } while (!found);
               // Debug.Log("entity: " + entity.Index + " office pos:" + officePosition.x + " " + officePosition.y);
                //  }
                //  else
                //    officePosition = offices[UnityEngine.Random.Range(0, offices.Length)];

                symptomsProbability = GenerateNormalRandom(0.30f, 0.1f, 0.25f, 1f) * 100; //30% sintomi
                humanDeathProbability = GenerateNormalRandom(0.03f, 0.1f, 0.01f, 1f) * 100; //3% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(5f * 25f * 60f, 15f * 25f * 60f);
                if (!conf.lockdown)
                    jobEssentiality = 1f;
                else
                    jobEssentiality = GenerateNormalRandom(0.2f, 0.5f, 0f, 1f);

            }
            else if (age == HumanStatus.Retired) // 60-90 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                symptomsProbability = GenerateNormalRandom(0.50f, 0.1f, 0.5f, 1f) * 100; //50% sintomi
                humanDeathProbability = GenerateNormalRandom(0.10f, 0.1f, 0.1f, 1f) * 100; // 10% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(1f * 25f * 60f, 10f * 25f * 60f);

            }
          
            //Vector3 position = new float3((UnityEngine.Random.Range(0, gridWidth)) * 10f + UnityEngine.Random.Range(0, 10f), (UnityEngine.Random.Range(0, gridHeight)) * 10f + UnityEngine.Random.Range(0, 10f), 0);

            Vector3 position = new float3(homePosition.x * 10f + UnityEngine.Random.Range(0, 10f), homePosition.y * 10f + UnityEngine.Random.Range(0, 10f), 0);

            //Vector3 position = new float3(homePosition.x *10f , homePosition.y * 10f, 0);

            entityManager.SetComponentData(entity, new TileComponent
            {
                currentTile = TileMapEnum.TileMapSprite.Home,
                currentFloor = homePosition.z
            });


            //To add a buffer to an entity, you can use the normal methods of adding a component type onto an entity:
            entityManager.AddBuffer<PathPosition>(entity);
           // UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                hunger = UnityEngine.Random.Range(0, 10 * 60),
                sportivity = UnityEngine.Random.Range(0, 10 * 60),
                sociality = UnityEngine.Random.Range(0, 10 * 60),
                fatigue = UnityEngine.Random.Range(0, 10 * 60),
                grocery = UnityEngine.Random.Range(0, 3 * 25 * 60),
                socialResposibility = socialResponsability,
                jobEssentiality = jobEssentiality,
                homePosition = homePosition,
                officePosition = officePosition,
                age = familyInfo.age,
                familyKey = familyInfo.familyKey,
                sectionKey = familyInfo.sectionKey,
                PROvax = PROvax,
                need4vax = 0f,
                firstDoseTime = firstDoseTime,
                immunityTime = 0f
            });

            //Debug.Log(symptomsProbability + age.ToString());
            //Debug.Log(humanDeathProbability + age.ToString());

            //components depending on infection
            float uvWidth = 1f;
            float uvHeight = 1f / 5;
            float uvOffsetX = 0f;



            float infectiousThreshold, recoveredThreshold, exposedThreshold;
            //infectiousThreshold è quanto tempo le entità rimangono infette
            //exposedThreshold è quanto tempo le entità rimangono in incubazione
            //recoveredThreshold è quanto tempo le entità ci stanno per recuperare dal covid


            //float symptomsProbability = UnityEngine.Random.Range(0, 100);
            // if (symptomsProbability > 100 - conf.probabilityOfSymptomatic)
            // {
            // Debug.Log(symptomsProbability + age.ToString());
            //minDaysSymptoms 1 and maxDaySympotoms 5
            //   mean = (0.5f + 4f) * 60 * 24 / 2;
            //     sigma = (4f * 60 * 24 - mean) / 3;
            //    infectiousThreshold = GenerateNormalRandom(mean, sigma, 0.5f * 24 * 60, 4 * 24 * 60);
            // }
            //  else
            //  {


            mean = (conf.MinDaysInfectious + conf.MaxDaysInfectious) * 60 * 24 / 2;
            sigma = (conf.MaxDaysInfectious * 60 * 24 - mean) / 3;
            infectiousThreshold = GenerateNormalRandom(mean, sigma, conf.MinDaysInfectious * 24 * 60, conf.MaxDaysInfectious * 24 * 60);
            //AUMENTO IL TEMPO DI ESSERE CONTAGIOSI IN BASE ALL'ETA'
            infectiousThreshold += Percent(infectiousThreshold, (int)age);

            // }

            //Debug.Log(infectiousThreshold + age.ToString());
            //float humanDeathProbability = UnityEngine.Random.Range(0, 100);
            //  if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
            //{

            mean = (conf.MinDaysRecovered + conf.MaxDaysRecovered) * 60 * 24 / 2;
            sigma = (conf.MaxDaysRecovered * 60 * 24 - mean) / 3;
            // }
            recoveredThreshold = GenerateNormalRandom(mean, sigma, conf.MinDaysRecovered * 24 * 60, conf.MaxDaysRecovered * 24 * 60);
            //AUMENTO IL TEMPO DI RECUPERO IN BASE ALL'ETA', CALCOLANDO LA PERCENTUALE DI RECOVERED E AGGIUNGENDOLA
           // recoveredThreshold += Percent(recoveredThreshold, (int)age);
            // Debug.Log(recoveredThreshold + age.ToString());

            mean = (conf.MinDaysExposed + conf.MaxDaysExposed) * 60 * 24 / 2;
            sigma = (conf.MaxDaysExposed * 60 * 24 - mean) / 3;
            //TEMPO DI INCUBAZIONE NON INFLUENZATO DALL'ETA'
            exposedThreshold = GenerateNormalRandom(mean, sigma, conf.MinDaysExposed * 60 * 24, conf.MaxDaysExposed * 60 * 24);
            exposedThreshold += Percent(exposedThreshold, UnityEngine.Random.Range(-30,10));

            if (numberOfInfects > 0)
            {


                numberOfInfects--;

                //l'età influenza la probabilità di presentare sintomi

                entityManager.SetComponentData(entity, new InfectionComponent
                {
                    myRndValue = -1f,
                    criticalDisease = false,
                    intensiveCare = false,
                    status = Status.infectious,
                    oldstatus = Status.exposed,
                    contagionCounter = 0,
                    infectiousCounter = 0,
                    exposedCounter = 0,
                    recoveredCounter = 0,
                    symptomatic = true,
                    infected = true,
                    firstHumanSymptomsProbability = symptomsProbability,
                    firstHumanDeathProbability = humanDeathProbability,
                    currentImmunityLevel = 0.01f,
                    currentHumanSymptomsProbability = symptomsProbability,
                    currentHumanDeathProbability = humanDeathProbability,

                    infectiousThreshold = infectiousThreshold,
                    exposedThreshold = exposedThreshold,
                    recoveredThreshold = recoveredThreshold,
                    doses = 0
                });
                //graphics
                float uvOffsetY = 0.0f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.exposed });

            }
            else
            {
                entityManager.SetComponentData(entity, new InfectionComponent
                {
                    myRndValue = -1f,
                    criticalDisease = false,
                    intensiveCare = false,
                    infected = false,
                    status = Status.susceptible,
                    oldstatus = Status.susceptible,
                    contagionCounter = 0,
                    infectiousCounter = 0,
                    exposedCounter = 0,
                    recoveredCounter = 0,

                    firstHumanSymptomsProbability = symptomsProbability,
                    firstHumanDeathProbability = humanDeathProbability,
                    currentImmunityLevel = 0.01f,
                    currentHumanSymptomsProbability = symptomsProbability,
                    currentHumanDeathProbability = humanDeathProbability,

                    infectiousThreshold = infectiousThreshold,
                    exposedThreshold = exposedThreshold,
                    recoveredThreshold = recoveredThreshold,
                    doses = 0
                });
                //graphics
                float uvOffsetY = 0.2f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.susceptible });

            }


            //speed
            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeedY = UnityEngine.Random.Range(0.5f, 2f), moveSpeedX = UnityEngine.Random.Range(0.5f, 2f), });

            //initial position
            entityManager.SetComponentData(entity, new Translation
            {
                Value = position
            });


            entityManager.SetComponentData(entity, new PathFollow
            {
                pathIndex = -1
            });
        }
        // famGenerator.PrintTemplateDebug();
        schoolMap.Dispose();
        officesMap.Dispose();
        //offices.Dispose();
        //schools.Dispose();
        entityArray.Dispose();
        famGenerator.Disposing();

    }

    public static float GenerateNormalRandom(float mean, float sigma, float min, float max)
    {
        //calcolo di una variabile aleatoria e distribuita gaussianamente con media e varianza usando la trasformazione di Box-Muller

        float rand1 = UnityEngine.Random.Range(0.0f, 1.0f);
        float rand2 = UnityEngine.Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        float generatedNumber = (mean + sigma * n);

        generatedNumber = Mathf.Clamp(generatedNumber, min, max);

        return generatedNumber;
    }

    private void OnDestroy()
    {
        housesMap.Dispose();
        places.Dispose();
        //NeighbourQuadrants.Dispose();
        pathFindingMap.Dispose();
    }

    public float Percent(float total, int percent)
    {
        return (total * percent) / 100;
    }

    public float ICUproportion(float totalH)
    {
        return (totalH * 14) / 100000f;
    }

    public TemplateInfo FillTemplateData(int totalPopulation)
    {

        TemplateInfo t = new TemplateInfo();
        float d = (totalPopulation / 4f) * (template1Percent / 100f);
        t.template1Total = (int)Math.Ceiling(d);
        d = (totalPopulation / 5f) * (template2Percent / 100f);
        t.template2Total = (int)Math.Ceiling(d);
        d = (totalPopulation / 3f) * (template3Percent / 100f);
        t.template3Total = (int)Math.Ceiling(d);
        d = (totalPopulation / 2f) * (template4Percent / 100f);
        t.template4Total = (int)Math.Ceiling(d);
        d = (totalPopulation / 2f) * (template5Percent / 100f);
        t.template5Total = (int)Math.Ceiling(d);

        return t;
    }

    public static int GetPositionHashMapKey(int x, int y)
    {
        return (int)(math.floor(x / conf.sectionSize) + (quadrantYMultiplier * math.floor(y / conf.sectionSize)));
    }

    public static int CalculateIndex(int x, int y, int cellsize)
    {
        int modX;
        int modY;
        int index;
        if (x >= cellsize)
            modX = x % cellsize;
        else
            modX = x;
        if (y >= cellsize)
            modY = y % cellsize;
        else
            modY = y;

        if(modX >= modY)
        {
            index = modY * (cellsize + 1) + (modX - modY);
        }
        else
        {
            index = modY * (cellsize + 1) - (modY - modX);
        }

        return index;

    }
}
