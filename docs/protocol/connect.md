# Connection

The game server should be hosted at a predetermined location and should not change. The current server location information is:
* **Address:** `instapokerserver.rodrigoappelt.com`
* **Port:** `12512`

> [!NOTE]
> Not currently hosted while in active development.

As the [Basics](basics.md) document explains, the connection uses TCP as the transport layer protocol.

## Application Version Identification Phase

Both the server and the client sends each other the following identifiers: 
* **Client:** sends to the server the string `InstaPoker.Client`
* **Server:** sends to the client the string `InstaPoker.Server`

These act as a simple barrier to weed-out other applications from connecting to the server and using resources.

After that, both server and client send each other its respective versions. Serialization of the @System.Version object is as described in the [Basics](basics.md#common-data-types) section.

Sequentially, the client sends the chosen username as a string.

