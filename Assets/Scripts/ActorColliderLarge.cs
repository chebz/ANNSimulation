using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ActorColliderLarge : MonoBehaviour {
    public AIActor actor;
    public int colliderID;

    bool Validate(GameObject other)
    {
        if (!other.name.Equals("TriggerSmall"))
            return false;

        var otherCollider = other.GetComponent<ActorColliderSmall>();
        if (otherCollider == null)
            return false;

        if (otherCollider.actor == actor)
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
