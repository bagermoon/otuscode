@rem echo off
@chcp 65001
@set PGSQL_HOME=C:\Users\nrash\work\apps\pgsql.17.6
@set DATABASE=Restaurants

"%PGSQL_HOME%\bin\psql" -q -a -v AUTOCOMMIT=OFF -f %1  postgresql://master:master@localhost:5432/%DATABASE%
