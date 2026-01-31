# Protocol Basics

Apart from the initial connection handshake, all communication between client and server are made with **messages** (or sometimes called packets). Each message has a fixed size header and a variable length payload. Messages also have different semantic types. Each message
can be one of three types:
* **Notification:** a message that just conveys information from one party to another. It can be result of an action, like `User clicked a button` or declaring that something happened: `A user left the game`. 
* **Request:** a message that asks for more information or an action that expects a clear response from the other party. 
* **Response:** a message associated with a unique request type, and transports data related with the request.

> [!NOTE]
> Delivery confirmation is handled by the underlying protocol, TCP. So the rules defined here expect that every message will eventually reach the other party in the correct order that it was sent, with no errors or "packet losses".

> [!WARNING]
> As currently plain TCP is used, communication is unsecure by default. At the same time, the server does not do any authentication of user data, accepting any balance the client reports as the truth. In the future, the underlying protocol may be changed to a encrypted one (TLS, with @System.Net.Security.SslStream ) and app-issued certificates alongside "Unique Hardware IDs" to prevent account creationg spam (and currency multiplication).

## Message Format

| Message Type | Payload Size | Payload |
|--------------|--------------|---------|
| GUID - 16 bytes| integer - 4 bytes| binary data - `Payload Size` bytes |

For a GUID of new message type be recognized in the application, it must be added to the cached map at @InstaPoker.Core.Messages.MessageTypeCache

The payload must be correctly read in its entirety using the methods @InstaPoker.Core.Messages.Message.Write and @InstaPoker.Core.Messages.Message.Read in each message type definition.

Documentation about a message type and its payload schema may be wrong or outdated. Always refer to the source code and the order of operations using the @System.IO.BinaryReader and @System.IO.BinaryWriter classes.

## Common Data Types

Some data types use non-standard (in networking) serialization schemas. Currently, these are:
* **@System.String :** serialized as a length-prefixed string. The length is encoded as a 7-bit integer. This allows for a small decrease in packet size.
* **@System.Version :** encoded as 4 sequential 7-bit encoded integers. These represent, in order, the major, minor, build and revision numbers of the version.