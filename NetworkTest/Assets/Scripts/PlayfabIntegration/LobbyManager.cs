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
                PlayFabLogging.Log("Already in a lobby");
                return;
            }

            PlayFabLogging.Log("Creating lobby");

            var request = new CreateLobbyRequest
            {
                Owner = GetMultiplayerEntityKey(),
                MaxPlayers = 2,
                Members = new List<Member> { new Member { MemberEntity = GetMultiplayerEntityKey() } },
                UseConnections = true,
                OwnerMigrationPolicy = OwnerMigrationPolicy.Manual
            };

            PlayFabMultiplayerAPI.CreateLobby(
                request,
                OnCreatedLobby,
                (error) => PlayFabLogging.LogError("Couldn't create lobby", error)
                );
        }

        private void OnCreatedLobby(CreateLobbyResult result)
        {
            PlayFabLogging.Log($"Created lobby : ID = {result.LobbyId}");
            onLobbyCreated?.Invoke(result);
            currentLobbyID = result.LobbyId;
        }

        public void JoinLobby(string connectionString)
        {
            if (IsInALobby)
            {
                PlayFabLogging.Log("Player already in a lobby");
                return;
            }

            PlayFabLogging.Log($"Joining lobby : connection string is {connectionString}");

            var request = new JoinLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                ConnectionString = connectionString
            };

            PlayFabMultiplayerAPI.JoinLobby(
                request,
                OnJoinedLobby,
                (error) => PlayFabLogging.LogError("Couldn't join lobby", error)
                );
        }

        private void OnJoinedLobby(JoinLobbyResult result)
        {
            PlayFabLogging.Log($"Joined Lobby : ID is {result.LobbyId}");
            currentLobbyID = result.LobbyId;
            onJoinedLobby?.Invoke(result);
        }

        public void LeaveCurrentLobby()
        {
            if (!IsInALobby)
            {
                PlayFabLogging.Log("Currently not in a lobby");
                return;
            }
            LeaveLobby(currentLobbyID);
        }

        public void LeaveLobby(string lobbyID)
        {
            PlayFabLogging.Log("Attempt to leave current lobby");

            var request = new LeaveLobbyRequest
            {
                LobbyId = lobbyID,
                MemberEntity = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.LeaveLobby(
                request,
                OnLeftLobby,
                (error) => PlayFabLogging.LogError("Couldn't leave lobby", error)
                );
        }

        private void OnLeftLobby(LobbyEmptyResult result)
        {
            PlayFabLogging.Log("Left lobby");
            currentLobbyID = "";
        }

        public void FindLobbies()
        {
            PlayFabLogging.Log("Attempt to find lobbies");

            var request = new FindLobbiesRequest { };

            PlayFabMultiplayerAPI.FindLobbies(
                request,
                OnFindLobbies,
                (error) => PlayFabLogging.LogError("Couldn't find lobbies", error)
                );
        }

        private void OnFindLobbies(FindLobbiesResult result)
        {
            PlayFabLogging.Log($"Found {result.Lobbies.Count} {(result.Lobbies.Count > 1 ? "lobbies" : "lobby")}");
            for (int i = 0; i < result.Lobbies.Count; i++)
            {
                LobbySummary lobbySummary = result.Lobbies[i];
                PlayFabLogging.Log($"-{i}- | {lobbySummary.CurrentPlayers} player{(lobbySummary.CurrentPlayers > 1 ? "s" : "")} \n Code : {lobbySummary.ConnectionString} \n ID : {lobbySummary.LobbyId} ");
            }
        }

        public void JoinArrangedLobby(string arrangementString)
        {
            PlayFabLogging.Log($"Trying to join arranged lobby with code : {arrangementString}");

            var request = new JoinArrangedLobbyRequest
            {
                ArrangementString = arrangementString
            };

            PlayFabMultiplayerAPI.JoinArrangedLobby(
                request,
                OnJoinArrangedLobby,
                (error) => PlayFabLogging.LogError("Couldn't join lobby", error)
                );
        }

        private void OnJoinArrangedLobby(JoinLobbyResult result)
        {
            PlayFabLogging.Log($"Joined arranged lobby : {result.LobbyId}");
        }

        public void SetLobbyName(string lobbyID, string name)
        {
            PlayFabLogging.Log("Attempt to set lobby name");

            var request = new UpdateLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                LobbyData = new Dictionary<string, string> { { "name", name } },
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.UpdateLobby(
                request,
                OnSetLobbyName,
                (error) => PlayFabLogging.LogError("Couldn't set Lobby Name", error)
                );
        }

        private void OnSetLobbyName(LobbyEmptyResult result)
        {
            PlayFabLogging.Log($"Set lobby name");
        }

        public void SetAsLobbyOwner(string lobbyID)
        {
            PlayFabLogging.Log("Attempt to set lobby owner");

            var request = new UpdateLobbyRequest
            {
                LobbyId = lobbyID,
                Owner = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.UpdateLobby(
                request,
                OnSetAsLobbyOwner,
                (error) => PlayFabLogging.LogError("Couldn't set as lobby owner", error)
                );
        }

        private void OnSetAsLobbyOwner(LobbyEmptyResult result)
        {
            PlayFabLogging.Log("Set as lobby owner");
        }

        public void GetLobby(string lobbyID, Action<GetLobbyResult> onGotLobby)
        {
            PlayFabLogging.Log("Attempt to get lobby");
            onGetLobby = onGotLobby;

            var request = new GetLobbyRequest
            {
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.GetLobby(
                request,
                OnGetLobby,
                (error) => PlayFabLogging.LogError("Couldn't get lobby", error)
                );
        }

        private void OnGetLobby(GetLobbyResult result)
        {
            PlayFabLogging.Log($"Found lobby");
            onGetLobby?.Invoke(result);
        }

        #region DELETE LOBBIES
        // Does not work, needs a "server" entity...
        public void DeleteLobby(string lobbyID)
        {
            PlayFabLogging.Log($"Attempt to delete lobby {lobbyID}");

            var request = new DeleteLobbyRequest
            {
                LobbyId = lobbyID,
                AuthenticationContext = new PlayFabAuthenticationContext
                {
                    EntityId = PlayFabManager.Instance.EntityID,
                    EntityType = "game_server"
                }
            };

            PlayFabMultiplayerAPI.DeleteLobby(
                request,
                OnLobbyDeleted,
                (error) => PlayFabLogging.LogError("Couldn't delete lobby", error)
                );
        }

        private void OnLobbyDeleted(LobbyEmptyResult result)
        {
            PlayFabLogging.Log($"Deleted lobby");
        }

        public void DeleteAllLobbies()
        {
            PlayFabLogging.Log("Attempt to find lobbies");

            var request = new FindLobbiesRequest { };

            PlayFabMultiplayerAPI.FindLobbies(
                request,
                (result) => result.Lobbies.ForEach(lobby => DeleteLobby(lobby.LobbyId)),
                (error) => PlayFabLogging.LogError("Couldn't find all lobbies", error)
                );
        }
        #endregion
    }
}
