using System.Collections.Generic;
using UnityEngine;

public class PlantaController : MonoBehaviour
{
    public GenerationManager generationManager;

    public float velocidad;
    public float campo_vision;

    public Renderer slime_renderer;

    public bool isActivated = false; //must be false

    public LayerMask sensableObjects;

    private Animator animator;

    void Awake()
    {
        generationManager = FindObjectOfType<GenerationManager>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void setProperties(
                float n_velocidad,
                float n_campo_vision,
                Color n_color)
    {

        velocidad = n_velocidad;
        campo_vision = n_campo_vision;
        
        slime_renderer.material.color = n_color;

        isActivated = true;

        if (Random.Range(0f, 1f) <= generationManager.mutacion_prob)
        {
            mutate();
        }
    }

    private void mutate()
    {
        velocidad *= Random.Range(0.4f, 2f);
        campo_vision *= Random.Range(0.4f, 2f);

        slime_renderer.material.color = Color.Lerp(slime_renderer.material.color, new Color(Random.value, Random.value, Random.value, 1.0f), Random.Range(0f, 1f));
    }

    public void getDamage()
    {
        generationManager.mostrarEfecto(generationManager.muerteEffect, transform);
        animator.SetTrigger("Morir");
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
