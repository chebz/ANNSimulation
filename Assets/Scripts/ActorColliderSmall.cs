using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class ActorColliderSmall : MonoBehaviour
{
    public AIActor actor;
    public int colliderID;

    bool Validate(GameObject other)
    {
        var mine = other.GetComponent<Mine>();
        if (mine == null)
            return false;

        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Validate(other.gameObject))
        {
            actor.TriggerEnter(colliderID, other);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (Validate(other.gameObject))
        {
            actor.TriggerLeave(colliderID, other);
        }
    }
}
