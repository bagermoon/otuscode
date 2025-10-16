@rem echo off
@call env.bat

"%PGSQL_HOME%\bin\pg_ctl" stop -D "%DBFOLDERPATH%"
