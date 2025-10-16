using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class BlockSFX : MonoBehaviour //appears when a block happens and blockhp depends on 
{
    public FightingPlayerController owner;
    public float lastHit;
    public Renderer render;
    private Color color;
    private float colorVal;
    private float value;
    public PhotonView photonView;
    private float ColorAlpha;
    // Start is called before the first frame update
    void Start()
    {
        owner = GetComponentInParent<FightingPlayerController>();
        render = GetComponent<Renderer>();
        color = render.material.color;
        color.a = 0f;
        render.material.color = color;
        photonView = owner.photonView;
    }

// Update is called once per frame
    void Update()
    {
        value = owner.blockMeter;
        colorVal = Mathf.InverseLerp(0f, 100f, value);
        color = Color.Lerp(Color.red, Color.blue, colorVal);

        if (Time.time - lastHit > 1.5f)
        {
            ColorAlpha = 0;
        }
        color.a = ColorAlpha;
        render.material.color = color;
        
        
    }
    [PunRPC]
    public void RPC_Appear()//called by FPC
    {
        lastHit = Time.time;
        ColorAlpha = 0.6f;
        //block sound goes here


    }
    
    
}
