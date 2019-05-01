# Elastic Grid Network?
*Elastic Grid Network* is a P2P grid networking solution for the *Bryllite-Platform*.  

On the *Bryllite-Platform*, all nodes with mining authority must communicate at the same time to reach an agreement for block’s agreement. Because all mining nodes participate in the agreement, the blocks connected to the chain become irreversible, which will solve the *double-spending problem* and allow immediate transaction settlement without waiting for the *confirm time*.  

However, the participation of all mining nodes in an agreement means that as the number of nodes participating in the network grows, the burden of network transmission increases and there is a danger that the agreement can not be reached within the time limit. For Example, assume that there are about 10,000 mining nodes participating in the Bryllite network, and that the size of the block to be verified in the BCP agreement step is 1MB. In this case, the data that the node requesting the block verification needs to transfer is about `10 GB (1 MB * 10,000)`, and it is difficult to reach a consensus by transmitting and verifying the block to all the nodes within the BCP time limit.  

To solve these problems, *Elastic Grid Network Solution* was designed to be suitable for *Bryllite-Platform*. This solution is an efficient grid network technology in which the layout of grid is flexibly applied according to the size of mining nodes participating in the network.

# Notice
This project and code is conceptual code that shows how the *Elastic Grid Network Protocol* works and is not actual code of the *Bryllite-Platform*

# Environment
Visual Studio 2017  
[.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)

# Basic Principle of Elastic Grid Network Operation
Mining nodes participating in the network know the addresses of all mining nodes participating in the network through the *Node Discovery Service* ( here `PeerListServiceApp` )

If one node(sender) wants to send a message to the entire network,

* The sender determines the `layout` of the grid based on the size of the mining node participating in the network. The layout expressed in a 3D-Coordinates system of (x, y, z). (Eg layout = {3, 3, 3})

* The coordinates of each node are deterministic by the layout.  
Reference: [Layout Decision based on nPeers](#layout-decision-based-on-npeers)

* The sender randomly selects one of the nodes belonging to the grid and transmits a message to the node, The recipient receiving this message relays the message to all nodes in the grid.
If the sender is included in the grid while selecting one of the nodes belonging to the grid, always select the sender. If the message transfer to the selected node fails, select the next node and send it again.

* Depending on the layout, the relay is done in the order of z-axis, y-axis, x-axis.

# Layout Decision based on nPeers
The layout is determined by the sender when sending message, and is determined by the number(`nPeers`) of nodes participating in the entire network and the number(`N`) of nodes to be included in a grid.

> N: Number of nodes to include in one grid  
> nPeers: The total number of nodes participating in the network

The figure below shows how to determine the layout according to the number of total nodes participating in the network(`nPeers`) and the number of nodes to be included in a grid(`N`)

![layout_decision_1](https://user-images.githubusercontent.com/38033465/53714769-f64e8880-3e92-11e9-85b8-eed71251081f.jpg)
![layout_decision_2](https://user-images.githubusercontent.com/38033465/53714772-f8184c00-3e92-11e9-9cea-21d680406164.jpg)

Each box in the above figure has about `N` nodes

# Computing the Coordinates of a node
The Coordinates of the node are determined by extracting the `hx`, `hy`, and `hz` values from the hash value of the node address, and performing the remaining operations on each coordinate axis size of the layout

![hx,hy,hz](https://user-images.githubusercontent.com/38033465/53714789-11b99380-3e93-11e9-9a5f-aeb94da44145.jpg)

~~~
CoordinatesOf(Node).X = 1 + ( hx % layout.X )
CoordinatesOf(Node).Y = 1 + ( hy % layout.Y )
CoordinatesOf(Node).Z = 1 + ( hz % layout.Z )
~~~

These coordinates are deterministic according to the address of the nodes and layout, and they are uniformly distributed in each grid

# How message broadcating works
For example, if you have `81` participating nodes and the layout is `{3, 3, 3}`, you can describe message broadcasting as follows: One box in the figure below contains about `3` nodes.

## Z-Axis message transmission
If the size of the layout's z-axis is greater than 1, the entire node is divided by the z coordinates as shown below, and the message is relayed to each coordinate.

![broadcast-x](https://user-images.githubusercontent.com/38033465/53714872-64934b00-3e93-11e9-8891-324a07810bec.jpg)

* Sends a message to one randomly selected node among all nodes with a z-axis coordinate of 1 ( In this case, since the sender is included, select the sender )
* Sends a message to one randomly selected node among all nodes with a z-axis coordiate of 2
* Sends a message to one randomly selected node among all nodes with a z-axis coordiate of 3
* The node received this message relays the message on the Y axis in the same ways as the [Y Axis message transmission](#y-axis-message-transmission) below

## Y-Axis message transmission
If the size of the layout's y-axis is greater than 1, the entire node is divided by the y coordinates as show below, and the message is relayed to each coordinate. When relaying messages received via Z-Axis message transmission to the Y-Axis, the range of the entire node is limited to the corresponding z-axis coordinates

![broadcast-y](https://user-images.githubusercontent.com/38033465/53714875-665d0e80-3e93-11e9-8dda-a0cdd647b942.jpg)

* Sends a message to one randomly selected node among all nodes with a y-axis coordinates of 1
* Sends a message to one randomly selected node among all nodes with a y-axis coordinates of 2 ( In this case, since the sender is included, select the sender )
* Sends a message to one randomly selected node among all nodes with a y-axis coordinates of 3
* The node received this message relays the message on the X axis in the same ways as the [X Axis message transmission](#x-axis-message-transmission) below

## X-Axis message transmission
If the size of the layout's x-axis is greater than 1, the entire node is divided by the x coordinates shown below, and the message is relayed to each coordinate. When relaying message received via Y-Axis message transmission to the X-Axis, the range of the entire node is limited to the corresponding z-axis coordinates and y-axis coordinates

![broadcast-z](https://user-images.githubusercontent.com/38033465/53714883-6826d200-3e93-11e9-8ecb-8507a9a81af4.jpg)

* Sends a message to one randomly selected node among all nodes with a x-axis coordinates of 1
* Sends a message to one randonly selected node among all nodes with a x-axis coordinates of 2 ( In this case, since the sender in included, select the sender )
* Sends a message to one randonly selected node among all nodes with a x-axis coordinates of 3
* The node that receives this message sends a message to all nodes that belong to that coordinate. This process completes the message broadcasting to the entire network

## Coverage
* A node can broadcast to `N ^ 4` nodes with `N * 4` send call.  
* A node can broadcast to `4,096` nodes with `32` send call ( if `N = 8` )  
* A node can broadcast to `65,536` nodes with `64` send call ( if `N = 16` )  
* A node can broadcast to `1,048,576` nodes with `128` send call ( if `N = 32` )  
* And also can be extended to 4D-Coordiantes system( x, y, z, w )  
A node can broadcast to `N ^ 5` nodes with `N * 5` send call on 4D-Coordinates system.  

# Demo
* Prebuilt binary for windows10-x64 present in `Demo/win-x64/` Folder
* `PeerListServiceApp.exe`: Peer List sync service ( means Peer Discovery service )
* `ElasticNodeServiceApp.exe`: P2P Node Service ( will be forked as `nPeers` )
* run `demo.bat` in `Demo/win-x64/` 
( [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) must be installed, and the folder(`C:\Program Files\dotnet\`) where `dotnet.exe` exists must be registered in **PATH** )
* type `test.start()` in ElasticNodeServiceApp.exe console
* default environment variables : `n=8`, `nPeers=32`, `msgKBytes=128`, `nTimes=10`
* you can change environment variables with `set(n,16)`, `set(nPeers,64)`, `set(msgKBytes,256)`, `set(nTimes,20)`, ... in console

![elasticnodeserviceapp_demo_screenshot](https://user-images.githubusercontent.com/39185929/57013410-60db3500-6c46-11e9-9ccb-986419a5c1b1.png)

# Test Reports

![TEST_RESULT](https://user-images.githubusercontent.com/39185929/57013490-b7e10a00-6c46-11e9-9092-40b5842125c4.png)

**TEST Environments**

* Windows 10 x64, .NET Core 2.2
* Intel Core i7-8700K 3.70GHz, 6 core, 12 logical processor
* 64GB DDR4

