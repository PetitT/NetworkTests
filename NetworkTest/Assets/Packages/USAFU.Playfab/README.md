# Users, Saving, Achievements for Unity

This is the PlayFab implementation of USAFU.

## Setup in PlayFab

### Create a new PlayFab account and title
- In Settings/API Features, enable "Allow client to start games"
- Enable Multiplayer services

### Create a matchmaking queue
- Check Enable Server Allocation and define the build ID that is going to be allocated for that queue
- Add a rule of "Region selection" type
- The rule name doesn't matter
- The attribute path corresponds to the name of the attribute that will be passed by code, it should start with $. unless it is a region selection rule 
- For instance if you want a queue with an Elo rule and a Latency rule, define a "Difference rule" with an attribute path "$.elo", and a "Region selection rule" with an attribute "latencies" 
- The attributes must be serialized into a json
<details><summary>In code, you will pass the custom attributes like this :</summary>
  ```
		[Serializable]
        public struct MatchmakingAttributes
    	{
        	public int elo;
        	public Latencies[] latencies;
    	}

    	[Serializable]
    	public struct Latencies
    	{
        	public string region;
        	public int latency;
        }
        
	    MatchmakingAttributes attributes = new MatchmakingAttributes
        {
           	elo = 50,
            	latencies = new Latencies[]
            	{
                    new Latencies 
		    {
                        region = "NorthEurope",
                        latency = 100
                    }
                }
        };
        
	    string jsonAttributes = JsonUtility.ToJson( attributes );
	    
	    USAFUCore.Get().OnlineSessions.StartMatchmaking(
            new List<IUniqueUserId> { USAFUCore.Get().UserSystem.GetUniqueUserId( 0 ) },
            "My_Matchmaking_Queue",
            new OnlineSessionSettings
            {
                Settings = new Dictionary<string, OnlineSessionSetting>
                {
                    {StringConstants.MATCHMAKING_ATTRIBUTES, new OnlineSessionSetting{ Data = json} },
                    {StringConstants.MATCHMAKING_TIME, new OnlineSessionSetting { Data = "30" } }
                }
            } );
   ```

### Upload a server build
- Make a dedicated server build 
- Compress the build into a Zip File, the executable must be at the root
- In PlayFab, go to Multiplayer/Servers/New Build
- Set Server Type as Container
- Upload the build
- Set "Mount Path" as "C:\Assets"
- Set "Start Command" as [Mount Path]\[Your game executable] (e.g. "C:\Assets\NetworkTest.exe")
- Select a region and the number of servers
- In network, set the port name, port number and protocol (3600/TCP)

## Setup in Unity

### Define ENABLE_PLAYFABSERVER_API in the project settings
- Don't forget to also add it in Server build

### Update the PlayFabSharedSettings in the Resources folder 
- Fill in the title ID and Developer Secret Key that you get from the PlayFab Dashboard
- Set Request type to "Unity Web Request"

### Update the USAFUSettings
- If connect with device is disabled, the player will connect using a random identifier
- The buildID corresponds to the buildID from the PlayFab console, it must be rewritten after a new build has been uploaded
- The preferred regions are used to request a multiplayer server
