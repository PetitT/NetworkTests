# PlayFab integration for Unity

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
- In code, you will pass the custom attributes object like this :
```
new
{
	elo = 15,
        latencies = new object[]
        {
        	new
                {
                	region = "NorthEurope",
                        latency = 100
                 }
         }
 }
```
### Upload a server build
- Set the build type in the PlayFabConfiguration asset as "REMOTE_SERVER"
- Make a dedicated server build 
- Compress the build into a Zip File, the executable must be at the root
- In PlayFab, go to Multiplayer/Servers/New Build
- Set Server Type as Container
- Upload the build
- Set "Mount Path" as "C:\Assets"
- Set "Start Command" as [Mount Path]\[Your game executable] (e.g. "C:\Assets\NetworkTest.exe")
- Select a region and the number of servers
- In network, set the name as "game_port", port at 3600 and TCP protocol

## Setup in Unity

### Log into playfab in the PlayFabEditorExtentions window
- Fill in the studio and titleID in Settings
- Set Request type to Unity www
- In Settings/API, enable Client/Entity/Server API

### Define ENABLE_PLAYFABSERVER_API in the project settings
- Don't forget to also add it in Server build

### Create a new PlayFabConfiguration in the Resources folder
- The buildID corresponds to the buildID from the PlayFab console, it must be rewritten after a new build has been uploaded
- The preferred regions are used to request a multiplayer server
