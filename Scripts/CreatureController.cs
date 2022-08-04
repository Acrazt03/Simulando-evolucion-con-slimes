using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
/*
    public GenerationManager generationManager;

    public float energia_actual;

    public float ganas_reproducir = 0;

    public float velocidad = 10f;

    public float perdida_energia = 1f;

    public Generadores[] fuentes_energia; //PP
    public HashSet<Dietas> dietas;
    //public HashSet<> modelos;

    public float campo_vision = 10f;

    public CreatureState estado_actual;
    public CreatureActions accion_actual;

    public Renderer slime_renderer;
    public GameObject p_gen_1;
    public GameObject p_gen_2;

    [SerializeField]
    private bool accionTerminada = true;
    private bool isActivated = false; //Should be false

    [SerializeField]
    private GameObject criatura_objetivo;

    [SerializeField]
    public HashSet<GameObject> parejas_en_vista;

    //Hay que hacer que sea proporcional al tamaño sino habra momentos que no lo alcance
    [SerializeField]
    private float MinDist = 0.5f;

    public LayerMask sensableObjects;

    //Los que entre en esta lista son gameobjects que pueden ser comidos por esta criatura
    //(no son de la misma especie y son fuente de comida)
    [SerializeField]
    public HashSet<GameObject> comida_en_vista; 

    // Start is called before the first frame update
    void Awake()
    {
        generationManager = FindObjectOfType<GenerationManager>();

        energia_actual = generationManager.energia_inicial;

        fuentes_energia = new Generadores[2];

        comida_en_vista = new HashSet<GameObject>();
        parejas_en_vista = new HashSet<GameObject>();
        dietas = new HashSet<Dietas>();

    }

// Update is called once per frame
void Update()
    {
        if (isActivated && (estado_actual != CreatureState.Muerto))
        {

            if (alcanzar_objetivo())
            {

                if(accion_actual == CreatureActions.Buscar_comida)
                {
                    comer();
                }
                else
                {
                    reproducir();
                }

                accionTerminada = true;
            }

            if (accionTerminada)
            {
                Pensar();
            }

            ganas_reproducir += Time.deltaTime;
            perder_energia();

        }
    }

    private void perder_energia()
    {
        if (energia_actual > 0)
        {
            if (dietas.Contains(Dietas.Sol) && dietas.Count == 1)
            {
                energia_actual = generationManager.energia_inicial;

            }
            else
            {
                energia_actual -= perdida_energia * Time.deltaTime; //Agregar modificadores eficiencia o whatever
            }
        }
        else
        {
            estado_actual = CreatureState.Muerto;
            energia_actual = 0;
            Destroy(gameObject);
        }
    }

    public void setProperties(
                float n_velocidad,
                float n_campo_vision,
                Color n_color,
                Generadores n_fuente_a,
                Generadores n_fuente_b)
    {

        velocidad = n_velocidad;
        campo_vision = n_campo_vision;

        fuentes_energia[0] = n_fuente_a;
        fuentes_energia[1] = n_fuente_b;

        slime_renderer.material.color = n_color;

        isActivated = true;


        if (Random.Range(0f, 1f) <= generationManager.mutacion_prob)
        {
            mutate();
        }

        configurar_modelo();

        // Agrega la modificacion por perdida
        perdida_energia = (velocidad + campo_vision)/2;

        configurar_dieta();

    }

    private void configurar_modelo()
    {

        MeshFilter gen_1 = p_gen_1.GetComponent<MeshFilter>();
        MeshFilter gen_2 = p_gen_2.GetComponent<MeshFilter>();

        HashSet<Generadores> temp = new HashSet<Generadores>();

        foreach (Generadores fuente_energia in fuentes_energia)
        {
            temp.Add(fuente_energia);
        }

        if (temp.Contains(Generadores.Carnivoro))
        {
            p_gen_1.transform.position += new Vector3(0f, -0.05f, 0f);
            gen_1.mesh = generationManager.carnivoros_viking;
            if (temp.Contains(Generadores.Planta))
            {
                gen_2.mesh = generationManager.plant_flower;
            }
            else if(temp.Contains(Generadores.Hervivoro))
            {
                p_gen_2.transform.position += Vector3.up*0.09f;

                gen_2.mesh = generationManager.herb_leaf;
            }
        }
        else if(temp.Contains(Generadores.Hervivoro))
        {
            p_gen_1.transform.position += new Vector3(0f, 0.20f,0f);
            gen_1.mesh = generationManager.herb_leaf;

            if (temp.Contains(Generadores.Planta))
            {
                gen_2.mesh = generationManager.plant_flower;
            }
            
        }
        else if(temp.Contains(Generadores.Planta))
        {
            p_gen_1.transform.position += Vector3.right * 0.08f;
            gen_1.mesh = generationManager.plant_flower;

        }
    }

    private void mutate()
    {

        velocidad *= Random.Range(0.4f, 2f);
        campo_vision *= Random.Range(0.4f, 2f);

        slime_renderer.material.color = Color.Lerp(slime_renderer.material.color, new Color(Random.value, Random.value, Random.value, 1.0f), Random.Range(0f, 1f));

        fuentes_energia[0] = (Generadores) Random.Range(0, 4);
    }

    private void configurar_dieta()
    {
        foreach (Generadores fuente_energia in fuentes_energia){
            switch (fuente_energia)
            {
                case Generadores.Carnivoro:
                    dietas.Add(Dietas.Herbivoros);
                    //dietas.Add(Dietas.Carnivoros); //No se si agregarlo XD
                    break;

                case Generadores.Planta:
                    dietas.Add(Dietas.Sol);

                    break;
                case Generadores.Hervivoro:
                    dietas.Add(Dietas.Plantas);
                    
                    break;
            }
        }

    }

    private bool esComida(Generadores[] fuentes_energia)
    {
        foreach (Generadores fuente_energia in fuentes_energia)
        {
            switch (fuente_energia)
            {
                case Generadores.Carnivoro:

                    if (dietas.Contains(Dietas.Carnivoros))
                    {
                        return true;
                    }
                    break;

                case Generadores.Planta:
                    if (dietas.Contains(Dietas.Plantas))
                    {
                        return true;
                    }

                    break;
                case Generadores.Hervivoro:
                    if (dietas.Contains(Dietas.Herbivoros))
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    //Ver si existe algun objeto de interes en el campo de vision (comida, criaturas)
    private void Ver()
    {

        //I should Add a flush to the lists so creatures that are gone don't get referenced DONE
        if(comida_en_vista.Count > 0)
        {
            comida_en_vista.Clear();
        }

        if (comida_en_vista.Count > 0)
        {
            parejas_en_vista.Clear();
        }

        Collider[] objetos_de_interes = Physics.OverlapSphere(transform.position, campo_vision, sensableObjects);
        foreach (var objeto_de_interes in objetos_de_interes)
        {

            if (objeto_de_interes.gameObject.Equals(gameObject))
            {
                continue;
            }

            if (objeto_de_interes.TryGetComponent(out CreatureController criatura))
            {

                if(criatura.estado_actual != CreatureState.Muerto)
                {
                    //Saber si la criatura es comida, pareja o no interesa
                    if (dietas.SetEquals(criatura.dietas)) //Si tienen la misma dieta son de la misma especie
                    {
                        parejas_en_vista.Add(criatura.gameObject);
                    }
                    else if (esComida(criatura.fuentes_energia)) //La criatura no esta muerta, pues chequear si es comida
                    {
                        comida_en_vista.Add(criatura.gameObject);
                    }
                }   
            }
        }
    }

    private bool CheckViability(float value, float expected_value)
    {
        return value < expected_value * generationManager.porcentaje_crtitico_energia;
    }

    private void Pensar()
    {

        CreatureActions accion;

        //Caso especial para plantas (no comen)
        if (dietas.Contains(Dietas.Sol) && dietas.Count == 1) 
        {
            estado_actual = CreatureState.Buscando_pareja;
            accion = CreatureActions.Buscar_pareja;
        }
        else if (CheckViability(energia_actual, generationManager.energia_inicial))
        {
            // Fix energia
            accion = CreatureActions.Buscar_comida;
            estado_actual = CreatureState.Buscando_comida;
        }
        else //Si no tiene demasiada hambre, pues haz otra cosa si ya terminaste de hacer lo que estabas haciendo
        {
            accion = Choose_random_Action();
        }

        accionTerminada = false;

        Actuar(accion);
    }

    private CreatureActions Choose_random_Action()
    {
        //Choose between buscar_comida o buscar pareja
        if(Random.Range(0f, 1f) >= 0.5)
        {
            estado_actual = CreatureState.Buscando_comida;
            return CreatureActions.Buscar_comida;
        }
        else if(puedeReproducir())
        {
            estado_actual = CreatureState.Buscando_pareja;
            return CreatureActions.Buscar_pareja;
        }
        else
        {
            estado_actual = CreatureState.Buscando_comida;
            return CreatureActions.Buscar_comida;
        }
    }

    private void Actuar(CreatureActions accion)
    {
        switch (accion)
        {
            case CreatureActions.Buscar_comida:
                buscar_comida();
                break;
            case CreatureActions.Buscar_pareja:
                buscar_pareja();
                break;
        }
    }

    private void buscar_comida()
    {

        if (buscar(comida_en_vista)) //Objetivo encontrado y fijado
        {
            accionTerminada = false;
        }
        else
        {
            accionTerminada = true;
        }
    }

    private void buscar_pareja()
    {

        if (buscar(parejas_en_vista)) //Objetivo encontrado y fijado
        {
            accionTerminada = false;
        }
        else
        {
            accionTerminada = true;
        }

    }

    private bool buscar(HashSet<GameObject> objetivos)
    {
        
        Ver();

        if(objetivos.Count > 0)
        {
            return fijar_objetivo(objetivos);
        }
        else
        {
            return false;
        }

    }

    private bool fijar_objetivo(HashSet<GameObject> objetivos)
    {
        float dist = float.MaxValue;

        foreach (GameObject objetivo in objetivos)
        {

            if (objetivo)
            {
                //Consigue el objetivo mas cercano (Quizas deba basarlo en el objetivo con mas energia o deseabilidad)
                float dist_obj = Vector3.Distance(objetivo.transform.position, transform.position);
                if (dist_obj < dist)
                {
                    dist = dist_obj;

                    //Fija el objetivo
                    criatura_objetivo = objetivo;
                    return true;
                }
            }
        }

        return false;
    }

    //Te acerca un frame al objetivo y te dice si lo alcanzo
    private bool alcanzar_objetivo()
    {

        if (criatura_objetivo)
        {
            if (Vector3.Distance(transform.position, criatura_objetivo.transform.position) > MinDist)
            {
                transform.position += transform.forward * velocidad * Time.deltaTime;
                perder_energia();

                transform.LookAt(criatura_objetivo.transform);
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    private void comer()
    {
        if (criatura_objetivo) //Si la criatura huyo, persigela
        {
            getEnergy(generationManager.energia_inicial * 0.50f); //Debe ser proporcional al tamaño de la criatura
            criatura_objetivo.GetComponent<CreatureController>().getDamage();
        }
    }

    private bool puedeReproducir()
    {
        return ganas_reproducir >= generationManager.reproducion_tiempo && energia_actual > generationManager.energia_inicial *0.5f;
    }

    private void reproducir()
    {
        if (criatura_objetivo) //Si la criatura huyo, persigela
        {
            crear_nuevo_organismo();
        }
    }

    private float escoger(float p1, float p2)
    {
        if(Random.Range(0f, 1f) >= 0.5)
        {
            return p1;
        }
        else
        {
            return p2;
        }
    }

    private Generadores escoger_fuente(Generadores fuente_a, Generadores fuente_b)
    {
        if (Random.Range(0f, 1f) >= 0.5)
        {
            return fuente_a;
        }
        else
        {
            return fuente_b;
        }
    }

    private void crear_nuevo_organismo()
    {
        if (puedeReproducir())
        {
            energia_actual -= generationManager.energia_inicial * 0.5f;
            ganas_reproducir = 0;

            CreatureController p2 = criatura_objetivo.GetComponent<CreatureController>();

            p2.ganas_reproducir = 0;
            //p2.nada();

            float n_velocidad = escoger(velocidad, p2.velocidad);
            float n_campo_vision = escoger(campo_vision, p2.campo_vision);

            Generadores n_fuente_a = escoger_fuente(fuentes_energia[0], fuentes_energia[1]);
            Generadores n_fuente_b = escoger_fuente(p2.fuentes_energia[0], p2.fuentes_energia[1]);

            Color n_color = Color.Lerp(slime_renderer.material.color, p2.slime_renderer.material.color, Random.Range(0f, 1f));

            CreatureController n_criatura = Instantiate(generationManager.creaturePrefab, transform.position, transform.rotation).GetComponent<CreatureController>(); //Revisa lo del parent

            n_criatura.setProperties(
                n_velocidad,
                n_campo_vision,
                n_color,
                n_fuente_a,
                n_fuente_b
                );

        }
    }

    private void getEnergy(float energy)
    {
        if(energia_actual < generationManager.energia_inicial)
        {
            energia_actual += energy;
        }
    }

    public void getDamage()
    {
        estado_actual = CreatureState.Muerto;
        energia_actual = 0;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, campo_vision);

            if (criatura_objetivo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, criatura_objetivo.transform.position);
            }

            Ver();

            Gizmos.color = Color.blue;

            foreach (GameObject pareja in parejas_en_vista)
            {
                Gizmos.DrawLine(transform.position, pareja.transform.position);

            }

            Gizmos.color = Color.green;

            foreach (GameObject comida in comida_en_vista)
            {
                Gizmos.DrawLine(transform.position, comida.transform.position);

            }
        }
    }*/
}
