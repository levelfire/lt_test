unit SQLSocket;

interface

uses
  SysUtils, Classes, IOCPSocket, ADODB, DB, Windows, DefineUnit, BaseSocket;

type
  {* SQL��ѯSOCKET���� *}
  TSQLSocket = class(TBaseSocket)
  private
    {* ��ʼ���񴴽�TADOConnection���ر�����ʱ�ͷ� *}
    FBeginTrans: Boolean;
    FADOConn: TADOConnection;
  protected
    {* �������ݽӿ� *}
    procedure Execute(AData: PByte; const ALen: Cardinal); override;
    {* ����SQL���ִ�н�� *}
    procedure DoCmdSQLOpen;
    {* ִ��SQL��� *}
    procedure DoCmdSQLExec;
    {* ��ʼ���� *}
    procedure DoCmdBeginTrans;
    {* �ύ���� *}
    procedure DoCmdCommitTrans;
    {* �ع����� *}
    procedure DoCmdRollbackTrans;
  public
    procedure DoCreate; override;
    destructor Destroy; override;
    {* ��ȡSQL��� *}
    function GetSQL: string;
    property BeginTrans: Boolean read FBeginTrans;
  end;

implementation

uses Logger, DBConnect, BasisFunction;

{ TSQLSocket }

destructor TSQLSocket.Destroy;
begin
  if FBeginTrans then //�����ʼ��������ع�
  begin
    WriteLogMsg(ltError, Format('[%s] trans time out, rollback', [FUserName]));
    FADOConn.RollbackTrans;
  end;
  FreeAndNilEx(FSendStream);
  inherited;
end;

procedure TSQLSocket.DoCreate;
begin
  inherited;
  LenType := ltCardinal;
  FSocketFlag := sfSQL;
end;

procedure TSQLSocket.Execute(AData: PByte; const ALen: Cardinal);
var
  sErr: string;
  SQLCmd: TSQLCmd;
begin
  inherited;
  FRequest.Clear;
  FResponse.Clear;
  try
    AddResponseHeader;
    if ALen = 0 then
    begin
      DoFailure(CIPackLenError);
      DoSendResult(True);
      Exit;
    end;
    if DecodePacket(AData, ALen) then
    begin
      FResponse.Clear;
      AddResponseHeader;
      SQLCmd := StrToSQLCommand(Command);
      if (not (SQLCmd in [scLogin, scActive])) and (not FLogined) then
      begin //�����ȵ�½�����ܷ���ҵ��
        DoFailure(CINotLogin);
        DoSendResult(True);
        Exit;
      end;
      case SQLCmd of
        scLogin:
        begin
          DoCmdLogin;
          DoSendResult(True);
        end;
        scActive:
        begin
          DoSuccess;
          DoSendResult(True);
        end;
        scSQLOpen:
        begin
          DoCmdSQLOpen;
        end;
        scSQLExec:
        begin
          DoCmdSQLExec;
          DoSendResult(True);
        end;
        scBeginTrans:
        begin
          DoCmdBeginTrans;
          DoSendResult(True);
        end;
        scCommitTrans:
        begin
          DoCmdCommitTrans;
          DoSendResult(True);
        end;
        scRollbackTrans:
        begin
          DoCmdRollbackTrans;
          DoSendResult(True);
        end;
      else
        DoFailure(CINoExistCommand, 'Unknow Command');
        DoSendResult(True);
      end;
    end
    else
    begin
      DoFailure(CIPackFormatError, 'Packet Must Include \r\n\r\n');
      DoSendResult(True);
    end;
  except
    on E: Exception do //����δ֪���󣬶Ͽ�����
    begin
      sErr := RemoteAddress + ':' + IntToStr(RemotePort) + CSComma + 'Unknow Error: ' + E.Message;
      WriteLogMsg(ltError, sErr);
      Disconnect;
    end;
  end;
end;

procedure TSQLSocket.DoCmdSQLOpen;
var
  ADOConn: TADOConnection;
  Qry: TADOQuery;
  sFileName: string;
begin
  try
    GDBConnect.GetADOConnection(ADOConn);
    Qry := TADOQuery.Create(nil);
    try
      Qry.Connection := ADOConn;
      GDBConnect.SQLOpen(Qry, GetSQL); //�������ͨ�������ѯ
      sFileName := GetTmpFileEx(ExtractFilePath(GetModuleName(HInstance)), 'SQLOpen', '.xml');
      WriteLogMsg(ltInfo, 'SQLOpen FileName: ' + sFileName);
      Qry.SaveToFile(sFileName, pfXML);
      DoSuccess; //�·��ɹ�
      OpenWriteBuffer;
      DoSendResult(False);
      SendFile(sFileName, 0, 1024 * 1024, True, False); //�·���ѯ����ļ�
      FlushWriteBuffer(ioStream);
    finally
      Qry.Free;
      GDBConnect.ReleaseADOConnetion(ADOConn);
    end;
  except
    on E: Exception do
    begin
      DoFailure(CISQLOpenError);
      FResponse.Add(Format(CSFmtString, [CSMessage, E.Message]));
      DoSendResult(True);
    end;
  end;
end;

procedure TSQLSocket.DoCmdSQLExec;
var
  ADOConn: TADOConnection;
  Qry: TADOQuery;
  iEffectRow: Integer;
begin
  if not FBeginTrans then
    GDBConnect.GetADOConnection(ADOConn);
  try
    Qry := TADOQuery.Create(nil);
    if not FBeginTrans then
      Qry.Connection := ADOConn
    else
      Qry.Connection := FADOConn;
    try
      iEffectRow := GDBConnect.SQLExce(Qry, GetSQL); 
      DoSuccess;
      FResponse.Add(Format(CSFmtString, [CSEffectRow, IntToStr(iEffectRow)])); //�·�Ӱ������
    except
      on E: Exception do
      begin
        DoFailure(CISQLExecError);
        FResponse.Add(Format(CSFmtString, [CSMessage, E.Message]));
      end;
    end;
  finally
    if not FBeginTrans then
      GDBConnect.ReleaseADOConnetion(ADOConn);
  end;
end;

procedure TSQLSocket.DoCmdBeginTrans;
begin
  if FBeginTrans then //�Ѿ���ʼ���������ٿ�ʼ����
  begin
    DoFailure(CIHavedBeginTrans);
    Exit;
  end;
  try
    GDBConnect.GetADOConnection(FADOConn);
    FADOConn.BeginTrans;
    FBeginTrans := True;
    DoSuccess;
  except
    on E: Exception do
    begin
      DoFailure(CIBeginTransError);
      FResponse.Add(Format(CSFmtString, [CSMessage, E.Message]));
    end;
  end;
end;

procedure TSQLSocket.DoCmdCommitTrans;
begin
  if not FBeginTrans then //���û�п�ʼ���������ύ
  begin
    DoFailure(CINotExistTrans);
    Exit;
  end;
  try
    FADOConn.CommitTrans;
    GDBConnect.ReleaseADOConnetion(FADOConn);
    FADOConn := nil;
    FBeginTrans := False;
    DoSuccess;
  except
    on E: Exception do
    begin
      DoFailure(CICommitTransError);
      FResponse.Add(Format(CSFmtString, [CSMessage, E.Message]));
    end;
  end;
end;

procedure TSQLSocket.DoCmdRollbackTrans;
begin
  if not FBeginTrans then //���û�п�ʼ�������ܻع�
  begin
    DoFailure(CINotExistTrans);
    Exit;
  end;
  try
    FADOConn.RollbackTrans;
    GDBConnect.ReleaseADOConnetion(FADOConn);
    FADOConn := nil;
    FBeginTrans := False;
    DoSuccess;
  except
    on E: Exception do
    begin
      DoFailure(CIRollbackTransError);
      FResponse.Add(Format(CSFmtString, [CSMessage, E.Message]));
    end;
  end;
end;

function TSQLSocket.GetSQL: string;
var
  utfSQL: UTF8String;
begin
  SetLength(utfSQL, FRequestDataLen);
  CopyMemory(@utfSQL[1], FRequestData, FRequestDataLen); //��ȡ����
  Result := Utf8ToAnsi(utfSQL);
end;

end.
