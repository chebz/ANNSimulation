using ANNCSharp;
using CPAI.GeneticAlgorithm;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public int epochNum = 0;

    public int collisions = 0;

    private float timeLeft;

    public static LevelManager instance = null;

    private GeneticAlgorithm mGa;

    [System.NonSerialized]
    public List<Mine> mines;

    [System.NonSerialized]
    public List<AIActor> actors;

    public List<AIActor> actorPrefabs;

    public Mine minePrefab;

    public float epochTime = 10.0f;

    public int population = 100;

    public int mineCount = 20;

    public double boundsPenalty = 10.0f;

    public double collisionPenalty = 10.0f;

    public double crowdRewardFriend = 1;
    public double crowdRewardEnemy = 5;

    public Text epochText;

    public Text bestFitnessText;

    public Text worstFitnessText;

    public Text avFitnessText;

    public Text remainingText;

    public Text collisionsText;

    void Awake()
    {
        instance = this;

        NeuronSettings nSettings = new NeuronSettings();
        nSettings.mBias = AISettings.BIAS;

        NeuronLayerSettings layerSettings = new NeuronLayerSettings(nSettings);
        layerSettings.mNumNeuronsPerLayerMin = AISettings.NUMNEURONS;
        layerSettings.mNumNeuronsPerLayerMax = AISettings.NUMNEURONS;

        NeuronNetSettings nnSettings = new NeuronNetSettings(layerSettings);

        nnSettings.mNumInputs = AISettings.NUMINPUTS;
        nnSettings.mNumOutputs = AISettings.NUMOUTPUTS;
        nnSettings.mNumLayersMin = AISettings.NUMLAYERS;
        nnSettings.mNumLayersMax = AISettings.NUMLAYERS;

        GeneticAlgorithmSettings gaSettings = new GeneticAlgorithmSettings(nnSettings.mFactory);
        gaSettings.mMaxPopulation = population;

        mGa = new GeneticAlgorithm(gaSettings);

        mines = new List<Mine>();
        actors = new List<AIActor>();
    }

    void Start()
    {
        foreach (var actorPrefab in actorPrefabs)
        {
            for (int iNN = 0; iNN < mGa.Population.Count / actorPrefabs.Count; iNN++)
            {
                SpawnActor(actorPrefab.gameObject);
            }
        }

        Refresh();
    }

    private void Refresh()
    {
        for (int iNN = 0; iNN < mGa.Population.Count; iNN++)
        {
            actors[iNN].Refresh((NeuronNet)mGa.Population[iNN]);
        }

        PoolManager.Pools["Mines"].DespawnAll();
        for (int iMine = 0; iMine < mineCount; iMine++)
        {
            SpawnMine();
        }

        timeLeft = epochTime;
        collisions = 0;
    }

    private void SpawnActor(GameObject prefab)
    {
        AIActor actor = PoolManager.Pools["Actors"].Spawn(prefab).GetComponent<AIActor>();
        float posX = Random.Range(-50.0f, 50.0f);
        float posZ = Random.Range(-50.0f, 50.0f);
        float posY = 0;
        actor.transform.position = new Vector3(posX, posY, posZ);
        float rotY = Random.Range(0, 360.0f);
        actor.transform.rotation = Quaternion.Euler(0, rotY, 0);
    }

    private void SpawnMine()
    {
        Mine mine = PoolManager.Pools["Mines"].Spawn("Mine").GetComponent<Mine>();
        float posX = Random.Range(-50.0f, 50.0f);
        float posZ = Random.Range(-50.0f, 50.0f);
        float posY = 0;
        mine.transform.position = new Vector3(posX, posY, posZ);
    }

    void Update()
    {
        if (timeLeft < 0) //|| mines.Count == 0
        {            
            epochNum++;
            epochText.text = string.Format("Epoch = {0}", epochNum);
            bestFitnessText.text = string.Format("BestFitness = {0}", mGa.BestFitness);
            worstFitnessText.text = string.Format("WorstFitness = {0}", mGa.WorstFitness);
            avFitnessText.text = string.Format("AvFitness = {0}", mGa.AverageFitness);
            remainingText.text = string.Format("Remaining = {0}", mines.Count);
            collisionsText.text = string.Format("Collisions = {0}", collisions);
            mGa.epoch();
            Refresh();
        }
        else
        {
            timeLeft -= Time.deltaTime;
        }
    }
}
