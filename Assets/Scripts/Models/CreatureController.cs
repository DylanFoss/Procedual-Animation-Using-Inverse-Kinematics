using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
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
