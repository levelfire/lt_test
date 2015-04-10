unit DefineUnit;

interface

uses
  SysUtils, Classes;

type
  {* SQL Server��Ȩ��ʽ *}
  TDBAuthentication = (daWindows, daSQLServer);
  {* SOCKET�б� *}
  TSocketFlag = (sfNone, sfSQL, sfUpload, sfDownload, sfRemoteStream, sfThroughput, sfControl = 8, sfLog = 9);
  {* Control���� *}
  TControlCmd = (ccNone, ccLogin, ccActive, ccGetClients);
  {* SQL���� *}
  TSQLCmd = (scNone, scLogin, scActive, scSQLOpen, scSQLExec, scBeginTrans,
    scCommitTrans, scRollbackTrans);
  {* Upload���� *}
  TUploadCmd = (ucNone, ucLogin, ucActive, ucDir, ucCreateDir, ucDeleteDir, ucFileList,
    ucDeleteFile, ucUpload, ucData, ucEof);
  {* Download���� *}
  TDownloadCmd = (dcNone, dcLogin, dcActive, dcDir, dcFileList, dcDownload);
  {* Զ���ļ������� *}
  TRemoteStreamCommand = (rscNone, rscFileExists, rscOpenFile, rscSetSize, rscGetSize,
    rscSetPosition, rscGetPosition, rscRead, rscWrite, rscSeek, rscCloseFile);
  {* �������������� *}
  TThroughputCommand = (tcNone, tcCyclePacket);

type
  TRemoteStreamMode = (rsmRead, rsmReadWrite);

const
  CSCompanyName = 'SQLDebug_Fan';
  CSSoftwareName = 'IOCPDemo';
  {* ͨ�ô����� *}
  CISuccessCode = $00000000; //��ȷ����
  CINoExistCommand = $00000001; //�������
  CIPackLenError = $00000002; //���ݰ����ȴ���
  CIPackFormatError = $00000003; //���ݰ���ʽ����ȷ��û�а���˫�س�����
  CIUnknowError = $00000004; //����δ֪����
  CICommandNoCompleted = $00000005; //�������
  CIParameterError = $00000006; //��������
  CIUserNotExistOrPasswordError = $00000007; //�û������ڻ��������
  CINotLogin = $00000008; //�û�û�е�½
  {* SQLЭ�� *}
  CISQLOpenError = $01000001; //SQL��ѯ����
  CISQLExecError = $01000002; //SQLִ�г���
  CIHavedBeginTrans = $01000003; //�Ѿ�����������
  CIBeginTransError = $01000004; //�����������
  CINotExistTrans = $01000005; //û����BeginTrans
  CICommitTransError = $01000006; //CommitTransʧ��
  CIRollbackTransError = $01000007; //RollbackTransʧ��
  {* UploadЭ�� *}
  CIDirNotExist = $02000001; //Ŀ¼������
  CICreateDirError = $02000002; //����Ŀ¼ʧ��
  CIDeleteDirError = $02000003; //ɾ��Ŀ¼ʧ��
  CIFileNotExist = $02000004; //�ļ�������
  CIFileInUseing = $02000005; //�ļ�����ʹ����
  CINotOpenFile = $02000006; //�������ݻ�Eof֮ǰ��������Upload
  CIDeleteFileFailed = $02000007; //ɾ���ļ�ʧ��
  CIFileSizeError = $02000008; //�ļ���С����

  {* ��������ݷָ��� *}
  CSCmdSeperator = #13#10#13#10;
  CSCrLf = #13#10;
  CSTxtSeperator = #1; //�ı��ָ�����#1
  CSRequest = 'Request';
  CSResponse = 'Response';
  CSCommand = 'Command';
  CSComma = ';';
  CSEqualSign = '=';
  CSCode = 'Code';
  CSUserName = 'UserName';
  CSPassword = 'Password';
  CSSQL = 'SQL';
  CSSendFile = 'SendFile';
  CSData = 'Data';
  CSFileName = 'FileName';
  CSFileSize = 'FileSize';
  CSCompressSize = 'CompressSize';
  CSMessage = 'Message';
  CSTitle = 'Title';
  CSItem = 'Item';
  CSEffectRow = 'EffectRow';
  CSParentDir = 'ParentDir';
  CSDirName = 'DirName';
  CSPacketSize = 'PacketSize';
  CSMode = 'Mode';
  CSOffset = 'Offset';
  CSSeekOrigin = 'SeekOrigin';
  CSSize = 'Size';
  CSPosition = 'Position';
  CSCount = 'Count';
  CSFmtString = '%s=%s';
  CSFmtInt = '%s=%d';
  {* Control�����ı� *}
  CSControlCmd: array[TControlCmd] of string = ('', 'Login', 'Active', 'GetClients');
  {* SQL�����ı� *}
  CSSQLCmd: array[TSQLCmd] of string = ('', 'Login', 'Active', 'SQLOpen', 'SQLExec',
    'BeginTrans', 'CommitTrans', 'RollbackTrans');
  {* Upload�����ı� *}
  CSUploadCmd: array[TUploadCmd] of string = ('', 'Login', 'Active', 'Dir',
    'CreateDir', 'DeleteDir', 'FileList', 'DeleteFile', 'Upload', 'Data', 'Eof');
  {* Download�����ı� *}
  CSDownloadCmd: array[TDownloadCmd] of string = ('', 'Login', 'Active', 'Dir',
    'FileList', 'Download');
  {* Զ���ļ������� *}
  CSRemoteStreamCommand: array[TRemoteStreamCommand] of string = ('', 'FileExists',
    'OpenFile', 'SetSize', 'GetSize', 'SetPosition', 'GetPosition', 'Read',
    'Write', 'Seek', 'CloseFile');
  {* �������������� *}
  CSThroughputCommand: array[TThroughputCommand] of string = ('', 'CyclePacket');

{* ��ȡSocket��־�ı� *}
function GetSocketFlagStr(const ASocketFlag: TSocketFlag): string;
{* �ַ�תControl���� *}
function StrToControlCommand(const ACommand: string): TControlCmd;
{* �ַ�תSQL���� *}
function StrToSQLCommand(const ACommand: string): TSQLCmd;
{* �ַ�תUpload���� *}
function StrToUploadCommand(const ACommand: string): TUploadCmd;
{* �ַ�תDownload���� *}
function StrToDownloadCommand(const ACommand: string): TDownloadCmd;

implementation

function GetSocketFlagStr(const ASocketFlag: TSocketFlag): string;
begin
  case ASocketFlag of
    sfSQL: Result := 'SQL';
    sfUpload: Result := 'Upload';
    sfDownload: Result := 'Download';
    sfControl: Result := 'Control';
    sfLog: Result := 'Log';
  else
    Result := '';
  end;
end;

function StrToControlCommand(const ACommand: string): TControlCmd;
var
  i: TControlCmd;
begin
  Result := ccNone;
  for i := Low(TControlCmd) to High(TControlCmd) do
  begin
    if SameText(ACommand, CSControlCmd[i]) then
    begin
      Result := i;
      Break;
    end;
  end;
end;

function StrToSQLCommand(const ACommand: string): TSQLCmd;
var
  i: TSQLCmd;
begin
  Result := scNone;
  for i := Low(TSQLCmd) to High(TSQLCmd) do
  begin
    if SameText(ACommand, CSSQLCmd[i]) then
    begin
      Result := i;
      Break;
    end;
  end;
end;

function StrToUploadCommand(const ACommand: string): TUploadCmd;
var
  i: TUploadCmd;
begin
  Result := ucNone;
  for i := Low(TUploadCmd) to High(TUploadCmd) do
  begin
    if SameText(ACommand, CSUploadCmd[i]) then
    begin
      Result := i;
      Break;
    end;
  end;
end;

function StrToDownloadCommand(const ACommand: string): TDownloadCmd;
var
  i: TDownloadCmd;
begin
  Result := dcNone;
  for i := Low(TDownloadCmd) to High(TDownloadCmd) do
  begin
    if SameText(ACommand, CSDownloadCmd[i]) then
    begin
      Result := i;
      Break;
    end;
  end;
end;

end.

