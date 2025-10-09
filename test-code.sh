#!/bin/bash

# permet de savoir les erreurs dans tous les scripts pour dédug
cd JeuxOlympiques
dotnet restore
dotnet build
dotnet publish -c Release -o ../out
