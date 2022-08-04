using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnivoroController : MonoBehaviour
{
    public GenerationManager generationManager;
    public float hambre_actual = 0;
    public float ganas_reproducir = 0;

    public float velocidad;
    public float campo_vision;
    public float hambre_critica;
    public float ganar_reproduccion_critica;

    public float perdida_energetica;

    private GameObject parejaObjetivo;
    private GameObject comidaObjetivo;

    public Renderer slime_renderer;

    public bool isActivated = false; //must be false

    public LayerMask sensableObjects;

    private Animator animator;

    public float counter = 0f;
    void Awake()
    {
        generationManager = FindObjectOfType<GenerationManager>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivated)
        {
            actalizar_stats();
        }
    }

    private void actalizar_stats()
    {

        if(counter >= generationManager.tiempo_actualizacion)
        {
            counter = 0;
        
            if(hambre_actual >= generationManager.max_stat)
            {
                generationManager.mostrarEfecto(generationManager.muerteEffect, transform);
                animator.SetTrigger("Morir");
            }
            else if (ganas_reproducir >= generationManager.max_stat)
            {
                ganas_reproducir = generationManager.max_stat;
            }

            hambre_actual += 1 + perdida_energetica; //Aqui pon que tan rapido te da hambre
            ganas_reproducir += 1; //Aqui pon que tan rapido te dan ganas de reprocuir
        }
        else
        {
            counter += Time.deltaTime;
        }
    }

    public void setProperties(
                float n_velocidad,
                float n_campo_vision,
                float n_ganas_reproduccion_critica,
                Color n_color)
    {

        velocidad = n_velocidad;
        campo_vision = n_campo_vision;
        ganar_reproduccion_critica = n_ganas_reproduccion_critica;

        slime_renderer.material.color = n_color;

        isActivated = true;

        if (Random.Range(0f, 1f) <= generationManager.mutacion_prob)
        {
            mutate();
        }

        // Agrega la modificacion por perdida
        perdida_energetica = (velocidad * campo_vision)/3;

    }

    private void mutate()
    {
        velocidad *= Random.Range(0.4f, 2f);
        campo_vision *= Random.Range(0.4f, 2f);

        ganar_reproduccion_critica = Random.Range(0.05f, 0.95f);

        slime_renderer.material.color = Color.Lerp(slime_renderer.material.color, new Color(Random.value, Random.value, Random.value, 1.0f), Random.Range(0f, 1f));
    }


    /*public bool TieneHambre()
    {
        return hambre_actual >= generationManager.max_stat * hambre_critica;
    }*/

    public bool DeberiaReproducir()
    {
        if (ganas_reproducir > hambre_actual)
        {
            return ganas_reproducir - hambre_actual >= ganar_reproduccion_critica;
        }
        else
        {
            return false;
        }
    }

    /*public bool PuedeReproducir()
    {
        return ganas_reproducir >= generationManager.max_stat * ganar_reproduccion_critica;
    }*/

    public void setParejaObjetivo(GameObject pareja)
    {
        parejaObjetivo = pareja;
    }

    public bool existePareja()
    {
        return parejaObjetivo;
    }

    public void reproducir()
    {
        crear_nuevo_organismo();
    }

    public void setComidaObjetivo(GameObject comida)
    {
        comidaObjetivo = comida;
    }

    public bool existeComida()
    {
        return comidaObjetivo;
    }

    public void comer()
    {
        generationManager.mostrarEfecto(generationManager.golpeEffect, transform);
        comidaObjetivo.GetComponent<HerbivoroController>().getDamage();
        hambre_actual = 0;
    }

    private float escoger(float p1, float p2)
    {
        if (Random.Range(0f, 1f) >= 0.5)
        {
            return p1;
        }
        else
        {
            return p2;
        }
    }

    private void crear_nuevo_organismo()
    {

        //RECUEDRDA HACER QUE AMBOS ORGANISMOS ESTEN DE ACUERDO TBD

        //hambre_actual += ganas_reproducir*0.8f;
        ganas_reproducir = 0;
        generationManager.mostrarEfecto(generationManager.reproduccionEffect, transform);

        CarnivoroController p2 = parejaObjetivo.GetComponent<CarnivoroController>();

        p2.ganas_reproducir = 0;

        float n_velocidad = escoger(velocidad, p2.velocidad);
        float n_campo_vision = escoger(campo_vision, p2.campo_vision);
        float n_hambre_critica = escoger(campo_vision, p2.campo_vision);
        float n_ganas_reproduccion_critica = escoger(campo_vision, p2.campo_vision);

        Color n_color = Color.Lerp(slime_renderer.material.color, p2.slime_renderer.material.color, Random.Range(0f, 1f));

        CarnivoroController n_criatura = Instantiate(generationManager.carnivoroPrefab, transform.position, transform.rotation).GetComponent<CarnivoroController>(); 

        n_criatura.setProperties(
            n_velocidad,
            n_campo_vision,
            n_ganas_reproduccion_critica,
            n_color
            );
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, campo_vision);

            /*if (objetivoFijado && objeto_objetivo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, objeto_objetivo.transform.position);
            }

            Gizmos.color = Color.blue;

            foreach (GameObject pareja in parejas_en_vista)
            {
                Gizmos.DrawLine(transform.position, pareja.transform.position);

            }

            Gizmos.color = Color.green;

            foreach (GameObject comida in comida_en_vista)
            {
                Gizmos.DrawLine(transform.position, comida.transform.position);

            }*/
        }
    }
}

