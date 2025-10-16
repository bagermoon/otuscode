@rem echo off
@set PGSQL_HOME=C:\Users\nrash\work\apps\pgsql.17.6
@set DBFOLDERNAME=restaurantsdb
@set DBFOLDERPATH=%~dp0\..\%DBFOLDERNAME%
@set DATABASE=Restaurants
@set SCHEMA=main
@set LOGFILENAME=%DBFOLDERNAME%
@set mode=%~1

