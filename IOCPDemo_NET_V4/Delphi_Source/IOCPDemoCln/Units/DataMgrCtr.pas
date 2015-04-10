unit DataMgrCtr;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages, ControlSocket,
  LogSocket, SQLSocket, UploadSocket, DownloadSocket;

type
  TDataMgrCtr = class(TObject)
  private
    {* �������Ͷ˿� *}
    FHost: string;
    FPort: Integer;
    {* �û��� *}
    FUser, FPassword: string;
    {* Socket�����������ʱ������ֹͣʱ���� *}
    FSQLSocket: TSQLSocket;
    FLogSocket: TLogSocket;
    FControlSocket: TControlScoket;
    FUploadSocket: TUploadSocket;
    FDownloadSocket: TDownloadSocket;
    {* ֪ͨ��� *}
    FNotifyHandle: THandle;
    {* ��ʱ�ļ���·�� *}
    FTmpPath: string;
  public
    constructor Create; virtual;
    destructor Destroy; override;
    {* ���ӷ����� *}
    function Connect(const ASvr: string; APort: Integer): Boolean;
    {* ��½ *}
    function Login(const AUser, APassword: string): Boolean;
    {* ���� *}
    procedure Start;
    {* ֹͣ *}
    procedure Stop;
    {* ��ȡ��ʱ�ļ� *}
    function GetTmpFile(const AExt: string): string;

    property Host: string read FHost;
    property Port: Integer read FPort;
    property User: string read FUser;
    property Password: string read FPassword;
    property SQLSocket: TSQLSocket read FSQLSocket;
    property LogSocket: TLogSocket read FLogSocket;
    property ControlSocket: TControlScoket read FControlSocket;
    property UploadSocket: TUploadSocket read FUploadSocket;
    property DownloadSocket: TDownloadSocket read FDownloadSocket;
    property NotifyHandle: THandle read FNotifyHandle write FNotifyHandle;
  end;

var
  GDataMgrCtr: TDataMgrCtr;

implementation

{ TDataMgrCtr }

constructor TDataMgrCtr.Create;
begin
  FSQLSocket := TSQLSocket.Create;
  FLogSocket := TLogSocket.Create;
  FControlSocket := TControlScoket.Create;
  FUploadSocket := TUploadSocket.Create;
  FDownloadSocket := TDownloadSocket.Create;
end;

destructor TDataMgrCtr.Destroy;
begin
  FSQLSocket.Free;
  FLogSocket.Free;
  FControlSocket.Free;
  FUploadSocket.Free;
  FDownloadSocket.Free;
  inherited;
end;

function TDataMgrCtr.Connect(const ASvr: string; APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := FControlSocket.Connect(FHost, FPort) and FLogSocket.Connect(FHost, FPort);
  if Result then
  begin
    FSQLSocket.Host := FHost;
    FSQLSocket.Port := FPort;
    FUploadSocket.Host := FHost;
    FUploadSocket.Port := FPort;
    FDownloadSocket.Host := FHost;
    FDownloadSocket.Port := FPort;
  end;
end;

function TDataMgrCtr.Login(const AUser, APassword: string): Boolean;
begin
  FUser := AUser;
  FPassword := APassword;
  Result := FControlSocket.Login(AUser, APassword);
  if Result then
  begin
    FSQLSocket.User := AUser;
    FSQLSocket.Password := APassword;
    FUploadSocket.User := AUser;
    FUploadSocket.Password := APassword;
    FDownloadSocket.User := AUser;
    FDownloadSocket.Password := APassword;
  end;
end;

procedure TDataMgrCtr.Start;
begin
  FTmpPath := AppPath + 'TmpFile' + PathDelim;
  if not DirectoryExists(FTmpPath) then
    CreateDir(FTmpPath)
  else
    ClearDirectory(FTmpPath, True, False);
  FLogSocket.Start;
  FControlSocket.Start;
end;

procedure TDataMgrCtr.Stop;
begin
  FControlSocket.Stop;
  FreeAndNil(FControlSocket);
  FLogSocket.Stop;
  FreeAndNil(FLogSocket);
  FSQLSocket.Disconnect;
  FreeAndNilEx(FSQLSocket);
  FUploadSocket.Disconnect;
  FreeAndNilEx(FUploadSocket);
  FDownloadSocket.Disconnect;
  FreeAndNilEx(FDownloadSocket);

  FControlSocket := TControlScoket.Create;
  FLogSocket := TLogSocket.Create;
  FSQLSocket := TSQLSocket.Create;
  FUploadSocket := TUploadSocket.Create;
  FDownloadSocket := TDownloadSocket.Create;
end;

function TDataMgrCtr.GetTmpFile(const AExt: string): string;
begin
  Result := FTmpPath + GetGUIDString + AExt;
end;

end.
