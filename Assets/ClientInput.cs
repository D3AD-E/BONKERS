using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientInput : NetworkBehaviour
{
    public override void NetworkStart()
    {
        // TODO Don't use NetworkBehaviour for just NetworkStart [GOMPS-81]
        if (!IsClient || !IsOwner)
        {
            enabled = false;
            // dont need to do anything else if not the owner
            return;
        }

        //// find the hero action UI bar
        //GameObject actionUIobj = GameObject.FindGameObjectWithTag("HeroActionBar");
        //actionUIobj.GetComponent<Visual.HeroActionBar>().RegisterInputSender(this);
    }

    private void Update()
    {
        
    }
}
