@rem echo off
@call env.bat

pushd ..
"%PGSQL_HOME%\bin\pg_ctl" start -D "%DBFOLDERPATH%" -l %LOGFILENAME%.log
popd
