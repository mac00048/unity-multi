using UnityEngine;
using PurrNet;

public class RotationMimic : NetworkBehaviour
{
    [SerializeField] private Transform mimicObj;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }

    private void Update()
    {

        if (!mimicObj)
            return;

        transform.rotation = mimicObj.rotation;
    }
}
