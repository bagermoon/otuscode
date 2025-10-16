@rem echo off
@call env.bat

@pushd ..
@if not exist "%DBFOLDERPATH%\" (
@%PGSQL_HOME%\bin\initdb -U postgres -E utf8 -W -D "%DBFOLDERPATH%"
@mkdir "%DBFOLDERPATH%\tblspcs\restaurants"
@"%PGSQL_HOME%\bin\pg_ctl" start -D "%DBFOLDERPATH%" -l %LOGFILENAME%.log
@echo CREATE ROLE master WITH LOGIN NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION; ^
ALTER ROLE master PASSWORD 'master'; ^
CREATE TABLESPACE restaurants_tbs OWNER master LOCATION '%DBFOLDERPATH%\tblspcs\restaurants'; ^
ALTER TABLESPACE restaurants_tbs OWNER TO master; ^
CREATE DATABASE "%DATABASE%" WITH OWNER = master ENCODING = 'UTF8' TABLESPACE = restaurants_tbs CONNECTION LIMIT = -1;| %PGSQL_HOME%\bin\psql.exe -h localhost -U postgres -d postgres
@echo CREATE SCHEMA IF NOT EXISTS %SCHEMA% AUTHORIZATION master;| %PGSQL_HOME%\bin\psql.exe postgresql://master:master@localhost:5432/%DATABASE%
)
@popd

