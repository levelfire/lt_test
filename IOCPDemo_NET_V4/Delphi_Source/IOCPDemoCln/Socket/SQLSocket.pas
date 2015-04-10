unit SQLSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TSQLSocket = class(TThreadClientSocket)
  private
    {* �û��� *}
    FUser, FPassword: string;
  protected
    {* ��������Ƿ���� *}
    function CheckActive: Boolean;
    {* ���� *}
    function ReConnect: Boolean;
    {* �����͵�¼ *}
    function ReConnectAndLogin: Boolean;
    {* ���ش������� *}
    function GetErrorCodeString(const AErrorCode: Cardinal): string; override;
  public
    constructor Create; override;
    destructor Destroy; override;
    {* ���ӷ����� *}
    function Connect(const ASvr: string; APort: Integer): Boolean;
    {* �Ͽ����� *}
    procedure Disconnect;
    {* ��½ *}
    function Login(const AUser, APassword: string): Boolean;
    {* ��ѯSQL��� *}
    function SQLOpen(const ASQL: string; AStream: TStream): Boolean;
    {* ִ��SQL��� *}
    function SQLExec(const ASQL: string; var AEffectRow: Integer): Boolean;
    {* ��ʼ���� *}
    function BeginTrans: Boolean;
    {* �ύ���� *}
    function CommitTrans: Boolean;
    {* �ع����� *}
    function RollbackTrans: Boolean;
    property User: string read FUser write FUser;
    property Password: string read FPassword write FPassword;
  end;

implementation

{ TSQLSocket }

constructor TSQLSocket.Create;
begin
  inherited;
  FTimeOutMS := 60 * 1000; //��ʱ����Ϊ60S
end;

destructor TSQLSocket.Destroy;
begin
  inherited;
end;

function TSQLSocket.GetErrorCodeString(const AErrorCode: Cardinal): string;
begin
  case AErrorCode of
    CISQLOpenError: Result := 'SQL Open Error';
    CISQLExecError: Result := 'SQL Exec Error';
    CIHavedBeginTrans: Result := 'Haved Begin Trans';
    CIBeginTransError: Result := 'Begin Trans Error';
    CINotExistTrans: Result := 'No Begin Trans';
    CICommitTransError: Result := 'Commit Trans Error';
    CIRollbackTransError: Result := 'Rollback Trans Error';
  else
    Result := inherited GetErrorCodeString(AErrorCode);
  end;
end;

function TSQLSocket.CheckActive: Boolean;
var
  slRequest: TStringList;
begin
  slRequest := TStringList.Create;
  try
    try
      Result := ControlCommand(CSSQLCmd[scActive], nil, nil, '');
    except
      Result := False;
    end;
  finally
    slRequest.Free;
  end;
end;

function TSQLSocket.ReConnect: Boolean;
begin
  FTimeOutMS := CI_ReadTimeOut;
  if FClient.Connected and (not CheckActive) then
    Disconnect; //�Ͽ�����������
  if not FClient.Connected then
  begin
    Result := Connect(FHost, FPort);
  end
  else
    Result := True;
end;

function TSQLSocket.ReConnectAndLogin: Boolean;
begin
  FTimeOutMS := CI_ReadTimeOut;
  if FClient.Connected and (not CheckActive) then
    Disconnect; //�Ͽ�����������
  if not FClient.Connected then
  begin 
    Result := Connect(FHost, FPort);
    if Result then
      Result := Login(FUser, FPassword);
  end
  else
    Result := True;
end;

function TSQLSocket.Connect(const ASvr: string; APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(#1);
end;

procedure TSQLSocket.Disconnect;
begin
  inherited Disconnect;
end;

function TSQLSocket.Login(const AUser, APassword: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnect;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSUserName, AUser]));
    slRequest.Add(Format(CSFmtString, [CSPassword, MD5String(APassword)]));
    Result := ControlCommand(CSSQLCmd[scLogin], slRequest, nil, '');
    if Result then
    begin
      FUser := AUser;
      FPassword := APassword;
    end;
  finally
    slRequest.Free;
  end;
end;

function TSQLSocket.SQLOpen(const ASQL: string; AStream: TStream): Boolean;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  Result := RequestDataThread(CSSQLCmd[scSQLOpen], nil, ASQL, AStream);
end;

function TSQLSocket.SQLExec(const ASQL: string; var AEffectRow: Integer): Boolean;
var
  slResponse: TStringList;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slResponse := TStringList.Create;
  try
    Result := ControlCommandThread(CSSQLCmd[scSQLExec], nil, slResponse, ASQL);
    if Result then
      AEffectRow := StrToInt(slResponse.Values[CSEffectRow]);
  finally
    slResponse.Free;
  end;
end;

function TSQLSocket.BeginTrans: Boolean;
var
  slResponse: TStringList;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slResponse := TStringList.Create;
  try
    Result := ControlCommandThread(CSSQLCmd[scBeginTrans], nil, slResponse, '');
  finally
    slResponse.Free;
  end;
end;

function TSQLSocket.CommitTrans: Boolean;
var
  slResponse: TStringList;
begin
  slResponse := TStringList.Create;
  try
    Result := ControlCommandThread(CSSQLCmd[scCommitTrans], nil, slResponse, '');
  finally
    slResponse.Free;
  end;
end;

function TSQLSocket.RollbackTrans: Boolean;
var
  slResponse: TStringList;
begin
  slResponse := TStringList.Create;
  try
    Result := ControlCommandThread(CSSQLCmd[scRollbackTrans], nil, slResponse, '');
  finally
    slResponse.Free;
  end;
end;

end.
