using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnivoroExplorarBehaviour : StateMachineBehaviour
{
    GenerationManager generationManager;
    CarnivoroController controller;

    GameObject[] puntosExploracion;
    int puntoExploracionActual;

    HashSet<GameObject> parejas_en_vista;
    HashSet<GameObject> comida_en_vista;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.transform.GetComponent<CarnivoroController>();
        generationManager = FindObjectOfType<GenerationManager>();

        puntosExploracion = GameObject.FindGameObjectsWithTag("PuntoExploracion");
        puntoExploracionActual = Random.Range(0, puntosExploracion.Length);

        parejas_en_vista = new HashSet<GameObject>();
        comida_en_vista = new HashSet<GameObject>();

        animator.SetBool("PerseguirComida", false);
        animator.SetBool("PerseguirPareja", false);

    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller.isActivated)
        {
            Ver();
            if (comida_en_vista.Count > 0 || parejas_en_vista.Count > 0)
            {
                //Hay comida

                if (parejas_en_vista.Count > 0 && controller.DeberiaReproducir())
                {
                    //Reproducete
                    animator.SetBool("PerseguirPareja", true);

                }
                else if (comida_en_vista.Count > 0 && controller.hambre_actual > 0)
                {
                    //Come
                    animator.SetBool("PerseguirComida", true);

                }
                else
                {
                    Mover();
                }
                /*
                if (comida_en_vista.Count > 0 && controller.TieneHambre())
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

        Collider[] objetos_de_interes = Physics.OverlapSphere(controller.transform.position, controller.campo_vision, controller.sensableObjects);
        foreach (var objeto_de_interes in objetos_de_interes)
        {

            if (objeto_de_interes.gameObject.Equals(controller.gameObject))
            {
                continue;
            }

            if (objeto_de_interes.TryGetComponent(out CarnivoroController pareja))
            {
                if (pareja)
                {
                    parejas_en_vista.Add(pareja.gameObject);
                }
            }
            else if (objeto_de_interes.TryGetComponent(out HerbivoroController comida))
            {
                if (comida)
                {
                    comida_en_vista.Add(comida.gameObject);
                }
            }
        }
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
