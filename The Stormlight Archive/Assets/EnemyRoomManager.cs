using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoomManager : MonoBehaviour
{
    [SerializeField] GameObject Entrance;
    [SerializeField] GameObject Exit;
    [SerializeField] GameObject CastOrigin;

    [SerializeField] float enemiesPresent = 0f;
    [SerializeField] Vector3 castHalfExtents = new Vector3(5f, 5f, 100f);

    BoxCollider2D ActivationTrigger;
    BoxCollider2D EntranceCollider;
    BoxCollider2D ExitCollider;
    SpriteRenderer EntranceSprite;
    SpriteRenderer ExitSprite;

    private bool _roomActive = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(CastOrigin.transform.position, castHalfExtents);
    }

    // Start is called before the first frame update
    void Start()
    {
        EntranceCollider = Entrance.GetComponent<BoxCollider2D>();
        EntranceSprite = Entrance.GetComponent<SpriteRenderer>();
        ExitCollider = Exit.GetComponent<BoxCollider2D>();
        ExitSprite = Exit.GetComponent<SpriteRenderer>();
        ActivationTrigger = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_roomActive)
        {
            _roomActive = CastForEnemies();
            Debug.Log(_roomActive);
            if (!_roomActive)
            {
                Entrance.SetActive(false);
                Exit.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Entrance.SetActive(true);
            Exit.SetActive(true);
            ActivationTrigger.enabled = false;
            _roomActive = true;
        }
    }

    private bool CastForEnemies()
    {
        //Debug.Log(CastOrigin.transform.position);
        //RaycastHit2D boxCheck = Physics2D.BoxCast(new Vector2(CastOrigin.transform.position.x, CastOrigin.transform.position.y), castHalfExtents, 0f, Vector2.right, 1f, LayerMask.NameToLayer("Enemy"));
        Collider2D[] boxCheck = Physics2D.OverlapBoxAll(new Vector2(CastOrigin.transform.position.x, CastOrigin.transform.position.y), new Vector2(castHalfExtents.x, castHalfExtents.y), 0f);
        bool EnemyPresent = false;
        foreach (Collider2D collider in boxCheck)
        {
            //Debug.Log(collider.tag);
            if (collider.tag == "Enemy")
            {
                EnemyPresent = true;
                break;
            }
        }
        //Debug.Log(EnemyPresent);
        return EnemyPresent;
    }

    public bool GetRoomActive()
    {
        return _roomActive;
    }
}
