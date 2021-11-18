using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicChainManager : MonoBehaviour
{

    KinematicChain kc;
    GameObject chain;

    //[SerializeField]
    //public GameObject endEffector;

    GameObject endEffector;

    // Start is called before the first frame update
    void Start()
    {
        createChain();

        // could be in a function
        endEffector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endEffector.name = "End Effector";
        endEffector.transform.position = kc.end;

    }

    // Update is called once per frame
    void Update()
    {

        Vector3[] points;
        points = kc.Solve(endEffector.transform.position);

        for (int i = 0; i < kc.segments.Count; i++)
        {
            chain.transform.GetChild(i).transform.position = points[i];
        }
        //TODO: this is hardcoded, don't forget to change this.
        chain.transform.GetChild(5).transform.position = points[5];
    }

    public void createChain()
    {
        kc = new KinematicChain(5);
        onChainCreated();
    }

    public void onChainCreated()
    {

        GameObject go = new GameObject();
        go.transform.SetParent(transform, true);

        go.name = "KinematicChain";

        // iterator in a foreach, god...
        int it = 0;

        GameObject end = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        foreach (Segment limb in kc.segments)
        {
            GameObject j = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            j.name = it == 0 ? "Joint" + (it) + "_origin" : "Joint" + (it);
            j.transform.position = limb.joint;
            j.transform.SetParent(go.transform, true);

            var sr = j.GetComponent<Renderer>();
            sr.material.SetColor("_Color", it == 0 ? Color.green : Color.red);

            end.name = "Joint" + (it + 1);
            end.transform.position = kc.end;

            sr = end.GetComponent<Renderer>();
            sr.material.SetColor("_Color", Color.blue);


            //GameObject l = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //l.name = "Limb";
            //l.transform.position = new Vector3(0, ( (kc.cumLength(it)) + limb.length * 0.5f), 0);
            //l.transform.localScale = new Vector3(0.5f, limb.length, 0.5f);
            //l.transform.SetParent(j.transform, true);

            it++;
        }

        end.transform.SetParent(go.transform, true);

        chain = go;
    }
}
