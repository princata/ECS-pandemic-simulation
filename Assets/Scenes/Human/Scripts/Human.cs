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
    public int intensiveCarePercent;
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

    public NativeArray<Vector3Int> houses;
    public static NativeMultiHashMap<int, TileInfo> places;

    public const int quadrantYMultiplier = 1000;
    public const float quadrantCellSize = 50f;
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
        float tmp = Percent(conf.NumberOfHumans, intensiveCarePercent);
        this.totalIntensiveCare = Mathf.RoundToInt(tmp);
        vaccinationPolicy = conf.VaccinationPolicy;
        //Time Scale
        Time.timeScale = conf.TimeScale; //DA PARAMETRIZZARE

        entityArray = new NativeArray<Entity>(conf.NumberOfHumans, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        // Get grid size
        int gridWidth = Testing.Instance.grid.GetWidth();
        int gridHeight = Testing.Instance.grid.GetHeight();

        //initialize family generator
        famGenerator = new FamilyGenerator();

        templateInfo = FillTemplateData(conf.numberOfHumans);

        // Get houses and offices from grid
        List<Vector3Int> housesList = new List<Vector3Int>();
        List<Vector3Int> officesList = new List<Vector3Int>();
        List<Vector3Int> schoolsList = new List<Vector3Int>();
        List<Vector3Int> OAhomeList = new List<Vector3Int>();
        // var mapGrid = Testing.Instance.grid.GetGridByValue((GridNode gn) => { return gn.GetTileType(); });
        var mapGrid = Testing.Instance.grid;
        places = new NativeMultiHashMap<int, TileInfo>(gridWidth * gridHeight, Allocator.Persistent);
        places.Capacity = gridWidth * gridHeight;
        NativeMultiHashMap<int, TileInfo>.ParallelWriter places2 = places.AsParallelWriter();
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)//Inserire controllo piani
            {
                int f = 0;
                string tiles = mapGrid.GetGridObject(i, j).GetTiles().ToString("X"); //conversione numero in hex
                foreach (var floor in tiles) //analisi di ogni char rappresentante un piano
                {

                    if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Home)
                    {
                        housesList.Add(new Vector3Int(i, j, f++));
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Office)
                    {
                        officesList.Add(new Vector3Int(i, j, f++));
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.School)
                    {
                        schoolsList.Add(new Vector3Int(i, j, f++));
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.OAhome)
                    { 
                        OAhomeList.Add(new Vector3Int(i, j, f++)); 
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Pub)
                    {
                        int hashmapkey = GetPositionHashMapKey(i, j);
                        places2.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Pub
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Park)
                    {
                        int hashmapkey = GetPositionHashMapKey(i, j);
                        places2.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Park
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Supermarket)
                    {
                        int hashmapkey = GetPositionHashMapKey(i, j);
                        places2.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Supermarket
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Hospital)
                    {
                        int hashmapkey = GetPositionHashMapKey(i, j);
                        places2.Add(hashmapkey, new TileInfo
                        {
                            x = i,
                            y = j,
                            floor = f++,
                            type = TileMapEnum.TileMapSprite.Hospital
                        });
                    }
                    else if (int.Parse(floor.ToString(), System.Globalization.NumberStyles.HexNumber) == (int)TileMapEnum.TileMapSprite.Gym)
                    {
                        int hashmapkey = GetPositionHashMapKey(i, j);
                        places2.Add(hashmapkey, new TileInfo
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
        NativeArray<Vector3Int> offices = officesList.ToNativeArray<Vector3Int>(Allocator.Temp);
        NativeArray<Vector3Int> schools = schoolsList.ToNativeArray<Vector3Int>(Allocator.Temp);
        NativeArray<Vector3Int> OAhouses = OAhomeList.ToNativeArray<Vector3Int>(Allocator.Temp);
        famGenerator.SetHouses(housesList, OAhouses);
        famGenerator.SetTemplateInfo(templateInfo);
        houses = housesList.ToNativeArray<Vector3Int>(Allocator.Persistent);
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
          

            if (conf.Lockdown)
                socialResponsability = GenerateNormalRandom(0.75f, 0.25f, 0.50f, 0.99f);
            else
                socialResponsability = GenerateNormalRandom(0.5f, 0.3f, 0f, 0.99f);

            if (vaccinationPolicy)
            {
                if (socialResponsability > 0.35f)
                    PROvax = true;
                else
                    PROvax = false;
            }

            if (age == HumanStatus.Student) // 5-30 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                officePosition = (Vector3Int)schools[UnityEngine.Random.Range(0, schools.Length)];
                symptomsProbability = GenerateNormalRandom(0.2f, 0.1f, 0f, 1f) * 100; //20% sintomi
                humanDeathProbability = GenerateNormalRandom(0.01f, 0.1f, 0.01f, 1f) * 100; //1% IFR (INFECTION FATALITY RATE)
                if (vaccinationPolicy && PROvax)
                    firstDoseTime = UnityEngine.Random.Range(30f * 25f * 60f, 90f * 25f * 60f);
                
                jobEssentiality = 1f;
                    
            }
            else if (age == HumanStatus.Worker) //30-60 age
            {
                UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
                officePosition = offices[UnityEngine.Random.Range(0, offices.Length)];
                symptomsProbability = GenerateNormalRandom(0.30f, 0.1f, 0.25f, 1f) * 100; //30% sintomi
                humanDeathProbability = GenerateNormalRandom(0.03f, 0.1f, 0.01f, 1f) * 100; //3% IFR (INFECTION FATALITY RATE)
                if (vaccinationPolicy && PROvax)
                    firstDoseTime = UnityEngine.Random.Range(20f * 25f * 60f, 60f * 25f * 60f);
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
                if (vaccinationPolicy && PROvax)
                     firstDoseTime = UnityEngine.Random.Range(1f * 25f * 60f, 30f * 25f * 60f);
                   
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
            UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
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
            recoveredThreshold += Percent(recoveredThreshold, (int)age);
            // Debug.Log(recoveredThreshold + age.ToString());

            mean = (conf.MinDaysExposed + conf.MaxDaysExposed) * 60 * 24 / 2;
            sigma = (conf.MaxDaysExposed * 60 * 24 - mean) / 3;
            //TEMPO DI INCUBAZIONE NON INFLUENZATO DALL'ETA'
            exposedThreshold = GenerateNormalRandom(mean, sigma, conf.MinDaysExposed * 60 * 24, conf.MaxDaysExposed * 60 * 24);

            if (numberOfInfects > 0)
            {
                
                numberOfInfects--;

                //l'età influenza la probabilità di presentare sintomi

                entityManager.SetComponentData(entity, new InfectionComponent
                {
                    myRndValue = -1f,
                    criticalDisease = false,
                    intensiveCare = false,                    
                    status = Status.exposed,
                    oldstatus = Status.exposed,
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
        
        offices.Dispose();
        schools.Dispose();
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
        Instance.houses.Dispose();
        places.Dispose();
    }

    public float Percent(float total, int percent)
    {
        return (total * percent) / 100;
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
        return (int)(math.floor(x / quadrantCellSize) + (quadrantYMultiplier * math.floor(y / quadrantCellSize)));
    }
}
