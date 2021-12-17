using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationTexts : MonoBehaviour
{
    public static Configuration conf;

    // Start is called before the first frame update
    void Start()
    {
        conf = Configuration.CreateFromJSON();
        if (!conf.vaccinationPolicy)
        {
           
            gameObject.SetActive(false);
        }
        
           
    }
    
   

    
}
