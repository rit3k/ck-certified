echo ------------------------ LocBaml to output ------------------------
copy ..\..\Setup\LocBaml.exe ..\..\Output\Release\LocBaml.exe

echo ------------------------ Create fr-FR ------------------------
cd ..\..\Output\Release\
mkdir fr-FR

LocBaml /generate en-US\Host.Services.resources.dll /trans:..\..\Host.Services\Locale\fr-FR.txt /cult:fr-FR /out:fr-FR

pause