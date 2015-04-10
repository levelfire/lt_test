unit UploadSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TUploadSocket = class(TThreadClientSocket)
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
    {* ��ȡĿ¼ *}
    function Dir(const AParentDir: string; ADirectory: TStrings): Boolean;
    {* ����Ŀ¼ *}
    function CreateDir(const AParentDir, ADirName: string): Boolean;
    {* ɾ��Ŀ¼ *}
    function DeleteDir(const AParentDir, ADirName: string): Boolean;
    {* ��ȡ�ļ��б� *}
    function FileList(const ADirName: string; AFiles: TStrings): Boolean;
    {* ɾ���ļ� *}
    function DeleteFile(const ADirName: string; AFiles: TStrings): Boolean;
    {* ��ʼ�ϴ� *}
    function Upload(const ADirName: string; const AFileName: string; var AFilePosition: Int64): Boolean;
    {* �������� *}
    function Data(const ABuffer; const ACount: Integer): Boolean;
    {* ���ͽ��� *}
    function Eof(const AFileSize: Int64): Boolean;
    property User: string read FUser write FUser;
    property Password: string read FPassword write FPassword;
  end;

implementation

{ TUploadSocket }

constructor TUploadSocket.Create;
begin
  inherited;
  FTimeOutMS := 60 * 1000; //��ʱ����Ϊ60S
end;

destructor TUploadSocket.Destroy;
begin   
  inherited;
end;

function TUploadSocket.GetErrorCodeString(
  const AErrorCode: Cardinal): string;
begin
  case AErrorCode of
    CIDirNotExist: Result := 'Directory Not Exist';
    CICreateDirError: Result := 'Create Directory Failed';
    CIDeleteDirError: Result := 'Delete Directory Failed';
    CIFileNotExist: Result := 'File Not Exist';
    CIFileInUseing: Result := 'File In Useing';
    CINotOpenFile: Result := 'Not Open File';
    CIDeleteFileFailed: Result := 'Delete File Failed';
    CIFileSizeError: Result := 'File Size Error';
  else
    Result := inherited GetErrorCodeString(AErrorCode);
  end;
end;

function TUploadSocket.CheckActive: Boolean;
var
  slRequest: TStringList;
begin
  slRequest := TStringList.Create;
  try
    try
      Result := ControlCommand(CSUploadCmd[ucActive], nil, nil, '');
    except
      Result := False;
    end;
  finally
    slRequest.Free;
  end;
end;

function TUploadSocket.ReConnect: Boolean;
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

function TUploadSocket.ReConnectAndLogin: Boolean;
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

function TUploadSocket.Connect(const ASvr: string;
  APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(#2);
end;

procedure TUploadSocket.Disconnect;
begin
  inherited Disconnect;
end;

function TUploadSocket.Login(const AUser, APassword: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnect;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSUserName, AUser]));
    slRequest.Add(Format(CSFmtString, [CSPassword, MD5String(APassword)]));
    Result := ControlCommand(CSUploadCmd[ucLogin], slRequest, nil, '');
    if Result then
    begin
      FUser := AUser;
      FPassword := APassword;
    end;
  finally
    slRequest.Free;
  end;
end;

function TUploadSocket.Dir(const AParentDir: string; ADirectory: TStrings): Boolean;
var
  slRequest, slResponse: TStringList;
  i: Integer;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slRequest := TStringList.Create;
  slResponse := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSParentDir, AParentDir]));
    Result := ControlCommandThread(CSUploadCmd[ucDir], slRequest, slResponse, '');
    if Result then
    begin
      ADirectory.Clear;
      for i := 0 to slResponse.Count - 1 do
      begin
        if SameText(slResponse.Names[i], CSItem) then
        begin
          ADirectory.Add(slResponse.ValueFromIndex[i]);
        end;
      end;
    end;
  finally
    slResponse.Free;
    slRequest.Free;
  end;
end;

function TUploadSocket.CreateDir(const AParentDir,
  ADirName: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSParentDir, AParentDir]));
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    Result := ControlCommandThread(CSUploadCmd[ucCreateDir], slRequest, nil, '');
  finally
    slRequest.Free;
  end;
end;

function TUploadSocket.DeleteDir(const AParentDir,
  ADirName: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSParentDir, AParentDir]));
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    Result := ControlCommandThread(CSUploadCmd[ucDeleteDir], slRequest, nil, '');
  finally
    slRequest.Free;
  end;
end;

function TUploadSocket.FileList(const ADirName: string;
  AFiles: TStrings): Boolean;
var
  slRequest, slResponse: TStringList;
  i: Integer;
  sItem: string;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slRequest := TStringList.Create;
  slResponse := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    Result := ControlCommandThread(CSUploadCmd[ucFileList], slRequest, slResponse, '');
    if Result then
    begin
      AFiles.Clear;
      for i := 0 to slResponse.Count - 1 do
      begin
        if SameText(slResponse.Names[i], CSItem) then
        begin
          sItem := slResponse.ValueFromIndex[i];
          sItem := StringReplace(sItem, #1, CSEqualSign, []);
          AFiles.Add(sItem);
        end;
      end;
    end;
  finally
    slResponse.Free;
    slRequest.Free;
  end;
end;

function TUploadSocket.DeleteFile(const ADirName: string;
  AFiles: TStrings): Boolean;
var
  slRequest: TStringList;
  i: Integer;
begin
  Result := ReConnectAndLogin;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    for i := 0 to AFiles.Count - 1 do
      slRequest.Add(Format(CSFmtString, [CSItem, AFiles[i]]));
    Result := ControlCommandThread(CSUploadCmd[ucDeleteFile], slRequest, nil, '');
  finally
    slRequest.Free;
  end;
end;

function TUploadSocket.Upload(const ADirName, AFileName: string;
  var AFilePosition: Int64): Boolean;
var
  slRequest, slResponse: TStringList;
begin
  slRequest := TStringList.Create;
  slResponse := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    slRequest.Add(Format(CSFmtString, [CSFileName, AFileName]));
    Result := ControlCommand(CSUploadCmd[ucUpload], slRequest, slResponse, '');
    if Result then
      AFilePosition := StrToInt64(slResponse.Values[CSFileSize]);
  finally
    slRequest.Free;
    slResponse.Free;
  end;
end;

function TUploadSocket.Data(const ABuffer; const ACount: Integer): Boolean;
var
  dwPacketLen, dwCommandLen: Cardinal;
  utfHeader: UTF8String;
  sHeader: string;
begin
  try
    sHeader := '[' + CSRequest + ']' + CSCrLf + CSCommand + CSEqualSign + CSData + CSCmdSeperator;
    utfHeader := AnsiToUtf8(sHeader);
    dwCommandLen := Length(utfHeader);
    dwPacketLen := SizeOf(Cardinal) + dwCommandLen + Cardinal(ACount);
    //FClient.OpenWriteBuffer(-1);
    FClient.WriteCardinal(dwPacketLen, False); //�������ݰ���С
    FClient.WriteCardinal(dwCommandLen, False); //���������
    FClient.WriteBuffer(PUTF8String(utfHeader)^, dwCommandLen, True); //������������
    FClient.WriteBuffer(ABuffer, ACount);
    //FClient.FlushWriteBuffer(-1); //���͵�ǰȫ������
    Result := True;
  except
    on E: Exception do
    begin
      FLastError := E.Message;
      Result := False;
    end;
  end;
end;

function TUploadSocket.Eof(const AFileSize: Int64): Boolean;
var
  slRequest: TStringList;
begin
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSFileSize, IntToStr(AFileSize)]));
    Result := ControlCommand(CSUploadCmd[ucEof], slRequest, nil, '');
  finally
    slRequest.Free;
  end;
end;

end.
