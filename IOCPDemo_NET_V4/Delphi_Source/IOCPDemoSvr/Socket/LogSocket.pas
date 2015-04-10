unit LogSocket;

interface

uses
  SysUtils, Classes, Windows, Types, IOCPSocket, BaseSocket, Logger, DefineUnit;

type
  TLogSocket = class(TBaseSocket)
  private
    {* ��־д��SOCKETʵ���� *}
    FLogSocketHandler: TLogSocketHandler;
  protected
    { *�������ݽӿڣ�ÿ�����������д������ⲿ�������������ﲻ�ټ���* }
    procedure Execute(AData: PByte; const ALen: Cardinal); override;
  public
    procedure DoCreate; override;
    destructor Destroy; override;
    procedure WriteLogToClient(const LogType: TLogType; const LogMsg: string); 
  end;

implementation

uses OptionSet;

{ TLogSocket }

procedure TLogSocket.DoCreate;
begin
  inherited;
  LenType := IOCPSocket.ltNull;
  FSocketFlag := sfLog;
  OpenWriteBuffer;
  WriteString(CSCrLf + CSCompanyName + ' ' + CSSoftwareName + ' Log output' + CSCrLf);
  WriteString('Press ESC to exit...' + CSCrLf);
  FlushWriteBuffer(ioWrite);
  FLogSocketHandler := TLogSocketHandler.Create(GLogger);
  FLogSocketHandler.OnLog := WriteLogToClient;
  FLogSocketHandler.LogFilter := GIniOptions.LogSocket;
  GLogger.LogHandlerMgr.Add(FLogSocketHandler);
end;

destructor TLogSocket.Destroy;
begin
  GLogger.LogHandlerMgr.Delete(FLogSocketHandler);
  FLogSocketHandler.Free;
  inherited;
end;

procedure TLogSocket.Execute(AData: PByte; const ALen: Cardinal);
var
  vkByte: Byte;
begin
  inherited;
  if ALen = 1 then
  begin
    vkByte := AData^;
    if vkByte = VK_ESCAPE then
      Disconnect //�����������룬���ΪESC���˳�
  end
  else
  begin
    OpenWriteBuffer;
    WriteString(CSCompanyName + ' ' + CSSoftwareName + ' Log output' + CSCrLf);
    WriteString('Press ESC to exit...' + CSCrLf);
    FlushWriteBuffer(ioWrite);
  end;
end;

procedure TLogSocket.WriteLogToClient(const LogType: TLogType;
  const LogMsg: string);
var
  s: string;
begin
  s := LogMsg + CSCrLf;
  try
    if Connected then
    begin
      OpenWriteBuffer;
      WriteString(s);
      FlushWriteBuffer(ioWrite);
    end;
  except
    ;
  end;
end;

end. 
