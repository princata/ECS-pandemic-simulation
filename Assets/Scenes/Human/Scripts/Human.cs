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
    public int[] templateTotal;
    public int[] templates;
    public int[] nComponents;
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
    public bool heatmap;
    public bool randomSocResp;
    public float socialRespons;
    public float noVaxPercentage;
    public float remoteWorkerPecent;
    public int[] inputAge;
 
    public float symptomsStudent;
    public float ifrStudent;
    public float symptomsWorker;
    public float ifrWorker;
    public float symptomsRetired;
    public float ifrRetired;

    public int minDaysFDTstudent;
    public int maxDaysFDTstudent;
    public int minDaysFDTworker;
    public int maxDaysFDTworker;
    public int minDaysFDTretired;
    public int maxDaysFDTretired;

    public int[] templates;
    public float[] templateDistrib;

    public long totalIntensiveCare;
    private static FamilyGenerator famGenerator;
    private static TemplateInfo templateInfo;

    [SerializeField] public Mesh mesh;
    [SerializeField] public Material healthyMaterial;
    [SerializeField] public Material sickMaterial;
    [SerializeField] public Material heatmapMaterial;
    [SerializeField] public Material humanSpriteMaterial;

    //public NativeArray<Vector3Int> houses;
    public NativeArray<int> NeighbourQuadrants;
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
        int numberOfInfects = conf.numberOfInfects;
        quadrantCellSize = conf.sectionSize;
        inputAge = conf.inputAge;
        heatmap = conf.heatmap;
        vaccinationPolicy = conf.vaccinationPolicy;
        randomSocResp = conf.randomSocResp;
        if (!randomSocResp)
            socialRespons = conf.socialResponsibility/100f;
        noVaxPercentage = conf.noVaxPercentage/100f;
        symptomsStudent = conf.symptomsStudent/100f;
        ifrStudent = conf.ifrStudent/100f;
        symptomsWorker = conf.symptomsWorker/100f;
        ifrWorker = conf.ifrWorker/100f;
        symptomsRetired = conf.symptomsRetired/100f;
        ifrRetired = conf.ifrRetired/100f;
        minDaysFDTstudent = conf.minDaysFDTstudent;
        maxDaysFDTstudent = conf.maxDaysFDTstudent;
        minDaysFDTworker = conf.minDaysFDTworker;
        maxDaysFDTworker = conf.maxDaysFDTworker;
        minDaysFDTretired = conf.minDaysFDTretired;
        maxDaysFDTretired = conf.maxDaysFDTretired;
        remoteWorkerPecent = conf.remoteWorkerPercent / 100f;
        templates = conf.familyTemplate;
        templateDistrib = conf.familyDistrib;
        //Time Scale
        Time.timeScale = conf.timeScale; 

        int gridWidth = Testing.Instance.grid.GetWidth();
        int gridHeight = Testing.Instance.grid.GetHeight();

        float tmp = ICUproportion(conf.numberOfHumans, conf.icu4100k); //ICU proportion based on number of ICU per 100k inhabitants
        this.totalIntensiveCare = Mathf.RoundToInt(tmp);

        entityArray = new NativeArray<Entity>(conf.numberOfHumans, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        NeighbourQuadrants = new NativeArray<int>(8, Allocator.Persistent);
        NeighbourQuadrants[0] = 1; // right
        NeighbourQuadrants[1] = 1 + quadrantYMultiplier; //right-up
        NeighbourQuadrants[2] = quadrantYMultiplier; //up
        NeighbourQuadrants[3] = quadrantYMultiplier - 1; //left up
        NeighbourQuadrants[4] = -1; //left
        NeighbourQuadrants[5] = 0 - (1 + quadrantYMultiplier); //left down
        NeighbourQuadrants[6] = -quadrantYMultiplier; //down
        NeighbourQuadrants[7] = 0 - (quadrantYMultiplier - 1); //right down
        // Get grid size
       
        //initialize family generator
        famGenerator = new FamilyGenerator();

        templateInfo = FillTemplateData(conf.numberOfHumans, templates, templateDistrib);

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
            for (int j = 0; j < gridHeight; j++)
            {

                int hashmapkey = GetPositionHashMapKey(i, j);
               
                pathFindingMap.Add(hashmapkey, new Pathfinding.PathNode  //adding cells to each quadrant in map
                {
                    x = i,
                    y = j,
                    //index = CalculateIndex(i, j, (int)quadrantCellSize),

                    gCost = int.MaxValue,

                    isWalkable = mapGrid.GetGridObject(i, j).IsWalkable(),
                    cameFromNodeIndex = -1,

                    //pathNodeArray[index] = pathNode
                }); 

                int f = 0;
                string tiles = mapGrid.GetGridObject(i, j).GetTiles().ToString("X"); 
                
                foreach (var floor in tiles) //analysing each floor in every cell and add it to the right hashmap
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
        famGenerator.SetHouses(housesMap, OAhouses, numberOfInfects);
        famGenerator.SetTemplateInfo(templateInfo);
        //houses = housesList.ToNativeArray<Vector3Int>(Allocator.Persistent);
        //famGenerator.PrintTemplateDebug();
        float symptomsProbability = 0f;
        float humanDeathProbability = 0f;
        
        float jobEssentiality = 0f;
        float firstDoseTime = 0f;
        bool PROvax = false;
        int oldfamily = -1;
        //TODO model social responsibility
        for (int i = 0; i < entityArray.Length; i++)
        {
            Entity entity = entityArray[i];
            FamilyInfo familyInfo = famGenerator.GetFamilyAndAgeDetail();
            HumanStatus age = familyInfo.age;
            
            float speed = AgentFeatures.GetSpeedForAgeComfortable(AgentFeatures.GetAgentAge(inputAge), AgentFeatures.GetAgentGender());

            var homePosition = familyInfo.homePosition;
            var officePosition = Vector3Int.zero;
            var hashmapkeyHome = GetPositionHashMapKey(homePosition.x, homePosition.y);
            var familykey = familyInfo.familyKey;
         //   Debug.Log($"entity {entity.Index} hmkHome {hashmapkeyHome} sectionkey{familyInfo.sectionKey}");
            var startKey = hashmapkeyHome;
            //var found = false;
            //var count = 0;
            
            if(randomSocResp)
                socialRespons = GenerateNormalRandom(0.5f, 0.45f, 0f, 0.99f);

            
            if (socialRespons > noVaxPercentage) //percentage of NOVAX in the simulation (considered only if vaccinationPolicy is set true)
                PROvax = true;
            else
                PROvax = false;
            

            if (age == HumanStatus.Student) // 5-30 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                
                for (int l = 0; l < NeighbourQuadrants.Length; l++)
                {
                    if (schoolMap.ContainsKey(hashmapkeyHome))//this check allows to pick only the valid quadrants in the map
                    {
                        NativeMultiHashMap<int, Vector3Int>.Enumerator e = schoolMap.GetValuesForKey(hashmapkeyHome);
                        while (e.MoveNext())
                        {
                            schoolsList.Add(e.Current);
                        }
                        e.Dispose();
                        hashmapkeyHome = startKey;
                        hashmapkeyHome += NeighbourQuadrants[l];
                    }
                    else
                    {
                        hashmapkeyHome = startKey;
                        hashmapkeyHome += NeighbourQuadrants[l];
                    }
                }
                

                if (schoolsList.Count <= 0)
                {
                    Debug.LogError($"The map is incorrectly built, no office is found in a space of {quadrantCellSize}x{quadrantCellSize} times 9. Rebuild your map, otherwise try to increase section size");
                    UnityEditor.EditorApplication.isPlaying = false;

                }
                else
                {
                    if (schoolsList.Count == 1)
                        officePosition = schoolsList[0];
                    else
                        officePosition = schoolsList[UnityEngine.Random.Range(0, schoolsList.Count)];
                        
                   schoolsList.Clear();
                }
                                  
                symptomsProbability = GenerateNormalRandom(symptomsStudent, 0.1f, 0f, 1f) * 100; //20% symptoms for young people
                humanDeathProbability = GenerateNormalRandom(ifrStudent, 0.1f, 0.01f, 1f) * 100; //1% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(minDaysFDTstudent * 25f * 60f, maxDaysFDTstudent * 25f * 60f);

                jobEssentiality = 1f;

            }

            else if (age == HumanStatus.Worker) //30-60 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

                for (int l = 0; l < NeighbourQuadrants.Length; l++)
                {
                    if (officesMap.ContainsKey(hashmapkeyHome))//this check allows to pick only the valid quadrants in the map
                    {
                        NativeMultiHashMap<int, Vector3Int>.Enumerator e = officesMap.GetValuesForKey(hashmapkeyHome);
                        while (e.MoveNext())
                        {
                            officesList.Add(e.Current);
                        }
                        e.Dispose();
                        hashmapkeyHome = startKey;
                        hashmapkeyHome += NeighbourQuadrants[l];
                    }
                    else
                    {
                        hashmapkeyHome = startKey;
                        hashmapkeyHome += NeighbourQuadrants[l];
                    }
                }
                if (officesList.Count <= 0)//NON HO TROVATO NULLA 
                {
                    Debug.LogError($"The map is incorrectly built, no school is found in a space of {quadrantCellSize}x{quadrantCellSize} times 9. Rebuild your map, otherwise try to increase section size");
                    UnityEditor.EditorApplication.isPlaying = false;

                }
                else
                {
                    if (officesList.Count == 1)
                        officePosition = officesList[0];
                    else
                        officePosition = officesList[UnityEngine.Random.Range(0, officesList.Count)];
                       
                    officesList.Clear();
                }
           
               // Debug.Log("entity: " + entity.Index + " office pos:" + officePosition.x + " " + officePosition.y);
                //  }
                //  else
                //    officePosition = offices[UnityEngine.Random.Range(0, offices.Length)];

                symptomsProbability = GenerateNormalRandom(symptomsWorker, 0.1f, 0.25f, 1f) * 100; //30% sintomi
                humanDeathProbability = GenerateNormalRandom(ifrWorker, 0.1f, 0.01f, 1f) * 100; //3% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(minDaysFDTworker * 25f * 60f, maxDaysFDTworker * 25f * 60f);
               
                jobEssentiality = GenerateNormalRandom(remoteWorkerPecent, 0.1f, 0f, 1f); //percentuale lavoratori da remoto 30%

            }
            else if (age == HumanStatus.Retired) // 60-90 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                symptomsProbability = GenerateNormalRandom(symptomsRetired, 0.1f, 0.5f, 1f) * 100; //50% sintomi
                humanDeathProbability = GenerateNormalRandom(ifrRetired, 0.1f, 0.1f, 1f) * 100; // 10% IFR (INFECTION FATALITY RATE)
                if (PROvax)
                    firstDoseTime = UnityEngine.Random.Range(minDaysFDTretired * 25f * 60f, maxDaysFDTretired * 25f * 60f);

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
                socialResposibility = socialRespons,
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


            mean = (conf.minDaysInfectious + conf.maxDaysInfectious) * 60 * 24 / 2;
            sigma = (conf.maxDaysInfectious * 60 * 24 - mean) / 3;
            infectiousThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysInfectious * 24 * 60, conf.maxDaysInfectious * 24 * 60);
            //AUMENTO IL TEMPO DI ESSERE CONTAGIOSI IN BASE ALL'ETA'
            infectiousThreshold += Percent(infectiousThreshold, (int)age);

            // }

            //Debug.Log(infectiousThreshold + age.ToString());
            //float humanDeathProbability = UnityEngine.Random.Range(0, 100);
            //  if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
            //{

            mean = (conf.minDaysRecovered + conf.maxDaysRecovered) * 60 * 24 / 2;
            sigma = (conf.maxDaysRecovered * 60 * 24 - mean) / 3;
            // }
            recoveredThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysRecovered * 24 * 60, conf.maxDaysRecovered * 24 * 60);
            //AUMENTO IL TEMPO DI RECUPERO IN BASE ALL'ETA', CALCOLANDO LA PERCENTUALE DI RECOVERED E AGGIUNGENDOLA
           // recoveredThreshold += Percent(recoveredThreshold, (int)age);
            // Debug.Log(recoveredThreshold + age.ToString());

            mean = (conf.minDaysExposed + conf.maxDaysExposed) * 60 * 24 / 2;
            sigma = (conf.maxDaysExposed * 60 * 24 - mean) / 3;
            //TEMPO DI INCUBAZIONE NON INFLUENZATO DALL'ETA'
            exposedThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysExposed * 60 * 24, conf.maxDaysExposed * 60 * 24);
            exposedThreshold += Percent(exposedThreshold, UnityEngine.Random.Range(-30,10));

            if (numberOfInfects > 0 && oldfamily != familykey)
            {

                oldfamily = familykey;
                numberOfInfects--;
                Debug.Log($"symptomatic in section: {startKey}");
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
                if (!heatmap)
                {
                    spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                    spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                }
                else
                {
                    spriteSheetAnimationData.uv = new Vector4(1f, 1f, 1f, uvOffsetY);
                    spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity,new Vector3(20f,20f));
                }

                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.infectious });

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
            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeedY = speed, moveSpeedX = speed, });

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
        NeighbourQuadrants.Dispose();
        pathFindingMap.Dispose();
    }

    public float Percent(float total, int percent)
    {
        return (total * percent) / 100;
    }

    public float ICUproportion(float totalH, int icu4100k)
    {
        return (totalH * icu4100k) / 100000f;
    }

    public TemplateInfo FillTemplateData(int totalPopulation, int[] templates, float[] percentages)//computing how many families for each template must be present
    {

        TemplateInfo t = new TemplateInfo();
        t.templateTotal = new int[templates.Length];
        for(int i = 0; i < templates.Length; i++)
        {
            int n = templates[i];
            int j = 0;
            do
            {
                n = n / 10;
                j++;
            }
            while (Math.Abs(n) >= 1);
            float d = (totalPopulation / j) * (percentages[i] / 100f);
            t.templateTotal[i] = (int)Math.Ceiling(d);
            t.nComponents[i] = j;
        }


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
