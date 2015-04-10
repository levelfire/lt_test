unit ControlSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TOnClients = procedure(AItems: TStrings) of object;
  TControlThread = class;
  TControlScoket = class(TBaseClientSocket)
  private
    {* �û��� *}
    FUser, FPassword: string;
    {* �����߳� *}
    FControlThread: TControlThread;
    {* �����յ�֪ͨ�¼� *}
    FOnClients: TOnClients;
  protected
    {* ��������Ƿ���� *}
    function CheckActive: Boolean;
    {* ���� *}
    function ReConnect: Boolean;
    {* �����͵�¼ *}
    function ReConnectAndLogin: Boolean;
  public
    constructor Create; override;
    destructor Destroy; override;
    {* ���ӷ����� *}
    function Connect(const ASvr: string; APort: Integer): Boolean;
    {* �Ͽ����� *}
    procedure Disconnect;
    {* ��½ *}
    function Login(const AUser, APassword: string): Boolean;
    {* ��ȡ�ͻ����б� *}
    function GetClients(AItems: TStrings): Boolean;
    {* ���� *}
    procedure Start;
    {* ֹͣ *}
    procedure Stop;
    property User: string read FUser;
    property OnClients: TOnClients read FOnClients write FOnClients;
  end;

  TControlThread = class(TThread)
  private
    FControlScoket: TControlScoket;
    {* �����б�洢 *}
    FClients: TStringList;
    {* ���������յ�֪ͨ�¼� *}
    procedure DoClients;
  protected
    procedure Execute; override;
  end;

implementation

uses IdTCPConnection, DataMgrCtr, ClientDefineUnit, Logger;

{ TStatImportScoket }

constructor TControlScoket.Create;
begin
  inherited;
  FTimeOutMS := 60 * 1000; //��ʱ����Ϊ60S
end;

destructor TControlScoket.Destroy;
begin
  inherited;
end;

function TControlScoket.Connect(const ASvr: string;
  APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(#8);
end;

procedure TControlScoket.Disconnect;
begin
  inherited Disconnect;
end;

function TControlScoket.Login(const AUser, APassword: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnect;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSUserName, AUser]));
    slRequest.Add(Format(CSFmtString, [CSPassword, MD5String(APassword)]));
    Result := ControlCommand(CSControlCmd[ccLogin], slRequest, nil, '');
    if Result then
    begin
      FUser := AUser;
      FPassword := APassword;
    end;
  finally
    slRequest.Free;
  end;
end;

function TControlScoket.CheckActive: Boolean;
var
  slRequest: TStringList;
begin
  slRequest := TStringList.Create;
  try
    try
      Result := ControlCommand(CSControlCmd[ccActive], nil, nil, '');
    except
      Result := False;
    end;
  finally
    slRequest.Free;
  end;
end;

function TControlScoket.ReConnect: Boolean;
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

function TControlScoket.ReConnectAndLogin: Boolean;
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

function TControlScoket.GetClients(AItems: TStrings): Boolean;
var
  slResponse: TStringList;
  i: Integer;
begin
  Result := ReConnectAndLogin;
  if Result then
  begin
    slResponse := TStringList.Create;
    try
      Result := ControlCommand(CSControlCmd[ccGetClients], nil, slResponse, '');
      if Result then
      begin
        for i := 0 to slResponse.Count - 1 do
        begin
          if SameText(slResponse.Names[i], 'Item') then
            AItems.Add(slResponse.ValueFromIndex[i]); 
        end;
      end;
    finally
      slResponse.Free;
    end;
  end;
end;

procedure TControlScoket.Start;
begin
  FControlThread := TControlThread.Create(True);
  FControlThread.FreeOnTerminate := False;
  FControlThread.FControlScoket := Self;
  FControlThread.Resume;
end;

procedure TControlScoket.Stop;
begin
  Disconnect;
  FControlThread.Terminate;
  FControlThread.WaitFor;
  FControlThread.Free;
end;

{ TStatImportThread }

procedure TControlThread.DoClients;
begin
  if Assigned(FControlScoket.OnClients) then
    FControlScoket.OnClients(FClients);
end;

procedure TControlThread.Execute;
var
  i: Integer;
begin
  inherited;
  FClients := TStringList.Create;
  try
    while (not Terminated) and (FControlScoket.Connected) do
    begin
      try
        FClients.Clear;
        if FControlScoket.GetClients(FClients) then
          Synchronize(DoClients);
        for i := 0 to 10 * 1000 div 10 do //10Sˢһ��
        begin
          if Terminated then Break;
          Sleep(10);
        end;
      except
        on E: Exception do
        begin
          WriteLogMsg(ltError, 'Get Clients Error, Message: ' + E.Message);
          Break;
        end;
      end;
    end;
    PostMessage(GDataMgrCtr.NotifyHandle, WM_Disconnect, 0, 0);
  finally
    FClients.Free;
  end;
end;

end.
