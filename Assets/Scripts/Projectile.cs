using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Pausable
{
    public WorldManager.Team team;
    public Vector3 direction;
    public Vector3 position;
    public RobotController target = null;
    public float speed;
    public bool dead = false;
    public RobotController owner = null;

    public Weapon.Trajectory trajectory = Weapon.Trajectory.Straight;

    public RobotController.ItemFlag itemFlag;

    // Start is called before the first frame update
    void Start()
    {
        WorldManager.current_instance.AddProjectile(this, team);

        OnStart();
    }

    protected virtual void OnStart() { }

    private void OnDestroy()
    {
        WorldManager.current_instance.RemoveProjectile(this);
    }
}
