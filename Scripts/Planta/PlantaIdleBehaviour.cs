using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantaIdleBehaviour : StateMachineBehaviour
{
    GenerationManager generationManager;
    PlantaController controller;

    GameObject[] puntosExploracion;
    int puntoExploracionActual;

    HashSet<GameObject> enemigos_en_vista;
    float run_for_sec = 3f;
    float counter = 0;
    bool isRunning = false;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.transform.GetComponent<PlantaController>();
        generationManager = FindObjectOfType<GenerationManager>();

        puntosExploracion = GameObject.FindGameObjectsWithTag("PuntoExploracion");
        puntoExploracionActual = Random.Range(0, puntosExploracion.Length);

        enemigos_en_vista = new HashSet<GameObject>();
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (controller.isActivated)
        {
            Ver();
            if (enemigos_en_vista.Count > 0)
            {
                isRunning = true;
                //Hay objetivos de interes
                //Huir del enemigo
                //Moverte al punto de exploracion mas lejos
                //puntoExploracionActual = Random.Range(0, puntosExploracion.Length);
                puntoExploracionActual = CalcularPuntoMasCercano();
            }

            if (isRunning)
            {
                if(counter >= run_for_sec)
                {
                    counter = 0;
                    isRunning = false;
                }
                else
                {
                    Mover();
                    counter += Time.deltaTime;
                }
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
    }

    //Ver si existe algun objeto de interes en el campo de vision (comida, criaturas)
    private void Ver()
    {

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
            if (objeto_de_interes.TryGetComponent(out HerbivoroController enemigo))
            {
                if (enemigo)
                {
                    enemigos_en_vista.Add(enemigo.gameObject);
                }
            }
        }
    }

    private int CalcularPuntoMasCercano()
    {
        int resultado = 0;

        float dist = float.MaxValue;

        for (int i = 0; i < puntosExploracion.Length; i++)
        {
            float dist_obj = Vector3.Distance(puntosExploracion[i].transform.position, controller.transform.position);
            if (dist_obj < dist)
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
