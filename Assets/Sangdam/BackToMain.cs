using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{

    // Start is called before the first frame update
    public void Main()
    {   
        SceneManager.LoadScene("main menu");
    }

    // Update is called once per frame
    
}
