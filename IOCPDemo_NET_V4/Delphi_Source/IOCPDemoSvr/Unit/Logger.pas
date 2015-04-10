unit Logger;

interface

uses
  Windows, SysUtils, Classes, SyncObjs, ActiveX, Messages, Graphics;

const
  {* ��־��Ϣ *}
  WM_LOG = WM_USER + 6001;

type
  {* ��־���� *}
  TLogType = (ltInfo, ltSucc, ltWarn, ltError, ltDebug);
  TLogFilter = array[TLogType] of Boolean;
  {* ��־������� *}
  TLogProcess = (lpFile, lpDesktop, lpDatabase, lpSocket);
  TLogProcessSet = set of TLogProcess;
  {* ��־�����¼� *}
  TOnLog = procedure(const ALogType: TLogType; const ALog: string) of object;

  TLogMgr = class;
  TLogHandler = class;
  TLogHandlerMgr = class;
  TLogThread = class;

  {* ��־ʵ���࣬�ṩ�ⲿ�ӿ� *}
  TLogger = class(TObject)
  private
    {* ��־�б��� *}
    FLogMgr: TLogMgr;
    {* ��־�ָ��� *}
    FLogSeparator: Char;
    {* ��־�Ƿ��ʱ��� *}
    FDateTimeStamp: Boolean;
    {* ��־�������б� *}
    FLogHandlerMgr: TLogHandlerMgr;
    {* ��־�����߳� *}
    FLogThread: TLogThread;
  public
    constructor Create; virtual;
    destructor Destroy; override;
    {* �����־ *}
    procedure AddLog(const ALogType: TLogType; const ALogMsg: string); overload;
    {* ���� *}
    procedure Start;
    {* ֹͣ *}
    procedure Stop;

    property LogSeparator: Char read FLogSeparator write FLogSeparator;
    property DateTimeStamp: Boolean read FDateTimeStamp write FDateTimeStamp;
    property LogMgr: TLogMgr read FLogMgr;
    property LogHandlerMgr: TLogHandlerMgr read FLogHandlerMgr;
  end;

  {* ��־��¼���� *}
  TLogRecordBase = class(TObject)
  protected
    {* ��־�ӿڶ��� *}
    FLogger: TLogger;
    {* ��־���� *}
    FLogType: TLogType;
    {* ��־ʱ�� *}
    FLogTime: TDateTime;
    {* ��־���� *}
    FLogMsg: string;
    {* ���ش������� *}
    function GetLogTypeStr(const ALogType: TLogType): string;
  public
    constructor Create(ALogger: TLogger; const ALogType: TLogType;
      const ALogTime: TDateTime; const ALogMsg: string); virtual;
    destructor Destroy; override;
    {* ��ʽ������ *}
    function FormatLog: string; virtual;

    property LogType: TLogType read FLogType;
    property LogTime: TDateTime read FLogTime;
    property LogMsg: string read FLogMsg;
  end;

  {* ��־�б��࣬������־�б��Լ����ṩ�����ܣ���Ҫ�ڵ��õ�ʱ����� *}
  TLogMgr = class(TObject)
  private
    {* ��־�ӿڶ��� *}
    FLogger: TLogger;
    {* �б������ *}
    FList: TList;
    {* �� *}
    FLock: TCriticalSection;
    {* ��ȡ���� *}
    function GetCount: Integer;
    {* ��ȡĳһ���� *}
    function GetItems(const AIndex: Integer): TLogRecordBase;
  public
    constructor Create(ALogger: TLogger); virtual;
    destructor Destroy; override;
    {* ��� *}
    procedure Clear;
    {* ��� *}
    procedure Add(const ALogType: TLogType; const ALogMsg: string); overload;
    procedure Add(ALogRecord: TLogRecordBase); overload;
    {* ɾ�� *}
    procedure Delete(const AIndex: Integer);
    {* �������ݣ�����������б� *}
    procedure CutData(ALogMgr: TLogMgr);

    property Count: Integer read GetCount;
    property Items[const AIndex: Integer]: TLogRecordBase read GetItems; default;
  end;

  {* ��־�����߳� *}
  TLogThread = class(TThread)
  private
    FLogger: TLogger;
  protected
    procedure Execute; override;
  end;

  TLogHandlerMgr = class(TObject)
  private
    {* �б� *}
    FList: TList;
    {* �� *}
    FLock: TCriticalSection;
    {* ��ȡ���� *}
    function GetCount: Integer;
    {* ��ȡĳһ���� *}
    function GetItems(const AIndex: Integer): TLogHandler;
  public
    constructor Create; virtual;
    destructor Destroy; override;
    {* ��� *}
    procedure Clear;
    {* ��� *}
    procedure Add(ALogHandler: TLogHandler);
    {* ɾ�� *}
    procedure Delete(ALogHandler: TLogHandler);

    property Count: Integer read GetCount;
    property Items[const AIndex: Integer]: TLogHandler read GetItems; default;
  end;

  {* ��־������� *}
  TLogHandler = class(TObject)
  private
    {* ��־�ӿڶ��� *}
    FLogger: TLogger;
    {* ���� *}
    FLogFilter: TLogFilter;
  protected
    constructor Create(ALogger: TLogger); virtual;
  public
    {* ִ����־����Ľӿ� *}
    procedure DoExceute(const ALogRecord: TLogRecordBase); virtual;
    {* ���� *}
    function DoFilter(const ALogRecord: TLogRecordBase): Boolean; virtual;

    property LogFilter: TLogFilter read FLogFilter write FLogFilter;
  end;

  {* ��־д�ļ� *}
  TLogFileHandler = class(TLogHandler)
  private
    {* �ļ��� *}
    FFileStream: TFileStream;
    {* ѭ��д�ļ��� *}
    FFileName1, FFileName2, FCurrFileName: string;
    {* �����ļ�����С����λByte��Ϊ0��ʾû������ *}
    FMaxFileSize: Int64;
  public
    constructor Create(ALogger: TLogger; const AFileName1, AFileName2: string); reintroduce; virtual;
    destructor Destroy; override;
    {* ����־������� *}
    procedure Clear;
    {* ִ����־����Ľӿ� *}
    procedure DoExceute(const ALogRecord: TLogRecordBase); override;
    {* ���� *}
    function DoFilter(const ALogRecord: TLogRecordBase): Boolean; override;

    property FileName1: string read FFileName1;
    property FileName2: string read FFileName2;
    property MaxFileSize: Int64 read FMaxFileSize write FMaxFileSize;
  end;

  {* ������ʾ��־ *}
  TLogDesktopHandler = class(TLogHandler)
  private
    {* ������Ϣ��� *}
    FHandle: THandle;
  public
    {* AHandleΪ������Ϣ��� *}
    constructor Create(ALogger: TLogger; const AHandle: THandle); reintroduce; virtual;
    destructor Destroy; override;
    {* ִ����־����Ľӿ� *}
    procedure DoExceute(const ALogRecord: TLogRecordBase); override;
    {* ���� *}
    function DoFilter(const ALogRecord: TLogRecordBase): Boolean; override;
  end;

  
  {* Socket�·���־ *}
  TLogSocketHandler = class(TLogHandler)
  private
    {* ��־��Ӧ�¼� *}
    FOnLog: TOnLog;
    {* ������־�¼� *}
    procedure DoLog(const ALogType: TLogType; const ALog: string);
  public
    constructor Create(ALogger: TLogger); reintroduce; virtual;
    {* ִ����־����Ľӿ� *}
    procedure DoExceute(const ALogRecord: TLogRecordBase); override;
    {* ���� *}
    function DoFilter(const ALogRecord: TLogRecordBase): Boolean; override;
    property OnLog: TOnLog read FOnLog write FOnLog;
  end;

  {* ���ݿ�д��־ *}
  TLogDatabaseHandler = class(TLogHandler)
  private
    {* ��־��Ӧ�¼� *}
    FOnLog: TOnLog;
    {* ������־�¼� *}
    procedure DoLog(const ALogType: TLogType; const ALog: string);
  public
    constructor Create(ALogger: TLogger); reintroduce; virtual;
    {* ִ����־����Ľӿ� *}
    procedure DoExceute(const ALogRecord: TLogRecordBase); override;
    {* ���� *}
    function DoFilter(const ALogRecord: TLogRecordBase): Boolean; override;
    property OnLog: TOnLog read FOnLog write FOnLog; 
  end;

var
  GLogger, GDebugLogger: TLogger;

procedure InitLogger(const ALogSizeKB: Integer; const ALogPath: string);
procedure UnInitLogger;
procedure WriteLogMsg(const ALogType: TLogType; const ALogMsg: string);
procedure WriteLogMsgFmt(const ALogType: TLogType; const ALogMsg: string; AParams: array of const);
procedure WriteDebugLogMsg(const ALogType: TLogType; const ALogMsg: string);
{* ��ȡ��־��ɫ *}
function GetLogColor(const ALogType: TLogType): TColor;

const
  CSLogType: array[TLogType] of string = ('Info', 'Succ', 'Warn', 'Error', 'Debug');
  {* �س����� *}
  CSCrLf = #13#10;
  CDefaultColor: array[TLogType] of TColor = (clBlack, clBlack, clFuchsia, clRed, clBlue);

implementation

uses BasisFunction;

procedure InitLogger(const ALogSizeKB: Integer; const ALogPath: string);
var
  sAppFileName, sLogFile1, sLogFile2: string;
  LogFileHandler: TLogFileHandler;
begin
  GLogger := TLogger.Create;
  sAppFileName := ChangeFileExt(ExtractFileName(ParamStr(0)), '');
  sLogFile1 := ExtractFilePath(ParamStr(0)) + ALogPath + '\' + sAppFileName + '-0.log';
  sLogFile2 := ExtractFilePath(ParamStr(0)) + ALogPath + '\' + sAppFileName + '-1.log';
  LogFileHandler := TLogFileHandler.Create(GLogger, sLogFile1, sLogFile2);
  LogFileHandler.MaxFileSize := ALogSizeKB * 1024;
  GLogger.LogHandlerMgr.Add(LogFileHandler);
  GLogger.Start;

  GDebugLogger := TLogger.Create;
  sAppFileName := ChangeFileExt(ExtractFileName(ParamStr(0)), '');
  sLogFile1 := ExtractFilePath(ParamStr(0)) + ALogPath + '\' + sAppFileName + '_Debug_0.log';
  sLogFile2 := ExtractFilePath(ParamStr(0)) + ALogPath + '\' + sAppFileName + '_Debug_1.log';
  LogFileHandler := TLogFileHandler.Create(GDebugLogger, sLogFile1, sLogFile2);
  LogFileHandler.MaxFileSize := ALogSizeKB * 1024;
  GDebugLogger.LogHandlerMgr.Add(LogFileHandler);
  GDebugLogger.Start;
end;

procedure UnInitLogger;
begin
  GLogger.Stop;
  FreeAndNil(GLogger);
  GDebugLogger.Stop;
  FreeAndNil(GDebugLogger);
end;

procedure WriteLogMsg(const ALogType: TLogType; const ALogMsg: string);
begin
  if Assigned(GLogger) then
    GLogger.AddLog(ALogType, ALogMsg);
end;

procedure WriteLogMsgFmt(const ALogType: TLogType; const ALogMsg: string; AParams: array of const);
begin
  WriteLogMsg(ALogType, Format(ALogMsg, AParams));
end;

procedure WriteDebugLogMsg(const ALogType: TLogType; const ALogMsg: string);
begin
  if Assigned(GDebugLogger) then
    GDebugLogger.AddLog(ALogType, ALogMsg);
end;

function GetLogColor(const ALogType: TLogType): TColor;
begin
  if (ALogType >= Low(TLogType)) and (ALogType <= High(TLogType)) then
    Result := CDefaultColor[ALogType]
  else
    Result := clBlack;
end;

{ TLogger }

constructor TLogger.Create;
begin
  FLogMgr := TLogMgr.Create(Self);
  FLogSeparator := ';'; //Ĭ��Ϊ�ֺ�
  FDateTimeStamp := True;
  FLogHandlerMgr := TLogHandlerMgr.Create;
end;

destructor TLogger.Destroy;
begin
  FLogMgr.Free;
  FLogHandlerMgr.Free;
  inherited;
end;

procedure TLogger.AddLog(const ALogType: TLogType; const ALogMsg: string);
begin
  FLogMgr.Add(ALogType, ALogMsg);
end;

procedure TLogger.Start;
begin
  if not Assigned(FLogThread) then
  begin
    FLogThread := TLogThread.Create(True);
    FLogThread.FLogger := Self;
    FLogThread.FreeOnTerminate := False;
    FLogThread.Resume;
  end;
end;

procedure TLogger.Stop;
begin
  if Assigned(FLogThread) then
  begin
    FLogThread.Terminate;
    FLogThread.WaitFor;
    FLogThread.Free;
    FLogThread := nil;
  end;
end;

{ TLogRecordBase }

constructor TLogRecordBase.Create(ALogger: TLogger; const ALogType: TLogType;
  const ALogTime: TDateTime; const ALogMsg: string);
begin
  FLogger := ALogger;
  FLogType := ALogType;
  FLogTime := ALogTime;
  FLogMsg := ALogMsg;
end;

destructor TLogRecordBase.Destroy;
begin
  inherited;
end;

function TLogRecordBase.FormatLog: string;
begin
  Result := GetLogTypeStr(FLogType) + FLogger.LogSeparator + FLogMsg;
  if FLogger.DateTimeStamp then
    Result := DateTimeToStr(FLogTime) + FLogger.LogSeparator + Result;
end;

function TLogRecordBase.GetLogTypeStr(const ALogType: TLogType): string;
begin
  if (ALogType >= Low(TLogType)) and (ALogType <= High(TLogType)) then
    Result := CSLogType[ALogType]
  else
    Result := '';
end;

{ TLogMgr }

constructor TLogMgr.Create(ALogger: TLogger);
begin
  FLogger := ALogger;
  FList := TList.Create;
  FLock := TCriticalSection.Create;
end;

destructor TLogMgr.Destroy;
begin
  Clear;
  FList.Free;
  FLock.Free;
  inherited;
end;

procedure TLogMgr.Clear;
var
  i: Integer;
begin
  FLock.Enter;
  try
    for i := 0 to FList.Count - 1 do
    begin
      TLogRecordBase(FList[i]).Free;
    end;
    FList.Clear;
  finally
    FLock.Leave;
  end;
end;

procedure TLogMgr.Add(ALogRecord: TLogRecordBase);
begin
  FLock.Enter;
  try
    FList.Add(ALogRecord);
  finally
    FLock.Leave;
  end;
end;

procedure TLogMgr.Add(const ALogType: TLogType; const ALogMsg: string);
var
  LogRecord: TLogRecordBase;
begin
  LogRecord := TLogRecordBase.Create(FLogger, ALogType, Now, ALogMsg);
  Add(LogRecord);
end;

procedure TLogMgr.Delete(const AIndex: Integer);
var
  LogRecord: TLogRecordBase;
begin
  FLock.Enter;
  try
    LogRecord := TLogRecordBase(FList.Items[AIndex]);
    LogRecord.Free;
    FList.Delete(AIndex);
  finally
    FLock.Leave;
  end;
end;

function TLogMgr.GetCount: Integer;
begin
  Result := FList.Count;
end;

function TLogMgr.GetItems(const AIndex: Integer): TLogRecordBase;
begin
  Result := FList[AIndex];
end;

procedure TLogMgr.CutData(ALogMgr: TLogMgr);
var
  i: Integer;
begin
  FLock.Enter;
  try
    for i := 0 to Count - 1 do
    begin
      ALogMgr.Add(TLogRecordBase(Items[i]));
    end;
    FList.Clear; //��������б������ͷ��ڴ�
  finally
    FLock.Leave;
  end;
end;

{ TLogThread }

procedure TLogThread.Execute;
var
  LogMgr: TLogMgr;
  i, j: Integer;
begin
  inherited;
  CoInitialize(nil);
  try
    while not Terminated do
    begin
      try
        if FLogger.LogMgr.Count > 0 then
        begin
          LogMgr := TLogMgr.Create(FLogger);
          try
            FLogger.LogMgr.CutData(LogMgr); //�ȸ�������
            for i := 0 to LogMgr.Count - 1 do
            begin
              for j := FLogger.LogHandlerMgr.Count - 1 downto 0 do
              begin
                if FLogger.LogHandlerMgr.Items[j].DoFilter(LogMgr.Items[i]) then
                  FLogger.LogHandlerMgr.Items[j].DoExceute(LogMgr.Items[i]);
              end;
            end;
            LogMgr.Clear;
          finally
            LogMgr.Free;
          end;
        end;
      except
        ; //�����ˣ���������ִ��
      end;
      Sleep(100);
    end;
  finally
    CoUninitialize;
  end;
end;

{ TLogHandlerMgr }

function TLogHandlerMgr.GetCount: Integer;
begin
  Result := FList.Count;
end;

function TLogHandlerMgr.GetItems(const AIndex: Integer): TLogHandler;
begin
  Result := FList[AIndex];
end;

procedure TLogHandlerMgr.Clear;
var
  i: Integer;
begin
  FLock.Enter;
  try
    for i := 0 to Count - 1 do
    begin
      Items[i].Free;
    end;
    FList.Clear;
  finally
    FLock.Leave;
  end;
end;

procedure TLogHandlerMgr.Add(ALogHandler: TLogHandler);
begin
  FLock.Enter;
  try
    FList.Add(ALogHandler);
  finally
    FLock.Leave;
  end;
end;

procedure TLogHandlerMgr.Delete(ALogHandler: TLogHandler);
var
  iIndex: Integer;
begin
  FLock.Enter;
  try
    iIndex := FList.IndexOf(ALogHandler);
    if iIndex <> -1 then
      FList.Delete(iIndex);
  finally
    FLock.Leave;
  end;
end;

constructor TLogHandlerMgr.Create;
begin
  FList := TList.Create;
  FLock := TCriticalSection.Create;
end;

destructor TLogHandlerMgr.Destroy;
begin
  Clear;
  FList.Free;
  FLock.Free;
  inherited;
end;

{ TLogHandler }

constructor TLogHandler.Create(ALogger: TLogger);
begin
  FLogger := ALogger;
  FLogFilter[ltInfo] := True;
  FLogFilter[ltSucc] := True;
  FLogFilter[ltWarn] := True;
  FLogFilter[ltError] := True;
  FLogFilter[ltDebug] := True;
end;

procedure TLogHandler.DoExceute(const ALogRecord: TLogRecordBase);
begin

end;

function TLogHandler.DoFilter(const ALogRecord: TLogRecordBase): Boolean;
begin
  if (ALogRecord.LogType in [Low(TLogType)..High(TLogType)]) and (FLogFilter[ALogRecord.LogType]) then
    Result := True
  else
    Result := False;
end;

{ TFileHandler }

constructor TLogFileHandler.Create(ALogger: TLogger; const AFileName1, AFileName2: string);
var
  sLogPath: string;
begin
  inherited Create(ALogger);
  FFileName1 := AFileName1;
  FFileName2 := AFileName2;
  FCurrFileName := FFileName1;
  sLogPath := ExtractFilePath(FCurrFileName);
  if not DirectoryExists(sLogPath) then
    ForceDirectories(sLogPath);
  CreateFileOnDisk(FCurrFileName);
  FFileStream := TFileStream.Create(FCurrFileName, fmOpenReadWrite or fmShareDenyWrite);
  FFileStream.Position := FFileStream.Size;
end;

destructor TLogFileHandler.Destroy;
begin
  FFileStream.Free;
  inherited;
end;

procedure TLogFileHandler.Clear;
begin
  FFileStream.Size := 0;
  FFileStream.Position := 0;
end;

procedure TLogFileHandler.DoExceute(const ALogRecord: TLogRecordBase);
var
  sLog: string;
begin
  inherited;
  sLog := ALogRecord.FormatLog;
  //���Log��С������������ƣ���ʼд����һ���ļ�
  if (FMaxFileSize > 0) and (FFileStream.Position > FMaxFileSize) then
  begin
    if FCurrFileName = FFileName1 then
      FCurrFileName := FFileName2
    else
      FCurrFileName := FFileName1;
    FFileStream.Free;  //���ͷ��ϵ��ļ�
    CreateFileOnDisk(FCurrFileName);
    FFileStream := TFileStream.Create(FCurrFileName, fmOpenReadWrite or fmShareDenyWrite);
    if FFileStream.Size > FMaxFileSize then //��������ļ���С����д
    begin
      FFileStream.Size := 0;
      FFileStream.Position := 0;
    end
    else //������д
      FFileStream.Seek(FFileStream.Size, soFromBeginning);
  end;

  FFileStream.Write(PChar(sLog)^, Length(sLog)); //д����־
  FFileStream.Write(CSCrLf, 2); //д��س�����
end;

function TLogFileHandler.DoFilter(const ALogRecord: TLogRecordBase): Boolean;
begin
  Result := inherited DoFilter(ALogRecord);
end;

{ TDesktopHandler }

constructor TLogDesktopHandler.Create(ALogger: TLogger;
  const AHandle: THandle);
begin
  inherited Create(ALogger);
  FHandle := AHandle;
end;

destructor TLogDesktopHandler.Destroy;
begin
  inherited;
end;

procedure TLogDesktopHandler.DoExceute(const ALogRecord: TLogRecordBase);
var
  sLog: string;
begin
  inherited;
  sLog := ALogRecord.FormatLog;
  SendMessage(FHandle, WM_LOG, Ord(ALogRecord.LogType), Integer(PChar(sLog)));
end;

function TLogDesktopHandler.DoFilter(const ALogRecord: TLogRecordBase): Boolean;
begin
  Result := inherited DoFilter(ALogRecord);
end;

{ TLogSocketHandler }

constructor TLogSocketHandler.Create(ALogger: TLogger);
begin
  inherited Create(ALogger);
end;

procedure TLogSocketHandler.DoLog(const ALogType: TLogType; const ALog: string);
begin
  if Assigned(FOnLog) then
    FOnLog(ALogType, ALog);
end;

procedure TLogSocketHandler.DoExceute(const ALogRecord: TLogRecordBase);
var
  sLog: string;
begin
  inherited;
  sLog := ALogRecord.FormatLog;
  DoLog(ALogRecord.LogType, sLog);
end;

function TLogSocketHandler.DoFilter(const ALogRecord: TLogRecordBase): Boolean;
begin
  Result := inherited DoFilter(ALogRecord);
end;

{ TLogDatabaseHandler }

constructor TLogDatabaseHandler.Create(ALogger: TLogger);
begin
  inherited Create(ALogger);
end;

procedure TLogDatabaseHandler.DoLog(const ALogType: TLogType;
  const ALog: string);
begin
  if Assigned(FOnLog) then
    FOnLog(ALogType, ALog);
end;

procedure TLogDatabaseHandler.DoExceute(const ALogRecord: TLogRecordBase);
var
  sLog: string;
begin
  inherited;
  sLog := ALogRecord.FormatLog;
  DoLog(ALogRecord.LogType, sLog);
end;

function TLogDatabaseHandler.DoFilter(
  const ALogRecord: TLogRecordBase): Boolean;
begin
  Result := inherited DoFilter(ALogRecord);
end;

end.
