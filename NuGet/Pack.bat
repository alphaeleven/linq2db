cd ..\Source
call Compile.bat

cd ..\NuGet

del *.nupkg

NuGet Pack linq2db.nuspec
NuGet Pack linq2db.SqlServer.nuspec
rem rename linq2db.*.nupkg linq2db.nupkg
