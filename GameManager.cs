using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager theOneGameManager;

    //Variables that contain needed filepaths for accessing game data
    public static string mechDesignRootFilepath;
    public static string mechProtocolRootFilepath;
    public static string mechTrainingProtocolFilepath;
    public static string mechTrainingDummyProtocolFilepath;
    public static string mechPlayerSaveDataFilepath;
    public static string enemyProtocolRootFilepath;

    //Variables for global quantitative parameters like scaling factors
    public static float mechDesignsToBattleScaleFactor = 0.33f;

    private void Awake()
    {
        if (theOneGameManager == null)
        {
            theOneGameManager = this;
        }
        if (theOneGameManager != this)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeMechDataFilepath();
        InitializeMechProtocolFilepath();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public static void InitializeEnemyProtocolFilepath()
    {

    }
    public static void InitializeMechDataFilepath()
    {
        mechDesignRootFilepath = Path.Combine(Application.persistentDataPath, "Designs");
        if (!Directory.Exists(mechDesignRootFilepath))
        {
            Directory.CreateDirectory(mechDesignRootFilepath);
        }
    }

    public static void InitializeMechProtocolFilepath()
    {
        mechProtocolRootFilepath = Path.Combine(Application.persistentDataPath, "Protocols");
        if (!Directory.Exists(mechProtocolRootFilepath))
        {
            //Debug.Log("Creating directory...");
            Directory.CreateDirectory(mechProtocolRootFilepath);
        }
        mechTrainingProtocolFilepath = Path.Combine(GameManager.mechProtocolRootFilepath, "TrainingProtocol.dat");
        mechTrainingDummyProtocolFilepath = Path.Combine(GameManager.mechProtocolRootFilepath, "TrainingDummyProtocol.dat");
    }

    public static void InitializeMechPlayerDataFilepath()
    {

    }

    public static void SceneLoadMechLab()
    {
        SceneManager.LoadScene("Mech Lab");
    }

    public static void SceneLoadProtocols()
    {
        SceneManager.LoadScene("Protocols Screen");
    }

    public static void SceneLoadDesigns()
    {
        SceneManager.LoadScene("Designs Screen");
    }

    public static void SceneLoadParts()
    {
        SceneManager.LoadScene("Parts Screen");
    }

    public static void SceneLoadResearch()
    {
        SceneManager.LoadScene("Research Screen");
    }

    public static void SceneLoadBase()
    {
        SceneManager.LoadScene("Robot Base");
    }

    public static void SceneLoadTraining()
    {
        SceneManager.LoadScene("Training Room");
    }
}
