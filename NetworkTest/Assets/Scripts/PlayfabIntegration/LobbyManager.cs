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
        public event Action<string> onLobbyCreated;
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

        public void CreateLobby(Action<string> OnLobbyCreated)
        {
            if (IsInALobby)
            {
                Debug.Log("Already in a lobby");
                return;
            }

            OnLobbyCreated = onLobbyCreated;
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
            Debug.Log($"Created lobby : {result.LobbyId}");
            onLobbyCreated?.Invoke(result.ConnectionString);
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
        }

        public void LeaveCurrentLobby()
        {
            if (!IsInALobby)
            {
                Debug.Log("Currently not in a lobby");
                return;
            }

            Debug.Log("Attempt to leave current lobby");

            PlayFabMultiplayerAPI.LeaveLobby(
                new LeaveLobbyRequest
                {
                    LobbyId = currentLobbyID,
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
                Debug.Log($"Lobby {i} : {lobbySummary.CurrentPlayers} player{(lobbySummary.CurrentPlayers > 1 ? "s" : "")}");
            }
        }

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

        public void DeleteAllLobbies()
        {
            Debug.Log("Attempt to find lobbies");
            PlayFabMultiplayerAPI.FindLobbies(
                new FindLobbiesRequest { },
                (result) => result.Lobbies.ForEach(lobby => DeleteLobby(lobby.LobbyId)),
                OnFailedToFindLobbies
                );
        }

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
        #endregion
    }
}
