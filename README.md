# Elastic Grid Network?
*Elastic Grid Network* is a P2P grid networking solution for the *Bryllite-Platform*.  

On the *Bryllite-Platform*, all nodes with mining authority must communicate at the same time to reach an agreement for block’s agreement. Because all mining nodes participate in the agreement, the blocks connected to the chain become irreversible, which will solve the *double-spending problem* and allow immediate transaction settlement without waiting for the *confirm time*.  

However, the participation of all mining nodes in an agreement means that as the number of nodes participating in the network grows, the burden of network transmission increases and there is a danger that the agreement can not be reached within the time limit. For Example, assume that there are about 10,000 mining nodes participating in the Bryllite network, and that the size of the block to be verified in the BCP agreement step is 1MB. In this case, the data that the node requesting the block verification needs to transfer is about 10 GB (1 MB * 10,000), and it is difficult to reach a consensus by transmitting and verifying the block to all the nodes within the BCP time limit.  

To solve these problems, *Elastic Grid Network Solution* was designed to be suitable for *Bryllite-Platform*. This solution is an efficient grid network technology in which the layout of grid is flexibly applied according to the size of mining nodes participating in the network.


# Notice
This is not real code of Bryllite-Platform.  
This project is Elastic Grid Network Concept code which explaining how it works.  

# Environment
Visual Studio 2017  
.NET Core 2.2

# Elastic Grid Network Concept
This chapter describes how Elastic Grid Network works.  

## Layout Decision based on Node count

> N: Node count in a Grid cell ( predefined, eg. 16 )  
> nPeers: Node count in whole network

![layout_decision_1](https://user-images.githubusercontent.com/38033465/53714769-f64e8880-3e92-11e9-85b8-eed71251081f.jpg)
![layout_decision_2](https://user-images.githubusercontent.com/38033465/53714772-f8184c00-3e92-11e9-9cea-21d680406164.jpg)


## Computing Coordinates of a node

`ElasticLayout layout = ElasticLayout.DefineLayout( nPeers )`

![hash256 nodeaddress](https://user-images.githubusercontent.com/38033465/53714789-11b99380-3e93-11e9-9a5f-aeb94da44145.jpg)

```
x-coordinates = 1 + ( hx % layout.X )
y-coordinates = 1 + ( hy % layout.Y )
z-coordinates = 1 + ( hz % layout.z )
```

`ElasticLayout.DefineLayout( nPeers ).ComputeNodeCoordinates( NodeAddress ) = ( x, y, z )`

## How broadcasting works

![broadcast_1](https://user-images.githubusercontent.com/38033465/53714872-64934b00-3e93-11e9-8891-324a07810bec.jpg)
![broadcast_2](https://user-images.githubusercontent.com/38033465/53714875-665d0e80-3e93-11e9-8dda-a0cdd647b942.jpg)
![broadcast_3](https://user-images.githubusercontent.com/38033465/53714883-6826d200-3e93-11e9-8ecb-8507a9a81af4.jpg)

* `Sending to (x, y, z)` means sending message to one node in (x,y,z) randomly selected.  
If you send message to coordinates (0, 0, 1), Randomly selects one of all the nodes having the Z-Axis Coordinate of 1, and send message to the corresponding node.
* A node who received incomplete coordinates message, relay the message to randomly selected node in a slice( me if slice contains me )   
* A node who received complete coordinates message, relay the message to all node in a cell  

## Coverage

* A node can broadcast to N^4 nodes with 4N send call.  
* A node can broadcast to 4,096 nodes with 32 send call ( if N = 8 )  
* A node can broadcast to 65,536 nodes with 64 send call ( if N = 16 )  
* A node can broadcast to 1,048,576 nodes with 128 send call ( if N = 32 )  

* And also can be extended to 4D-Coordiantes system( x, y, z, w )  
A node can broadcast to N^5 nodes with 5N send call on 4D-Coordinates system.  

# Demo
* Prebuilt binary for windows10-x64 present in `Demo/win-x64/` Folder
* PeerListServiceApp.exe : Peer List sync service ( means Peer Discovery service )
* ElasticNodeServiceApp.exe: P2P Node Service ( will be forked as nPeers )
* run `demo.bat` in `Demo/win-x64/` ( [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) Installed & `C:\Program Files\dotnet\` in PATH )
* type `test.start()` in ElasticNodeServiceApp.exe console
* default environment variables : `n=8`, `nPeers=32`, `msgKBytes=128`, `nTimes=10`
* you can change environment variables with `set(n,16)`, `set(nPeers,128)`, ... in console

![Demo](https://user-images.githubusercontent.com/39185929/56866588-ddd39800-6a15-11e9-86ff-32866e2f5965.png)

# Test Result

![TEST RESULT](http://drive.google.com/uc?export=view&id=1MhvwBksWWMW7Cwlf2QUpyl9I1r3EnEWw)

* TEST Environment
> Windows 10 x64, .NET Core 2.2
> Intel Core i7-8700K 3.70GHz, 6 core, 12 logical processor
> 64GB DDR4

