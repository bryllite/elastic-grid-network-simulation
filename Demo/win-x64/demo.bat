@echo off
cd PeerListServiceApp
start cmd /k "PeerListServiceApp.exe"
timeout 1
cd ../ElasticNodeServiceApp
start cmd /k "ElasticNodeServiceApp.exe --console"
