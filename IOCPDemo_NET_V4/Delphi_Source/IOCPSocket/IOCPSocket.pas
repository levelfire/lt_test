unit IOCPSocket;
{* |<PRE>
================================================================================
* ������ƣ�TaxSvr �� IOCPSocket.pas
* ��Ԫ���ƣ�IOCPSocket.pas
* ��Ԫ���ߣ�SQLDebug_Fan <fansheng_hx@163.com>  
* ��    ע��-��ɶ˿ڷ�װ�� 
*           -��װ��ɶ˿�Ҫע���ڴ����룬һ��Ҫ��ѭ��ԭ���Ǳ��߳����뱾�߳��ͷ�
*           -��������һЩ�쳣��DELPHI�Դ����ڴ��������ⲻ����������һЩ����
*           -���ڴ���������Լ���������FastMM�����Ҫ�����̵߳�FreeOnTerminate
* ����ƽ̨��Windows XP + Delphi 7
* ���ݲ��ԣ�Windows XP, Delphi 7
* �� �� �����õ�Ԫ�е��ַ��������ϱ��ػ�����ʽ
--------------------------------------------------------------------------------
* ���¼�de ¼��-
*           - 
================================================================================
|</PRE>}
interface

uses
  Windows, SysUtils, Classes, Messages, SyncObjs, JwaWinsock2, ActiveX,
  DateUtils;

const
  {* �������ݵĻ�������С��������յ������ݳ�����С����ֳɼ��������� *}
  MAX_IOCPBUFSIZE = 4096;
  {* IOCP�˳���־ *}
  SHUTDOWN_FLAG = $FFFFFFFF;
  {* �س����� *}
  LF = #10;
  CR = #13;
  EOL = CR + LF;
  {* ������������ *}
  MAX_IDLELOCK = 50;

type
  {* ��ɶ˿ڲ������� *}
  TIocpOperate = (ioNone, ioRead, ioWrite, ioStream, ioExit);
  PIocpRecord = ^TIocpRecord;
  TIocpRecord = record
    Overlapped: TOverlapped;   //��ɶ˿��ص��ṹ
    WsaBuf: TWsaBuf;           //��ɶ˿ڵĻ���������
    IocpOperate: TIOCPOperate; //��ǰ��������
  end;
  {* TCP�������ṹ�� *}
  TTCPKeepAlive = packed record
    OnOff: Cardinal;
    KeepAliveTime: Cardinal;
    KeepAliveInterval: Cardinal;
  end;

  EObjectList = class(Exception);
  {* �̰߳�ȫ��������͵��б���ȡ�б���Ҫ��Lock����ȡ֮��UnLock�����Һ����Դ��� *}
  TObjectList = class(TList) { TObjectList, Lock}
  private
    FLock: TCriticalSection;
    FClassType: TClass;
    FData: Pointer;
    FName: string;
    FTag: integer;
    function GetItems(Index: Integer): TObject;
    procedure SetItems(Index: Integer; const Value: TObject);
  protected
    procedure ClassTypeError(Message: string);
  public
    procedure Lock;
    procedure UnLock;
    function Expand: TObjectList;
    function Add(AObject: TObject): Integer;
    function IndexOf(AObject: TObject): Integer; overload;
    procedure Delete(Index: Integer); overload;
    procedure Clear; override;
    function Remove(AObject: TObject): Integer;
    procedure Insert(Index: Integer; Item: TObject);
    function First: TObject;
    function Last: TObject;
    property ItemClassType: TClass read FClassType;
    property Items[Index: Integer]: TObject read GetItems write SetItems; default;

    constructor Create; overload;
    constructor Create(AClassType: TClass); overload;
    destructor Destroy; override;
    property Data: Pointer read FData write FData;
  published
    property Tag: integer read FTag write FTag;
    property Name: string read FName write FName;
  end;

  ESocketError = class(Exception);
  EIocpError = class(Exception);
  TIocpServer = class;

  {* ������������ص��˿ڻ��� *}
  TSendOverlapped = class
  private
    FList: TList;
    FLock: TCriticalSection;
    procedure Clear;
  public
    constructor Create;
    destructor Destroy; override;
    {* ����һ�� *}
    function Allocate(ALen: Cardinal): PIocpRecord;
    {* �ͷ�һ�� *}
    procedure Release(AValue: PIocpRecord);
  end;
  
  {* �������ͣ�Word���ͻ���Cardinal���� *}
  TLenType = (ltNull, ltWord, ltCardinal);
  {* �ͻ��˶�ӦSocket���� *}
  TSocketHandle = class
  private
    {* �Ե�SOCKET���� *}
    FSocket: TSocket;
    FIocpServer: TIocpServer;
    {* ���� *}
    FConnected: Boolean;
    {* Զ������ *}
    FRemoteAddress: string;
    FRemotePort: Word; 
    {* �������� *}
    FLocalAddress: string;
    FLocalPort: Word;
    {* ����Ͷ������ĵ�ַ��Ϣ *}
    FIocpRecv: PIocpRecord;
    {* ����IOCP�ṹ�建�� *}
    FSendOverlapped: TSendOverlapped;
    {* ���յ����� *}
    FInputBuf: TMemoryStream;
    {* ���͵����� *}
    FOutputBuf: TMemoryStream;
    {* �������� *}
    FLenType: TLenType;
    {* ���ĳ��� *}
    FPacketLen: Integer;
    {* ���Ͷ�����룬�����ⲿ���� *}
    FLock: TCriticalSection;
    {* ��ʱ��0��ʾû�г�ʱ *}
    FTimeOutMS: Cardinal;
    {* �ϴμ���ʱ�䣬������ָ�յ����� *}
    FActiveTime: TDateTime;
    {* ����ʱ�� *}
    FConnectTime: TDateTime;
    {* ��ʶ�Ƿ����ڴ����� *}
    FExecuting: Boolean;
  protected
    {* ��ȡSOCKETԶ������ *}
    function GetRemoteAddress: string;
    function GetRemotePort: Word;
    procedure GetRemoteInfo;
    {* ��ȡSOCKET�������� *}
    function GetLocalAddress: string;
    function GetLocalPort: Word;
    procedure GetLocalInfo;
    {* ������յ������ݣ�ÿ�����ݰ��ĸ�ʽ���ȷ����ȣ�Ȼ�����ݣ����Ա���ճ�� *}
    procedure ReceiveData(AData: PAnsiChar; const ALen: Cardinal);
    {* ������յ����� *}
    procedure Process;
    {* ���ദ��������ݽӿ� *}
    procedure Execute(AData: PByte; const ALen: Cardinal); virtual;
    {* ���ó�ʱ *}
    procedure SetTimeOut(const ATimeOutMS: Cardinal);
    {* �������緵�ص��쳣 *}
    procedure ProcessNetError(const AErrorCode: Integer);
    {* �������Ļص����������������Ҫ����������������������� *}
    procedure WriteStream(const AWriteNow: Boolean = True); overload; virtual;
  public
    constructor Create(const AIocpServer: TIocpServer; const ASocket: TSocket);
    procedure DoCreate; virtual;
    destructor Destroy; override;
    {* �Ͽ����� *}
    procedure Disconnect;
    {* Ͷ�ݽ������� *}
    procedure PreRecv;
    {* ����IO��ɶ˿����� *}
    procedure ProcessIOComplete(AIocpRecord: PIocpRecord; const ACount: Cardinal);
    {* ��ȡ���� *}
    procedure ReadWord(AData: PAnsiChar; var AWord: Word; const AConvert: Boolean = True);
    procedure ReadCardinal(AData: PAnsiChar; var ACardinal: Cardinal; const AConvert: Boolean = True);
    {* ���ͺ��� *}
    procedure OpenWriteBuffer;
    procedure FlushWriteBuffer(const AIocpOperate: TIocpOperate);
    procedure WriteBuffer(const ABuffer; const ACount: Integer);
    procedure WriteWord(AValue: Word; const AConvert: Boolean = True); 
    procedure WriteCardinal(AValue: Cardinal; const AConvert: Boolean = True); 
    procedure WriteInteger(AValue: Integer; const AConvert: Boolean = True);
    procedure WriteSmallInt(AValue: SmallInt; const AConvert: Boolean = True); 
    procedure WriteString(const AValue: string);
    procedure WriteLn(const AValue: string);
    {* �� *}
    procedure Lock;
    procedure UnLock;
    {* ���� *}
    property Socket: TSocket read FSocket;
    property RemoteAddress: string read GetRemoteAddress;
    property RemotePort: Word read GetRemotePort;
    property LocalAddress: string read GetLocalAddress;
    property LocalPort: Word read GetLocalPort;
    property LenType: TLenType read FLenType write FLenType;
    property TimeOutMS: Cardinal read FTimeOutMS write SetTimeOut;
    property Connected: Boolean read FConnected;
    property ActiveTime: TDateTime read FActiveTime;
    property ConnectTime: TDateTime read FConnectTime;
    property IocpServer: TIocpServer read FIocpServer;
    property Executing: Boolean read FExecuting;
  end;

  {* �ͻ��˶������ *}
  TClientSocket = record
    Lock: TCriticalSection;
    SocketHandle: TSocketHandle;
    IocpRecv: TIocpRecord; //Ͷ������ṹ��
    IdleDT: TDateTime;
  end;
  PClientSocket = ^TClientSocket;
  
  {* �ͻ��˶�ӦSocket������� *}
  TSocketHandles = class(TObject)
  private
    {* ����ʹ���б������� *}
    FList: TList;
    {* ����ʹ���б������� *}
    FIdleList: TList;
    {* �� *}
    FLock: TCriticalSection;
    {* ��ȡĳһ�� *}
    function GetItems(const AIndex: Integer): PClientSocket;
    {* ��ȡ�ܸ��� *}
    function GetCount: Integer;
    {* ��� *}
    procedure Clear;
    {* ��ȡ�������Ӹ��� *}
    function GetIdleCount: Integer;
  public
    constructor Create; virtual;
    destructor Destroy; override;
    {* ���� *}
    procedure Lock;
    {* ���� *}
    procedure UnLock;
    {* ���һ������ *}
    function Add(ASocketHandle: TSocketHandle): Integer;
    {* ɾ�� *}
    procedure Delete(const AIndex: Integer); overload;
    procedure Delete(ASocketHandle: TSocketHandle); overload;
    property Items[const AIndex: Integer]: PClientSocket read GetItems; default;
    property Count: Integer read GetCount;
    property IdleCount: Integer read GetIdleCount;
  end;

  TAcceptThread = class;
  TWorkThreads = class;
  TIocpThreadPool = class;

  TOnConnect = procedure(const ASocket: TSocket; var AAllowConnect: Boolean;
    var SocketHandle: TSocketHandle) of object;
  TOnError = procedure(const AName: string; const AError: string) of object;
  TOnDisconnect = procedure(const ASocketHandle: TSocketHandle) of object;
  {* IOCP�������� *}
  TIocpServer = class(TComponent)
  private
    {* �˿� *}
    FPort: Word;                          
    {* �׽��� *}
    FSocket: TSocket;                      
    {* ��ɶ˿ھ�� *}
    FIocpHandle: THandle;                  
    {* �ͻ��˶��� *}
    FSocketHandles: TSocketHandles;
    {* �����߳� *}      
    FWorkThreads: TWorkThreads;
    {* �����߳� *}
    FAcceptThread: TAcceptThread;
    {* �Ƿ��Ѿ����� *}
    FActive: Boolean;
    {* �����¼� *}
    FOnConnect: TOnConnect;
    {* �����¼� *}
    FOnError: TOnError;
    {* �Ͽ��¼� *}
    FOnDisconnect: TOnDisconnect;
    {* ���տͻ������ӵ��̳߳� *}
    FAcceptThreadPool: TIocpThreadPool;
    {* ����ʱ�� *}
    FConnectTime: TDateTime;
    {* ������̺߳����ٹ����߳� *}
    FMaxWorkThrCount, FMinWorkThrCount: Integer;
    {* �����������̺߳���С���������߳� *}
    FMaxCheckThrCount, FMinCheckThrCount: Integer;
  protected
    procedure Open;
    procedure Close;
    procedure SetActive(const AValue: Boolean);
    {* ����ʱ�������¼� *}
    function DoConnect(const ASocket: TSocket; var SocketHandle: TSocketHandle): Boolean;
    {* �������󴥷����¼� *}
    procedure DoError(const AName, AError: string);
    {* �Ͽ����Ӵ������¼� *}
    procedure DoDisconnect(const ASocketHandle: TSocketHandle);
    {* �Ͽ�ĳ�����ӣ������Ͽ��¼� *}
    procedure FreeSocketHandle(const ASocketHandle: TSocketHandle); overload;
    procedure FreeSocketHandle(const AIndex: Integer); overload;
  public
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;
    {* ���ܿͻ������󣬹������߳�TAcceptThread���� *}
    procedure AcceptClient;
    {* �жϽ��յĿͻ����Ƿ�Ϸ� *}
    procedure CheckClient(const ASocket: TSocket);
    {* ����ͻ��˲������������߳�TWorkThread���� *}
    function WorkClient: Boolean;
    {* ����Socket���������һ���ַ�����ʱ����False *}
    function ReadChar(const ASocket: TSocket; var AChar: Char; const ATimeOutMS: Integer): Boolean;
    {* �ж�ָ��ʱ����Socket�Ƿ������ݴ����� *}
    function CheckTimeOut(const ASocket: TSocket; const ATimeOutMS: Integer): Boolean;
    {* ��������� *}
    procedure CheckDisconnectedClient;
    property ConnectTime: TDateTime read FConnectTime;
    property SocketHandles: TSocketHandles read FSocketHandles;
  published
    property Active: Boolean read FActive write SetActive;
    property Port: Word read FPort write FPort;
    property OnConnect: TOnConnect read FOnConnect write FOnConnect;
    property OnError: TOnError read FOnError write FOnError;
    property OnDisconnect: TOnDisconnect read FOnDisconnect write FOnDisconnect;
    property MaxWorkThrCount: Integer read FMaxWorkThrCount write FMaxWorkThrCount;
    property MinWorkThrCount: Integer read FMinWorkThrCount write FMinWorkThrCount;
    property MaxCheckThrCount: Integer read FMaxCheckThrCount write FMaxCheckThrCount;
    property MinCheckThrCount: Integer read FMinCheckThrCount write FMinCheckThrCount;
  end;

  TIocpThread = class(TThread)
  private
    FIocpServer: TIocpServer;
  public
    constructor Create(const AServer: TIocpServer; CreateSuspended: Boolean); reintroduce;
  end;

  TAcceptThread = class(TIocpThread)
  protected
    procedure Execute; override;
  end;

  TWorkThread = class(TIocpThread)
  protected
    procedure Execute; override;
  end;

  TWorkThreads = class(TObjectList)
  private
    function GetItems(AIndex: Integer): TWorkThread;
    procedure SetItems(AIndex: Integer; AValue: TWorkThread);
  public
    constructor Create;
    property Items[AIndex: Integer]: TWorkThread read GetItems write SetItems; default;
  end;
  
  TCheckThread = class(TIocpThread)
  private
    FIocpThreadPool: TIocpThreadPool;
  protected
    procedure Execute; override;
  public
    property IocpThreadPool: TIocpThreadPool read FIocpThreadPool write FIocpThreadPool;
  end;

  TCheckThreads = class(TObjectList)
  private
    function GetItems(AIndex: Integer): TCheckThread;
    procedure SetItems(AIndex: Integer; AValue: TCheckThread);
  public
    constructor Create;
    property Items[AIndex: Integer]: TCheckThread read GetItems write SetItems; default;
  end;

  {* ��ɶ˿�ʵ�ֵ��̳߳أ���Ҫ���ڽ��տͻ����������� *}
  TIocpThreadPool = class
  private
    {* ��ɶ˿ڷ������ *}
    FIocpServer: TIocpServer;
    {* ��ɶ˿ھ�� *}
    FIocpHandle: THandle;
    {* �߳��� *}                       
    FCheckThreads: TCheckThreads;
    FActive: Boolean;
  protected
    procedure Open;
    procedure Close;
    procedure SetActive(const AValue: Boolean);
  public
    constructor Create(const AServer: TIocpServer); reintroduce;
    destructor Destroy; override;
    {* Ͷ��һ��SOCKET�������� *}
    procedure PostSocket(const ASocket: TSocket);
    property Active: Boolean read FActive write SetActive;
  end;

function GetLastWsaErrorStr: string;
function GetLastErrorStr: string;
function GetCPUCount: Integer;
procedure Register;

resourcestring
  SErrClassType = 'Class type mismatch. Expect: %s , actual: %s';
  SErrOutBounds = 'Out of bounds,The value %d not between 0 and %d.';

implementation

uses TypInfo;

procedure Register;
begin
  RegisterComponents('Fan', [TIocpServer]);
end;

function GetLastWsaErrorStr: string;
begin
  Result := SysErrorMessage(WSAGetLastError);
end;

function GetLastErrorStr: string;
begin
  Result := SysErrorMessage(GetLastError);
end;

function GetCPUCount: Integer;
var
  SysInfo: TSystemInfo;
begin
  FillChar(SysInfo, SizeOf(SysInfo), 0);
  GetSystemInfo(SysInfo);
  Result := SysInfo.dwNumberOfProcessors;
end;

{ TObjectList }

function TObjectList.Add(AObject: TObject): Integer;
begin
  Result := -1;
  Lock;
  try
    if (AObject = nil) or (AObject is FClassType) then
      Result := inherited Add(AObject)
    else
      ClassTypeError(AObject.ClassName);
  finally
    UnLock;
  end;
end;

constructor TObjectList.Create;
begin
  Create(TObject);
end;

constructor TObjectList.Create(AClassType: TClass);
begin
  inherited Create;
  FLock := TCriticalSection.Create;;
  FClassType := AClassType;
end;

procedure TObjectList.Delete(Index: Integer);
begin
  Lock;
  try
    inherited Delete(Index);
  finally
    UnLock;
  end;
end;

procedure TObjectList.ClassTypeError(Message: string);
begin
  raise EObjectList.CreateFmt(SErrClassType, [FClassType.ClassName, Message]);
end;

function TObjectList.Expand: TObjectList;
begin
  Lock;
  try
    Result := (inherited Expand) as TObjectList;
  finally
    UnLock;
  end;
end;

function TObjectList.First: TObject;
begin
  Lock;
  try
    Result := TObject(inherited First);
  finally
    UnLock;
  end;
end;

function TObjectList.GetItems(Index: Integer): TObject;
begin
  Result := TObject(inherited Items[Index]);
end;

function TObjectList.IndexOf(AObject: TObject): Integer;
begin
  Lock;
  try
    Result := inherited IndexOf(AObject);
  finally
    UnLock;
  end;
end;

procedure TObjectList.Insert(Index: Integer; Item: TObject);
begin
  Lock;
  try
    if (Item = nil) or (Item is FClassType) then
      inherited Insert(Index, Pointer(Item))
    else
      ClassTypeError(Item.ClassName);
  finally
    UnLock;
  end;
end;

function TObjectList.Last: TObject;
begin
  Lock;
  try
    Result := TObject(inherited Last);
  finally
    UnLock;
  end;
end;

function TObjectList.Remove(AObject: TObject): Integer;
begin
  Lock;
  try
    Result := IndexOf(AObject);
    if Result >= 0 then Delete(Result);
  finally
    UnLock;
  end;
end;

procedure TObjectList.SetItems(Index: Integer; const Value: TObject);
begin
  if Value = nil then
    Exit
  else if Value is FClassType then
    inherited Items[Index] := Value
  else
    ClassTypeError(Value.ClassName);
end;

destructor TObjectList.Destroy;
begin                
  inherited Destroy;
  FLock.Free;
end;

procedure TObjectList.Lock;
begin
  FLock.Enter;
end;

procedure TObjectList.UnLock;
begin
  FLock.Leave;
end;

procedure TObjectList.Clear;
var
  i : Integer;
begin
  for i := 0 to Count - 1 do
  if Assigned(Items[i]) then
  begin
    Items[i] := nil;
  end;
  inherited;
end;

{ TSendOverlapped }

procedure TSendOverlapped.Clear;
var
  i: Integer;
begin
  FLock.Enter;
  try
    for i := 0 to FList.Count - 1 do
    begin
      FreeMemory(PIocpRecord(FList.Items[i]).WsaBuf.buf);
      PIocpRecord(FList.Items[i]).WsaBuf.buf := nil;
      Dispose(PIocpRecord(FList.Items[i]));
    end;
    FList.Clear;
  finally
    FLock.Leave;
  end;
end;

constructor TSendOverlapped.Create;
begin
  inherited Create;
  FList := TList.Create;
  FLock := TCriticalSection.Create;
end;

destructor TSendOverlapped.Destroy;
begin
  Clear;
  FList.Free;
  FLock.Free;
  inherited;
end;

function TSendOverlapped.Allocate(ALen: Cardinal): PIocpRecord;
begin
  FLock.Enter;
  try
    New(Result);
    Result.Overlapped.Internal := 0;
    Result.Overlapped.InternalHigh := 0;
    Result.Overlapped.Offset := 0;
    Result.Overlapped.OffsetHigh := 0;
    Result.Overlapped.hEvent := 0;
    Result.IocpOperate := ioNone;
    Result.WsaBuf.buf := GetMemory(ALen);
    Result.WsaBuf.len := ALen;
    FList.Add(Result);
  finally
    FLock.Leave;
  end;
end;

procedure TSendOverlapped.Release(AValue: PIocpRecord);
var
  i: Integer;
begin
  FLock.Enter;
  try
    for i := 0 to FList.Count - 1 do
    begin
      if Cardinal(AValue) = Cardinal(FList[i]) then
      begin
        FreeMemory(PIocpRecord(FList.Items[i]).WsaBuf.buf);
        PIocpRecord(FList.Items[i]).WsaBuf.buf := nil;
        Dispose(PIocpRecord(FList.Items[i]));
        FList.Delete(i);
        Break;
      end;
    end;
  finally
    FLock.Leave;
  end;
end;

{ TSocketHandle }

constructor TSocketHandle.Create(const AIocpServer: TIocpServer;
  const ASocket: TSocket);
begin
  inherited Create;
  FIocpServer := AIocpServer;
  FSocket := ASocket;
  FSendOverlapped := TSendOverlapped.Create;
  FInputBuf := TMemoryStream.Create;
  FConnected := True;
  FConnectTime := Now;
  FActiveTime := Now;
  DoCreate;
end;

procedure TSocketHandle.DoCreate;
begin

end;

destructor TSocketHandle.Destroy;
begin
  closesocket(FSocket);
  FSendOverlapped.Free;
  FInputBuf.Free;
  if Assigned(FOutputBuf) then
    FOutputBuf.Free;
  inherited;
end;

procedure TSocketHandle.Disconnect;
begin
  FConnected := False;
  closesocket(FSocket);
  //IocpRec := FSendOverlapped.Allocate(0);
  //IocpRec.IocpOperate := ioExit;
  //PostQueuedCompletionStatus(FIocpServer.FIocpHandle, 0, DWORD(Self), @IocpRec.Overlapped);
end;

function TSocketHandle.GetRemoteAddress: string;
begin
  GetRemoteInfo;
  Result := FRemoteAddress;
end;

function TSocketHandle.GetRemotePort: Word;
begin
  GetRemoteInfo;
  Result := FRemotePort;
end;

procedure TSocketHandle.GetRemoteInfo;
var
  SockAddrIn: TSockAddrIn;
  iSize: Integer;
begin
  if FRemoteAddress = '' then
  begin
    iSize := SizeOf(SockAddrIn);
    getpeername(FSocket, @SockAddrIn, iSize);
    FRemoteAddress := inet_ntoa(SockAddrIn.sin_addr);
    FRemotePort := ntohs(SockAddrIn.sin_port);
  end;
end;

function TSocketHandle.GetLocalAddress: string;
begin
  GetLocalInfo;
  Result := FLocalAddress;
end;

function TSocketHandle.GetLocalPort: Word;
begin
  GetLocalInfo;
  Result := FLocalPort;
end;

procedure TSocketHandle.GetLocalInfo;
var
  SockAddrIn: TSockAddrIn;
  iSize: Integer;
begin
  if FLocalAddress = '' then
  begin
    iSize := SizeOf(SockAddrIn);
    getsockname(FSocket, @SockAddrIn, iSize);
    FLocalAddress := inet_ntoa(SockAddrIn.sin_addr);
    FLocalPort := ntohs(SockAddrIn.sin_port);
  end;
end;

procedure TSocketHandle.PreRecv;
var
  iFlags, iTransfer: Cardinal;
  iErrCode: Integer;
begin
  FIocpRecv.Overlapped.Internal := 0;
  FIocpRecv.Overlapped.InternalHigh := 0;
  FIocpRecv.Overlapped.Offset := 0;
  FIocpRecv.Overlapped.OffsetHigh := 0;
  FIocpRecv.Overlapped.hEvent := 0;
  FIocpRecv.IocpOperate := ioRead;
  iFlags := 0;
  if WSARecv(FSocket, @FIocpRecv.WsaBuf, 1, iTransfer, iFlags, @FIocpRecv.Overlapped,
    nil) = SOCKET_ERROR then
  begin
    iErrCode := WSAGetLastError;
    if iErrCode = WSAECONNRESET then //�ͻ��˱��ر�
      FConnected := False;
    if iErrCode <> ERROR_IO_PENDING then //���׳��쳣�������쳣�¼�
    begin
      FIocpServer.DoError('WSARecv', GetLastWsaErrorStr);
      ProcessNetError(iErrCode);
    end;
  end;
end;

procedure TSocketHandle.ProcessIOComplete(AIocpRecord: PIocpRecord;
  const ACount: Cardinal);
begin
  FExecuting := True;
  try
    case AIocpRecord.IocpOperate of
      ioNone: Exit;
      ioRead: //�յ�����
      begin
        FActiveTime := Now;
        ReceiveData(AIocpRecord.WsaBuf.buf, ACount);
        if FConnected then
          PreRecv; //Ͷ������
      end;
      ioWrite: //����������ɣ���Ҫ�ͷ�AIocpRecord��ָ��
      begin
        FActiveTime := Now;
        FSendOverlapped.Release(AIocpRecord);
      end;
      ioStream:
      begin
        FActiveTime := Now;
        FSendOverlapped.Release(AIocpRecord);
        WriteStream; //����������
      end;
    end;
  finally
    FExecuting := False;
  end;
end;

procedure TSocketHandle.ReceiveData(AData: PAnsiChar; const ALen: Cardinal);
begin
  FInputBuf.Write(AData^, ALen);
  Process;
end;

procedure TSocketHandle.Process;
var
  AData, ALast, NewBuf: PByte;
  iLenOffset, iOffset, iReserveLen: Integer;

  function ReadLen: Integer;
  var
    wLen: Word;
    cLen: Cardinal;
  begin
    FInputBuf.Position := iOffset;
    if FLenType = ltWord then
    begin
      FInputBuf.Read(wLen, SizeOf(wLen));
      //wLen := ntohs(wLen);
      Result := wLen;
    end
    else
    begin
      FInputBuf.Read(cLen, SizeOf(cLen));
      //cLen := ntohl(cLen);
      Result := cLen;
    end;
  end;
begin
  case FLenType of
    ltWord, ltCardinal:
    begin
      if FLenType = ltWord then
        iLenOffset := 2
      else
        iLenOffset := 4;
      iReserveLen := 0;
      FPacketLen := 0;
      iOffset := 0;
      if FPacketLen <= 0 then
      begin
        if FInputBuf.Size < iLenOffset then Exit;
        FInputBuf.Position := 0; //�ƶ�����ǰ��
        FPacketLen := ReadLen;
        iOffset := iLenOffset;
        iReserveLen := FInputBuf.Size - iOffset;
        if FPacketLen > iReserveLen then //����һ�����ĳ���
        begin
          FInputBuf.Position := FInputBuf.Size; //�ƶ�������Ա���պ�������
          FPacketLen := 0;
          Exit;
        end;
      end;
      while (FPacketLen > 0) and (iReserveLen >= FPacketLen) do //������ݹ���������
      begin
        AData := Pointer(Longint(FInputBuf.Memory) + iOffset); //ȡ�õ�ǰ��ָ��
        Execute(AData, FPacketLen);
        iOffset := iOffset + FPacketLen; //�Ƶ���һ����
        FPacketLen := 0;
        iReserveLen := FInputBuf.Size - iOffset;
        if iReserveLen > iLenOffset then //ʣ�µ�����
        begin
          FPacketLen := ReadLen;
          iOffset := iOffset + iLenOffset;
          iReserveLen := FInputBuf.Size - iOffset;
          if FPacketLen > iReserveLen then //����һ�����ĳ��ȣ���Ҫ�ѳ��Ȼ���
          begin
            iOffset := iOffset - iLenOffset;
            iReserveLen := FInputBuf.Size - iOffset;
            FPacketLen := 0;
          end;
        end
        else //���������ֽ���
          FPacketLen := 0;
      end;
      if iReserveLen > 0 then //��ʣ�µ��Լ���������
      begin
        ALast := Pointer(Longint(FInputBuf.Memory) + iOffset);
        GetMem(NewBuf, iReserveLen);
        try
          CopyMemory(NewBuf, ALast, iReserveLen);
          FInputBuf.Clear;
          FInputBuf.Write(NewBuf^, iReserveLen);
        finally
          FreeMemory(NewBuf);
        end;
      end
      else
      begin
        FInputBuf.Clear;
      end;
    end;
  else
    begin
      FInputBuf.Position := 0;
      AData := Pointer(Longint(FInputBuf.Memory)); //ȡ�õ�ǰ��ָ��
      Execute(AData, FInputBuf.Size);
      FInputBuf.Clear;
    end;
  end;
end;

procedure TSocketHandle.Execute(AData: PByte; const ALen: Cardinal);
begin
end;

procedure TSocketHandle.ReadWord(AData: PAnsiChar; var AWord: Word;
  const AConvert: Boolean);
begin
  Move(AData^, AWord, SizeOf(AWord));
  if AConvert then
    AWord := ntohs(AWord);
end;

procedure TSocketHandle.ReadCardinal(AData: PAnsiChar; var ACardinal: Cardinal;
  const AConvert: Boolean);
begin
  Move(AData^, ACardinal, SizeOf(ACardinal));
  if AConvert then
    ACardinal := ntohl(ACardinal);
end;

procedure TSocketHandle.OpenWriteBuffer;
begin
  if Assigned(FOutputBuf) then
    FreeAndNil(FOutputBuf);
  FOutputBuf := TMemoryStream.Create;
end;

procedure TSocketHandle.FlushWriteBuffer(const AIocpOperate: TIocpOperate);
var
  IocpRec: PIocpRecord;
  iErrCode: Integer;
  dSend, dFlag: DWORD;
begin
  IocpRec := FSendOverlapped.Allocate(FOutputBuf.Size);
  IocpRec.Overlapped.Internal := 0;
  IocpRec.Overlapped.InternalHigh := 0;
  IocpRec.Overlapped.Offset := 0;
  IocpRec.Overlapped.OffsetHigh := 0;
  IocpRec.Overlapped.hEvent := 0;
  IocpRec.IocpOperate := AIocpOperate;
  System.Move(PAnsiChar(FOutputBuf.Memory)[0], IocpRec.WsaBuf.buf^, FOutputBuf.Size);
  dFlag := 0;
  if WSASend(FSocket, @IocpRec.WsaBuf, 1, dSend, dFlag, @IocpRec.Overlapped, nil) = SOCKET_ERROR then
  begin
    iErrCode := WSAGetLastError;
    if iErrCode <> ERROR_IO_PENDING then
    begin
      FIocpServer.DoError('WSASend', GetLastWsaErrorStr);
      ProcessNetError(iErrCode);
    end;
  end;
  FreeAndNil(FOutputBuf);
end;

procedure TSocketHandle.WriteBuffer(const ABuffer; const ACount: Integer);
begin
  FOutputBuf.WriteBuffer(ABuffer, ACount);
end;

procedure TSocketHandle.WriteWord(AValue: Word; const AConvert: Boolean = True);
begin
  if AConvert then
    AValue := htons(AValue);
  WriteBuffer(AValue, SizeOf(AValue));
end;

procedure TSocketHandle.WriteCardinal(AValue: Cardinal;
  const AConvert: Boolean);
begin
  if AConvert then
    AValue := htonl(AValue);
  WriteBuffer(AValue, SizeOf(AValue));
end;

procedure TSocketHandle.WriteInteger(AValue: Integer; const AConvert: Boolean);
begin
  WriteCardinal(Cardinal(AValue), AConvert);
end;

procedure TSocketHandle.WriteSmallInt(AValue: SmallInt; const AConvert: Boolean);
begin
  WriteWord(Word(AValue), AConvert);
end;

procedure TSocketHandle.WriteString(const AValue: string);
var
  iLen: Integer;
begin
  iLen := Length(AValue);
  if iLen > 0 then
    WriteBuffer(PChar(AValue)^, iLen);
end;

procedure TSocketHandle.WriteLn(const AValue: string);
begin
  WriteString(AValue + EOL);
end;

procedure TSocketHandle.WriteStream(const AWriteNow: Boolean);
begin
  //������Ҫ�̳д˺���
end;

procedure TSocketHandle.Lock;
begin
  FLock.Enter;
end;

procedure TSocketHandle.UnLock;
begin
  FLock.Leave;
end;

procedure TSocketHandle.SetTimeOut(const ATimeOutMS: Cardinal);
const
  SIO_KEEPALIVE_VALS = IOC_IN or IOC_VENDOR or 4; 
var
  InKeepAlive, OutKeepAlive: TTCPKeepAlive;
  Ret: Cardinal;
  Opt: BOOL;
begin
  //���´�����Լ�Ȿ�������ߵ��쳣������Է���������Ҫ����ѭ�����
  FTimeOutMS := ATimeOutMS;
  if FTimeOutMS > 0 then
  begin
    InKeepAlive.OnOff := 1;
    InKeepAlive.KeepAliveTime := ATimeOutMS; //�����ʱû���κ������ͶϿ�
    InKeepAlive.KeepAliveInterval := 3000; //ÿ����÷�������
  end
  else
  begin
    InKeepAlive.OnOff := 0;
  end;
  Opt := True;
  if setsockopt(FSocket, SOL_SOCKET, SO_KEEPALIVE, @Opt, SizeOf(Opt)) = SOCKET_ERROR then
  begin
    ESocketError.Create(GetLastWsaErrorStr);
  end;
  if WSAIoctl(FSocket, SIO_KEEPALIVE_VALS, @InKeepAlive, SizeOf(InKeepAlive),
    @OutKeepAlive, SizeOf(OutKeepAlive), @Ret, nil, nil) = SOCKET_ERROR then
  begin
    ESocketError.Create(GetLastWsaErrorStr);
  end;
end;

procedure TSocketHandle.ProcessNetError(const AErrorCode: Integer);
begin
  if AErrorCode = WSAECONNRESET then //�ͻ��˶Ͽ�����
    FConnected := False
  else if AErrorCode = WSAEDISCON then //�ͻ��˶Ͽ�����
    FConnected := False
  else if AErrorCode = WSAENETDOWN then //�����쳣
    FConnected := False
  else if AErrorCode = WSAENETRESET then //���������ص����쳣
    FConnected := False
  else if AErrorCode = WSAESHUTDOWN then //���ӱ��ر�
    FConnected := False
  else if AErrorCode = WSAETIMEDOUT then //���Ӷϵ����������쳣
    FConnected := False;
end;

{ TSocketHandles }

constructor TSocketHandles.Create;
begin
  FList := TList.Create;
  FIdleList := TList.Create;
  FLock := TCriticalSection.Create;
end;

destructor TSocketHandles.Destroy;
begin
  Clear;
  FList.Free;
  FIdleList.Free;
  FLock.Free;
  inherited;
end;

function TSocketHandles.GetItems(const AIndex: Integer): PClientSocket;
begin
  Result := FList[AIndex];
end;

function TSocketHandles.GetCount: Integer;
begin
  Result := FList.Count;
end;

function TSocketHandles.GetIdleCount: Integer;
begin
  Result := FIdleList.Count;
end;

procedure TSocketHandles.Clear;
var
  i: Integer;
  ClientSocket: PClientSocket;
begin
  for i := 0 to Count - 1 do
  begin
    ClientSocket := Items[i];
    ClientSocket.Lock.Free;
    ClientSocket.SocketHandle.Free;
    FreeMemory(ClientSocket.IocpRecv.WsaBuf.buf);
    ClientSocket.IocpRecv.WsaBuf.buf := nil;
    Dispose(ClientSocket);
  end;
  FList.Clear;
  for i := 0 to FIdleList.Count - 1 do
  begin
    ClientSocket := FIdleList[i];
    ClientSocket.Lock.Free; //�ͷ���
    FreeMemory(ClientSocket.IocpRecv.WsaBuf.buf);
    ClientSocket.IocpRecv.WsaBuf.buf := nil;
    Dispose(ClientSocket);
  end;
  FIdleList.Clear;
end;

procedure TSocketHandles.Lock;
begin
  FLock.Enter;
end;

procedure TSocketHandles.UnLock;
begin
  FLock.Leave;
end;

function TSocketHandles.Add(ASocketHandle: TSocketHandle): Integer;
var
  ClientSocket, IdleClientSocket: PClientSocket;
  i: Integer;
begin
  ClientSocket := nil;
  for i := FIdleList.Count - 1 downto 0 do
  begin
    IdleClientSocket := FIdleList.Items[i];
    if Abs(MinutesBetween(Now, IdleClientSocket.IdleDT)) > 30 then
    begin
      ClientSocket := IdleClientSocket;
      FIdleList.Delete(i);
      Break;
    end;
  end;
  if not Assigned(ClientSocket) then
  begin
    New(ClientSocket);
    ClientSocket.Lock := TCriticalSection.Create;
    ClientSocket.IocpRecv.WsaBuf.buf := GetMemory(MAX_IOCPBUFSIZE);
    ClientSocket.IocpRecv.WsaBuf.len := MAX_IOCPBUFSIZE;
  end;
  ClientSocket.SocketHandle := ASocketHandle;
  ClientSocket.IdleDT := Now;
  ASocketHandle.FLock := ClientSocket.Lock;
  ASocketHandle.FIocpRecv := @ClientSocket.IocpRecv;
  Result := FList.Add(ClientSocket);
end;

procedure TSocketHandles.Delete(const AIndex: Integer);
var
  ClientSocket: PClientSocket;
begin
  ClientSocket := FList[AIndex];
  ClientSocket.Lock.Enter;
  try
    ClientSocket.SocketHandle.Free;
    ClientSocket.SocketHandle := nil;
  finally
    ClientSocket.Lock.Leave;
  end;
  FList.Delete(AIndex);
  ClientSocket.IdleDT := Now;
  FIdleList.Add(ClientSocket);
end;

procedure TSocketHandles.Delete(ASocketHandle: TSocketHandle);
var
  i, iIndex: Integer;
begin
  iIndex := -1;
  for i := 0 to Count - 1 do
  begin
    if Items[i].SocketHandle = ASocketHandle then
    begin
      iIndex := i;
      Break;
    end;
  end;
  if iIndex <> -1 then
  begin
    Delete(iIndex);
  end;
end;

{ TIocpServer }

constructor TIocpServer.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FSocketHandles := TSocketHandles.Create;
  FWorkThreads := TWorkThreads.Create;
  FSocket := INVALID_SOCKET;
  FIocpHandle := 0;
  FMaxWorkThrCount := 160;
  FMinWorkThrCount := 120;
  FMaxCheckThrCount := 60;
  FMinCheckThrCount := 40;
  FAcceptThreadPool := TIocpThreadPool.Create(Self);
end;

destructor TIocpServer.Destroy;
begin
  if Active then
    Active := False;
  FWorkThreads.Free;
  FSocketHandles.Free;
  FAcceptThreadPool.Free;
  inherited;
end;

procedure TIocpServer.Open;
var
  WsaData: TWsaData;
  iNumberOfProcessors, i, iWorkThreadCount: Integer;
  WorkThread: TWorkThread;
  Addr: TSockAddr;
begin
  if WSAStartup($0202, WsaData) <> 0 then
    raise ESocketError.Create(GetLastWsaErrorStr);
  FIocpHandle := CreateIoCompletionPort(INVALID_HANDLE_VALUE, 0, 0, 0);
  if FIocpHandle = 0 then
    raise ESocketError.Create(GetLastErrorStr);
  FSocket := WSASocket(PF_INET, SOCK_STREAM, 0, nil, 0, WSA_FLAG_OVERLAPPED);
  if FSocket = INVALID_SOCKET then
    raise ESocketError.Create(GetLastWsaErrorStr);
  FillChar(Addr, SizeOf(Addr), 0);
  Addr.sin_family := AF_INET;
  Addr.sin_port := htons(FPort);
  Addr.sin_addr.S_addr := htonl(INADDR_ANY); //���κε�ַ�ϼ���������ж����������ÿ�鶼����
  if bind(FSocket, @Addr, SizeOf(Addr)) <> 0 then
    raise ESocketError.Create(GetLastWsaErrorStr);
  if listen(FSocket, MaxInt) <> 0 then 
     raise ESocketError.Create(GetLastWsaErrorStr);
  iNumberOfProcessors := GetCPUCount;
  iWorkThreadCount := iNumberOfProcessors * 2 + 4; //���ڷ�����������ܱȽϷ�ʱ�䣬����߳���ΪCPU*2+4
  if iWorkThreadCount < FMinWorkThrCount then
    iWorkThreadCount := FMinWorkThrCount;
  if iWorkThreadCount > FMaxWorkThrCount then
    iWorkThreadCount := FMaxWorkThrCount;
  for i := 0 to iWorkThreadCount - 1 do 
  begin
    WorkThread := TWorkThread.Create(Self, True);
    FWorkThreads.Add(WorkThread);
    WorkThread.Resume;
  end;
  FAcceptThreadPool.Active := True;
  FAcceptThread := TAcceptThread.Create(Self, True);
  FAcceptThread.Resume;
end;

procedure TIocpServer.Close;
var
  i: Integer;
begin
  FAcceptThread.Terminate;
  closesocket(FSocket);
  FAcceptThread.Free;
  FAcceptThreadPool.Active := False;
  FSocket := INVALID_SOCKET;
  //�˳�ִ���߳�
  for i := 0 to FWorkThreads.Count - 1 do
  begin
    FWorkThreads.Items[i].Terminate;
    PostQueuedCompletionStatus(FIocpHandle, 0, 0, Pointer(SHUTDOWN_FLAG));
  end;
  //�ͷ�ִ���߳�
  for i := 0 to FWorkThreads.Count - 1 do
  begin
    FWorkThreads.Items[i].Free;
  end;
  FWorkThreads.Clear;
  FSocketHandles.Clear;
  CloseHandle(FIocpHandle);
end;

procedure TIocpServer.SetActive(const AValue: Boolean);
begin
  if FActive = AValue then Exit;
  FActive := AValue;
  if AValue then
  begin
    try
      Open;
    except
      on E: Exception do
      begin
        FActive := False;
        raise Exception.Create(E.Message);
      end;
    end;
  end
  else
  begin
    try
      Close;
    except
      on E: Exception do
      begin
        FActive := True;
        raise Exception.Create(E.Message);
      end;
    end;
  end;
end;

procedure TIocpServer.AcceptClient;
var
  ClientSocket: TSocket;
begin
  ClientSocket := WSAAccept(FSocket, nil, nil, nil, 0);
  if ClientSocket <> INVALID_SOCKET then
  begin
    if not FActive then
    begin
      closesocket(ClientSocket);
      Exit;
    end;
    FAcceptThreadPool.PostSocket(ClientSocket);
  end;
end;

procedure TIocpServer.CheckClient(const ASocket: TSocket);
var
  SocketHandle: TSocketHandle;
  iIndex: Integer;
  ClientSocket: PClientSocket;
begin
  SocketHandle := nil;
  if not DoConnect(ASocket, SocketHandle) then //������������ӣ����˳�
  begin
    closesocket(ASocket);
    Exit;
  end;
  FSocketHandles.Lock; //�ӵ��б���
  try
    iIndex := FSocketHandles.Add(SocketHandle);
    ClientSocket := FSocketHandles.Items[iIndex];
  finally
    FSocketHandles.UnLock;
  end;
  if CreateIoCompletionPort(ASocket, FIOCPHandle, DWORD(ClientSocket), 0) = 0 then
  begin
    DoError('CreateIoCompletionPort', GetLastWsaErrorStr);
    FSocketHandles.Lock; //���Ͷ�ݵ��б���ʧ�ܣ���ɾ��
    try
      FSocketHandles.Delete(iIndex);
    finally
      FSocketHandles.UnLock;
    end;
  end
  else
  begin
    SocketHandle.PreRecv; //Ͷ�ݽ�������
  end;
end;

function TIocpServer.DoConnect(const ASocket: TSocket; var SocketHandle: TSocketHandle): Boolean;
begin
  Result := True;
  FConnectTime := Now;
  if Assigned(FOnConnect) then
  begin
    FOnConnect(ASocket, Result, SocketHandle);
  end;
end;

procedure TIocpServer.DoError(const AName, AError: string);
begin
  if Assigned(FOnError) then
    FOnError(AName, AError);
end;

function TIocpServer.WorkClient: Boolean;
var
  ClientSocket: PClientSocket;
  IocpRecord: PIocpRecord;
  iWorkCount: Cardinal;
begin
  IocpRecord := nil;
  iWorkCount := 0;
  ClientSocket := nil;
  Result := False;
  if not GetQueuedCompletionStatus(FIocpHandle, iWorkCount, DWORD(ClientSocket),
    POverlapped(IocpRecord), INFINITE) then //�˴��п��ܶ���̴߳���ͬһ��SocketHandle���������Ҫ����
  begin  //�ͻ����쳣�Ͽ�
    if Assigned(ClientSocket) and Assigned(ClientSocket.SocketHandle) then
    begin
      ClientSocket.SocketHandle.FConnected := False;
      Exit;
    end;
  end;
  if Cardinal(IocpRecord) = SHUTDOWN_FLAG then
    Exit;
  if not FActive then
    Exit;
  Result := True;

  if Assigned(ClientSocket) and Assigned(ClientSocket.SocketHandle) then
  begin
    if ClientSocket.SocketHandle.Connected then
    begin
      if iWorkCount > 0 then //������յ�������
      begin
        try
          ClientSocket.Lock.Enter;
          try
            if Assigned(ClientSocket.SocketHandle) then
            begin
              ClientSocket.SocketHandle.ProcessIOComplete(IocpRecord, iWorkCount);
              if not ClientSocket.SocketHandle.Connected then
                FreeSocketHandle(ClientSocket.SocketHandle);
            end;
          finally
            ClientSocket.Lock.Leave;
          end;
        except
          on E: Exception do
            DoError('ProcessIOComplete', E.Message);
        end;
      end
      else //�����ɸ���Ϊ0����״̬Ϊ���գ����ͷ�����
      begin
        if IocpRecord.IocpOperate = ioRead then
        begin
          ClientSocket.Lock.Enter;
          try
            if Assigned(ClientSocket.SocketHandle) then
              FreeSocketHandle(ClientSocket.SocketHandle);
          finally
            ClientSocket.Lock.Leave;
          end;
        end
        else
          DoError('WorkClient', 'WorkCount = 0, Code: ' + IntToStr(GetLastError) + ', Message: ' + GetLastErrorStr);
      end;
    end
    else //�Ͽ�����
    begin
      ClientSocket.Lock.Enter;
      try
        if Assigned(ClientSocket.SocketHandle) then
          FreeSocketHandle(ClientSocket.SocketHandle);
      finally
        ClientSocket.Lock.Leave;
      end;
    end;
  end
  else //WorkCountΪ0��ʾ�������쳣����¼��־
    DoError('GetQueuedCompletionStatus', 'Return SocketHandle nil');
end;

procedure TIocpServer.DoDisconnect(const ASocketHandle: TSocketHandle);
begin
  if Assigned(FOnDisconnect) then
    FOnDisconnect(ASocketHandle);
end;

procedure TIocpServer.FreeSocketHandle(const ASocketHandle: TSocketHandle);
begin
  DoDisconnect(ASocketHandle);
  FSocketHandles.Lock;
  try
    FSocketHandles.Delete(ASocketHandle);
  finally
    FSocketHandles.UnLock;
  end;
end;

procedure TIocpServer.FreeSocketHandle(const AIndex: Integer);
begin
  DoDisconnect(FSocketHandles.Items[AIndex].SocketHandle);
  FSocketHandles.Lock;
  try
    FSocketHandles.Delete(AIndex);
  finally
    FSocketHandles.UnLock;
  end;
end;

function TIocpServer.ReadChar(const ASocket: TSocket; var AChar: Char; const ATimeOutMS: Integer): Boolean;
var
  iRead: Integer;
begin
  Result := CheckTimeOut(ASocket, ATimeOutMS);
  if Result then
  begin
    iRead := recv(ASocket, AChar, 1, 0);
    Result := iRead = 1; 
  end;
end;

function TIocpServer.CheckTimeOut(const ASocket: TSocket;
  const ATimeOutMS: Integer): Boolean;
var
  tmTo: TTimeVal;
  FDRead: TFDSet;
begin
  FillChar(FDRead, SizeOf(FDRead), 0);
  FDRead.fd_count := 1;
  FDRead.fd_array[0] := ASocket;
  tmTo.tv_sec := ATimeOutMS div 1000;
  tmTo.tv_usec := (ATimeOutMS mod 1000) * 1000;
  Result := Select(0, @FDRead, nil, nil, @tmTO) = 1;
end;

procedure TIocpServer.CheckDisconnectedClient;
var
  iCount: Integer;
  ClientSocket: PClientSocket;

  function GetDisconnectSocket: PClientSocket;
  var
    i: Integer;
  begin
    Result := nil;
    FSocketHandles.Lock;
    try
      for i := FSocketHandles.Count - 1 downto 0 do
      begin
        if (not FSocketHandles.Items[i].SocketHandle.Connected)
          and (not FSocketHandles.Items[i].SocketHandle.Executing) then
        begin
          Result := FSocketHandles.Items[i];
          Break;
        end;
      end;
    finally
      FSocketHandles.UnLock;
    end;
  end;
begin
  ClientSocket := GetDisconnectSocket;
  iCount := 0;
  while (ClientSocket <> nil) and (iCount < 1024 * 1024) do
  begin
    //WriteLogMsg(ltWarn, 'CheckDisconnectedClient Free Socket Handle, Idle Time: '
    //  + IntToStr(SecondsBetween(Now, ClientSocket.IdleDT)));
    ClientSocket.Lock.Enter;
    try
      if Assigned(ClientSocket.SocketHandle) then
        FreeSocketHandle(ClientSocket.SocketHandle);
    finally
      ClientSocket.Lock.Leave;
    end;
    //WriteLogMsg(ltWarn, 'CheckDisconnectedClient Get Next Disconnected Socket');
    ClientSocket := GetDisconnectSocket;
    Inc(iCount);
  end;
end;

{ TIocpThread }

constructor TIocpThread.Create(const AServer: TIocpServer; CreateSuspended: Boolean);
begin
  inherited Create(CreateSuspended);
  FIocpServer := AServer;
end;

{ TAcceptThread }

procedure TAcceptThread.Execute;
begin
  inherited;
  while (not Terminated) and FIocpServer.Active do
  begin
    try
      FIocpServer.AcceptClient;
    except
    end;
  end;
end;

{ TWorkThread }

procedure TWorkThread.Execute;
begin
  inherited;
  CoInitialize(nil);
  try
    while (not Terminated) and FIocpServer.Active do
    begin
      try
        if not FIocpServer.WorkClient then Continue;
      except  //����һ���쳣����ֹ�����߳�
        on E: Exception do
          FIocpServer.DoError('WorkClient', E.Message);
      end;
    end;
  finally
    CoUninitialize;
  end;
end;

{ TWorkThreads }

constructor TWorkThreads.Create;
begin
  inherited Create(TWorkThread);
end;

function TWorkThreads.GetItems(AIndex: Integer): TWorkThread;
begin
  Result := TWorkThread(inherited Items[AIndex]);
end;

procedure TWorkThreads.SetItems(AIndex: Integer;
  AValue: TWorkThread);
begin
  inherited Items[AIndex] := AValue;
end;

{ TCheckThread }

procedure TCheckThread.Execute;
var
  Socket: TSocket;
  iWorkCount: Cardinal;
  Overlapped: POverlapped;
begin
  inherited;
  CoInitialize(nil);
  try
    while (not Terminated) and FIocpServer.Active do
    begin
      if not GetQueuedCompletionStatus(FIocpThreadPool.FIocpHandle, iWorkCount, Socket,
        Overlapped, INFINITE) then Continue;
      if Cardinal(Overlapped) = SHUTDOWN_FLAG then Exit; //�˳���־
      try
        FIocpServer.CheckClient(Socket);
      except
        on E: Exception do
        begin
          FIocpServer.DoError('CheckClient', E.Message);
        end;
      end;
    end;
  finally
    CoUninitialize;
  end;
end;

{ TCheckThreads }

constructor TCheckThreads.Create;
begin
  inherited Create(TCheckThread);
end;

function TCheckThreads.GetItems(AIndex: Integer): TCheckThread;
begin
  Result := TCheckThread(inherited Items[AIndex]);
end;

procedure TCheckThreads.SetItems(AIndex: Integer; AValue: TCheckThread);
begin
  inherited Items[AIndex] := AValue;
end;

{ TIocpThreadPool }

constructor TIocpThreadPool.Create(const AServer: TIocpServer);
begin
  FIocpServer := AServer;
  FCheckThreads := TCheckThreads.Create;
end;

destructor TIocpThreadPool.Destroy;
begin
  FCheckThreads.Free;                
  inherited;
end;

procedure TIocpThreadPool.Open;
var
  i, iCount: Integer;
  CheckThread: TCheckThread;
begin
  FIocpHandle := CreateIoCompletionPort(INVALID_HANDLE_VALUE, 0, 0, 0);
  iCount := GetCPUCount * 2;
  if iCount < FIocpServer.MinCheckThrCount then
    iCount := FIocpServer.MinCheckThrCount;
  if iCount > FIocpServer.MaxCheckThrCount then
    iCount := FIocpServer.MaxCheckThrCount;
  for i := 1 to iCount do
  begin
    CheckThread := TCheckThread.Create(FIocpServer, True);
    FCheckThreads.Add(CheckThread);
    CheckThread.IocpThreadPool := Self;
    CheckThread.Resume;
  end;
end;

procedure TIocpThreadPool.Close;
var
  i: Integer;
begin
  //�˳��߳�ִ��
  for i := 0 to FCheckThreads.Count - 1 do
  begin
    FCheckThreads.Items[i].Terminate;
    PostQueuedCompletionStatus(FIocpHandle, 0, 0, Pointer(SHUTDOWN_FLAG));
  end;
  //�ͷ��߳�
  for i := 0 to FCheckThreads.Count - 1 do
  begin
    FCheckThreads.Items[i].Free;
  end;
  FCheckThreads.Clear;
  CloseHandle(FIocpHandle);
end;

procedure TIocpThreadPool.SetActive(const AValue: Boolean);
begin
  if FActive = AValue then Exit;
  if AValue then
    Open
  else
    Close;
  FActive := AValue;
end;

procedure TIocpThreadPool.PostSocket(const ASocket: TSocket);
begin
  PostQueuedCompletionStatus(FIocpHandle, 0, ASocket, nil);
end;

end.
