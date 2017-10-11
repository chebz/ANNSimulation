using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ANNCSharp;

public class AIActor : MonoBehaviour
{
    struct ActorKvp
    {
        public AIActor actor;
        public float dist;
    }


    public List<AIActor> actorsEnemy = new List<AIActor>();

    public List<AIActor> actorsFriend = new List<AIActor>();

    private List<ActorKvp> nearbyActors = new List<ActorKvp>();

    private float mDist = 0;

    private float timeFromSpawn;

    private List<double> inputs;

    private NeuronNet mNN;

    public float speed = 0.1f;

    public float rotSpeed = 10.0f;

    public float sizeGrow = 1.0f;

    public float scaleSpeed = 1.0f;

    public float visibilityRadius = 10.0f;

    public float startingFitness = 1000.0f;

    public Color color;

    public Renderer colorRenderer;

    public Vector2 direction;

    public Transform mesh;

   
    void Awake()
    {
        inputs = new List<double>();
        for (int iInput = 0; iInput < AISettings.NUMINPUTS; iInput++)
        {
            inputs.Add(0);
        }
        colorRenderer.material.color = color;
    }

    void OnSpawned()
    {        
        LevelManager.instance.actors.Add(this);
    }

    void OnDespawned()
    {
        LevelManager.instance.actors.Remove(this);
    }

    public void Refresh(NeuronNet nn)
    {
        mNN = nn;
        timeFromSpawn = 0;
        mesh.localScale = Vector3.one;
        mNN.Fitness = startingFitness;
    }

    Mine GetClosestMine()
    {
        Mine closest = null;
        float minDist = float.MaxValue;

        foreach (var mine in LevelManager.instance.mines)
        {  
            float dist = Vector3.Distance(mine.transform.position, transform.position);

            if (dist > visibilityRadius)
                continue;

            if (dist < minDist)
            {
                closest = mine;
                minDist = dist;
            }
        }

        return closest;
    }

    void GetClosestActors()
    {
        actorsFriend.Clear();
        actorsEnemy.Clear();

        foreach (var actor in LevelManager.instance.actors)
        {
            
            if (actor == this)
                continue;            
            
            float dist = Vector3.Distance(actor.transform.position, transform.position);

            if (dist > visibilityRadius)
                continue;

            actor.mDist = dist;

            if (actor.color == color && actorsFriend.Count < 10)
                actorsFriend.Add(actor);
            else if (actorsEnemy.Count < 10)
                actorsEnemy.Add(actor);
            else break;           
        }

        actorsFriend.Sort((x, y) => x.mDist.CompareTo(y.mDist));
        actorsEnemy.Sort((x, y) => x.mDist.CompareTo(y.mDist));
    }

    void Update()
    {
        int iInput = 0;

        inputs[iInput++] = transform.forward.x;
        inputs[iInput++] = transform.forward.z;

        var mine = GetClosestMine();
        if (mine != null)
        {
            Vector3 mineDir = (mine.transform.position - transform.position).normalized;
            inputs[iInput++] = mineDir.x;
            inputs[iInput++] = mineDir.z;
        }
        else
        {
            inputs[iInput++] = 0;
            inputs[iInput++] = 0;
        }

        // GetClosestActors();

        //for (int iActor = 0; iActor < 10; iActor++)
        //{
        //    var actor = iActor < actorsFriend.Count ? actorsFriend[iActor] : null;

        //    if (actor != null)
        //    {
        //        Vector3 actorDir = (actor.transform.position - transform.position).normalized;
        //        inputs[iInput++] = (double)actorDir.x;
        //        inputs[iInput++] = (double)actorDir.z;
        //    }
        //    else
        //    {
        //        inputs[iInput++] = 0;
        //        inputs[iInput++] = 0;
        //    }
        //}


        //for (int iActor = 0; iActor < 10; iActor++)
        //{
        //    var actor = iActor < actorsEnemy.Count ? actorsEnemy[iActor] : null;

        //    if (actor != null)
        //    {
        //        Vector3 actorDir = (actor.transform.position - transform.position).normalized;
        //        inputs[iInput++] = (double)actorDir.x;
        //        inputs[iInput++] = (double)actorDir.z;
        //    }
        //    else
        //    {
        //        inputs[iInput++] = 0;
        //        inputs[iInput++] = 0;
        //    }
        //}

        //Vector2 dirTo = (new Vector2(transform.position.x, transform.position.z)).normalized;
        //inputs[iInput++] = (double)dirTo.x;
        //inputs[iInput++] = (double)dirTo.y;

        //for (int iActor = 0; iActor < 10; iActor++)
        //{
        //    var actor = nearbyActors.Count > iActor ? nearbyActors[iActor].actor : null;

        //    if (actor != null)
        //    {
        //        Vector3 actorDir = (actor.transform.position - transform.position).normalized;
        //        inputs[iInput++] = (double)actorDir.x;
        //        inputs[iInput++] = (double)actorDir.z;
        //    }
        //    else
        //    {
        //        inputs[iInput++] = 0;
        //        inputs[iInput++] = 0;
        //    }
        //}

        //inputs[iInput++] = (double)(transform.position.x / 50.0f);
        //inputs[iInput++] = (double)(transform.position.z / 50.0f);

        //inputs[iInput++] = 1;

        //mNN.Fitness = startingFitness + (actorsFriend.Count - actorsEnemy.Count);
        //mNN.Fitness = (nearbyActors.Count) * LevelManager.instance.crowdReward;
        //mNN.Fitness = 50.0f - Vector2.Distance(new Vector2(transform.position.x, transform.position.z), Vector2.zero) +
        //    actorsFriend.Count * LevelManager.instance.crowdRewardEnemy - actorsEnemy.Count * LevelManager.instance.crowdRewardEnemy;
        mNN.Update(inputs);

        Vector3 forwardTo = (new Vector3((float)mNN.Outputs[0], 0, (float)mNN.Outputs[1])).normalized;
        transform.forward = Vector3.Lerp(transform.forward, forwardTo, Time.deltaTime * rotSpeed);
        direction = new Vector3(transform.forward.x, transform.forward.z);

        //float speedFactor = (float)mNN.Outputs[2];
        float speedFactor = 1.0f;
        transform.position += transform.forward * speed * (speedFactor + 1.0f) / 2.0f;

        mesh.localScale = Vector3.Lerp(transform.localScale,
            Vector3.one + Vector3.up * sizeGrow * Mathf.Max((float)mNN.Fitness - startingFitness, 1.0f),
            Time.deltaTime * scaleSpeed);

        if (transform.position.x > 50.0f || transform.position.x < -50.0f ||
               transform.position.z > 50.0f || transform.position.z < -50.0f)
        {
            //mNN.Fitness -= LevelManager.instance.boundsPenalty;
            if (transform.position.x > 50.0f)
                //transform.position = new Vector3(50, transform.position.y, transform.position.z);
                transform.position = new Vector3(-50, transform.position.y, transform.position.z);
            else if (transform.position.x < -50.0f)
                //transform.position = new Vector3(-50, transform.position.y, transform.position.z);
                transform.position = new Vector3(50, transform.position.y, transform.position.z);

            if (transform.position.z > 50.0f)
                //transform.position = new Vector3(transform.position.x, transform.position.y, 50);
                transform.position = new Vector3(transform.position.x, transform.position.y, -50);
            else if (transform.position.z < -50.0f)
                //transform.position = new Vector3(transform.position.x, transform.position.y, -50);
                transform.position = new Vector3(transform.position.x, transform.position.y, 50);
        }

        timeFromSpawn += Time.deltaTime;
    }

    public void TriggerEnter(int id, Collider other)
    {
        switch (id)
        {
            case 0:
                if (other.tag == "Mine")
                {
                    mNN.Fitness += 1.0;
                }
                else if (other.tag == "Actor")
                {
                    //mNN.Fitness += 1.0;
                }
                break;
            case 1:
                if (other.tag == "Actor")
                {
                    //AIActor actor = other.transform.parent.GetComponent<AIActor>();

                    //if (nearbyActors.Count < 10)
                    //{
                    //    nearbyActors.Add(new ActorKvp()
                    //        {
                    //            actor = actor,
                    //            dist = Vector3.Distance(transform.position, actor.transform.position)
                    //        }
                    //    );

                    //    nearbyActors.Sort((x, y) => x.dist.CompareTo(y.dist));
                    //}

                    //if (actor.color == color)
                    //{
                    //    actorsFriend.Add(actor);
                    //}
                    //else
                    //{
                    //    actorsEnemy.Add(actor);
                    //}
                }
                break;
        }
    }

    public void TriggerLeave(int id, Collider other)
    {
        switch (id)
        {
            case 0:
                break;
            case 1:
                if (other.tag == "Actor")
                {
                    //AIActor actor = other.transform.parent.GetComponent<AIActor>();
                    //var kvp = nearbyActors.Find(x => x.actor == actor);
                    //if (actor != null)
                    //    nearbyActors.Remove(kvp);
                    //if (other.transform.parent.GetComponent<AIActor>().color == color)
                    //{
                    //    actorsFriend.Remove(actor);
                    //}
                    //else
                    //{
                    //    actorsEnemy.Remove(actor);
                    //}
                }
                break;
        }
    }
}
