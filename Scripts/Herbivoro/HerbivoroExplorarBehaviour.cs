using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerbivoroExplorarBehaviour : StateMachineBehaviour
{
    GenerationManager generationManager;
    HerbivoroController controller;

    GameObject[] puntosExploracion;
    int puntoExploracionActual;

    HashSet<GameObject> parejas_en_vista;
    HashSet<GameObject> comida_en_vista;
    HashSet<GameObject> enemigos_en_vista;

    //bool encontroEnemigo = false;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.transform.GetComponent<HerbivoroController>();
        generationManager = FindObjectOfType<GenerationManager>();

        puntosExploracion = GameObject.FindGameObjectsWithTag("PuntoExploracion");
        puntoExploracionActual = Random.Range(0, puntosExploracion.Length);

        parejas_en_vista = new HashSet<GameObject>();
        comida_en_vista = new HashSet<GameObject>();
        enemigos_en_vista = new HashSet<GameObject>();

        animator.SetBool("PerseguirComida", false);
        animator.SetBool("PerseguirPareja", false);
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller.isActivated)
        {
            Ver();
            if (comida_en_vista.Count > 0 || parejas_en_vista.Count > 0 || enemigos_en_vista.Count > 0)
            {
                //Hay objetivos de interes

                /*if (enemigos_en_vista.Count > 0 && !encontroEnemigo)
                {
                    //Huir del enemigo
                    //Moverte al punto de exploracion mas lejos
                    encontroEnemigo = true;
                    puntoExploracionActual = CalcularPuntoMasLejos();
                    Mover();
                }else */if (parejas_en_vista.Count > 0 && controller.DeberiaReproducir())
                {
                    //Reproducete
                    animator.SetBool("PerseguirPareja", true);

                }
                else if(comida_en_vista.Count > 0 && controller.hambre_actual > 0)
                {
                    //Come
                    animator.SetBool("PerseguirComida", true);

                }
                /*else if(enemigos_en_vista.Count == 0)
                {
                    encontroEnemigo = false;
                    Mover();
                }*/
                else
                {
                    Mover();
                }

                /*if (comida_en_vista.Count > 0 && controller.TieneHambre())
                {
                    //Come
                    animator.SetBool("PerseguirComida", true);
                }
                else if (parejas_en_vista.Count > 0 && controller.PuedeReproducir())
                {
                    //Reproducete
                    animator.SetBool("PerseguirPareja", true);
                }
                else if (comida_en_vista.Count > 0)
                {
                    //Come
                    animator.SetBool("PerseguirComida", true);
                }else if (enemigos_en_vista.Count > 0)
                {
                    //Huir del enemigo
                    //Moverte al punto de exploracion mas lejos
                    puntoExploracionActual = CalcularPuntoMasLejos();
                    Mover();
                }
                else
                {
                    Mover();
                }*/

            }
            else
            {
                //sigue explorando
                Mover();
            }
        }
    }

    private void Mover()
    {
        if (Vector2.Distance(controller.transform.position, puntosExploracion[puntoExploracionActual].transform.position) > generationManager.MinDist)
        {
            controller.transform.position = Vector3.MoveTowards(controller.transform.position, puntosExploracion[puntoExploracionActual].transform.position, controller.velocidad * Time.deltaTime);
            controller.transform.LookAt(puntosExploracion[puntoExploracionActual].transform);
        }
        else
        {
            puntoExploracionActual = Random.Range(0, puntosExploracion.Length);
        }
    }

    //Ver si existe algun objeto de interes en el campo de vision (comida, criaturas)
    private void Ver()
    {

        //I should Add a flush to the lists so creatures that are gone don't get referenced DONE
        if (comida_en_vista.Count > 0)
        {
            comida_en_vista.Clear();
        }

        if (parejas_en_vista.Count > 0)
        {
            parejas_en_vista.Clear();
        }

        if (enemigos_en_vista.Count > 0)
        {
            enemigos_en_vista.Clear();
        }

        Collider[] objetos_de_interes = Physics.OverlapSphere(controller.transform.position, controller.campo_vision, controller.sensableObjects);
        foreach (var objeto_de_interes in objetos_de_interes)
        {

            if (objeto_de_interes.gameObject.Equals(controller.gameObject))
            {
                continue;
            }

            if (objeto_de_interes.TryGetComponent(out HerbivoroController pareja))
            {
                if (pareja)
                {
                    parejas_en_vista.Add(pareja.gameObject);
                }
            }
            else if (objeto_de_interes.TryGetComponent(out PlantaController comida))
            {
                if (comida)
                {
                    comida_en_vista.Add(comida.gameObject);
                }
            }else if (objeto_de_interes.TryGetComponent(out CarnivoroController enemigo))
            {
                if(enemigo)
                {
                    enemigos_en_vista.Add(enemigo.gameObject);
                }
            }
        }
    }

    private int CalcularPuntoMasLejos()
    {
        int resultado = 0;

        float dist = 0;

        for(int i = 0; i < puntosExploracion.Length; i++)
        {
            float dist_obj = Vector3.Distance(puntosExploracion[i].transform.position, controller.transform.position);
            if(dist_obj > dist)
            {
                dist = dist_obj;
                resultado = i;
            }
        }

        return resultado;
    }

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    //override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    //override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    //{
    //    
    //}
}
