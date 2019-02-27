@echo off
start cmd /k "TrackerServiceApp.exe"
timeout 5
start cmd /k "NodeServerApp 127.0.0.1 19000:19063"
