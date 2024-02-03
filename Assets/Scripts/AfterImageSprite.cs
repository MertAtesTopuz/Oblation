using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageSprite : MonoBehaviour
{
    [SerializeField] private float activeTime = 0.1f;
    [SerializeField] private float alphaSet = 0.8f;
    private float timeActivated;
    private float alpha;
    private float alphaMultiplier = 0.85f;

    private Transform player;

    private SpriteRenderer spi;
    private SpriteRenderer playerSpi;
    private SpriteMask spiMask;

    private Color color;

    void OnEnable()
    {
        spi = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSpi = player.GetComponent<SpriteRenderer>();
        spiMask = GetComponent<SpriteMask>();

        alpha = alphaSet;
        spi.sprite = playerSpi.sprite;
        spiMask.sprite = spi.sprite;
        transform.position = player.position;
        transform.rotation = player.rotation;
        timeActivated = Time.time;
    }

    void Update()
    {
        alpha *= alphaMultiplier;
        //color = new Color(1f,1f,1f, alpha);
        color = new Color(255f,255f,255,255f);
        spi.color = color;

        if(Time.time >= (timeActivated +activeTime))
        {
            AfterImagePool.instance.AddToPool(gameObject);
        }
    }
    
}
