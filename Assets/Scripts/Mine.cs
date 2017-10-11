using UnityEngine;
using System.Collections;
using PathologicalGames;

public class Mine : MonoBehaviour
{
    void OnSpawned()
    {
        LevelManager.instance.mines.Add(this);
    }

    void OnDespawned()
    {
        LevelManager.instance.mines.Remove(this);
    }

    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Actor" && other.name.Equals("TriggerSmall"))
        {
            PoolManager.Pools["Mines"].Despawn(transform);

            //float posX = Random.Range(-50.0f, 50.0f);
            //float posZ = Random.Range(-50.0f, 50.0f);
            //float posY = 0;
            //transform.position = new Vector3(posX, posY, posZ);
        }
    }
}
