using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public EnemyScriptableObject enemyData;
    Transform player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>().transform;
    }


    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, enemyData.MoveSpeed * Time.deltaTime); //Constantyl move towards the player
    }
}
