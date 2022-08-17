using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayFabIntegration
{
    public class LobbyManager
    {
        public event Action<CreateLobbyResult> onLobbyCreated;
        public event Action<JoinLobbyResult> onJoinedLobby;
        private event Action<GetLobbyResult> onGetLobby;
        public bool IsInALobby => !string.IsNullOrEmpty(currentLobbyID);

        private string currentLobbyID;
        private PlayFab.MultiplayerModels.EntityKey entityKey;

        /// <summary>
        /// This is a workaround to convert the entity key from two different namespaces
        /// </summary>
        /// <returns></returns>
        private PlayFab.MultiplayerModels.EntityKey GetMultiplayerEntityKey()
        {
            if (entityKey == null)
            {
                PlayFab.ClientModels.EntityKey clientkey = PlayFabManager.Instance.EntityKey;
                PlayFab.MultiplayerModels.EntityKey multiplayerKey = new EntityKey { Id = clientkey.Id, Type = clientkey.Type };
                entityKey = multiplayerKey;
            }
            return entityKey;
        }

        public void CreateLobby()
        {
            if (IsInALobby)
            {
                Debug.Log("Already in a lobby");
                return;
            }

            Debug.Log("Creating lobby");

            PlayFabMultiplayerAPI.CreateLobby(
                new CreateLobbyRequest
                {
                    Owner = GetMultiplayerEntityKey(),
                    MaxPlayers = 2,
                    Members = new List<Member> { new Member { MemberEntity = GetMultiplayerEntityKey() } }
                },
                OnCreatedLobby,
                OnFailedToCreateLobby
                );
        }

        private void OnCreatedLobby(CreateLobbyResult result)
        {
            Debug.Log($"Created lobby : ID = {result.LobbyId}");
            onLobbyCreated?.Invoke(result);
            currentLobbyID = result.LobbyId;
        }

        public void JoinLobby(string connectionString)
        {
            if (IsInALobby)
            {
                Debug.Log("Player already in a lobby");
                return;
            }

            Debug.Log($"Joining lobby : connection string is {connectionString}");
            PlayFabMultiplayerAPI.JoinLobby(
                new JoinLobbyRequest
                {
                    MemberEntity = GetMultiplayerEntityKey(),
                    ConnectionString = connectionString                    
                },
                OnJoinedLobby,
                OnFailedToJoinLobby
                );
        }

        private void OnJoinedLobby(JoinLobbyResult result)
        {
            Debug.Log($"Joined Lobby : ID is {result.LobbyId}");
            currentLobbyID = result.LobbyId;
            onJoinedLobby?.Invoke(result);
        }

        public void LeaveCurrentLobby()
        {
            if (!IsInALobby)
            {
                Debug.Log("Currently not in a lobby");
                return;
            }
            LeaveLobby(currentLobbyID);
        }

        public void LeaveLobby(string lobbyID)
        {
            Debug.Log("Attempt to leave current lobby");

            PlayFabMultiplayerAPI.LeaveLobby(
                new LeaveLobbyRequest
                {
                    LobbyId = lobbyID,
                    MemberEntity = GetMultiplayerEntityKey()
                },
                OnLeftLobby,
                OnFailedToLeaveLobby
                );
        }

        private void OnLeftLobby(LobbyEmptyResult result)
        {
            Debug.Log("Left lobby");
            currentLobbyID = "";
        }

        public void FindLobbies()
        {
            Debug.Log("Attempt to find lobbies");
            PlayFabMultiplayerAPI.FindLobbies(

                new FindLobbiesRequest { },
                OnFindLobbies,
                OnFailedToFindLobbies
                );
        }

        private void OnFindLobbies(FindLobbiesResult result)
        {
            Debug.Log($"Found {result.Lobbies.Count} {(result.Lobbies.Count > 1 ? "lobbies" : "lobby")}");
            for (int i = 0; i < result.Lobbies.Count; i++)
            {
                LobbySummary lobbySummary = result.Lobbies[i];
                Debug.Log($"-{i}- | {lobbySummary.CurrentPlayers} player{(lobbySummary.CurrentPlayers > 1 ? "s" : "")} \n Code : {lobbySummary.ConnectionString} \n ID : {lobbySummary.LobbyId} ");
            }
        }

        public void JoinArrangedLobby(string arrangementString)
        {
            Debug.Log($"Trying to join arranged lobby with code : {arrangementString}");
            PlayFabMultiplayerAPI.JoinArrangedLobby(
                new JoinArrangedLobbyRequest
                {
                    ArrangementString = arrangementString
                },
                OnJoinArrangedLobby,
                OnFailedToJoinArrangedLobby
                );
        }

        private void OnJoinArrangedLobby(JoinLobbyResult result)
        {
            Debug.Log($"Joined arranged lobby : {result.LobbyId}");
        }

        public void SetLobbyName(string lobbyID, string name)
        {
            Debug.Log("Attempt to set lobby name");
            PlayFabMultiplayerAPI.UpdateLobby(
                new UpdateLobbyRequest
                {
                    MemberEntity = GetMultiplayerEntityKey(),
                    LobbyData = new Dictionary<string, string> { { "name", name } },
                    LobbyId = lobbyID
                },
                OnSetLobbyName,
                OnFailedToSetLobbyName
                );
        }

        private void OnSetLobbyName(LobbyEmptyResult result)
        {
            Debug.Log($"Set lobby name");
        }

        public void GetLobby(string lobbyID, Action<GetLobbyResult> onGotLobby)
        {
            Debug.Log("Attempt to get lobby");
            onGetLobby = onGotLobby;
            PlayFabMultiplayerAPI.GetLobby(
                new GetLobbyRequest
                {
                    LobbyId = lobbyID
                },
                OnGetLobby,
                OnFailedToGetLobby
                );
        }

        private void OnGetLobby(GetLobbyResult result)
        {
            Debug.Log($"Found lobby");
            onGetLobby?.Invoke(result);
        }

        #region DELETE LOBBIES
        // Does not work, needs a "server" entity...
        public void DeleteLobby(string lobbyID)
        {
            Debug.Log($"Attempt to delete lobby {lobbyID}");
            PlayFabMultiplayerAPI.DeleteLobby(
                new DeleteLobbyRequest
                {
                    LobbyId = lobbyID,
                    AuthenticationContext = new PlayFabAuthenticationContext
                    {
                        EntityId = PlayFabManager.Instance.EntityID,
                        EntityType = "game_server"
                    }
                },
                OnLobbyDeleted,
                OnFailedToDeleteLobby
                );
        }

        private void OnLobbyDeleted(LobbyEmptyResult result)
        {
            Debug.Log($"Deleted lobby");
        }

        public void DeleteAllLobbies()
        {
            Debug.Log("Attempt to find lobbies");
            PlayFabMultiplayerAPI.FindLobbies(
                new FindLobbiesRequest { },
                (result) => result.Lobbies.ForEach(lobby => DeleteLobby(lobby.LobbyId)),
                OnFailedToFindLobbies
                );
        }
        #endregion

        #region ERRORS
        private void OnFailedToCreateLobby(PlayFabError error)
        {
            Debug.Log($"Couldn't create lobby : {error.GenerateErrorReport()}");
        }

        private void OnFailedToJoinLobby(PlayFabError error)
        {
            Debug.Log($"Couldn't join lobby : {error.GenerateErrorReport()}");
        }

        private void OnFailedToLeaveLobby(PlayFabError error)
        {
            Debug.Log($"Failed to leave lobby : {error.GenerateErrorReport()}");
        }

        private void OnFailedToJoinArrangedLobby(PlayFabError error)
        {
            Debug.Log($"Couldn't join arranged lobby : {error.GenerateErrorReport()}");
        }

        private void OnFailedToFindLobbies(PlayFabError error)
        {
            Debug.Log($"Couldn't find lobbies : {error.GenerateErrorReport()}");
        }

        private void OnFailedToDeleteLobby(PlayFabError error)
        {
            Debug.Log($"Couldn't delete lobby : {error.GenerateErrorReport()}");
        }

        private void OnFailedToSetLobbyName(PlayFabError error)
        {
            Debug.Log($"Couldn't set lobby name : {error.GenerateErrorReport()}");
        }

        private void OnFailedToGetLobby(PlayFabError error)
        {
            Debug.Log($"Failed to get lobby : {error.GenerateErrorReport()}");
        }
        #endregion
    }
}
