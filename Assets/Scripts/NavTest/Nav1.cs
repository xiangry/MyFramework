using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Nav1 : MonoBehaviour
{
    // Start is called before the first frame update
    private NavMeshAgent navAgent;
    private Animator animator;
    public Vector3 endPos;
    public int endRadius;
    void Start()
    {
        navAgent = gameObject.AddComponent<NavMeshAgent>();
        navAgent.speed = 2;
        navAgent.stoppingDistance = 0.2f;
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    
    void Update()
    {
        if ((gameObject.transform.position - endPos).magnitude < endRadius)
        {
            ResetDestinationState(true);
            Logger.LogColor(Color.red,"达到目的地区域====================");
        }
    }
    public void SetEndPositionAndArea(Vector3 pos, int radius)
    {
        endPos = pos;
        endRadius = radius;
    }
    
    public void GotoOnePoint(Vector3 point )
    {
        animator.CrossFade("worldrun",0);
        animator.SetFloat("speed", 0.5f);
        gameObject.GetComponent<NavMeshAgent>().SetDestination(point);
    }
    
    public void ResetDestinationState(bool flag)
    {
        gameObject.GetComponent<NavMeshAgent>().isStopped = flag;
        if (flag)
        {
            animator.SetFloat("speed", 0);
        }
    }

}
