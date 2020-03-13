using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Nav2 : MonoBehaviour
{
    public GameObject cube2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 获取按键空格
        {
            cube2.SetActive(true);
//            cube2.GetComponent<NavMeshSurface>().BuildNavMesh();
            cube2.GetComponent<CharacterController>();
//            cube2.AddComponent<NavMeshObstacle>().carving = true;
        }
    }
}
