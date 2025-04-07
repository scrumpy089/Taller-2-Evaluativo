using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Buttons : MonoBehaviour
{
    public PlayerController Player;
    private float enemy1Damage = 10;
    private float Heal = 5;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Enemy1()
    {

        Player.TakeDamage(enemy1Damage);

        Player.hitFromRight = true;

        Player.hitForceX = 5;
        Player.hitForceY = 2;
        Player.hitTime = 3;

        Player.particleCount = 3;
    }

    public void itemHeath1()
    {


        Player.AddHealth(Heal);
        Player.particleCount = 1;


    }
}
