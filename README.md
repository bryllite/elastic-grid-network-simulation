# Elastic Grid Network Simulation
Elastic Grid Network Simulation code  
Elastic Grid Network system is used on Bryllite-Platform when there are a large number of node lists  

# Notice
This is not real code of Bryllite-Platform.  
This project is Elastic Grid Network Concept code which explaining how it works.  

# Environment
Visual Studio 2017  
.netframework 4.6.1  

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

A node who received incomplete coordinates message, relay the message to randomly selected node in a slice( me if slice contains me )   
A node who received complete coordinates message, relay the message to all node in a cell  



# Demo
run `demo.bat` in Bin path  
wait for a while until all nodes are connected and synchronized.  
type `run 128 10` in TrackerServiceApp Console ( 128k data 10 times broadcasting )
