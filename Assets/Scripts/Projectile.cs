using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public WorldManager.Team team;
    public WorldManager worldManager;
    public Vector3 direction;
    public Vector3 position;
    public RobotController target = null;
    public float speed;
    public bool dead = false;
    public RobotController owner = null;

    // Start is called before the first frame update
    void Start()
    {
        worldManager.AddProjectile(this, team);

        OnStart();
    }

    protected virtual void OnStart() { }

    private void OnDestroy()
    {
        worldManager.RemoveProjectile(this);
    }
}
