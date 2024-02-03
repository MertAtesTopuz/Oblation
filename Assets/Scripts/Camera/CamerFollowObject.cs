using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CamerFollowObject : MonoBehaviour
{
    [Header("Referances")]
    [SerializeField] private Transform playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float flipRotationTime = 0.5f;

    private CharacterController player;

    private bool faceRight;

    void Awake()
    {
        player = playerTransform.gameObject.GetComponent<CharacterController>();
        faceRight = player.faceRight;
    }

    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.2f, player.transform.position.z);
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject, DetarmineEndRotation(), flipRotationTime).setEaseInOutSine();
    }

    private float DetarmineEndRotation()
    {
        faceRight = !faceRight;

        if(faceRight)
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }
}
