using Oculus.Platform;
using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OculusIntegration
{
    public class GroupPresenceManager
    {
        public void SetGroupPresence(bool isJoinable, string destinationApiName, string lobbySessionID, string matchSessionID)
        {
            Debug.Log("Setting group presence");
            GroupPresenceOptions op = new GroupPresenceOptions();
            op.SetIsJoinable(isJoinable);
            op.SetDestinationApiName(destinationApiName);
            op.SetLobbySessionId(lobbySessionID);
            op.SetMatchSessionId(matchSessionID);
            GroupPresence.Set(op);
        }

        public void LaunchInvitePanel()
        {
            Debug.Log("Launching invite panel");
            InviteOptions op = new InviteOptions();
            GroupPresence.LaunchInvitePanel(op);
        }

        public void ClearPresence()
        {
            Debug.Log("Leaving group presence");
            GroupPresence.Clear();
        }
    }
}
