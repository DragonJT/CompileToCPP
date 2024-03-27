@echo off
if not defined vcvars call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x64
set vcvars=true
if not exist cpp mkdir cpp
dotnet run
cl cpp\main.cpp /Focpp\main /Fecpp\main
cpp\main.exe