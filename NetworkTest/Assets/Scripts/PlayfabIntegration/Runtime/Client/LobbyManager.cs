using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FishingCactus.PlayFabIntegration
{
    public class LobbyManager
    {
        //FIELDS
        private PlayFab.MultiplayerModels.EntityKey entityKey;
        public string currentLobbyID { get; private set; }

        //PROPERTIES
        public bool IsInALobby => !string.IsNullOrEmpty( currentLobbyID );

        //METHODS

        // Converts the entity key from two different namespaces
        private PlayFab.MultiplayerModels.EntityKey GetMultiplayerEntityKey()
        {
            if ( !PlayFabManager.Instance.IsLoggedIn )
            {
                PlayFabLogging.LogError( "Must be connected to Playfab to call this method" );
                return null;
            }

            if ( entityKey == null )
            {
                PlayFab.ClientModels.EntityKey clientkey = PlayFabManager.Instance.EntityKey;
                PlayFab.MultiplayerModels.EntityKey multiplayerKey = new EntityKey { Id = clientkey.Id, Type = clientkey.Type };
                entityKey = multiplayerKey;
            }

            return entityKey;
        }

        public void CreateLobby(
            uint maxPlayers, 
            Action<CreateLobbyResult> onLobbyCreated = null 
            )
        {
            if ( IsInALobby )
            {
                PlayFabLogging.Log( "Already in a lobby" );
                return;
            }

            PlayFabLogging.Log( "Creating lobby" );

            var request = new CreateLobbyRequest
            {
                Owner = GetMultiplayerEntityKey(),
                MaxPlayers = maxPlayers,
                Members = new List<Member> { new Member { MemberEntity = GetMultiplayerEntityKey() } },
                UseConnections = true,
                OwnerMigrationPolicy = OwnerMigrationPolicy.Manual
            };

            PlayFabMultiplayerAPI.CreateLobby(
               request,
               ( result ) =>
               { 
                   PlayFabLogging.Log( $"Created lobby : ID = { result.LobbyId }" );
                   currentLobbyID = result.LobbyId;
                   onLobbyCreated?.Invoke( result );
               },
               ( error ) =>
               {
                   PlayFabLogging.LogError( "Couldn't create lobby", error );
                   onLobbyCreated?.Invoke( null );
               });
        }

        public void JoinLobby(
            string connectionString, 
            Action<JoinLobbyResult> onLobbyJoined = null
            )
        {
            if ( IsInALobby )
            {
                PlayFabLogging.Log( "Player already in a lobby" );
                onLobbyJoined?.Invoke( null );
                return;
            }

            PlayFabLogging.Log( $"Joining lobby : connection string is { connectionString }" );

            var request = new JoinLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                ConnectionString = connectionString
            };

            PlayFabMultiplayerAPI.JoinLobby(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( $"Joined Lobby : ID is { result.LobbyId }" );
                    currentLobbyID = result.LobbyId;            
                    onLobbyJoined?.Invoke( result );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't join lobby", error );
                    onLobbyJoined?.Invoke( null );
                });
        }

        public void LeaveCurrentLobby( Action<bool> onleftLobby = null )
        {
            if ( !IsInALobby )
            {
                PlayFabLogging.Log( "Currently not in a lobby" );
                onleftLobby?.Invoke( false );
                return;
            }
            LeaveLobby( currentLobbyID, onleftLobby );
        }

        public void LeaveLobby(
            string lobbyID, 
            Action<bool> onLeftLobby = null
            )
        {
            PlayFabLogging.Log( "Attempt to leave current lobby" );

            var request = new LeaveLobbyRequest
            {
                LobbyId = lobbyID,
                MemberEntity = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.LeaveLobby(
                request,
                ( result ) =>
                {
                    PlayFabLogging.Log( "Left lobby" );
                    currentLobbyID = "";
                    onLeftLobby?.Invoke( true );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't leave lobby", error );
                    onLeftLobby?.Invoke( false );
                });
        }

        public void FindLobbies( Action<List<LobbySummary>> onFoundLobbies )
        {
            PlayFabLogging.Log( "Attempt to find lobbies" );
            
            var request = new FindLobbiesRequest { };

            PlayFabMultiplayerAPI.FindLobbies(
                request,
                ( result ) => 
                {
                    PlayFabLogging.Log( $"Found { result.Lobbies.Count } { ( result.Lobbies.Count == 1 ? "lobby" : "lobbies" ) }" );
                    onFoundLobbies?.Invoke( result.Lobbies );                    
                },
                ( error ) => 
                {
                    PlayFabLogging.LogError( "Couldn't find lobbies", error );
                    onFoundLobbies?.Invoke( null );
                });
        }

        public void SetCurrentLobbyData( Dictionary<string,string> data )
        {
            if ( string.IsNullOrEmpty( currentLobbyID ) )
            {
                Debug.Log( "Curently not in a lobby");
                return;
            }

            SetLobbyData( currentLobbyID, data );
        }

        public void SetLobbyData( 
            string lobbyID, 
            Dictionary<string, string> data 
            )
        {
            PlayFabLogging.Log( "Attempt to update lobby data" );

            var request = new UpdateLobbyRequest
            {
                MemberEntity = GetMultiplayerEntityKey(),
                LobbyData = data,
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.UpdateLobby(
               request,
               ( result ) => PlayFabLogging.Log( "Updated lobby data" ),
               ( error ) => PlayFabLogging.LogError( "Couldn't update lobby data", error )
               );
        }

        public void SetSelfAsLobbyOwner( string lobbyID )
        {
            PlayFabLogging.Log("Attempt to set lobby owner");

            var request = new UpdateLobbyRequest
            {
                LobbyId = lobbyID,
                Owner = GetMultiplayerEntityKey()
            };

            PlayFabMultiplayerAPI.UpdateLobby(
                request,
                ( result ) => PlayFabLogging.Log( "Set as lobby owner" ),
                ( error ) => PlayFabLogging.LogError( "Couldn't set as lobby owner", error )
                );
        }

        public void GetCurrentLobby( Action<Lobby> onGotLobby )
        {
            if ( string.IsNullOrEmpty( currentLobbyID ) )
            {
                PlayFabLogging.Log( "Currently not in a lobby" );
                onGotLobby?.Invoke( null );
                return;
            }
            GetLobby( currentLobbyID, onGotLobby );
        }

        public void GetLobby(
            string lobbyID, 
            Action<Lobby> onGotLobby
            )
        {
            PlayFabLogging.Log( "Attempt to get lobby" );

            var request = new GetLobbyRequest
            {
                LobbyId = lobbyID
            };

            PlayFabMultiplayerAPI.GetLobby(
                request,
                ( result ) =>
                { 
                    PlayFabLogging.Log( $"Found lobby with ID { lobbyID }" );
                    onGotLobby?.Invoke( result.Lobby );
                },
                ( error ) =>
                {
                    PlayFabLogging.LogError( "Couldn't get lobby", error );
                    onGotLobby?.Invoke( null );
                });
        }
    }
}
