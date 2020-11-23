using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour {

    List<Transform> zombies;
    public static int currCount;

    public Transform allPlayers;

    int maxZombies;
    float minSpawnDist = 1.5f;

    public Transform zombiePre;

	// Use this for initialization
	void Start () {
        maxZombies = 10;
        currCount = 0;
        zombies = new List<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        while (currCount < maxZombies)
        {
            Vector3 spawnLoc = new Vector3(Random.Range(-15f, 15f), 1f, Random.Range(-15f, 15f));
            Transform zombie = Instantiate<Transform>(zombiePre);
            zombie.position = spawnLoc;
            currCount++;
        }
	}
}
