echo ------------------------ LocBaml to output ------------------------
copy ..\..\..\..\Setup\LocBaml.exe ..\..\..\..\Output\Release\LocBaml.exe

echo ------------------------ Plugin contents ------------------------
copy ..\..\..\..\Output\Release\Plugins\AutoClick.dll ..\..\..\..\Output\Release\AutoClick.dll
mkdir ..\..\..\..\Output\Release\en-US
copy ..\..\..\..\Output\Release\Plugins\en-US\AutoClick.resources.dll ..\..\..\..\..\Output\Release\en-US\AutoClick.resources.dll

echo ------------------------ generate with LocBaml ------------------------
cd ..\..\..\..\Output\Release\
mkdir Plugins\fr-FR
LocBaml /generate Plugins\en-US\AutoClick.resources.dll /trans:..\..\Plugins\Accessibility\AutoClick\Locale\fr-FR.txt /cult:fr-FR /out:Plugins\fr-FR

echo ------------------------ clean ------------------------
del en-US\AutoClick.resources.dll
del LocBaml.exe
del AutoClick.dll
pause