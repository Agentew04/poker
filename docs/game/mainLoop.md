# Starting a Game

When the owner of a room clicks the button to start a game, it sends a message request to 
the server and awaits a response.

Before answering the request, the server sends a request to every player on the lobby 
asking how much balance each one has.

> [!NOTE]
> This will be changed in future versions, as the balance amount of each player will 
> be server authoritative, instead of client authoritative.

After every player responded the request, the server starts calculating some internal 
data, such as:

* Who will be the first "dealer"
* Creates one deck of cards (or more) and shuffles it.
* Automatically calculates what cards each player has and which 5 cards will be on the 
  table.

After this, the server sends a message to each player indicating that the game has 
been started and telling them who is the dealer, which cards each player has and a 
list of players. The server does **not** send data about the cards on the 
table or other player's cards. Also, the list of players is ordered correctly in the 
perspective of the receiving party, alongside some metadata about each one. 
A JSON representation of example data is: (see [Basics](../basics.md) to see how a 
message is actually transmitted)
```json5
{
  "dealer": "Bob",
  "hand": [
    {
      "value": "13",
      "suit": "spades"
    },
    {
      "value": "7",
      "suit": "hearts"
    }
  ],
  "players": [
    {
      // first item of the array is always the receiving party
      "username": "Alice",
      "balance": 500
    },
    {
      "username": "Bob",
      "balance": 200
    },
    {
      "username": "Charlie",
      "balance": 1000
    }
  ]
}
```

In the above example, Bob is the dealer, Charlie is the small blind and Alice (the 
player that received that message) is the big blind. Each client must deduce who is 
the small blind and big bling given the dealer's name in the message.

After the message is sent, the server sends a notification to everyone telling them 
that it is somebodies turn. The notification has a start timestamp, a duration and who 
needs to play. The duration and the start timestamp create a countdown timer, that 
each client must read and display to the end user.

> [!NOTE]
> The first player to play is not the dealer, nor small/big blind. It is always the first 
> person after the big blind.

The active player (the player mentioned on the last notification sent by the server), 
needs to send a request to the server telling them their action alongside a timestamp 
of the action. The server waits a bit more than the duration of the countdown to wait for 
stray packets in the network from the active user. 

1. If the user fails to send a response in time or sends a response with an invalid 
   timestamp, the server tells everyone that he has folded.
2. If the user sends an action message to the server in time, the server processes 
   that action internally and responds with
    1. "OK", if everything happened correctly
    2. "Fail", if something went wrong, like insufficient balance to Call/Raise.
       > [!NOTE]
       > The client application must also restrict user actions in these cases.
3. The server broadcasts the new game status to everyone.

The message contains the following data:
```json5
{
  "pot": "400",
  "players": [
    {
      "name": "Alice",
      "action": "check",
      "bet": 200
    },
    {
      "name": "Bob",
      "action": "raise",
      "bet": 250
    },
    {
      "name": "Charlie",
      "action": null,
      "bet": 0 // 0 indicates empty. do not show on UI
    }
  ],
  "table": [
    // empty because no cards have been revealed 
  ]
}
```

After broadcasting the new public status of the game to everyone, it sends the next 
notification and waits for the new active player's response.
