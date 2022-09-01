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
        public event Action<JoinLobbyResult> onJoinedLobby;
        public bool IsInALobby => !string.IsNullOrEmpty(currentLobbyID);

        private string currentLobbyID;
        private PlayFab.MultiplayerModels.EntityKey entityKey;

        // This is a workaround to convert the entity key from two different namespaces
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

        public void CreateLobby(Action<CreateLobbyResult> onLobbyCreated = null)
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
               (result) =>
               {
                   OnCreatedLobby(result);
                   onLobbyCreated?.Invoke(result);
               },
               (error) =>
               {
                   PlayFabLogging.LogError("Couldn't create lobby", error);
                   onLobbyCreated?.Invoke(null);
               }
               );
        }

        private void OnCreatedLobby(CreateLobbyResult result)
        {
            PlayFabLogging.Log($"Created lobby : ID = {result.LobbyId}");
            currentLobbyID = result.LobbyId;
            GetLobby(currentLobbyID);
        }

        public void JoinLobby(string connectionString, Action<JoinLobbyResult> onJoinLobby = null)
        {
            if (IsInALobby)
            {
                PlayFabLogging.Log("Player already in a lobby");
                onJoinLobby?.Invoke(null);
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
                (result) =>
                {
                    OnJoinedLobby(result);
                    onJoinLobby?.Invoke(result);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't join lobby", error);
                    onJoinLobby?.Invoke(null);
                }
                );
        }

        private void OnJoinedLobby(JoinLobbyResult result)
        {
            PlayFabLogging.Log($"Joined Lobby : ID is {result.LobbyId}");
            currentLobbyID = result.LobbyId;            
            onJoinedLobby?.Invoke(result);
        }

        public void LeaveCurrentLobby(Action<bool> onleftLobby = null)
        {
            if (!IsInALobby)
            {
                PlayFabLogging.Log("Currently not in a lobby");
                return;
            }
            LeaveLobby(currentLobbyID, onleftLobby);
        }

        public void LeaveLobby(string lobbyID, Action<bool> onLeftLobby = null)
        {
            PlayFabLogging.Log("Attempt to leave current lobby");

            var request = new LeaveLobbyRequest
            {
                LobbyId = lobbyID,
                MemberEntity = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.LeaveLobby(
                request,
                (result) =>
                {
                    OnLeftLobby(result);
                    onLeftLobby?.Invoke(true);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't leave lobby", error);
                    onLeftLobby?.Invoke(false);
                }
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

        public void JoinArrangedLobby(string arrangementString, Action<JoinLobbyResult> onJoin = null)
        {
            PlayFabLogging.Log($"Trying to join arranged lobby with code : {arrangementString}");

            var request = new JoinArrangedLobbyRequest
            {
                ArrangementString = arrangementString,
                MemberEntity = GetMultiplayerEntityKey(),
            };

            PlayFabMultiplayerAPI.JoinArrangedLobby(
                request,
                (result) =>
                {
                    OnJoinArrangedLobby(result);
                    onJoin?.Invoke(result);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't join lobby", error);
                    onJoin?.Invoke(null);
                }
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
                OnSetLobbyData,
                (error) => PlayFabLogging.LogError("Couldn't set Lobby Name", error)
                );
        }

        public void SetLobbyData(string lobbyID, Dictionary<string, string> data)
        {
            PlayFabLogging.Log("Attempt to set lobby data");

            var request = new UpdateLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                LobbyData = data,
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.UpdateLobby(
               request,
               OnSetLobbyData,
               (error) => PlayFabLogging.LogError("Couldn't set Lobby Name", error)
               );
        }

        private void OnSetLobbyData(LobbyEmptyResult result)
        {
            PlayFabLogging.Log($"Set lobby data");
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

        public void GetLobby(string lobbyID, Action<GetLobbyResult> onGotLobby = null)
        {
            PlayFabLogging.Log("Attempt to get lobby");

            var request = new GetLobbyRequest
            {
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.GetLobby(
                request,
                (result) =>
                {
                    OnGetLobby(result);
                    onGotLobby?.Invoke(result);
                },
                (error) =>
                {
                    PlayFabLogging.LogError("Couldn't get lobby", error);
                    onGotLobby?.Invoke(null);
                }
                );
        }

        private void OnGetLobby(GetLobbyResult result)
        {            
            PlayFabLogging.Log($"Found lobby");
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
