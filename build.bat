@echo off
setlocal EnableDelayedExpansion 

:: Requirements
if not "%msbuild%" == "" goto :skip_msbuild
if not exist "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" (
    echo ERROR: Microsoft.NET framework 3.5 not found. Make sure you have Visual Studio installed.
    goto _exit
) 
set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"

:skip_msbuild

if not "%sevenz%" == "" goto :skip_7z

set sevenz=7z
if exist "C:\Program Files\7-Zip\7z.exe" (
    set sevenz="C:\Program Files\7-Zip\7z.exe"
    goto prep
)

if exist "C:\Program Files(x86)\7-Zip\7z.exe" (
    set sevenz="C:\Program Files (x86)\7-Zip\7z.exe"
    goto prep
)  

where 7z >nul 2>nul
if not %errorlevel%==0 (
    echo ERROR: 7zip was not found. Make sure that you have it installed and in the PATH.
    goto _exit
)

:skip_7z

:prep
set target=Debug
set target_unsigned=Debug
if %1.==release. goto _if_setrelease
goto _ifj_setrelease
:_if_setrelease
set target=Release
set target_unsigned=Release-Unsigned
:_ifj_setrelease

:: Prepare the build directory
set build_base=build
set build_mtg=SEMI-LOADER
set "build=%build_base%\%build_mtg%"
set "build_zip=%build_base%\%build_mtg%.zip"

if exist "%build_base%" rmdir /q /s "%build_base%"
mkdir "%build_base%" 2>nul
mkdir "%build%" 2>nul

:: Build
where xbuild >nul 2>nul
if %errorlevel%==0 (
  ::call xbuild || goto :error
  rem
) else (
  call %msbuild% || goto :error
)

for /f "tokens=*" %%L in (build-files) do (
  set "line=%%L"
  setlocal enabledelayedexpansion
  echo !line!
  set str=!line:{TARGET}=%target%!
  echo !str!
  set "line=!line:/=\!"
  if not "!line:~0,1!"=="#" (
    set "file_ex=!line:{TARGET}=%target%!"
    set "file=!file_ex:{TARGET-UNSIGNED}=%target_unsigned%!"
    echo Copying '!file!' to '%build%/!target!'

    for %%i in (!file!) do (
      if exist %%~si\nul (
         robocopy "!file!" "%build%" /s /e
      ) else (
        copy "!file!" "%build%"
      )
    )
  )
  endlocal
)

:: Zipping it all up
pushd "%build%"
%sevenz% a SEMI-LOADER.zip * || goto :error
popd
move "%build%\SEMI-LOADER.zip" "%build_zip%" || goto :error

goto _exit

:: Error
:_error
echo "Error - terminating script."
exit /b 1

:: The End
:_exit
