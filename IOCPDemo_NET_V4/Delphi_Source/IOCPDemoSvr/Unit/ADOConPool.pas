unit ADOConPool;

interface

uses
  Windows, Messages, SysUtils, Classes, ADODB, SyncObjs, ActiveX, DateUtils;

type
  TADOConRec = record
    AdoCon: TADOConnection;
    Idle: Boolean;
    ReleaseDT: TDateTime;
  end;
  PADOConRec = ^TADOConRec;

  TMonitorThread = class;
  TADOConPool = class
  private
    {* ���Ӷ��� *}
    FADOConList: TList;
    {* ��ȡ������ *}
    FCurrIndex: Integer;
    {* �߳��� *}
    FLock: TCriticalSection;
    {* �����ַ��� *}
    FConnectionString: string;
    {* ���Ӷ��� *}
    FMonitorThread: TMonitorThread;
    procedure Connect(AdoCon: TADOConnection);
    {* ��������Ƿ���ڣ������� *}
    procedure CheckReConnect(AdoCon: TADOConnection);
    {* ����б� *}
    procedure Clear;
    {* �ͷų�ʱ��û��ʹ�õ����� *}
    procedure FreeIdleConnection;
    {* �������Ӵ� *}
    procedure SetConnectionString(const AValue: string);
  public
    constructor Create; virtual;
    destructor Destroy; override;
    {* ��ȡһ������ *}
    procedure GetConnection(var AdoCon: TADOConnection);
    procedure ReleaseConnection(var AdoCon: TADOConnection);

    property ConnectionString: string read FConnectionString write SetConnectionString;
  end;

  TMonitorThread = class(TThread)
  private
    FADOConPool: TADOConPool;
  protected
    procedure Execute; override;
  end;

implementation

uses BasisFunction, Logger;

{ TADOConPool }

constructor TADOConPool.Create;
begin
  inherited Create;
  FADOConList := TList.Create;
  FCurrIndex := 0;
  FLock := TCriticalSection.Create;
  FMonitorThread := TMonitorThread.Create(True);
  FMonitorThread.FreeOnTerminate := False;
  FMonitorThread.FADOConPool := Self;
  FMonitorThread.Resume;
end;

destructor TADOConPool.Destroy;
begin
  FMonitorThread.Terminate;
  FMonitorThread.WaitFor;
  FMonitorThread.Free;
  FLock.Free;
  Clear;
  FADOConList.Free;
  inherited;
end;

procedure TADOConPool.Connect(AdoCon: TADOConnection);
begin
  if AdoCon.Connected then
    AdoCon.Connected := False;
  AdoCon.ConnectionString := FConnectionString;
  AdoCon.Connected := True;
end;

procedure TADOConPool.CheckReConnect(AdoCon: TADOConnection);
var
  Qry: TADOQuery;
begin
  try
    Qry := TADOQuery.Create(nil);
    try
      Qry.Connection := AdoCon;
      Qry.Close;
      Qry.SQL.Text := 'SELECT 1';
      Qry.Open;
    finally
      Qry.Free;
    end;
  except
    on E: Exception do
    begin
      Connect(AdoCon);
    end;
  end;
end;

procedure TADOConPool.GetConnection(var AdoCon: TADOConnection);
var
  i: Integer;
  ADOConRec: PADOConRec;
begin
  FLock.Enter;
  try
    if FCurrIndex >= FADOConList.Count then
      FCurrIndex := FADOConList.Count - 1;
    if FCurrIndex < 0 then
      FCurrIndex := 0;
    ADOConRec := nil;
    for i := FCurrIndex to FADOConList.Count - 1 do //�ӵ�ǰλ�ò���
    begin
      if PADOConRec(FADOConList[i]).Idle then
      begin
        ADOConRec := FADOConList[i];
        FCurrIndex := i;
        Break;
      end;
    end;
    if ADOConRec = nil then
    begin
      for i := 0 to FCurrIndex - 1 do  //��ͷ���ҵ���ǰλ��
      begin
        if PADOConRec(FADOConList[i]).Idle then
        begin
          ADOConRec := FADOConList[i];
          FCurrIndex := i;
          Break;
        end;
      end;
    end;
    if ADOConRec <> nil then //�ҵ���
    begin
      ADOConRec.Idle := False;
      AdoCon := ADOConRec.AdoCon;
      CheckReConnect(AdoCon);
    end
    else
    begin //����������������
      CoInitialize(nil);
      try
        New(ADOConRec);
        ADOConRec.AdoCon := TADOConnection.Create(nil);
        ADOConRec.AdoCon.LoginPrompt := False;
        ADOConRec.AdoCon.CommandTimeout := 120;
        ADOConRec.Idle := False;
        FCurrIndex := FADOConList.Add(ADOConRec);
        AdoCon := ADOConRec.AdoCon;
        
        Connect(AdoCon);
      finally
        CoUninitialize;
      end;
    end;
  finally
    FLock.Leave;
  end;
end;

procedure TADOConPool.ReleaseConnection(var AdoCon: TADOConnection);
var
  i: Integer;
  AdoConRec: PADOConRec;
begin
  FLock.Enter;
  try
    if FCurrIndex >= FADOConList.Count then
      FCurrIndex := FADOConList.Count - 1;
    if FCurrIndex < 0 then
      FCurrIndex := 0;
    AdoConRec := nil;
    for i := FCurrIndex downto 0 do //�ӵ�ǰλ�����²���
    begin
      AdoConRec := FADOConList[i];
      if not Assigned(FADOConList[i]) then Continue;   //�˴�FADOConList[i]����Ϊ��
      if AdoConRec.AdoCon = AdoCon then
      begin
        AdoConRec.Idle := True;
        AdoConRec.ReleaseDT := GMTNow;
        FCurrIndex := i;
        Exit;
      end;
    end;
    for i := FCurrIndex + 1 to FADOConList.Count - 1 do
    begin
      AdoConRec := FADOConList[i];
      if not Assigned(FADOConList[i]) then Continue;   //�˴�FADOConList[i]����Ϊ��
      if AdoConRec.AdoCon = AdoCon then
      begin
        AdoConRec.Idle := True;
        AdoConRec.ReleaseDT := GMTNow;
        FCurrIndex := i;
        Exit;
      end;
    end; 
  finally
    FLock.Leave;
  end;
end;

procedure TADOConPool.Clear;
var
  i: Integer;
  AdoConRec: PADOConRec;
begin
  for i := 0 to FADOConList.Count - 1 do
  begin
    AdoConRec := FADOConList.Items[i];
    FreeAndNilEx(AdoConRec.AdoCon);
    Dispose(AdoConRec);
  end;
  FADOConList.Clear;
end;

procedure TADOConPool.FreeIdleConnection;
var
  i: Integer;
  AdoConRec: PADOConRec;
begin
  FLock.Enter;
  try
    for i := FADOConList.Count - 1 downto 0  do
    begin
      AdoConRec := FADOConList[i];
      if AdoConRec.Idle then
      begin
        if MinutesBetween(GMTNow, AdoConRec.ReleaseDT) >= 10 then //����10����û��ʹ�õ����ӣ��ͷ�
        begin
          try
            try
              FreeAndNilEx(AdoConRec.AdoCon);
              Dispose(AdoConRec);
            except
              on E: Exception do
              begin
                WriteLogMsg(ltError, Format('Error at TADOConPool.FreeIdleConnection, Current Index: %d, Total Count: %d, Message: %s',
                  [i, FADOConList.Count, E.Message]));
              end;
            end;
          finally
            //����ͷ�AdoConRec����������²�ִ��FADOConList.Delete(i)�����������ڸ��쳣�����
            //�������´η��ʾͻ�����˴���ȷ��������ɾ����������Ӱ�������ط������������ݿ⡣
            FADOConList.Delete(i);
          end;
          //��λ
          FCurrIndex := 0;
        end;
      end;
    end;
  finally
    FLock.Leave;
  end;
end;

procedure TADOConPool.SetConnectionString(const AValue: string);
var
  i: Integer;
  AdoConRec: PADOConRec;
begin
  for i := 0 to FADOConList.Count - 1 do
  begin
    AdoConRec := FADOConList.Items[i];
    if AdoConRec.AdoCon.Connected then
      AdoConRec.AdoCon.Connected := False;
    AdoConRec.AdoCon.ConnectionString := AValue;
  end;
  FConnectionString := AValue;
end;

{ TMonitorThread }

procedure TMonitorThread.Execute;
var
  i: Integer;
begin
  inherited;
  while not Terminated do
  begin
    for i := 0 to 5 * 60 * 10 do //5���Ӽ��һ��
    begin
      if Terminated then Exit;
      Sleep(100);
    end;
    try
      FADOConPool.FreeIdleConnection;
    except
      ;
    end;
  end;
end;

end.

