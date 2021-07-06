using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipHelper : NetworkBehaviour
{
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        // TODO Don't use NetworkBehaviour for just NetworkStart [GOMPS-81]
        if (IsLocalPlayer)
        {
            enabled = false;
            // dont need to do anything else if the owner
            return;
        }

        //// find the hero action UI bar
        //GameObject actionUIobj = GameObject.FindGameObjectWithTag("HeroActionBar");
        //actionUIobj.GetComponent<Visual.HeroActionBar>().RegisterInputSender(this);
    }

    [ServerRpc]
    public void FlipServerRpc(bool isLeft)
    {
        GetComponent<SpriteRenderer>().flipX = isLeft;
    }

    [ClientRpc]
    public void FlipClientRpc(bool isLeft)
    {
        GetComponent<SpriteRenderer>().flipX = isLeft;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
