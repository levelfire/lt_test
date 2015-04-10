unit LogSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TLogThread = class;
  TOnLog = procedure(const ALogMessage: string) of object;
  TLogSocket = class(TBaseClientSocket)
  private
    {* ��־��Ӧ��Ϣ *}
    FOnLog: TOnLog;
    {* �����߳� *}
    FLogThread: TLogThread;
  public
    constructor Create; override;
    destructor Destroy; override;
    {* ���ӷ����� *}
    function Connect(const ASvr: string; APort: Integer): Boolean;
    {* �Ͽ����� *}
    procedure Disconnect;
    {* ��ȡ��־ *}
    function GetLog: string;
    {* ���� *}
    procedure Start;
    {* ֹͣ *}
    procedure Stop;
    property OnLog: TOnLog read FOnLog write FOnLog;
  end;

  TLogThread = class(TThread)
  private
    {* ��־��Ϣ *}
    FLogMessage: string;
    {* Socket *}
    FLogSocket: TLogSocket;
    {* ������־������Ϣ *}
    procedure DoLog;
  protected
    procedure Execute; override;
  end;

implementation

uses DataMgrCtr, ClientDefineUnit, Logger;

{ TLogSocket }

constructor TLogSocket.Create;
begin
  inherited;
  FTimeOutMS := 60 * 1000; //��ʱ����Ϊ60S
end;

destructor TLogSocket.Destroy;
begin
  inherited;
end;

function TLogSocket.Connect(const ASvr: string; APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(#9);
end;

procedure TLogSocket.Disconnect;
begin
  inherited Disconnect;
end;

function TLogSocket.GetLog: string;
begin
  if FClient.Socket.Readable then
    Result := FClient.CurrentReadBuffer
  else
    Result := '';
end;

procedure TLogSocket.Start;
begin
  FLogThread := TLogThread.Create(True);
  FLogThread.FreeOnTerminate := False;
  FLogThread.FLogSocket := Self;
  FLogThread.Resume;
end;

procedure TLogSocket.Stop;
begin
  Disconnect;
  FLogThread.Terminate;
  FLogThread.WaitFor;
  FLogThread.Free;
end;

{ TLogThread }

procedure TLogThread.DoLog;
begin
  if Assigned(FLogSocket.OnLog) then
    FLogSocket.OnLog(FLogMessage);
end;

procedure TLogThread.Execute;
begin
  inherited;
  while (not Terminated) and (FLogSocket.Connected) do
  begin
    try
      FLogMessage := FLogSocket.GetLog;
      if FLogMessage <> '' then
        Synchronize(DoLog);
      Sleep(10);
    except
      on E: Exception do
      begin
        WriteLogMsg(ltError, 'Get Log Error, Message: ' + E.Message);
        Break;
      end;
    end;
  end;
  PostMessage(GDataMgrCtr.NotifyHandle, WM_Disconnect, 0, 0);
end;

end.
