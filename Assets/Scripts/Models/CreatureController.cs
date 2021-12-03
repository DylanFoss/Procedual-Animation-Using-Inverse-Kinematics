using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    [SerializeField] LegStepper one;
    [SerializeField] LegStepper two;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        one.TryMove();
        two.TryMove();
    }
}
