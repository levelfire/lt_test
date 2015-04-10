unit DownloadSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TDownloadSocket = class(TThreadClientSocket)
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
    {* ��ȡ�ļ��б� *}
    function FileList(const ADirName: string; AFiles: TStrings): Boolean;
    {* �����ļ� *}
    function Download(const ADirName: string; const AFileName: string;
      const APosition: Int64; const APacketSize: Cardinal; var AFileSize: Int64): Boolean;
    property User: string read FUser write FUser;
    property Password: string read FPassword write FPassword;
  end;

implementation

{ TUploadSocket }

constructor TDownloadSocket.Create;
begin
  inherited;
  FTimeOutMS := 60 * 1000; //��ʱ����Ϊ60S
end;

destructor TDownloadSocket.Destroy;
begin   
  inherited;
end;

function TDownloadSocket.GetErrorCodeString(
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

function TDownloadSocket.CheckActive: Boolean;
var
  slRequest: TStringList;
begin
  slRequest := TStringList.Create;
  try
    try
      Result := ControlCommand(CSDownloadCmd[dcActive], nil, nil, '');
    except
      Result := False;
    end;
  finally
    slRequest.Free;
  end;
end;

function TDownloadSocket.ReConnect: Boolean;
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

function TDownloadSocket.ReConnectAndLogin: Boolean;
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

function TDownloadSocket.Connect(const ASvr: string;
  APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(#3);
end;

procedure TDownloadSocket.Disconnect;
begin
  inherited Disconnect;
end;

function TDownloadSocket.Login(const AUser, APassword: string): Boolean;
var
  slRequest: TStringList;
begin
  Result := ReConnect;
  if not Result then Exit;
  slRequest := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSUserName, AUser]));
    slRequest.Add(Format(CSFmtString, [CSPassword, MD5String(APassword)]));
    Result := ControlCommand(CSDownloadCmd[dcLogin], slRequest, nil, '');
    if Result then
    begin
      FUser := AUser;
      FPassword := APassword;
    end;
  finally
    slRequest.Free;
  end;
end;

function TDownloadSocket.Dir(const AParentDir: string; ADirectory: TStrings): Boolean;
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
    Result := ControlCommandThread(CSDownloadCmd[dcDir], slRequest, slResponse, '');
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

function TDownloadSocket.FileList(const ADirName: string;
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
    Result := ControlCommandThread(CSDownloadCmd[dcFileList], slRequest, slResponse, '');
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

function TDownloadSocket.Download(const ADirName, AFileName: string;
  const APosition: Int64; const APacketSize: Cardinal; var AFileSize: Int64): Boolean;
var
  slRequest, slResponse: TStringList;
begin
  slRequest := TStringList.Create;
  slResponse := TStringList.Create;
  try
    slRequest.Add(Format(CSFmtString, [CSDirName, ADirName]));
    slRequest.Add(Format(CSFmtString, [CSFileName, AFileName]));
    slRequest.Add(Format(CSFmtString, [CSFileSize, IntToStr(APosition)])); //���ͱ������д�С��ȥ
    slRequest.Add(Format(CSFmtString, [CSPacketSize, IntToStr(APacketSize)]));
    Result := ControlCommand(CSDownloadCmd[dcDownload], slRequest, nil, '');
    if Result then
    begin
      Result := RecvCommand(slResponse);
      if Result then
        AFileSize := StrToInt64(slResponse.Values[CSFileSize]);
    end;
  finally
    slResponse.Free;
    slRequest.Free;
  end;
end;

end.
