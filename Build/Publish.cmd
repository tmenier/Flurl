@echo off

set workDir=%1
set publishSpec=%2
set publishDir=%3

cd %workDir%
if not exist "%publishDir%" mkdir "%publishDir%"
call NuGet.exe pack %publishSpec% -OutputDirectory %publishDir%