using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    //TODO: get legs into a data structure
    //TODO: force leg side priotrities to alternate
    //TODO: implement support for multiple gaits
    //TODO: implement debug flags to turn certain debug visuals on and off


    [SerializeField] LegStepper FRL;
    [SerializeField] LegStepper FLL;

    [SerializeField] LegStepper MRL;
    [SerializeField] LegStepper MLL;

    [SerializeField] LegStepper RRL;
    [SerializeField] LegStepper RLL;


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    IEnumerator LegUpdateCoroutine()
    {
        while (true)
        {
            do
            {
                FRL.TryMove();
                MLL.TryMove();
                RRL.TryMove();
                yield return null;
            }
            while (FRL.moving || MLL.moving || RRL.moving);

            do
            {
                FLL.TryMove();
                MRL.TryMove();
                RLL.TryMove();
                yield return null;
            }
            while (FLL.moving || MRL.moving || RLL.moving);
        }
    }

        // Update is called once per frame
        void Update()
        {

        }
}
