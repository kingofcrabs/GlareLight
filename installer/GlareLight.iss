#define use_dotnetfx40

#define MyAppSetupName 'GlareCalculator'
#define MyAppVersion '0.05'

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright= Copyright@InkAuto 2017
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany= 0.06
AppPublisher= InkAutomation
;AppPublisherURL=http://...
;AppSupportURL=http://...
;AppUpdatesURL=http://...
DefaultDirName={sd}\ProgramData\{#MyAppSetupName}
OutputBaseFilename={#MyAppSetupName}-{#MyAppVersion}
DefaultGroupName={#MyAppSetupName}
UninstallDisplayIcon={app}\MyProgram.exe
OutputDir=bin
SourceDir=.
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes

;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
;MinVersion=0,5.0
PrivilegesRequired=admin
ArchitecturesAllowed =x64
;ArchitecturesInstallIn64BitMode=x64 ia64

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
DestDir: {app}\; Source: src\*;    Flags: recursesubdirs         ; 
[Icons]
Name: "{group}\GlareCalculator"; Filename: "{app}\bins\GlareCalculator"
Name: "{group}\{cm:UninstallProgram,MyProgram}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\GlareCalculator"; Filename: "{app}\bins\GlareCalculator"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\GlareCalculator"; Filename: "{app}\bins\GlareCalculator.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\bins\GlareCalculator.exe"; Description: "{cm:LaunchProgram,GlareCalculator}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKLM; Subkey: Software\GlareCalculator; ValueType: string; ValueName: InstallPath; ValueData: {app}
#include "scripts\products.iss"

#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif


#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#ifdef use_dotnetfx11lp
#include "scripts\products\dotnetfx11lp.iss"
#endif
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
//#include "scripts\products\dotnetfx35.iss"
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
//#include "scripts\products\dotnetfx35lp.iss"
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif

#ifdef use_wic
#include "scripts\products\wic.iss"
#endif

#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif

#ifdef use_sqlcompact35sp2
#include "scripts\products\sqlcompact35sp2.iss"
#endif

#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2


[Code]
var dotnetRedistPath: string;
 ResultCode : Integer;
 dotNetV4PackFile:string;  

function InitializeSetup(): boolean;
begin
  dotNetV4PackFile:='{src}\dotNetFx40_Full_x86_x64.exe';

  dotnetRedistPath := ExpandConstant(dotNetV4PackFile);  

  //MsgBox(dotnetRedistPath, mbInformation, MB_OK);
  
	//init windows version
	initwinversion();

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	//check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	//dotnetfx35();
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	//dotnetfx35lp();
	dotnetfx35sp1lp();
#endif
#endif
   	
    
    Result := true;
	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40
     
	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then begin
       
       Exec(dotnetRedistPath, '', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);  
       if(not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then begin
          dotnetfx40Full();
       end;
  end;
#endif



end;
