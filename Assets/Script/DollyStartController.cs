using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cinemachine
{

    public class DollyStartController : MonoBehaviour
    {
        public bool hide = false;
        public GameObject appearance;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!hide)
            {
                if (Input.GetKeyDown("n"))
                {
                    //hideCar();
                    GetComponent<DollyCartSpeedController>().start();
                    GetComponent<DollyCartSpeedController>().driving = true;
                }
            }
            if (hide)
            {
                if (Input.GetKeyDown(KeyCode.V))
                {
                    appearance.gameObject.SetActive(true);
                    hide = false;
                }
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                appearance.gameObject.SetActive(!appearance.gameObject.activeInHierarchy);
            }
        }

        public void hideCar()
        {
            hide = true;
            appearance.SetActive(false);
        }
    }
}
