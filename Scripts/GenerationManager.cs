using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class GenerationManager : MonoBehaviour
{
    public GameObject creaturePrefab;
    public GameObject carnivoroPrefab;
    public GameObject herbivoroPrefab;
    public GameObject plantaPrefab;

    public GameObject golpeEffect;
    public GameObject muerteEffect;
    public GameObject reproduccionEffect;

    public bool GuardarStats = true;
    public bool SimEmpezada = false;
    public bool pausado = false;
    public int PoblacionInicialHerbivoros;
    public int PoblacionInicialCarnivoros;
    public int PlantasACadaMomento;

    public float mutacion_prob = 0.3f;
    public float max_stat = 100f;

    public float tiempo_actualizacion = 1f;

    public float counter = 0;

    private GameObject[] puntosSpawnCreature;

    public float MinDist = 0.5f;

    //Guarda por cada segundo cuantas criaturas hay de cada tipo
    [System.Serializable]
    public class Log_simulacion
    {
        public List<int> herbivorosVivos;
        public List<int> CarnivorosVivos;
        public List<int> PlantasVivas;

        public List<float> herb_vel_stats;
        public List<float> herb_cam_stats;
        public List<float> herb_gan_stats;
        
        public List<float> carn_vel_stats;
        public List<float> carn_cam_stats;
        public List<float> carn_gan_stats;

        public Log_simulacion()
        {
            herbivorosVivos = new List<int>();
            CarnivorosVivos = new List<int>();
            PlantasVivas = new List<int>();

            herb_vel_stats = new List<float>();
            herb_cam_stats = new List<float>();
            herb_gan_stats = new List<float>();

            carn_vel_stats = new List<float>();
            carn_cam_stats = new List<float>();
            carn_gan_stats = new List<float>();
        }
    }

    public Log_simulacion log_sim;

    public Scrollbar plantasCount;
    public Scrollbar herbCount;
    public Scrollbar carnCount;

    public Scrollbar mutProb;

    public TextMeshProUGUI plantasCountText;
    public TextMeshProUGUI herbCountText;
    public TextMeshProUGUI carnCountText;

    public TextMeshProUGUI mutProbText;

    public Sprite play;
    public Sprite pausa;
    public Sprite parar;

    public Image playBtn;
    public Image pararBtn;

    public Button pararSimBtn;

    // Start is called before the first frame update
    void Awake()
    {
        puntosSpawnCreature = GameObject.FindGameObjectsWithTag("PuntoSpawnCreature");

        log_sim = new Log_simulacion();

        Directory.CreateDirectory(Application.streamingAssetsPath + "/Logs_simulacion/");
    }

    // Update is called once per frame
    void Update()
    {

        if (counter >= 1f)
        {
            counter = 0;
            //SpawnPlantas(PlantasSpawnPorSegundo);
            if (SimEmpezada)
            {
                censar();
            }
        }

        counter += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.C))
        {
            SpawnCarnivoros(1);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SpawnHerbivoros(1);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnPlantas(1);
        }
    }

    private void SpawnPlantas(int qty)
    {
        for(int i = 0; i < qty; i++)
        {
            PlantaController n_planta = Instantiate(plantaPrefab, getRandomSpawnPoint(puntosSpawnCreature), Quaternion.identity).GetComponent<PlantaController>();
            n_planta.setProperties(5f, 3f, Color.green);
        }
    }

    private void SpawnHerbivoros(int qty)
    {
        for (int i = 0; i < qty; i++)
        {
            HerbivoroController n_hervivoro = Instantiate(herbivoroPrefab, getRandomSpawnPoint(puntosSpawnCreature), Quaternion.identity).GetComponent<HerbivoroController>();
            n_hervivoro.setProperties(3f, 6f, 5f, Color.white);
        }
    }

    private void SpawnCarnivoros(int qty)
    {
        for (int i = 0; i < qty; i++)
        {
            CarnivoroController n_hervivoro = Instantiate(carnivoroPrefab, getRandomSpawnPoint(puntosSpawnCreature), Quaternion.identity).GetComponent<CarnivoroController>();
            n_hervivoro.setProperties(3f, 6f, 5f, Color.red);
        }
    }

    private Vector3 getRandomSpawnPoint(GameObject[] spawnPoints)
    {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].transform.position;
    }

    public void mostrarEfecto(GameObject effectPrefab, Transform creature)
    {
        GameObject effect = Instantiate(effectPrefab, creature.position, creature.rotation);
        effect.transform.position += Vector3.up * 1.2f;
        effect.SetActive(true);
    }

    private void censar()
    {
        HerbivoroController[] herbivoros = GameObject.FindObjectsOfType<HerbivoroController>();
        CarnivoroController[] carnivoros = GameObject.FindObjectsOfType<CarnivoroController>();
        PlantaController[] plantas = GameObject.FindObjectsOfType<PlantaController>();

        log_sim.herbivorosVivos.Add(herbivoros.Length);
        log_sim.CarnivorosVivos.Add(carnivoros.Length);
        log_sim.PlantasVivas.Add(plantas.Length);

        if(plantas.Length < PlantasACadaMomento)
        {
            SpawnPlantas(PlantasACadaMomento - plantas.Length);
        }

        float vel_herb_promedio = 0f;
        float campo_herb_promedio = 0f;
        float ganas_rep_herb_promedio = 0f;

        //lleva un record del promedio de los 3 genes (vel, camp y ganas_rep) en la poblacion de herb y la de carn
        foreach (HerbivoroController herbivoro in herbivoros)
        {
            vel_herb_promedio += herbivoro.velocidad;
            campo_herb_promedio += herbivoro.campo_vision;
            ganas_rep_herb_promedio += herbivoro.ganar_reproduccion_critica;
        }
        
        if (herbivoros.Length > 0) {
            vel_herb_promedio /= herbivoros.Length;
            campo_herb_promedio /= herbivoros.Length;
            ganas_rep_herb_promedio /= herbivoros.Length;
        }

        log_sim.herb_vel_stats.Add(vel_herb_promedio);
        log_sim.herb_cam_stats.Add(campo_herb_promedio);
        log_sim.herb_gan_stats.Add(ganas_rep_herb_promedio);

        float vel_carn_promedio = 0f;
        float campo_carn_promedio = 0f;
        float ganas_rep_carn_promedio = 0f;

        foreach (CarnivoroController carnivoro in carnivoros)
        {
            vel_carn_promedio += carnivoro.velocidad;
            campo_carn_promedio += carnivoro.campo_vision;
            ganas_rep_carn_promedio += carnivoro.ganar_reproduccion_critica;
        }

        if(carnivoros.Length > 0)
        {
            vel_carn_promedio /= carnivoros.Length;
            campo_carn_promedio /= carnivoros.Length;
            ganas_rep_carn_promedio /= carnivoros.Length;
        }
        
        log_sim.carn_vel_stats.Add(vel_carn_promedio);
        log_sim.carn_cam_stats.Add(campo_carn_promedio);
        log_sim.carn_gan_stats.Add(ganas_rep_carn_promedio);
    }

    private void guardar_stats()
    {
        string jsonOutput = JsonUtility.ToJson(log_sim);

        string txtNombreDocumento = Application.streamingAssetsPath + "/Logs_simulacion/" + "Log_" + (int) Random.Range(0,10000) + ".json";

        if (!File.Exists(txtNombreDocumento)){
            File.WriteAllText(txtNombreDocumento, jsonOutput);
        }
    }

    public void GestionarSim()
    {
        if (!SimEmpezada)
        {
            playBtn.sprite = pausa;
            ResumeGame();
            EmpezarSim();
        }
        else if(!pausado)
        {
            playBtn.sprite = play;
            PauseGame();
        }
        else
        {
            playBtn.sprite = pausa;
            ResumeGame();
        }
    }
    void PauseGame()
    {
        pausado = true;
        Time.timeScale = 0;
    }
    void ResumeGame()
    {
        pausado = false;
        Time.timeScale = 1;
    }

    public void EmpezarSim()
    {
        PlantasACadaMomento = (int)(plantasCount.value * 350);
        PoblacionInicialHerbivoros = (int)(herbCount.value * 350);
        PoblacionInicialCarnivoros = (int)(carnCount.value * 350);
        mutacion_prob = mutProb.value;

        SimEmpezada = true;
        pararSimBtn.interactable = true;
        SpawnPlantas(PlantasACadaMomento);
        SpawnHerbivoros(PoblacionInicialHerbivoros);
        SpawnCarnivoros(PoblacionInicialCarnivoros);
    }

    public void ReiniciarSimulacion()
    {


        pararSimBtn.interactable = false;
        SimEmpezada = false;
        pausado = false;
        playBtn.sprite = play;

        PlantasACadaMomento = 0;
        PoblacionInicialHerbivoros = 0;
        PoblacionInicialCarnivoros = 0;
        mutacion_prob = 0;


        HerbivoroController[] herbivoros = GameObject.FindObjectsOfType<HerbivoroController>();
        CarnivoroController[] carnivoros = GameObject.FindObjectsOfType<CarnivoroController>();
        PlantaController[] plantas = GameObject.FindObjectsOfType<PlantaController>();

        foreach(HerbivoroController herbivoro in herbivoros)
        {
            Destroy(herbivoro.gameObject);
        }

        foreach (CarnivoroController carnivoro in carnivoros)
        {
            Destroy(carnivoro.gameObject);
        }

        foreach (PlantaController planta in plantas)
        {
            Destroy(planta.gameObject);
        }
    }

    public void OnCambioPlantas()
    {
        PlantasACadaMomento = (int) (plantasCount.value * 350);
        plantasCountText.SetText(PlantasACadaMomento.ToString());
    }

    public void OnCambioHerb()
    {
        PoblacionInicialHerbivoros = (int)(herbCount.value * 350);
        herbCountText.SetText(PoblacionInicialHerbivoros.ToString());
    }

    public void OnCambioCarn()
    {
        PoblacionInicialCarnivoros = (int)(carnCount.value * 350);
        carnCountText.SetText(PoblacionInicialCarnivoros.ToString());

    }

    public void OnCambioMutProb()
    {
        mutacion_prob = mutProb.value;
        mutProbText.SetText((mutacion_prob*100).ToString("0.00") + "%");
    }

    public void OnApplicationQuit()
    {
        if (GuardarStats)
        {
            guardar_stats();
        }
    }
}
