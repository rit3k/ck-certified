; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{FD6D19BE-2313-4A37-AE2D-FB600FBE4B8C}}
AppName=CiviKey
AppVersion=1.0.0
AppPublisher=Invenietis
AppPublisherURL=http://www.invenietis.com/
DefaultDirName={pf}\DevCiviKey
DefaultGroupName=DevCiviKey
AllowNoIcons=yes
OutputDir=.\
OutputBaseFilename=Dev-CiviKey-Installer-1.0.0
Compression=lzma
SolidCompression=yes
SetupIconFile=Resources\CiviKey.ico
VersionInfoVersion=1.0.0
VersionInfoProductName=DevCiviKey
                               
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: ".\*"; DestDir: "{app}"; Excludes: "*.config, *.pdb, *.xml, *.ck, *vshost.exe*, *.manifest, *.iss, \Runtime, \PostInstallScript.exe, \DevCiviKeyPostInstallScript"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\..\Output\Debug\*"; DestDir: "{app}\DevCiviKey\Runtime\Current"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: ".\Resources\CK-Certified.exe.config"; DestDir: "{app}\DevCiviKey\Runtime\Current"
Source: "DevCiviKeyPostInstallScript.exe"; DestDir: "{tmp}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
           
[Run]
Filename: "{tmp}\DevCiviKeyPostInstallScript.exe"; Parameters: """True"" ""{app}\DevCiviKey"" ""{app}\DevCiviKey\Configurations"" ""{app}\DevCiviKey\Runtime\Current\CK-Certified.exe"""           
                                                                                                                                                                                                                                             ;Application directory                          ;.exe path
[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, serviceCount: cardinal;
    success: boolean;
begin
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;
    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;
    // .NET 4.0 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;
    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4\Client', 0) then begin
        MsgBox('CiviKey requires Microsoft .NET Framework 4.0.'#13#13
            'Please use Windows Update to install this version,'#13
            'and then re-run the CiviKey setup program.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;


