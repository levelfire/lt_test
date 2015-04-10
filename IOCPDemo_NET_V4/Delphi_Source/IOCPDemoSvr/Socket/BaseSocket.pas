unit BaseSocket;

interface

uses
  SysUtils, Classes, IOCPSocket, ADODB, DB, Windows, DateUtils, ActiveX,
  DefineUnit, ZLibEx;

type
  TBaseSocketClass = class of TBaseSocket;
  {* SOCKETЭ�鴦����� *}
  TBaseSocket = class(TSocketHandle)
  protected
    {* Э������ *}
    FSocketFlag: TSocketFlag;
    {* ���ݰ� *}
    FRequestData: PByte;
    FRequestDataLen: Integer;
    {* Э�������ͽ��մ� *}
    FRequest, FResponse: TStrings;
    {* ��½���û� *}
    FUserName: string;
    {* �Ƿ��Ѿ���½ *}
    FLogined: Boolean;
    {* Э��У������� *}
    FErrorCode: Integer;
    {* ������ *}
    FSendStream: TStream;
    FSendFile: string;
    {* �Ƿ�ɾ��������ɵ��ļ� *}
    FDeleteFile: Boolean;
    {* ��ȡ���ݻ��� *}
    FPacketSize: Integer;
    FSendBuffer: array of Byte;
    {* ���ͽ�� *}
    procedure DoSendResult(const AWriteNow: Boolean); virtual;
    {* �ַ���������� *}
    function DecodePacket(APacketData: PByte; const ALen: Integer): Boolean; virtual;
    {* �������� *}
    function GetCommand: string;
    {* ����ɹ�ִ�� *}
    procedure DoSuccess;
    {* ����ش��� *}
    procedure DoFailure(const ACode: Integer); overload;
    procedure DoFailure(const ACode: Integer; const AMessage: string); overload;
    {* ��ӷ�������ͷ *}
    procedure AddResponseHeader;
    {* �������Ϸ��� *}
    function CheckCommandKey(const ACommandKey: string): Boolean;
    function CheckAndGetValue(const ACommandKey: string; var AValue: string): Boolean; overload;
    function CheckAndGetValue(const ACommandKey: string; var AValue: Integer): Boolean; overload;
    function CheckAndGetValue(const ACommandKey: string; var AValue: Int64): Boolean; overload;
    function CheckAndGetValue(const ACommandKey: string; var AValue: Cardinal): Boolean; overload;
    function CheckAndGetValue(const ACommandKey: string; var AValue: Word): Boolean; overload;
    {* ���ڷ��ʹ��ļ���Ҫ���ûص�������ȫ��ѹ����ɶ˿��У����������ڴ���� *}
    procedure WriteStream(const AWriteNow: Boolean = True); override;
    {* ��¼�ŵ����� *}
    procedure DoCmdLogin; virtual;
  public
    procedure DoCreate; override;
    destructor Destroy; override;
    {* �����ļ�, APacketSizeΪÿ���·���С����λΪKB *}
    procedure SendFile(const AFileName: string; const APosition: Int64;
      const APacketSize: Cardinal; const ADeleteFile: Boolean; const AWriteNow: Boolean);
    procedure SendStream(AStream: TStream; const APacketSize: Cardinal; const AWriteNow: Boolean);
    {* ��������string���� *}
    function GetRequestDataStr: string;
    property Command: string read GetCommand;
    property SocketFlag: TSocketFlag read FSocketFlag;
    property UserName: string read FUserName;
  end;

implementation

uses BasisFunction, Logger;

{ TBaseSocket }

destructor TBaseSocket.Destroy;
begin
  FRequest.Free;
  FResponse.Free;
  inherited;
end;

procedure TBaseSocket.DoCreate;
begin
  inherited;
  FRequest := TStringList.Create;
  FResponse := TStringList.Create;
end;

function TBaseSocket.DecodePacket(APacketData: PByte;
  const ALen: Integer): Boolean;
var
  CommandLen: Integer;
  UTF8Command: UTF8String;
begin
  if ALen > 4 then //�����Ϊ4�ֽڣ�������ȱ������4
  begin
    CopyMemory(@CommandLen, APacketData, SizeOf(Cardinal)); //��ȡ�����
    Inc(APacketData, SizeOf(Cardinal));
    SetLength(UTF8Command, CommandLen);
    CopyMemory(PUTF8String(UTF8Command), APacketData, CommandLen); //��ȡ����
    Inc(APacketData, CommandLen);
    FRequestData := APacketData; //����
    FRequestDataLen := ALen - SizeOf(Cardinal) - CommandLen; //���ݳ���
    FRequest.Text := Utf8ToAnsi(UTF8Command); //��UTF8תΪAnsi
    Result := True;
  end
  else
    Result := False; 
end;

procedure TBaseSocket.DoSendResult(const AWriteNow: Boolean);
var
  wLen: Word;
  cLen: Cardinal;
  utf8Buff: UTF8String;
begin
  if not Connected then Exit;
  if AWriteNow then
    OpenWriteBuffer;
  utf8Buff := AnsiToUtf8(FResponse.Text); //תΪUTF8
  if LenType = ltWord then
  begin
    wLen := Length(utf8Buff) + SizeOf(Cardinal); //�ܳ���
    WriteWord(wLen, False);
  end
  else if LenType = ltCardinal then
  begin
    cLen := Length(utf8Buff) + SizeOf(Cardinal); //�ܳ���
    WriteCardinal(cLen, False);
  end;
  WriteCardinal(Length(utf8Buff), False); //�·������
  WriteBuffer(PUTF8String(utf8Buff)^, Length(utf8Buff)); //��������
  if AWriteNow then
    FlushWriteBuffer(ioWrite); //ֱ�ӷ���
end;

function TBaseSocket.GetCommand: string;
begin
  Result := FRequest.Values[CSCommand];
end;

procedure TBaseSocket.DoSuccess;
begin
  FResponse.Add(CSCode + CSEqualSign + IntToStr(CISuccessCode));
end;

procedure TBaseSocket.DoFailure(const ACode: Integer);
begin
  FResponse.Values[CSCode] := IntToStr(ACode);
  WriteLogMsg(ltError, GetSocketFlagStr(FSocketFlag) + CSComma + RemoteAddress + ':'
    + IntToStr(RemotePort) + CSComma + Command + ' Failed, Error Code: 0x' + IntToHex(ACode, 8));
end;

procedure TBaseSocket.DoFailure(const ACode: Integer;
  const AMessage: string);
begin
  FResponse.Values[CSCode] := IntToStr(ACode);
  FResponse.Values[CSMessage] := AMessage;
  WriteLogMsg(ltError, GetSocketFlagStr(FSocketFlag) + CSComma + RemoteAddress + ':'
    + IntToStr(RemotePort) + CSComma + Command + ' Failed, Error Code: 0x'
    + IntToHex(ACode, 8) + ', Message: ' + AMessage);
end;

procedure TBaseSocket.AddResponseHeader;
begin
  FResponse.Add('[' + CSResponse + ']');
  FResponse.Add(CSCommand + CSEqualSign + Command);
end;

function TBaseSocket.CheckCommandKey(const ACommandKey: string): Boolean;
begin
  if FRequest.IndexOfName(ACommandKey) = -1 then
  begin
    FErrorCode := CICommandNoCompleted;
    Result := False;
  end
  else
  begin
    Result := True;
  end;
end;

function TBaseSocket.CheckAndGetValue(const ACommandKey: string;
  var AValue: string): Boolean;
begin
  Result := CheckCommandKey(ACommandKey);
  if Result then
  begin
    AValue := FRequest.Values[ACommandKey];
  end;
end;

function TBaseSocket.CheckAndGetValue(const ACommandKey: string;
  var AValue: Integer): Boolean;
var
  sValue: string;
begin
  Result := CheckCommandKey(ACommandKey);
  if Result then
  begin
    sValue := FRequest.Values[ACommandKey];
    if not IsInteger(sValue) then
    begin
      FErrorCode := CIParameterError;
      Result := False;
    end
    else
    begin
      AValue := StrToInt(sValue);
      Result := True;
    end;
  end;
end;

function TBaseSocket.CheckAndGetValue(const ACommandKey: string;
  var AValue: Int64): Boolean;
var
  sValue: string;
begin
  Result := CheckCommandKey(ACommandKey);
  if Result then
  begin
    sValue := FRequest.Values[ACommandKey];
    if not IsInt64(sValue) then
    begin
      FErrorCode := CIParameterError;
      Result := False;
    end
    else
    begin
      AValue := StrToInt64(sValue);
      Result := True;
    end;
  end;
end;

function TBaseSocket.CheckAndGetValue(const ACommandKey: string;
  var AValue: Cardinal): Boolean;
var
  sValue: string;
begin
  Result := CheckCommandKey(ACommandKey);
  if Result then
  begin
    sValue := FRequest.Values[ACommandKey];
    if not IsInteger(sValue) then
    begin
      FErrorCode := CIParameterError;
      Result := False;
    end
    else
    begin
      AValue := StrToInt(sValue);
      Result := True;
    end;
  end;
end;

function TBaseSocket.CheckAndGetValue(const ACommandKey: string;
  var AValue: Word): Boolean;
var
  sValue: string;
begin
  Result := CheckCommandKey(ACommandKey);
  if Result then
  begin
    sValue := FRequest.Values[ACommandKey];
    if not IsInteger(sValue) then
    begin
      FErrorCode := CIParameterError;
      Result := False;
    end
    else
    begin
      AValue := StrToInt(sValue);
      Result := True;
    end;
  end;
end;

procedure TBaseSocket.WriteStream(const AWriteNow: Boolean = True);
var
  sDataHeader: string;
  utf8Header: UTF8String;
  dwLen, dwReadLen, dwHeaderLen: Cardinal;
begin
  if not Assigned(FSendStream) then Exit;
  if FSendStream.Position < FSendStream.Size then
  begin
    if AWriteNow then
      OpenWriteBuffer;
    sDataHeader := '[' + CSResponse + ']' + CSCrLf + CSCommand
      + CSEqualSign + CSData + CSCmdSeperator;
    utf8Header := AnsiToUtf8(sDataHeader);
    dwHeaderLen := Length(utf8Header);
    if Length(FSendBuffer) < FPacketSize then
      SetLength(FSendBuffer, FPacketSize);
    dwReadLen :=  FSendStream.Read(FSendBuffer[0], FPacketSize);    
    dwLen := SizeOf(Cardinal) + dwHeaderLen + dwReadLen; //�ܴ�С
    WriteCardinal(dwLen, False); //�·�����
    WriteCardinal(Length(utf8Header), False); //�·������
    WriteBuffer(PUTF8String(utf8Header)^, Length(utf8Header)); //�·�����ͷ
    WriteBuffer(FSendBuffer[0], dwReadLen); //�·�����
    if AWriteNow then
      FlushWriteBuffer(ioStream);
  end
  else //�������
  begin
    FreeAndNilEx(FSendStream);
    if FSendFile <> '' then
    begin
      try
        if FDeleteFile then
        begin
          if FileExists(FSendFile) and (not IsFileInUse(FSendFile)) then
            DeleteFile(PChar(FSendFile));
        end;
        FSendFile := '';
      except
        on E: Exception do
        begin
          WriteLogMsg(ltError, 'WriteStream DeleteFile, Error, Message: ' + E.Message);
        end;
      end;  
    end;
  end;
end;

procedure TBaseSocket.SendFile(const AFileName: string; const APosition: Int64;
      const APacketSize: Cardinal; const ADeleteFile: Boolean; const AWriteNow: Boolean);
begin
  if Assigned(FSendStream) then
    FreeAndNilEx(FSendStream);
  FSendStream := TFileStream.Create(AFileName, fmOpenRead or fmShareDenyWrite);
  FSendStream.Position := APosition;
  FSendFile := AFileName;
  FDeleteFile := ADeleteFile;
  FPacketSize := APacketSize;
  //�����ļ�ͷ
  FResponse.Clear;
  FResponse.Add('[' + CSResponse + ']');
  FResponse.Add(Format(CSFmtString, [CSCommand, CSSendFile]));
  FResponse.Add(Format(CSFmtString, [CSFileSize, IntToStr(FSendStream.Size - APosition)]));
  DoSuccess;
  DoSendResult(AWriteNow);
  WriteStream(AWriteNow); //��ʼ������
end;

procedure TBaseSocket.SendStream(AStream: TStream;
  const APacketSize: Cardinal; const AWriteNow: Boolean);
begin
  if Assigned(FSendStream) then
    FreeAndNilEx(FSendStream);
  FSendStream := AStream;
  FPacketSize := APacketSize;
  //�����ļ�ͷ
  FResponse.Clear;
  FResponse.Add('[' + CSResponse + ']');
  FResponse.Add(Format(CSFmtString, [CSCommand, CSSendFile]));
  FResponse.Add(Format(CSFmtString, [CSFileSize, IntToStr(FSendStream.Size - FSendStream.Position)]));
  DoSuccess;
  DoSendResult(AWriteNow);
  WriteStream(AWriteNow); //��ʼ������
end;

procedure TBaseSocket.DoCmdLogin;
var
  sUserName, sPassword: string;  
begin
  if not (CheckAndGetValue(CSUserName, sUserName) and CheckAndGetValue(CSPassword, sPassword)) then
  begin
    DoFailure(FErrorCode);
    Exit;
  end;
  FUserName := sUserName;
  if MD5Match('admin', sPassword) then //У������
  begin
    FLogined := True;
    DoSuccess;
    WriteLogMsg(ltInfo, Format('User: %s Login', [sUserName]));
  end
  else
  begin
    FLogined := False;
    DoFailure(CIUserNotExistOrPasswordError);
  end;
end;

function TBaseSocket.GetRequestDataStr: string;
begin
  Result := DataToHex(PChar(FRequestData), FRequestDataLen);
end;

end.
