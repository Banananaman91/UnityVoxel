using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuilder : MonoBehaviour
{
    public NavMeshSurface surface;

    //private void Start()
    //{
    //    InvokeRepeating("getmesh", 1,1);
    //}

    //public void Getmesh()
    //{
    //    surface = FindObjectOfType<NavMeshSurface>();

    //    surface.BuildNavMesh();
        
    //}

    

    //public NavMeshSurface[] surface;

    //private void Start()
    //{
    //    Invoke("getmesh", 1);
    //}

    //private void getmesh()
    //{
    //    surface = FindObjectsOfType<NavMeshSurface>();

    //    for (int i = 0; i < 1; i++)
    //    {
    //        surface[i].BuildNavMesh();
    //    }
    //}



    //GameObject[] getObjects()
    //{
    //    NavMeshSurface[] scripts = FindObjectsOfType<NavMeshSurface>();
    //    GameObject[] objects = new GameObject[scripts.Length];
    //    for (int i = 0; i < objects.Length; i++)
    //        objects[i] = scripts[i].gameobject;
    //    return objects;
    //}
}