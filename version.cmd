@echo off
::Sets the current version number as an environment variable

set /p version= < "%~dp0version"

echo ##teamcity[buildNumber '%version%_{build.number}']
echo ##teamcity[setParameter name='build.version' value='%version%']
echo ##teamcity[publishArtifacts 'version']
