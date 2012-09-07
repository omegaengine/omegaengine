@echo off
::Outputs the current version number for TeamCity

echo ##teamcity[buildNumber '0.8.0.{build.number}']