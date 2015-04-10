program IOCPDemoSvr;
{* |<PRE>
================================================================================
* ������ƣ�IOCPDemoSvr��IOCPDemoSvr.dpr
* ��Ԫ���ƣ�IOCPDemoSvr.dpr
* ��Ԫ���ߣ�SQLDebug_Fan <fansheng_hx@163.com>  
* ��    ע��- �����ṩ��Ԫ���ṩ���ܳ������з�ʽ
*           - ˫��ֱ�Ӵ������淽ʽ���У�����ʽ��Ҫע�����
*           - ע�������������[�������·��] -install
*           - ��ע�������������[�������·��] -uninstall
*           - �������б����-svc����
*           - ���Ʒ�����������ֻ������һ��
* ����ƽ̨��Windows XP + Delphi 7
* ���ݲ��ԣ�Windows XP, Delphi 7
* �� �� �����õ�Ԫ�е��ַ��������ϱ��ػ�����ʽ
--------------------------------------------------------------------------------
* ���¼�de ¼��-
*           -
================================================================================
|</PRE>}

uses
  ShareMem,
  Forms,
  Windows,
  SvcMgr,
  SysUtils,
  MainForm in 'Form\MainForm.pas' {FmMain},
  ServiceForm in 'Form\ServiceForm.pas' {IOCPDemoSvc: TService},
  DispatchCenter in 'Form\DispatchCenter.pas' {DMDispatchCenter: TDataModule},
  OptionSet in 'Unit\OptionSet.pas',
  ConfigForm in 'Form\ConfigForm.pas' {FmConfig},
  DefineUnit in 'Unit\DefineUnit.pas',
  ADOConPool in 'Unit\ADOConPool.pas',
  DBConnect in 'Unit\DBConnect.pas',
  BaseSocket in 'Socket\BaseSocket.pas',
  SQLSocket in 'Socket\SQLSocket.pas',
  Logger in 'Unit\Logger.pas',
  LogSocket in 'Socket\LogSocket.pas',
  ControlSocket in 'Socket\ControlSocket.pas',
  UploadSocket in 'Socket\UploadSocket.pas',
  DownloadSocket in 'Socket\DownloadSocket.pas',
  BasisFunction in 'Unit\BasisFunction.pas',
  RemoteStreamSocket in 'Socket\RemoteStreamSocket.pas';

{$R *.res}
{$R ..\WindowsXP_UAC.res}

const
  CSMutexName = 'Global\IOCPDemoSvr_Mutex';
var
  OneInstanceMutex: THandle;
  SecMem: SECURITY_ATTRIBUTES;
  aSD: SECURITY_DESCRIPTOR;
begin
  InitializeSecurityDescriptor(@aSD, SECURITY_DESCRIPTOR_REVISION);
  SetSecurityDescriptorDacl(@aSD, True, nil, False);
  SecMem.nLength := SizeOf(SECURITY_ATTRIBUTES);
  SecMem.lpSecurityDescriptor := @aSD;
  SecMem.bInheritHandle := False;
  OneInstanceMutex := CreateMutex(@SecMem, False, CSMutexName);
  if (GetLastError = ERROR_ALREADY_EXISTS)then
  begin
    DlgError('Error, IOCPDemo program or service already running!');
    CloseHandle(OneInstanceMutex);
    Exit;
  end;
  InitLogger(16 * 1024 * 1024, 'Log');
  InitOptionSet;
  GDBConnect := TDBConnection.Create(GIniOptions.DBServer, GIniOptions.DBName,
    GIniOptions.DBLoginName, GIniOptions.DBPassword, GIniOptions.DBAuthentication,
    GIniOptions.DBPort);
  try
    //����ǰ�װ��ж�ػ������з��������������߳�
    if FindCmdLineSwitch('svc', True) or
      FindCmdLineSwitch('install', True) or
      FindCmdLineSwitch('uninstall', True) then
    begin
      SvcMgr.Application.Initialize;
      SvcMgr.Application.Title := 'IOCPDemo Server';
      SvcMgr.Application.CreateForm(TIOCPDemoSvc, IOCPDemoSvc);
  SvcMgr.Application.CreateForm(TDMDispatchCenter, DMDispatchCenter);
      SvcMgr.Application.Run;
    end
    else
    begin
      Forms.Application.Initialize;
      Forms.Application.Title := 'IOCPDemo Server';
      Forms.Application.CreateForm(TFmMain, FmMain);
      Forms.Application.CreateForm(TDMDispatchCenter, DMDispatchCenter);
      Forms.Application.Run;
    end;
    if OneInstanceMutex <> 0 then
      CloseHandle(OneInstanceMutex);
  finally
    UnInitLogger;
    UnInitOptionSet;
    GDBConnect.Free;
  end;
end.
