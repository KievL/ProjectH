using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnNPCManager : MonoBehaviour
{
    public GameObject kidPrefab;

    GameObject demon;

    Vector2[] spawnSpots;
    GameObject[] spotsGameObjects;

    public int kidsQuantity;
    int randomSpotIndex;

    // Start is called before the first frame update
    void Start()
    {
        // variaveis
        demon = GameObject.Find("Demon");

        // preenche o array SpawnSpots com os spots
        spawnSpots = new Vector2[53];
        spotsGameObjects = GameObject.FindGameObjectsWithTag("Respawn");

        for (int i = 0; i < 53; i++)
        {
            spawnSpots[i] = spotsGameObjects[i].transform.position;
        }

        // escolhe um spot aleatorio pro Demon
        randomSpotIndex = Random.Range(0, spawnSpots.Length);
        demon.transform.position = spawnSpots[randomSpotIndex];

        // escolhe um spot aleatorio pras crianças
        List<GameObject> blockedSpots = new List<GameObject>();

        for (int i = 0; i < kidsQuantity; i++)
        {
            randomSpotIndex = Random.Range(0, spawnSpots.Length);
            GameObject spot = spotsGameObjects[randomSpotIndex];

            while (blockedSpots.Contains(spot))
            {
                randomSpotIndex = Random.Range(0, spawnSpots.Length);
                spot = spotsGameObjects[randomSpotIndex];
            }

            blockedSpots.Add(spot);

            Vector3 vectorSpot = new Vector3(spot.transform.position.x, spot.transform.position.y, -0.5f);

            GameObject instKid = Instantiate(kidPrefab, vectorSpot, Quaternion.identity);
            instKid.GetComponent<KidBehaviour>().id = i;
        }
    }
}
