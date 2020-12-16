using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuilder : MonoBehaviour
{

    public NavMeshSurface[] surface;

    private void Start()
    {
        surface = FindObjectsOfType<NavMeshSurface>();

        for (int i = 0; i < surface.Length; i++)
        {
            surface[i].BuildNavMesh();
        }
    }



    //GameObject[] getObjects()
    //{
    //    NavMeshSurface[] scripts = FindObjectsOfType<NavMeshSurface>();
    //    GameObject[] objects = new GameObject[scripts.Length];
    //    for (int i = 0; i < objects.Length; i++)
    //        objects[i] = scripts[i].gameobject;
    //    return objects;
    //}
}