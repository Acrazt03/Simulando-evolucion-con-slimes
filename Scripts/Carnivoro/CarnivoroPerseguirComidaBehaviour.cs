using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarnivoroPerseguirComidaBehaviour : StateMachineBehaviour
{
    GenerationManager generationManager;
    CarnivoroController controller;

    HashSet<GameObject> comidas_en_vista;
    GameObject comidaObjetivo;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.transform.GetComponent<CarnivoroController>();
        generationManager = FindObjectOfType<GenerationManager>();

        comidas_en_vista = new HashSet<GameObject>();

        VerComidas();

        if (comidas_en_vista.Count == 0)
        {
            animator.SetBool("PerseguirComida", false);
        }
        else
        {
            escogerComida();
        }
    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!comidaObjetivo || Vector2.Distance(controller.transform.position, comidaObjetivo.transform.position) > controller.campo_vision)
        {
            //Fuera de rango
            animator.SetBool("PerseguirComida", false);
        }
        else
        {
            Mover(animator);
        }
    }


    //Ver si existe algun objeto de interes en el campo de vision (comida, criaturas)
    private void VerComidas()
    {

        if (comidas_en_vista.Count > 0)
        {
            comidas_en_vista.Clear();
        }

        Collider[] objetos_de_interes = Physics.OverlapSphere(controller.transform.position, controller.campo_vision, controller.sensableObjects);
        foreach (var objeto_de_interes in objetos_de_interes)
        {

            if (objeto_de_interes.gameObject.Equals(controller.gameObject))
            {
                continue;
            }

            if (objeto_de_interes.TryGetComponent(out HerbivoroController comida))
            {
                if (comida)
                {
                    comidas_en_vista.Add(comida.gameObject);
                }
            }
        }
    }

    private void escogerComida()
    {
        float dist = float.MaxValue;

        foreach (GameObject comida in comidas_en_vista)
        {

            if (comida)
            {
                //Consigue el objetivo mas cercano (Quizas deba basarlo en el objetivo con mas energia o deseabilidad)
                float dist_obj = Vector3.Distance(comida.transform.position, controller.transform.position);
                if (dist_obj < dist)
                {
                    dist = dist_obj;

                    //Fija el objetivo
                    comidaObjetivo = comida;
                }
            }
        }
    }

    private void Mover(Animator animator)
    {

        if (Vector2.Distance(controller.transform.position, comidaObjetivo.transform.position) > generationManager.MinDist)
        {
            //Perseguir
            controller.transform.position = Vector3.MoveTowards(controller.transform.position, comidaObjetivo.transform.position, controller.velocidad * Time.deltaTime);
            controller.transform.LookAt(comidaObjetivo.transform);
        }
        else
        {
            //Alcanzado
            animator.SetTrigger("Comer");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.setComidaObjetivo(comidaObjetivo);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
