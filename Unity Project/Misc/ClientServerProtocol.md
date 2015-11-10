# Tower Defender client/server protocol #

URI Handler needs host id, match id, team id, username

Protocol message arguments are delimited with a pipe (i.e. '|').

| Message ID| Desc 		| Sent by/For	| Arguments | 
| --------- | ----------------- | ------------- | --------- |
| 1	    | join game	| from client, for all | username, team id, host id |
| 2	    | tower built 	| from client, for all | prefab name, teamid, position x, position y |
| 3	    | enemy dies in team territory | from client, for all | enemy id, team id |
| 4	    | health update | from client, for all | hostid, team id, health |
| 5	    | send enemy to opposing team | from client, for all | enemy id, prefab name, team id to attack, spawner id |
| 6	    | chat message | from client, for all | username, message content, team id (1/2 for team communication, 0 for all) |
| 7	    | keep alive | from client, for all | hostid |
| 8	    | join game acknowledge | from client, for new joining client | username, team id, host id |
| 9	    | | | |
| 10	    | | | |
