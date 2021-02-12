using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sceneloader : MonoBehaviour
{

    
    public void LoadAwal()
    {
        SceneManager.LoadScene("PortfolioScene");
    }
    public void LoadPulkit()
    {
        SceneManager.LoadScene("PulkitPortfolio");
    }
    public void LoadMPulkit()
    {
        SceneManager.LoadScene("MPulkitPortfolio");
    }
}