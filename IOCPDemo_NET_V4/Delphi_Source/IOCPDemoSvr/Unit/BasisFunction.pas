unit BasisFunction;

interface

uses
  Windows, SysUtils, Classes, Types, ShlObj, ActiveX, Graphics, Math, Messages;
  
type
  TMD5Ctx = record
    State: array[0..3] of Integer;
    Count: array[0..1] of Integer;
    case Integer of
      0: (BufChar: array[0..63] of Byte);
      1: (BufLong: array[0..15] of Integer);
  end;

  TDlgTemplateEx = packed record
    dlgTemplate: DLGITEMTEMPLATE;
    ClassName: string;
    Caption: string;
  end;

  TSelectDirectoryProc = function(const Directory: string): Boolean;
  TFindFileProc = procedure(const FileName: string; Info: TSearchRec);

const
  SInformation = 'Info';
  SError = 'Error';
  SErrDataType = 'Data type mismatch';

{ �ָ��ַ�����chΪ�ָ�����Source��Ҫ�ָ����ַ��� }
function SplitString(const source, ch: string): TStringDynArray; overload; stdcall;
{ �ָ��ַ�����chΪ�ָ�����Source��Ҫ�ָ����ַ��� }
procedure SplitString(const source, ch: string; Results: TStrings); overload; stdcall;
{ ���һ���ַ����Ƿ��ǿյ��ַ������մ���ָ���ַ���ĩ�ַ��в������ǿհ��ַ����ַ����� }
function IsEmptyStr(const Str: string): Boolean; stdcall;
{ ���ص�ǰӦ�ó���·�� }
function AppPath: string; stdcall;
{ ����ִ��ģ���ļ��� }
function AppFile: string; stdcall;
{ �ͷŲ��ÿ�һ������ָ�� }
procedure FreeAndNilEx(var Obj); stdcall;
{ �Ѷ���������ʮ�����ƻ� }
function DataToHex(Data: PChar; Len: integer): string; stdcall;

{ �Ƚ������ַ�����MD5����Ƿ�һ�� }
function MD5Match(const S, MD5Value: string): Boolean; stdcall;
{ ����ָ���ַ�����MD5ɢ��ֵ }
function MD5String(const Value: string): string; stdcall;
procedure MD5Init(var MD5Context: TMD5Ctx);
procedure MD5Update(var MD5Context: TMD5Ctx; const Data: PChar; Len: integer);
function MD5Final(var MD5Context: TMD5Ctx): string;
function MD5Print(D: string): string;          
{ ���ļ���MD5ɢ��ֵ }
function MD5File(FileName: string): string; stdcall;

{ �����ļ�������Shell���������� }
function CopyFiles(const Dest: string; const Files: TStrings; const UI: Boolean = False): Boolean; stdcall;
{ ����Ŀ¼������Shell���������� }
function CopyDirectory(const Source, Dest: string; const UI: Boolean = False): boolean; stdcall;
{ �ƶ��ļ���Shell��ʽ }
function MoveFiles(DestDir: string; const Files: TStrings; const UI: Boolean = False): Boolean; stdcall;
{ ������Ŀ¼��Shell��ʽ }
function RenameDirectory(const OldName, NewName: string): boolean; stdcall;
{ ɾ��Ŀ¼��Shell��ʽ }
function DeleteDirectory(const DirName: string; const UI: Boolean = False): Boolean; stdcall;
{ ���Ŀ¼��Shell��ʽ }
function ClearDirectory(const DirName: string; const IncludeSub, ToRecyle: Boolean): Boolean; stdcall;
{ ɾ���ļ���Shell��ʽ }
function DeleteFiles(const Files: TStrings; const ToRecyle: Boolean = True): Boolean; stdcall;
{ ����ָ���ļ��Ĵ�С��֧�ֳ����ļ� }
function FileSizeEx(const FileName: string): Int64; stdcall;
{ �����ļ���С��ʾ }
function GetFileSizeDisplay(const AFileSize: Int64): string;
{ ���ط����ٶ���ʾ }
function GetSpeedDisplay(const ASpeed: Double): string;

{ ��̬����һ��GUID }
function GetGUID: TGUID; stdcall;
{ ����һ����̬���ɵ�GUID���ַ��� }
function GetGUIDString: string; stdcall;
{ �ж�һ���ַ����Ƿ���һ���Ϸ���GUID }
function IsGUID(const S: string): Boolean; stdcall;
{ �������һ��API���õĴ�����Ϣ�ַ��� }
function GetLastErrorString: string; stdcall;

{ ��ʽ��ValueΪָ�����Width���ַ��� }
function FixFormat(const Width: integer; const Value: Double): string; overload; stdcall;
{ ��ʽ��ValueΪָ�����Width���ַ��� }
function FixFormat(const Width: integer; const Value: integer): string; overload; stdcall;
{ ����ָ��λ��С������ }
function FixDouble(const ACount: Integer; const AValue: Double): string; stdcall;
{ ����Delphi���������BUG��Round���� }
function RoundEx(Value: Extended): Int64; stdcall;
{ ������������ȡ�� }
function RoundUp(Value: Extended): Int64; stdcall;
{ ������������ȡ�� }
function RoundDown(Value: Extended): Int64; stdcall;
{ ����Delphi�������뺯��Round���������������BUG }
function ClassicRoundUp(const Value: Extended): Int64; stdcall;
{ ����Delphi�������뺯��Round���������������BUG }
function ClassicRoundDown(const Value: Extended): Int64; stdcall;
{ ����Delphi�������뺯��RoundTo��BUG }
function ClassicRoundTo(const AValue: Extended; const APrecision: TRoundToRange = 0): Extended; stdcall;
{ �����ߵ��ֽ� }
function xhl(const b: Byte): Byte; overload; stdcall;
{ �����ߵ��ֽ� }
function xhl(const W: Word): Word; overload; stdcall;
{ �����ߵ��ֽ� }
function xhl(const DW: DWord): Dword; overload; stdcall;

{ ��ʾһ�������ı��Ի��� }
function InputBoxEx(const ACaption, AHint, ADefault: string; out Text: string): Boolean; stdcall;
{ ����ָ�����ڵ����� }
function GetWindowClassName(hWnd: HWND): string; stdcall;
{ ���ش��ڵı��� }
function GetWindowCaption(hWnd: HWND): string; stdcall;
{ Ĭ�϶Ի�������� }
function DefDialogProc(Wnd: HWND; Msg: UINT; wParam: WPARAM; lParam: LPARAM): Integer; stdcall;
{ ��ʾһ���Զ���Ի��� }
function DialogBoxEx(const Controls: array of TDlgTemplateEx; const DlgProc: Pointer = nil): Integer;
{ �������������������� }
function MakeFont(FontName: string; Size, Bold: integer; StrikeOut, Underline, Italy: Boolean): Integer; stdcall;
{ ����ָ���ؼ������� }
procedure SetFont(hWnd: HWND; Font: HFONT); stdcall;
{ ��ʾһ����Ϣ�� }
procedure DlgInfo(const Msg, Title: string); stdcall;
{ ��ʾһ������Ի��� }
procedure DlgError(const Msg: string); stdcall;
{ ��װ��MessageBox }
function MsgBox(const Msg: string; const Title: string = 'Info'; dType: Integer = MB_OK + MB_ICONINFORMATION): Integer; stdcall;
{ ѡ��Ŀ¼�Ի��򣬺ܺ��õ�Ŷ }
function SelectDirectoryEx(var Path: string; const Caption, Root: string; BIFs: DWORD = $59;
  Callback: TSelectDirectoryProc = nil; const FileName: string = ''): Boolean; stdcall;

{ ��Ӳ�����洴���ļ� }
procedure CreateFileOnDisk(FileName: string); stdcall;
{ �ж��ļ��Ƿ�����ʹ�� }
function IsFileInUse(const FileName: string): Boolean; stdcall;
{ ������չ������һ����ʱ�ļ��� }
function GetTmpFile(const AExt: string): string; stdcall;
function GetTmpFileEx(const APath, APre, AExt: string): string; stdcall;
{ ����ϵͳ��ʱ�ļ�Ŀ¼ }
function TempPath: string; stdcall;
{ ����һ��ָ��Ŀ¼��ָ���ļ����ļ������� }
procedure GetFileList(Files: TStrings; Folder, FileSpec: string; SubDir: Boolean = True; CallBack: TFindFileProc = nil); stdcall;
{ ����һ��ָ��Ŀ¼��ָ���ļ����ļ������ϣ����������ļ����µ��ļ� }
procedure GetFileListName(Files: TStrings; const Folder, FileSpec: string); stdcall;
{ ����һ��ָ��Ŀ¼����Ŀ¼�ļ��� }
procedure GetDirectoryList(Directorys: TStrings; const Folder: string); stdcall;
{ ����һ��ָ��Ŀ¼����Ŀ¼���ļ��� }
procedure GetDirectoryListName(Directorys: TStrings; const Folder: string); stdcall;

{ �ж�Ch�Ƿ��ǿհ��ַ� }
function IsSpace(const Ch: Char): Boolean; stdcall;
{ �ж�Ch�Ƿ���ʮ�����������ַ� }
function IsXDigit(const Ch: Char): Boolean; stdcall;
{ �ж�һ���ַ����Ƿ������� }
function IsInteger(const S: string): Boolean; stdcall;
function IsInt64(const S: string): Boolean; stdcall;
{ �ж��ַ����Ƿ��Ǹ����� }
function IsFloat(const S: string): Boolean; stdcall;
{ �����ַ�����Seperator��ߵ��ַ���������LeftPart('ab|cd','|')='ab' }
function LeftPart(Source, Seperator: string): string; stdcall;
{ �����ַ�����Seperator�ұߵ��ַ���������RightPart('ab|cd','|')='cd' }
function RightPart(Source, Seperator: string): string; stdcall;
{ �����ַ�����Seperator���ұߵ��ַ���������LastRightPart('ab|cd|ef','|')='ef' }
function LastRightPart(Source, Seperator: string): string; stdcall;

{ ���ص�ǰϵͳ��GMT/UTCʱ�� }
function GMTNow: TDateTime; stdcall;
{ ת������ʱ��ΪUTCʱ�� }
function LocaleToGMT(const Value: TDateTime): TDateTime; stdcall;
{ ת��UTCʱ��Ϊ����ʱ�� }
function GMTToLocale(const Value: TDateTime): TDateTime; stdcall;

const
  CSHKeyNames: array[HKEY_CLASSES_ROOT..HKEY_DYN_DATA] of string = (
    'HKEY_CLASSES_ROOT', 'HKEY_CURRENT_USER', 'HKEY_LOCAL_MACHINE', 'HKEY_USERS',
    'HKEY_PERFORMANCE_DATA', 'HKEY_CURRENT_CONFIG', 'HKEY_DYN_DATA');
{ ת��HKEYΪ�ַ��� }
function HKEYToStr(const Key: HKEY): string; stdcall;
{ ת���ַ���ΪHKEY }
function StrToHKEY(const KEY: string): HKEY; stdcall;
{ ������ע���·������ȡHKEY }
function RegExtractHKEY(const Reg: string): HKEY; stdcall;
{ �������ַ�������ȡע���SubKey }
function RegExtractSubKey(const Reg: string): string; stdcall;
{ ����ע��� }
function RegExportToFile(const RootKey: HKEY; const SubKey, FileName: string): Boolean; stdcall;
{ ����ע��� }
function RegImportFromFile(const RootKey: HKEY; const SubKey, FileName: string): Boolean; stdcall;
{ ��ȡע�����ָ�����ַ��� }
function RegReadString(const RootKey: HKEY; const SubKey, ValueName: string): string; stdcall;
{ д���ַ�����ע����� }
function RegWriteString(const RootKey: HKEY; const SubKey, ValueName, Value: string): Boolean; stdcall;
{ ��ȡע�����ָ�����ַ��� }
function RegReadMultiString(const RootKey: HKEY; const SubKey, ValueName: string): string; stdcall;
{ д���ַ�����ע����� }
function RegWriteMultiString(const RootKey: HKEY; const SubKey, ValueName, Value: string): Boolean; stdcall;
{ ע����ȡ���� }
function RegReadInteger(const RootKey: HKEY; const SubKey, ValueName: string): integer; stdcall;
{ ע���д������ }
function RegWriteInteger(const RootKey: HKEY; const SubKey, ValueName: string; const Value: integer): Boolean; stdcall;
{ ע����ȡ���������� }
function RegReadBinary(const RootKey: HKEY; const SubKey, ValueName: string; Data: PChar; out Len: integer): Boolean; stdcall;
{ ע���д������� }
function RegWriteBinary(const RootKey: HKEY; const SubKey, ValueName: string; Data: PChar; Len: integer): Boolean; stdcall;
{ �ж�ע����Ƿ����ָ��ֵ }
function RegValueExists(const RootKey: HKEY; const SubKey, ValueName: string): Boolean; stdcall;
{ ע��������Ƿ���� }
function RegKeyExists(const RootKey: HKEY; const SubKey: string): Boolean; stdcall;
{ ɾ��ע������� }
function RegKeyDelete(const RootKey: HKEY; const SubKey: string): Boolean; stdcall;
{ ɾ��ע���ֵ }
function RegValueDelete(const RootKey: HKEY; const SubKey, ValueName: string): Boolean; stdcall;
{ ��ȡע���ָ�����������еļ�ֵ���б� }
function RegGetValueNames(const RootKey: HKEY; const SubKey: string; Names: TStrings): Boolean; stdcall;
{ ��ȡע�����ָ���������������������б� }
function RegGetKeyNames(const RootKey: HKEY; const SubKey: string; Names: TStrings): Boolean; stdcall;
{ Ϊ��ǰ���̴�/��ֹĳ��ָ����ϵͳ��Ȩ }
function EnablePrivilege(PrivName: string; bEnable: Boolean): Boolean; stdcall;

implementation

uses
  ShellAPI;

var
  ShareData: string; /// ������ʱ�洢���ݵĹ����������Ҫ���ڻص������У�

function SplitString(const source, ch: string): TStringDynArray;
{
  �ָ��ַ�����chΪ�ָ�����Source��Ҫ�ָ����ַ���
}
var
  temp: PChar;
  i: integer;
begin
  Result := nil;
  if Source = '' then exit;
  temp := PChar(source);
  i := AnsiPos(ch, temp);
  while i <> 0 do
  begin
    SetLength(Result, Length(Result) + 1);
    Result[Length(Result) - 1] := copy(temp, 1, i - 1);
    inc(temp, Length(Ch) + i - 1);
    i := AnsiPos(ch, temp);
  end;
  SetLength(Result, Length(Result) + 1);
  Result[Length(Result) - 1] := Temp;
end;

procedure SplitString(const source, ch: string; Results: TStrings);
{
  �ָ��ַ�����chΪ�ָ�����Source��Ҫ�ָ����ַ���
}
begin
  Results.CommaText := '"' + StringReplace(source, ch, '","', [rfReplaceAll]) + '"';
end;

function IsEmptyStr(const Str: string): Boolean;
begin
  Result := Trim(Str) = '';
end;

function AppPath: string;
{
  ���ر�Ӧ�ó����·��
}
begin
  Result := IncludeTrailingPathDelimiter(ExtractFilePath(AppFile));
end;

function AppFile: string;
{
  ���ؿ�ִ��ģ����ļ�����֧��DLL��
}
var
  Buff: array[0..MAX_PATH] of char;
begin
  GetModuleFileName(HInstance, Buff, SizeOf(Buff));
  Result := StrPas(Buff);
end;

procedure FreeAndNilEx(var Obj);
begin
  if Assigned(Pointer(Obj)) then FreeAndNil(Obj);
end;

function DataToHex(Data: PChar; Len: Integer): string;
{
  ��ָ���Ķ���������ת����ʮ�����Ʊ�ʾ���ַ���
}
begin
  SetLength(Result, Len shl 1);
  BinToHex(Data, PChar(Result), Len);
end;

procedure MD5Init(var MD5Context: TMD5Ctx);
begin
  FillChar(MD5Context, SizeOf(TMD5Ctx), #0);
  with MD5Context do
  begin
    State[0] := Integer($67452301);
    State[1] := Integer($EFCDAB89);
    State[2] := Integer($98BADCFE);
    State[3] := Integer($10325476);
  end;
end;

procedure MD5Transform(var Buf: array of LongInt; const Data: array of LongInt);
var
  A, B, C, D: LongInt;

  procedure Round1(var W: LongInt; X, Y, Z, Data: LongInt; S: Byte);
  begin
    Inc(W, (Z xor (X and (Y xor Z))) + Data);
    W := (W shl S) or (W shr (32 - S));
    Inc(W, X);
  end;

  procedure Round2(var W: LongInt; X, Y, Z, Data: LongInt; S: Byte);
  begin
    Inc(W, (Y xor (Z and (X xor Y))) + Data);
    W := (W shl S) or (W shr (32 - S));
    Inc(W, X);
  end;

  procedure Round3(var W: LongInt; X, Y, Z, Data: LongInt; S: Byte);
  begin
    Inc(W, (X xor Y xor Z) + Data);
    W := (W shl S) or (W shr (32 - S));
    Inc(W, X);
  end;

  procedure Round4(var W: LongInt; X, Y, Z, Data: LongInt; S: Byte);
  begin
    Inc(W, (Y xor (X or not Z)) + Data);
    W := (W shl S) or (W shr (32 - S));
    Inc(W, X);
  end;
begin
  A := Buf[0];
  B := Buf[1];
  C := Buf[2];
  D := Buf[3];

  Round1(A, B, C, D, Data[0] + Longint($D76AA478), 7);
  Round1(D, A, B, C, Data[1] + Longint($E8C7B756), 12);
  Round1(C, D, A, B, Data[2] + Longint($242070DB), 17);
  Round1(B, C, D, A, Data[3] + Longint($C1BDCEEE), 22);
  Round1(A, B, C, D, Data[4] + Longint($F57C0FAF), 7);
  Round1(D, A, B, C, Data[5] + Longint($4787C62A), 12);
  Round1(C, D, A, B, Data[6] + Longint($A8304613), 17);
  Round1(B, C, D, A, Data[7] + Longint($FD469501), 22);
  Round1(A, B, C, D, Data[8] + Longint($698098D8), 7);
  Round1(D, A, B, C, Data[9] + Longint($8B44F7AF), 12);
  Round1(C, D, A, B, Data[10] + Longint($FFFF5BB1), 17);
  Round1(B, C, D, A, Data[11] + Longint($895CD7BE), 22);
  Round1(A, B, C, D, Data[12] + Longint($6B901122), 7);
  Round1(D, A, B, C, Data[13] + Longint($FD987193), 12);
  Round1(C, D, A, B, Data[14] + Longint($A679438E), 17);
  Round1(B, C, D, A, Data[15] + Longint($49B40821), 22);

  Round2(A, B, C, D, Data[1] + Longint($F61E2562), 5);
  Round2(D, A, B, C, Data[6] + Longint($C040B340), 9);
  Round2(C, D, A, B, Data[11] + Longint($265E5A51), 14);
  Round2(B, C, D, A, Data[0] + Longint($E9B6C7AA), 20);
  Round2(A, B, C, D, Data[5] + Longint($D62F105D), 5);
  Round2(D, A, B, C, Data[10] + Longint($02441453), 9);
  Round2(C, D, A, B, Data[15] + Longint($D8A1E681), 14);
  Round2(B, C, D, A, Data[4] + Longint($E7D3FBC8), 20);
  Round2(A, B, C, D, Data[9] + Longint($21E1CDE6), 5);
  Round2(D, A, B, C, Data[14] + Longint($C33707D6), 9);
  Round2(C, D, A, B, Data[3] + Longint($F4D50D87), 14);
  Round2(B, C, D, A, Data[8] + Longint($455A14ED), 20);
  Round2(A, B, C, D, Data[13] + Longint($A9E3E905), 5);
  Round2(D, A, B, C, Data[2] + Longint($FCEFA3F8), 9);
  Round2(C, D, A, B, Data[7] + Longint($676F02D9), 14);
  Round2(B, C, D, A, Data[12] + Longint($8D2A4C8A), 20);

  Round3(A, B, C, D, Data[5] + Longint($FFFA3942), 4);
  Round3(D, A, B, C, Data[8] + Longint($8771F681), 11);
  Round3(C, D, A, B, Data[11] + Longint($6D9D6122), 16);
  Round3(B, C, D, A, Data[14] + Longint($FDE5380C), 23);
  Round3(A, B, C, D, Data[1] + Longint($A4BEEA44), 4);
  Round3(D, A, B, C, Data[4] + Longint($4BDECFA9), 11);
  Round3(C, D, A, B, Data[7] + Longint($F6BB4B60), 16);
  Round3(B, C, D, A, Data[10] + Longint($BEBFBC70), 23);
  Round3(A, B, C, D, Data[13] + Longint($289B7EC6), 4);
  Round3(D, A, B, C, Data[0] + Longint($EAA127FA), 11);
  Round3(C, D, A, B, Data[3] + Longint($D4EF3085), 16);
  Round3(B, C, D, A, Data[6] + Longint($04881D05), 23);
  Round3(A, B, C, D, Data[9] + Longint($D9D4D039), 4);
  Round3(D, A, B, C, Data[12] + Longint($E6DB99E5), 11);
  Round3(C, D, A, B, Data[15] + Longint($1FA27CF8), 16);
  Round3(B, C, D, A, Data[2] + Longint($C4AC5665), 23);

  Round4(A, B, C, D, Data[0] + Longint($F4292244), 6);
  Round4(D, A, B, C, Data[7] + Longint($432AFF97), 10);
  Round4(C, D, A, B, Data[14] + Longint($AB9423A7), 15);
  Round4(B, C, D, A, Data[5] + Longint($FC93A039), 21);
  Round4(A, B, C, D, Data[12] + Longint($655B59C3), 6);
  Round4(D, A, B, C, Data[3] + Longint($8F0CCC92), 10);
  Round4(C, D, A, B, Data[10] + Longint($FFEFF47D), 15);
  Round4(B, C, D, A, Data[1] + Longint($85845DD1), 21);
  Round4(A, B, C, D, Data[8] + Longint($6FA87E4F), 6);
  Round4(D, A, B, C, Data[15] + Longint($FE2CE6E0), 10);
  Round4(C, D, A, B, Data[6] + Longint($A3014314), 15);
  Round4(B, C, D, A, Data[13] + Longint($4E0811A1), 21);
  Round4(A, B, C, D, Data[4] + Longint($F7537E82), 6);
  Round4(D, A, B, C, Data[11] + Longint($BD3AF235), 10);
  Round4(C, D, A, B, Data[2] + Longint($2AD7D2BB), 15);
  Round4(B, C, D, A, Data[9] + Longint($EB86D391), 21);

  Inc(Buf[0], A);
  Inc(Buf[1], B);
  Inc(Buf[2], C);
  Inc(Buf[3], D);
end;

procedure MD5Update(var MD5Context: TMD5Ctx; const Data: PChar; Len: integer);
var
  Index, t: Integer;
begin
  //Len := Length(Data);
  with MD5Context do
  begin
    T := Count[0];
    Inc(Count[0], Len shl 3);
    if Count[0] < T then
      Inc(Count[1]);
    Inc(Count[1], Len shr 29);
    T := (T shr 3) and $3F;
    Index := 0;
    if T <> 0 then
    begin
      Index := T;
      T := 64 - T;
      if Len < T then
      begin
        Move(Data, Bufchar[Index], Len);
        Exit;
      end;
      Move(Data, Bufchar[Index], T);
      MD5Transform(State, Buflong);
      Dec(Len, T);
      Index := T;
    end;
    while Len > 64 do
    begin
      Move(Data[Index], Bufchar, 64);
      MD5Transform(State, Buflong);
      Inc(Index, 64);
      Dec(Len, 64);
    end;
    Move(Data[Index], Bufchar, Len);
  end
end;

function MD5Final(var MD5Context: TMD5Ctx): string;
var
  Cnt: Word;
  P: Byte;
  D: array[0..15] of Char;
  i: Integer;
begin
  for I := 0 to 15 do
    Byte(D[I]) := I + 1;
  with MD5Context do
  begin
    Cnt := (Count[0] shr 3) and $3F;
    P := Cnt;
    BufChar[P] := $80;
    Inc(P);
    Cnt := 64 - 1 - Cnt;
    if Cnt > 0 then
      if Cnt < 8 then
      begin
        FillChar(BufChar[P], Cnt, #0);
        MD5Transform(State, BufLong);
        FillChar(BufChar, 56, #0);
      end
      else
        FillChar(BufChar[P], Cnt - 8, #0);
    BufLong[14] := Count[0];
    BufLong[15] := Count[1];
    MD5Transform(State, BufLong);
    Move(State, D, 16);
    Result := '';
    for i := 0 to 15 do
      Result := Result + Char(D[i]);
  end;
  FillChar(MD5Context, SizeOf(TMD5Ctx), #0)
end;

function MD5Print(D: string): string;
var
  I: byte;
const
  Digits: array[0..15] of char =
  ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f');
begin
  Result := '';
  for I := 0 to 15 do
    Result := Result + Digits[(Ord(D[I + 1]) shr 4) and $0F] + Digits[Ord(D[I + 1]) and $0F];
end;

function MD5Match(const S, MD5Value: string): Boolean;
begin
  Result := SameText(MD5String(S), MD5Value);
end;

function MD5String(const Value: string): string;
{
  ��Value���м���MD5ɢ��ֵ
}
var
  MD5Context: TMD5Ctx;
begin
  MD5Init(MD5Context);
  MD5Update(MD5Context, PChar(Value), Length(Value));
  Result := MD5Print(MD5Final(MD5Context));
end;

function MD5File(FileName: string): string;
{
  ���ļ���MD5ɢ��ֵ
}
var
  FileHandle: THandle;
  MapHandle: THandle;
  ViewPointer: pointer;
  Context: TMD5Ctx;
begin
  MD5Init(Context);
  FileHandle := CreateFile(pChar(FileName), GENERIC_READ, FILE_SHARE_READ or FILE_SHARE_WRITE,
    nil, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL or FILE_FLAG_SEQUENTIAL_SCAN, 0);
  if FileHandle <> INVALID_HANDLE_VALUE then
  try
    MapHandle := CreateFileMapping(FileHandle, nil, PAGE_READONLY, 0, 0, nil);
    if MapHandle <> 0 then
    try
      ViewPointer := MapViewOfFile(MapHandle, FILE_MAP_READ, 0, 0, 0);
      if ViewPointer <> nil then
      try
        MD5Update(Context, ViewPointer, GetFileSize(FileHandle, nil));
      finally
        UnmapViewOfFile(ViewPointer);
      end;
    finally
      CloseHandle(MapHandle);
    end;
  finally
    CloseHandle(FileHandle);
  end;
  Result := MD5Print(MD5Final(Context));
end;

function CopyFiles(const Dest: string; const Files: TStrings; const UI: Boolean = False): Boolean;
{
  �����ļ��Ի���
}
var
  fo: TSHFILEOPSTRUCT;
  i: integer;
  FFiles: string;
begin
  for i := 0 to Files.Count - 1 do
    FFiles := FFiles + Files[i] + #0;
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_COPY;
    pFrom := PChar(FFiles + #0);
    pTo := pchar(Dest + #0);
    if UI then
      fFlags := FOF_ALLOWUNDO
    else
      fFlags := FOF_NOCONFIRMATION or FOF_NOCONFIRMMKDIR or FOF_ALLOWUNDO;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function CopyDirectory(const Source, Dest: string; const UI: Boolean = False): boolean;
{
  ����Ŀ¼�Ի���
}
var
  fo: TSHFILEOPSTRUCT;
begin
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_COPY;
    pFrom := PChar(source + #0);
    pTo := PChar(Dest + #0);
    if UI then
      fFlags := FOF_ALLOWUNDO
    else
      fFlags := FOF_NOCONFIRMATION or FOF_NOCONFIRMMKDIR;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function MoveFiles(DestDir: string; const Files: TStrings; const UI: Boolean = False): Boolean;
{
  �ƶ��ļ��Ի���
}
var
  fo: TSHFILEOPSTRUCT;
  i: integer;
  FFiles: string;
begin
  for i := 0 to Files.Count - 1 do
    FFiles := FFiles + Files[i] + #0;
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_MOVE;
    pFrom := PChar(FFiles + #0);
    pTo := pchar(DestDir + #0);
    if UI then
      fFlags := FOF_ALLOWUNDO
    else
      fFlags := FOF_NOCONFIRMATION or FOF_NOCONFIRMMKDIR or FOF_ALLOWUNDO;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function RenameDirectory(const OldName, NewName: string): boolean;
{
  ������Ŀ¼
}
var
  fo: TSHFILEOPSTRUCT;
begin
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_RENAME;
    pFrom := PChar(OldName + #0);
    pTo := pchar(NewName + #0);
    fFlags := FOF_NOCONFIRMATION + FOF_SILENT + FOF_ALLOWUNDO;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function DeleteDirectory(const DirName: string; const UI: Boolean = False): Boolean;
{
   ɾ��Ŀ¼
}
var
  fo: TSHFILEOPSTRUCT;
begin
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_DELETE;
    pFrom := PChar(DirName + #0);
    pTo := #0#0;
    fFlags := FOF_NOCONFIRMATION + FOF_SILENT;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function ClearDirectory(const DirName: string; const IncludeSub, ToRecyle: Boolean): Boolean;
{
  ���Ŀ¼
}
var
  fo: TSHFILEOPSTRUCT;
begin
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_DELETE;
    pFrom := PChar(DirName + '\*.*' + #0);
    pTo := #0#0;
    fFlags := FOF_SILENT or FOF_NOCONFIRMATION or FOF_NOERRORUI
      or (Ord(not IncludeSub) * FOF_FILESONLY)
      or (ORd(ToRecyle) or FOF_ALLOWUNDO);
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function DeleteFiles(const Files: TStrings; const ToRecyle: Boolean = True): Boolean;
{
   ɾ���ļ�,����ɾ��������վ��
}
var
  fo: TSHFILEOPSTRUCT;
  i: integer;
  FFiles: string;
begin
  for i := 0 to Files.Count - 1 do
    FFiles := FFiles + Files[i] + #0;
  FillChar(fo, SizeOf(fo), 0);
  with fo do
  begin
    Wnd := GetActiveWindow;
    wFunc := FO_DELETE;
    pFrom := PChar(FFiles + #0);
    pTo := nil;
    if ToRecyle then
      fFlags := FOF_ALLOWUNDO or FOF_NOCONFIRMATION
    else
      fFlags := FOF_NOCONFIRMATION;
  end;
  Result := (SHFileOperation(fo) = 0);
end;

function FileSizeEx(const FileName: string): Int64;
{
  �����ļ�FileName�Ĵ�С��֧�ֳ����ļ�
}
var
  Info: TWin32FindData;
  Hnd: THandle;
begin
  Result := -1;
  Hnd := FindFirstFile(PChar(FileName), Info);
  if (Hnd <> INVALID_HANDLE_VALUE) then
  begin
    Windows.FindClose(Hnd);
    Int64Rec(Result).Lo := Info.nFileSizeLow;
    Int64Rec(Result).Hi := Info.nFileSizeHigh;
  end;
end;

function GetFileSizeDisplay(const AFileSize: Int64): string;
{
  �����ļ���С��ʾ
}
begin
  if AFileSize >= 1024 * 1024 * 1024 then //����G
    Result := Format('%0.1f', [AFileSize / (1024 * 1024 * 1024)]) + ' GB'
  else if AFileSize >= 1024 * 1024 then //����M
    Result := Format('%0.1f', [AFileSize / (1024 * 1024)]) + ' MB'
  else if AFileSize >= 1024 then //����K
    Result := Format('%0.1f', [AFileSize / 1024]) + ' KB'
  else
    Result := IntToStr(AFileSize) + ' B';
end;

function GetSpeedDisplay(const ASpeed: Double): string;
{
  ���ط����ٶ���ʾ
}
begin
  if ASpeed >= 1024 * 1024 * 1024 then //����G
    Result := Format('%0.1f', [ASpeed / (1024 * 1024 * 1024)]) + ' GB/S'
  else if ASpeed >= 1024 * 1024 then //����M
    Result := Format('%0.1f', [ASpeed / (1024 * 1024)]) + ' MB/S'
  else if ASpeed >= 1024 then //����K
    Result := Format('%0.1f', [ASpeed / 1024]) + ' KB/S'
  else
    Result := Format('%0.1f',[ASpeed]) + ' B/S';
end;

function GetGUID: TGUID;
{
  ��̬����һ��GUID
}
begin
  CoCreateGuid(Result);
end;

function GetGUIDString: string;
{
  ��̬����һ��GUID�ַ���
}
begin
  Result := GUIDToString(GetGUID);
end;

function IsGUID(const S: string): Boolean;
{
  �ж�һ���ַ����Ƿ��ǺϷ���GUID
}
begin
  try
    StringToGUID(S);
    Result := True;
  except
    Result := False;
  end;
end;

function GetLastErrorString: string;
{
  ��ȡ���һ��API�����Ĵ�����Ϣ
}
begin
  Result := SysErrorMessage(GetLastError);
end;

function FixFormat(const Width: integer; const Value: Double): string; overload;
{
  ��ʽ��ValueΪָ�����Width���ַ���
}
var
  Len: integer;
begin
  Len := Length(IntToStr(Trunc(Value)));
  Result := Format('%*.*f', [Width, Width - Len, Value]);
end;

function FixFormat(const Width: integer; const Value: integer): string; overload;
{
  ��ʽ��ValueΪָ����ȵ��ַ����������λ�����Զ���0
}
begin
  Result := Format('%.*d', [Width, Value]);
end;

function FixDouble(const ACount: Integer; const AValue: Double): string;
{
  ����ָ��λ��С�����������治�����0
}
var
  dValue: Double;
begin
  dValue := ClassicRoundTo(AValue, ACount);
  Result := FloatToStr(dValue);
end;

function RoundEx(Value: Extended): Int64;
{
  ����Delphi���������BUG��Round����
}
  procedure Set8087CW(NewCW: Word);
  asm
    MOV     Default8087CW,AX
    FNCLEX
    FLDCW   Default8087CW
  end;

const
  RoundUpCW = $1B32;
var
  OldCW: Word;
begin
  OldCW := Default8087CW;
  try
    Set8087CW(RoundUpCW);
    Result := Round(Value);
  finally
    Set8087CW(OldCW);
  end;
end;

function RoundUp(Value: Extended): Int64;
{
  ������������ȡ��
}
var
  OldCW: TFPURoundingMode;
begin
  OldCW := GetRoundMode;
  try
    SetRoundMode(rmUp);
    Result := Round(Value);
  finally
    SetRoundMode(OldCW);
  end;
end;

function RoundDown(Value: Extended): Int64;
{
  ������������ȡ��
}
var
  OldCW: TFPURoundingMode;
begin
  OldCW := GetRoundMode;
  try
    SetRoundMode(rmDown);
    Result := Round(Value);
  finally
    SetRoundMode(OldCW);
  end;
end;

function RoundToEx(const AValue: Extended; const APrecision: TRoundToRange = 0): Extended;
{
  ����Delphi�������뺯��RoundTo��BUG
}
var
  ATrunc: Int64;
  APower: Extended;
  APre: TRoundToRange;
begin
  APre := Min(Max(APrecision, Low(TRoundToRange)), High(TRoundToRange));
  APower := Power(10, APre);
  ATrunc := Trunc(Int(AValue*APower));
  if AValue < 0 then ATrunc := ATrunc - 1;
  Result := AValue*APower;
  if Result >= ATrunc + 0.5 then
    Result := (ATrunc + 1) / APower
  else
    Result := ATrunc / APower;
end;

function ClassicRoundUp(const Value: Extended): Int64;
{
  ����Delphi�������뺯��Round��BUG��
  ��ȫ������ѧ�϶�����������뷽ʽ��������,
  ��������ķ���һ��Ϊ������
}
begin
  Result := Floor(Value + 0.5);
end;

function ClassicRoundDown(const Value: Extended): Int64;
{
  ����Delphi�������뺯��Round��BUG��
  ��������ķ���һ��Ϊ������
}
begin
  Result := Ceil(Value - 0.5);
end;

function ClassicRoundTo(const AValue: Extended; const APrecision: TRoundToRange): Extended;
{
  ����Delphi�������뺯��RoundTo��BUG����ȫ������ѧ�϶�����������뷽ʽ��������
}
var
  ATrunc: Int64;
  APower: Extended;
  APre: TRoundToRange;
begin
  APre := Min(Max(APrecision, Low(TRoundToRange)), High(TRoundToRange));
  APower := Power(10, APre);
  ATrunc := ClassicRoundUp(AValue*APower);
  Result := AValue*APower;
  if (Result > ATrunc + 0.5) or SameValue(Result, ATrunc + 0.5) then
    Result := (ATrunc + 1) / APower
  else
    Result := ATrunc / APower;
end;

function xhl(const B: Byte): Byte; overload;
{
  �����ߵ��ֽ�
}
asm
  rol al, 4   /// ѭ������4λ����
end;

function xhl(const W: word): word; overload;
{
  �����ߵ��ֽ�
}
asm
  // xchg ah,al
  rol ax, 8   /// ѭ������8λ����
end;

function xhl(const DW: DWord): Dword; overload;
{
  �����ߵ��ֽ�
}
asm
  rol eax, 16    /// ѭ������16λ����
end;

function InputBoxEx(const ACaption, AHint, ADefault: string; out Text: string): Boolean;
{
  ����Ի��򣬴�API���
}

  function DialogProc(Wnd: HWND; Msg: UINT; wParam: WPARAM; lParam: LPARAM): integer; stdcall;
  begin
    Result := 0;
    case Msg of
      WM_COMMAND:
        begin
          case LOWORD(wParam) of
            ID_OK:
              begin
                ShareData := GetWindowCaption(GetDlgItem(Wnd, 101));
              end;
          end;
        end;
    end;
    if Result = 0 then /// ������������δ������Ϣ
      Result := CallWindowProc(@DefDialogProc, Wnd, Msg, wParam, lParam);
  end;

var
  Controls: array of TDlgTemplateEx;
begin
  SetLength(Controls, 5);

  with Controls[0], dlgTemplate do /// ����Ի������
  begin
    style := DS_CENTER or DS_CONTEXTHELP
      or WS_SYSMENU or WS_DLGFRAME or WS_CAPTION or WS_VISIBLE;
    x := 0;
    y := 0;
    cx := 200;
    cy := 75;
    Caption := ACaption;
  end;

  with Controls[1], dlgTemplate do /// Hint label
  begin
    ClassName := 'EDIT';
    Caption := AHint + #13#10'Press Ctrl + Enter to enter a new line';
    style := ES_MULTILINE or ES_LEFT or WS_VISIBLE or ES_READONLY;
    dwExtendedStyle := 0;
    x := 10;
    y := 5;
    cx := 180;
    cy := 15;
    id := 100;
  end;

  with Controls[2], dlgTemplate do /// Edit Box
  begin
    ClassName := 'EDIT';
    Caption := ADefault;
    style := ES_MULTILINE or ES_AUTOVSCROLL
      or WS_VISIBLE or WS_BORDER or WS_TABSTOP or WS_VSCROLL;
    dwExtendedStyle := 0;
    x := 10;
    y := 25;
    cx := 180;
    cy := 30;
    id := 101;
  end;

  with Controls[3], dlgTemplate do /// OK Button
  begin
    ClassName := 'BUTTON';
    Caption := '&OK';
    style := WS_VISIBLE or WS_TABSTOP or BS_DEFPUSHBUTTON;
    dwExtendedStyle := 0;
    x := 125;
    y := 60;
    cx := 30;
    cy := 12;
    id := ID_OK;
  end;

  with Controls[4], dlgTemplate do /// OK Button
  begin
    ClassName := 'BUTTON';
    Caption := '&Cancel';
    style := WS_VISIBLE or WS_TABSTOP;
    dwExtendedStyle := 0;
    x := 160;
    y := 60;
    cx := 30;
    cy := 12;
    id := ID_CANCEL;
  end;
  Result := DialogBoxEx(Controls, @DialogProc) = ID_OK;
  if Result then Text := ShareData;
end;

function GetWindowClassName(hWnd: HWND): string;
{
   ����ָ�����ڵ�����
}
begin
  SetLength(Result, 255);
  GetClassName(hWnd, PChar(Result), Length(Result));
end;

function GetWindowCaption(hWnd: HWND): string;
{
  ����ָ�����ڵĴ��ڱ�������
}
begin
  SetLength(Result, GetWindowTextLength(hWnd) + 1);
  GetWindowText(hWnd, PChar(Result), Length(Result));
  Result := PChar(Result);
end;

function DefDialogProc(Wnd: HWND; Msg: UINT; wParam: WPARAM; lParam: LPARAM): Integer; stdcall;
{

}
var
  Icon: HICON;
  i: integer;
  C: array of TDlgTemplateEx;
  Font: HFONT;
begin
  Result := 0;
  C := nil;
  Font := 0;
  Icon := 0;
  case Msg of
    WM_INITDIALOG:
      begin
        C := Pointer(lParam);
        Font := MakeFont('����', 12, 0, False, False, False);
        for i := Succ(Low(C)) to High(C) do
        begin
          SetWindowText(GetDlgItem(Wnd, C[i].dlgTemplate.id), PChar(C[i].Caption));
          SetFont(GetDlgItem(Wnd, C[i].dlgTemplate.id), Font);
        end;
        Icon := LoadIcon(HInstance, 'mainicon');
        SendMessage(Wnd, WM_SETICON, ICON_BIG, IfThen(Icon <> 0, Icon, LoadIcon(0, IDI_WARNING)));
        Result := 1;
      end;
    WM_NOTIFY:
      {case LOWORD(wParam) of

      end };
    WM_COMMAND:
      case LOWORD(wParam) of
        IDOK, IDCANCEL:
          begin
            DeleteObject(Font);
            DestroyIcon(Icon);
            EndDialog(Wnd, wParam);
          end;
      else
        Result := LoWord(wParam);
      end;
    WM_CLOSE:
      begin
        DeleteObject(Font);
        DestroyIcon(Icon);
        EndDialog(Wnd, wParam);
      end;
    WM_HELP:
      begin
        DlgInfo('Copyright(C) , 2004, Dialog powered by DialogBoxEx.', SInformation);
      end;
  end;
end;

function DialogBoxEx(const Controls: array of TDlgTemplateEx; const DlgProc: Pointer = nil): Integer;
{
  ʹ���ڴ�ģ��������ʾһ���Ի���Controls����Ի�����ۺͶԻ�����Ŀ���ݣ�
  ���ʹ�ñ���������ο�DlgInputText��������Ϊ������ʾ��
  ��������Controls��ѡ��Ķ�Ӧ���ID��������OK���򷵻�OK��ť��ID
}
  function lpwAlign(lpIn: DWORD): DWORD;
  begin
    Result := lpIn + 3;
    Result := Result shr 2;
    Result := Result shl 2;
  end;

var
  hgbl: HGLOBAL; /// ����DiaologBoxInDirect���ڴ����ݿ�
  lpdt: PDLGTEMPLATE; /// �Ի���ģ�����ݽṹ
  lpwd: ^TWordArray;
  lpwsz: LPWSTR;
  lpdit: PDLGITEMTEMPLATE; /// �Ի�����Ŀģ������
  nchar: BYTE;
  i: Integer;
begin
  Result := 0;
  if Length(Controls) = 0 then Exit;

  hgbl := GlobalAlloc(GMEM_ZEROINIT, 4096);
  if hgbl = 0 then Exit;

  /// define dialog
  lpdt := GlobalLock(hgbl);
  lpdt.style := Controls[0].dlgTemplate.style and (not DS_SETFONT);
  lpdt.dwExtendedStyle := Controls[0].dlgTemplate.dwExtendedStyle;
  lpdt.cdit := Length(Controls) - 1;
  lpdt.x := Controls[0].dlgTemplate.x;
  lpdt.y := Controls[0].dlgTemplate.y;
  lpdt.cx := Controls[0].dlgTemplate.cx;
  lpdt.cy := Controls[0].dlgTemplate.cy;
  lpwd := Pointer(DWORD(lpdt) + SizeOf(TDlgTemplate));
  lpwd[0] := 0;
  lpwd[1] := 0;
  lpwsz := Pointer(DWORD(lpwd) + 4);
  nchar := MultiByteToWideChar(CP_ACP, 0, PChar(Controls[0].Caption), Length(Controls[0].Caption), lpwsz, 50) + 1;
  lpwd := Pointer(DWORD(lpwsz) + nchar * 2);
  lpwd := Pointer(lpwAlign(DWORD(lpwd))); // align DLGITEMTEMPLATE on DWORD boundary

  for i := Succ(Low(Controls)) to High(Controls) do
  begin /// Define Controls
    lpdit := Pointer(lpwd);
    lpdit.x := Controls[i].dlgTemplate.x;
    lpdit.y := Controls[i].dlgTemplate.y;
    lpdit.cx := Controls[i].dlgTemplate.cx;
    lpdit.cy := Controls[i].dlgTemplate.cy;
    lpdit.style := Controls[i].dlgTemplate.style;
    lpdit.id := Controls[i].dlgTemplate.id;
    lpwd := Pointer(DWORD(lpdit) + SizeOf(TDlgItemTemplate));
    lpwsz := Pointer(DWORD(lpwd));
    nchar := MultiByteToWideChar(CP_ACP, 0, PChar(Controls[i].ClassName), Length(Controls[i].ClassName), lpwsz, 50) + 1;
    lpwd := Pointer(DWORD(lpwsz) + nchar * 2);
    lpwd[0] := 0;
    lpwd := Pointer(lpwAlign(DWORD(lpwd) + 2)); // align DLGITEMTEMPLATE on DWORD boundary
  end;

  GlobalUnlock(hgbl);
  if DlgProc = nil then
    Result := DialogBoxIndirectParam(hInstance, PDlgTemplate(hgbl)^, GetActiveWindow, @DefDialogProc, Integer(@Controls))
  else
    Result := DialogBoxIndirectParam(hInstance, PDlgTemplate(hgbl)^, GetActiveWindow, DlgProc, Integer(@Controls));
  GlobalFree(hgbl);
end;

function MakeFont(FontName: string; Size, Bold: integer; StrikeOut, Underline, Italy: Boolean): Integer;
{
  ����ָ��������
}
begin
  Result := CreateFont(Size, 0, 0, 0, Bold,
    Ord(Italy), Ord(UnderLine), Ord(StrikeOut),
    DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, DEFAULT_QUALITY,
    DEFAULT_PITCH or FF_DONTCARE, PChar(FontName));
end;

procedure SetFont(hWnd: HWND; Font: HFONT);
{
  ���ÿؼ�������Ϊָ��������
}
begin
  SendMessage(hWnd, WM_SETFONT, Font, 0);
end;

procedure DlgInfo(const Msg, Title: string);
{
  �򵥵���ʾ��Ϣ�Ի���
}
begin
  MsgBox(Msg, Title);
end;

procedure DlgError(const Msg: string);
{
  ��ʾ������Ϣ�Ի���
}
begin
  MsgBox(Msg, PChar(SError), MB_ICONERROR + MB_OK);
end;

function MsgBox(const Msg: string; const Title: string = 'Info'; dType: integer = MB_OK + MB_ICONINFORMATION): integer;
{
  ��ʾһ����Ϣ�Ի���
}
begin
  Result := MessageBox(GetActiveWindow, PChar(Msg), PChar(Title), dType);
end;

function SelectDirectoryEx(var Path: string; const Caption, Root: string; BIFs: DWORD = $59;
  Callback: TSelectDirectoryProc = nil; const FileName: string = ''): Boolean;
{
  ���ñ�׼��Windows���Ŀ¼�Ի��򲢷����û�ѡ���Ŀ¼·�������ҿ���ǿ���û�ѡ��
  ��Ŀ¼�б������ĳ���ļ�
  ������
    Path�����롢�����������Ϊ��ʼ��ѡ���Ŀ¼�����Ϊ�û�ѡ���Ŀ¼·��
    Caption�����û�����ʾ��Ϣ
    Root����Ϊ��Ŀ¼��Ŀ¼�����Ϊ�գ������ѡ������Ŀ¼����Ϊ�����û�ֻ��ѡ��
          RootĿ¼��������Ŀ¼������ѡ��������Ŀ¼
    BIF�������û��ܹ�ѡ��Ŀ¼�����ͣ�������ѡ���������Ǵ�ӡ�������ļ����������
         ShellObject�Լ��Ի������ۣ������ǲ������½��ļ��а�ť��
    FileName�����Ϊ�գ���ô�û�����ѡ������Ŀ¼������Ļ����û�ѡ���Ŀ¼�������
         �ļ�FileName
    ����ֵ���û������Ok����True�����򷵻�False
}
const
  BIF_NEWDIALOGSTYLE = $0040;
type
  TMyData = packed record
    IniPath: PChar;
    FileName: PChar;
    Proc: TSelectDirectoryProc;
    Flag: DWORD;
  end;
  PMyData = ^TMyData;

  function BrowseCallbackProc(hwnd: HWND; uMsg: UINT; lParam: Cardinal; lpData: Cardinal): integer; stdcall;
  var
    PathName: array[0..MAX_PATH] of char;
  begin
    case uMsg of
      BFFM_INITIALIZED:
        SendMessage(Hwnd, BFFM_SETSELECTION, Ord(True), integer(PMyData(lpData).IniPath));
      BFFM_SELCHANGED:
        begin
          SHGetPathFromIDList(PItemIDList(lParam), @PathName);
          SendMessage(hwnd, BFFM_SETSTATUSTEXT, 0, LongInt(PChar(@PathName)));
          if Assigned(PMyData(lpData).Proc) then
            SendMessage(hWnd, BFFM_ENABLEOK, 0, Ord(PMyData(lpData).Proc(PathName)))
          else if PMyData(lpData).FileName <> nil then
            SendMessage(hWnd, BFFM_ENABLEOK, 0,
                        Ord(FileExists(IncludeTrailingPathDelimiter(PathName) + PMyData(lpData).FileName)))
          else if (BIF_VALIDATE and PMyData(lpData).Flag) = BIF_VALIDATE then
            SendMessage(hWnd, BFFM_ENABLEOK, 0, Ord(DirectoryExists(PathName)));
        end;
    end;
    Result := 0;
  end;

var
  BrowseInfo: TBrowseInfo;
  Buffer: PChar;
  RootItemIDList, ItemIDList: PItemIDList;
  ShellMalloc: IMalloc;
  IDesktopFolder: IShellFolder;
  Dummy: LongWord;
  Data: TMyData;
begin
  Result := False;
  FillChar(BrowseInfo, SizeOf(BrowseInfo), 0);
  if (ShGetMalloc(ShellMalloc) = S_OK) and (ShellMalloc <> nil) then
  begin
    Buffer := ShellMalloc.Alloc(MAX_PATH);
    try
      RootItemIDList := nil;
      if Root <> '' then
      begin
        SHGetDesktopFolder(IDesktopFolder);
        IDesktopFolder.ParseDisplayName(GetActiveWindow, nil, POleStr(WideString(Root)), Dummy, RootItemIDList, Dummy);
      end;

      with BrowseInfo do
      begin
        hwndOwner := GetActiveWindow;
        pidlRoot := RootItemIDList;
        pszDisplayName := Buffer;
        lpszTitle := PChar(Caption);
        ulFlags := BIFs;
        lpfn := @BrowseCallbackProc;
        Data.IniPath := PChar(Path);
        Data.Flag := BIFs;
        if FileName <> '' then
          Data.FileName := PChar(FileName)
        else
          Data.FileName := nil;
        Data.Proc := Callback;
        lParam := Integer(@Data);
      end;
      ItemIDList := ShBrowseForFolder(BrowseInfo);
      Result := ItemIDList <> nil;
      if Result then
      begin
        ShGetPathFromIDList(ItemIDList, Buffer);
        ShellMalloc.Free(ItemIDList);
        Path := IncludeTrailingPathDelimiter(StrPas(Buffer));
      end;
    finally
      ShellMalloc.Free(Buffer);
    end;
  end;
end;

procedure CreateFileOnDisk(FileName: string);
{
  �ڴ������洴��ָ�����ļ�
}
begin
  if not FileExists(FileName) then FileClose(FileCreate(FileName));
end;

function IsFileInUse(const FileName: string): boolean;
{
  �ж��ļ�FileName�Ƿ����ڱ���/ʹ��
}
var
  HFileRes: HFILE;
begin
  if not FileExists(FileName) then
  begin
    Result := False;
    Exit;
  end;

  try
    HFileRes := CreateFile(pchar(FileName), GENERIC_READ,
      0 {this is the trick!}, nil, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
    Result := (HFileRes = INVALID_HANDLE_VALUE);
    if not Result then
      CloseHandle(HFileRes);
  except
    Result := true;
  end;
end;

function GetTmpFile(const AExt: string): string;
{
   ������չ������һ����ʱ�ļ���
}
var
  sPath: string;
begin
  sPath := TempPath;
  repeat
    Result := sPath + '~' + IntToStr(GetTickCount) + AExt;
    Sleep(1);
  until not FileExists(Result);
end;

function GetTmpFileEx(const APath, APre, AExt: string): string;
{
   ������չ������һ����ʱ�ļ���
}
begin
  repeat
    Result := APath + APre + IntToStr(GetTickCount) + AExt;
    Sleep(1);
  until not FileExists(Result);
end;

function TempPath: string;
{
  ������ʱ�ļ���Ŀ¼·��
}
begin
  SetLength(Result, GetTempPath(0, PChar(Result)));
  ZeroMemory(PChar(Result), Length(Result));
  GetTempPath(Length(Result), PChar(Result));
  Result := PChar(Result);
end;

procedure GetFileList(Files: TStrings; Folder, FileSpec: string; SubDir: Boolean = True; CallBack: TFindFileProc = nil);
{
  ��ȡ�ļ����б�
  Files���������淵�ص��ļ����б�
  Folder����Ҫɨ����ļ���
  FileSpec���ļ�����֧��ͨ���*��?
  SubDir���Ƿ������Ŀ¼�µ��ļ�
}
var
  SRec: TSearchRec; //Required for Find* functions.
  FFolder: string;
begin
  FFolder := IncludeTrailingPathDelimiter(Folder);
  if FindFirst(FFolder + FileSpec, faAnyFile, SRec) = 0 then
  begin
    repeat
      if Assigned(CallBack) then CallBack(FFolder + SRec.Name, SRec);
      if ((SRec.Attr and faDirectory) <> faDirectory) and (SRec.Name[1] <> '.') then
        Files.Add(FFolder + SRec.Name);
    until FindNext(SRec) <> 0;
    FindClose(SRec);
  end;

  if SubDir then
    if FindFirst(FFolder + '*', faDirectory, SRec) = 0 then
    begin
      repeat
        if ((SRec.Attr and faDirectory) = faDirectory) and (SRec.Name[1] <> '.') then
          GetFileList(Files, FFolder + SRec.Name, FileSpec, SubDir, CallBack);
      until FindNext(SRec) <> 0;
      FindClose(SRec);
    end;
end;

procedure GetFileListName(Files: TStrings; const Folder, FileSpec: string);
{
  ����һ��ָ��Ŀ¼��ָ���ļ����ļ������ϣ����������ļ����µ��ļ�
  Files���������淵�ص��ļ����б�
  Folder����Ҫɨ����ļ���
  FileSpec���ļ�����֧��ͨ���*��?
}
var
  SRec: TSearchRec; //Required for Find* functions.
  FFolder: string;
begin
  FFolder := IncludeTrailingPathDelimiter(Folder);
  if FindFirst(FFolder + FileSpec, faAnyFile, SRec) = 0 then
  begin
    repeat
      if ((SRec.Attr and faDirectory) <> faDirectory) and (SRec.Name[1] <> '.') then
        Files.Add(SRec.Name);
    until FindNext(SRec) <> 0;
    FindClose(SRec);
  end;
end;

procedure GetDirectoryList(Directorys: TStrings; const Folder: string);
{
  ��ȡ�ļ����б�
  Directorys���������淵�ص�Ŀ¼�б�
}
var
  SRec: TSearchRec; //Required for Find* functions.
  FFolder: string;
begin
  FFolder := IncludeTrailingPathDelimiter(Folder);
  if FindFirst(FFolder + '*.*', faAnyFile, SRec) = 0 then
  begin
    repeat
      if ((SRec.Attr and faDirectory) = faDirectory) and (SRec.Name[1] <> '.') then
        Directorys.Add(FFolder + SRec.Name);
    until FindNext(SRec) <> 0;
    FindClose(SRec);
  end;
end;

procedure GetDirectoryListName(Directorys: TStrings; const Folder: string);
{
  ��ȡĿ¼�б�
  Directorys���������淵�ص�Ŀ¼���б�
}
var
  SRec: TSearchRec; //Required for Find* functions.
  FFolder: string;
begin
  FFolder := IncludeTrailingPathDelimiter(Folder);
  if FindFirst(FFolder + '*.*', faAnyFile, SRec) = 0 then
  begin
    repeat
      if ((SRec.Attr and faDirectory) = faDirectory) and (SRec.Name[1] <> '.') then
        Directorys.Add(SRec.Name);
    until FindNext(SRec) <> 0;
    FindClose(SRec);
  end;
end;

function IsSpace(const Ch: Char): Boolean;
{
  �ж�Ch�Ƿ��ǿհ��ַ�
}
begin
  Result := Ch in [#32, #8, #13, #10];
end;

function IsXDigit(const Ch: Char): Boolean;
{
  �ж�Ch�Ƿ���ʮ�����������ַ�
}
begin
  Result := ch in ['0'..'9', 'a'..'f', 'A'..'F'];
end;

function IsInteger(const S: string): Boolean;
{
  �ж��ַ���S�Ƿ���һ���Ϸ����������ǣ�����True�����򷵻�False
}
var
  iValue: Integer;
begin
  Result := TryStrToInt(S, iValue);
end;

function IsInt64(const S: string): Boolean;
var
  iValue: Int64;
begin
  Result := TryStrToInt64(S, iValue);
end;

function IsFloat(const S: string): Boolean;
{
  �ж��ַ���S�Ƿ���һ���Ϸ��ĸ��������ǣ�����True�����򷵻�False
}
var
  dValue: Double;
begin
  Result := TryStrToFloat(S, dValue);
end;

function LeftPart(Source, Seperator: string): string;
{
  �����ַ�����Seperator��ߵ��ַ���������LeftPart('ab|cd','|')='ab'
  ֻ���ص�һ��Seperator�������ַ��������Ҳ���Seperator���򷵻������ַ���
}
var
  iPos: integer;
begin
  iPos := Pos(Seperator, Source);
  if iPos > 0 then
    Result := Copy(Source, 1, iPos - 1)
  else
    Result := Source;
end;

function RightPart(Source, Seperator: string): string;
{
  �����ַ�����Seperator�ұ߱ߵ��ַ���������RightPart('ab|cd','|')='ab'
  ֻ���ص�һ��Seperator���ұ��ַ��������Ҳ���Seperator���򷵻������ַ���
}
var
  iPos: integer;
begin
  iPos := Pos(Seperator, Source);
  if iPos > 0 then
    Result := Copy(Source, iPos + Length(Seperator), Length(Source) - iPos)
  else
    Result := Source;
end;

function LastRightPart(Source, Seperator: string): string;
{
  �����ַ�����Seperator���ұߵ��ַ���������LastRightPart('ab|cd|ef','|')='ef'
}
var
  i, iPos, iLen, iSepLen: Integer;
  sPart: string;
begin
  iPos := 1;
  iLen := Length(Source);
  iSepLen := Length(Seperator);
  for i := iLen-iSepLen+1 downto 1 do
  begin
    sPart := Copy(Source, i, iSepLen);
    if sPart = Seperator then
    begin
      iPos := i+1;
      Break;
    end;
  end;
  Result := Copy(Source, iPos, MAXWORD);
end;

function GMTNow: TDateTime;
{
  ���ص�ǰ��ϵͳ��GMT/UTCʱ��
}
var
  TimeRec: TSystemTime;
begin
  GetSystemTime(TimeRec);
  Result := SystemTimeToDateTime(TimeRec);
end;

const
  MinsPerDay = 24 * 60;

function GetGMTBias: Integer;
var
  info: TTimeZoneInformation;
  Mode: DWord;
begin
  Mode := GetTimeZoneInformation(info);
  Result := info.Bias;
  case Mode of
    TIME_ZONE_ID_INVALID: RaiseLastOSError;
    TIME_ZONE_ID_STANDARD: Result := Result + info.StandardBias;
    TIME_ZONE_ID_DAYLIGHT: Result := Result + info.DaylightBias;
  end;
end;

function LocaleToGMT(const Value: TDateTime): TDateTime;
{
  �ѱ���ʱ��Valueת����GMT/UTCʱ��
}
begin
  Result := Value + (GetGMTBias / MinsPerDay);
end;

function GMTToLocale(const Value: TDateTime): TDateTime;
{
  ��GMT/UTCʱ��Valueת���ɱ���ʱ��
}
begin
  Result := Value - (GetGMTBias / MinsPerDay);
end;

function HKEYToStr(const Key: HKEY): string;
begin
  if (Key < HKEY_CLASSES_ROOT) or (Key > HKEY_DYN_DATA) then
    Result := ''
  else
    Result := CSHKeyNames[HKEY_CLASSES_ROOT];
end;

function StrToHKEY(const KEY: string): HKEY;
begin
  for Result := Low(CSHKeyNames) to High(CSHKeyNames) do
    if SameText(CSHKeyNames[Result], KEY) then
      Exit;
  Result := $FFFFFFFF;
end;

function RegExtractHKEY(const Reg: string): HKEY;
{
  ������ע���·������ȡHKEY
}
begin
  Result := StrToHKEY(LeftPart(Reg, '\'));
end;

function RegExtractSubKey(const Reg: string): string;
{
  �������ַ�������ȡע���SubKey
}
begin
  Result := RightPart(Reg, '\');
end;

function RegExportToFile(const RootKey: HKEY; const SubKey, FileName: string): Boolean;
{
  ����ע����ļ�
}
var
  Key: HKEY;
begin
  Result := False;
  EnablePrivilege('SeBackupPrivilege', True);
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_ALL_ACCESS, Key) then
  begin
    Result := RegSaveKey(Key, PChar(FileName), nil) = ERROR_SUCCESS;
    RegCloseKey(Key);
  end;
end;

function RegImportFromFile(const RootKey: HKEY; const SubKey, FileName: string): Boolean;
{
  ����ע���
}
begin
  EnablePrivilege('SeBackupPrivilege', True);
  Result := RegLoadKey(RootKey, PChar(SubKey), PChar(FileName)) = ERROR_SUCCESS;
end;

function RegReadString(const RootKey: HKEY; const SubKey, ValueName: string): string;
var
  Key: HKEY;
  T: DWORD;
  L: DWORD;
begin
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_ALL_ACCESS, Key) then
  begin
    if ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @T, nil, @L) then
    begin
      if T <> REG_SZ then raise Exception.Create(SErrDataType);
      SetLength(Result, L);
      RegQueryValueEx(Key, PChar(ValueName), nil, @T, PByte(PChar(Result)), @L);
    end;
    SetString(Result, PChar(Result), L - 1);
    RegCloseKey(Key);
  end;
end;

function RegReadInteger(const RootKey: HKEY; const SubKey, ValueName: string): integer;
var
  Key: HKEY;
  T: DWORD;
  L: DWORD;
begin
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_ALL_ACCESS, Key) then
  begin
    if ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @T, nil, @L) then
    begin
      if T <> REG_DWORD then raise Exception.Create(SErrDataType);
      RegQueryValueEx(Key, PChar(ValueName), nil, @T, @Result, @L);
    end;
    RegCloseKey(Key);
  end;
end;

function RegReadBinary(const RootKey: HKEY; const SubKey, ValueName: string; Data: PChar; out Len: integer): Boolean;
{
  ��ע����ж�ȡ����������
  RootKey��ָ������֧
  SubKey���Ӽ�������
  ValueName������������Ϊ�գ�Ϊ�ռ���ʾĬ��ֵ
  Data�������ȡ��������
  Len����ȡ�������ݵĳ���
}
var
  Key: HKEY;
  T: DWORD;
begin
  Result := False;
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_ALL_ACCESS, Key) then
  begin
    if ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @T, nil, @Len) then
    begin
      ReallocMem(Data, Len);
      Result := ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @T, PByte(Data), @Len);
    end;
    RegCloseKey(Key);
  end;
end;

function RegWriteString(const RootKey: HKEY; const SubKey, ValueName, Value: string): Boolean;
{
  д��һ���ַ�����ע�����
  RootKey��ָ������֧
  SubKey���Ӽ�������
  ValueName������������Ϊ�գ�Ϊ�ռ���ʾд��Ĭ��ֵ
  Value������
}
var
  Key: HKEY;
  R: DWORD;
begin
  Result := (ERROR_SUCCESS = RegCreateKeyEx(RootKey, PChar(SubKey), 0, 'Data',
    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nil, Key, @R)) and
    (ERROR_SUCCESS = RegSetValueEx(Key, PChar(ValueName), 0, REG_SZ, PChar(Value), Length(Value)));
  RegCloseKey(Key);
end;

function RegReadMultiString(const RootKey: HKEY; const SubKey, ValueName: string): string;
{
  ��ȡע�����ָ���Ķ��ַ���
}
var
  Key: HKEY;
  T: DWORD;
  L: DWORD;
begin
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_ALL_ACCESS, Key) then
  begin
    if ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @T, nil, @L) then
    begin
      if T <> REG_MULTI_SZ then raise Exception.Create(SErrDataType);
      SetLength(Result, L);
      RegQueryValueEx(Key, PChar(ValueName), nil, @T, PByte(PChar(Result)), @L);
    end;
    SetString(Result, PChar(Result), L);
    RegCloseKey(Key);
  end;
end;

function RegWriteMultiString(const RootKey: HKEY; const SubKey, ValueName, Value: string): Boolean;
{
  д���ַ�����ע�����
  ValueΪ#0�ָ��Ķ����ַ����������˫#0#0��β
}
var
  Key: HKEY;
  R: DWORD;
begin
  Result := (ERROR_SUCCESS = RegCreateKeyEx(RootKey, PChar(SubKey), 0, 'Data',
    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nil, Key, @R)) and
    (ERROR_SUCCESS = RegSetValueEx(Key, PChar(ValueName), 0, REG_MULTI_SZ, PChar(Value), Length(Value)));
  RegCloseKey(Key);
end;

function RegWriteInteger(const RootKey: HKEY; const SubKey, ValueName: string; const Value: integer): Boolean;
{
  д��һ��������ע�����
  RootKey��ָ������֧
  SubKey���Ӽ�������
  ValueName������������Ϊ�գ�Ϊ�ռ���ʾд��Ĭ��ֵ
  Value������
}
var
  Key: HKEY;
  R: DWORD;
begin
  Result := (ERROR_SUCCESS = RegCreateKeyEx(RootKey, PChar(SubKey), 0, 'Data',
    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nil, Key, @R)) and
    (ERROR_SUCCESS = RegSetValueEx(Key, PChar(ValueName), 0, REG_DWORD, @Value, SizeOf(Value)));
  RegCloseKey(Key);
end;

function RegWriteBinary(const RootKey: HKEY; const SubKey, ValueName: string; Data: PChar; Len: integer): Boolean;
{
  ��ע����ж�ȡ����������
}
var
  Key: HKEY;
  R: DWORD;
begin
  Result := (ERROR_SUCCESS = RegCreateKeyEx(RootKey, PChar(SubKey), 0, 'Data',
    REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, nil, Key, @R)) and
    (ERROR_SUCCESS = RegSetValueEx(Key, PChar(ValueName), 0, REG_BINARY, Data, Len));
  RegCloseKey(Key);
end;

function RegValueExists(const RootKey: HKEY; const SubKey, ValueName: string): Boolean;
{
  �ж�ע������Ƿ����ָ���ļ���
}
var
  Key: HKEY;
  Dummy: DWORD;
begin
  Result := False;
  if ERROR_SUCCESS = RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_READ, Key) then
  begin
    Result := ERROR_SUCCESS = RegQueryValueEx(Key, PChar(ValueName), nil, @Dummy, nil, @Dummy);
    RegCloseKey(Key);
  end;
end;

function RegKeyExists(const RootKey: HKEY; const SubKey: string): Boolean;
{
  �ж�ע������Ƿ����ָ���ļ�
}
var
  Key: HKEY;
begin
  Result := RegOpenKey(RootKey, PChar(SubKey), Key) = ERROR_SUCCESS;
  if Result then RegCloseKey(Key);
end;

function RegKeyDelete(const RootKey: HKEY; const SubKey: string): Boolean;
{
  ɾ��ע�����ָ��������
}
begin
  Result := RegDeleteKey(RootKey, PChar(SubKey)) = ERROR_SUCCESS;
end;

function RegValueDelete(const RootKey: HKEY; const SubKey, ValueName: string): Boolean;
{
  ɾ��ע�����ָ���ļ�ֵ
}
var
  RegKey: HKEY;
begin
  Result := False;
  if RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_SET_VALUE, RegKey) = ERROR_SUCCESS then
  begin
    Result := RegDeleteValue(RegKey, PChar(ValueName)) = ERROR_SUCCESS;
    RegCloseKey(RegKey);
  end
end;

function RegGetValueNames(const RootKey: HKEY; const SubKey: string; Names: TStrings): Boolean;
{
  ����ע�����ָ�������µ����еļ����б�
}
var
  RegKey: HKEY;
  I: DWORD;
  Size: DWORD;
  NumSubKeys: DWORD;
  NumSubValues: DWORD;
  MaxSubValueLen: DWORD;
  ValueName: string;
begin
  Result := False;
  if RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_READ, RegKey) = ERROR_SUCCESS then
  begin
    if RegQueryInfoKey(RegKey, nil, nil, nil, @NumSubKeys, nil, nil, @NumSubValues,
      @MaxSubValueLen, nil, nil, nil) = ERROR_SUCCESS then
    begin
      SetLength(ValueName, MaxSubValueLen + 1);
      if NumSubValues <> 0 then
        for I := 0 to NumSubValues - 1 do
        begin
          Size := MaxSubValueLen + 1;
          RegEnumValue(RegKey, I, PChar(ValueName), Size, nil, nil, nil, nil);
          Names.Add(PChar(ValueName));
        end;
      Result := True;
    end;
    RegCloseKey(RegKey);
  end;
end;

function RegGetKeyNames(const RootKey: HKEY; const SubKey: string; Names: TStrings): Boolean;
{
  ����ע�����ָ�������µ������Ӽ��������б�
}
var
  RegKey: HKEY;
  I: DWORD;
  Size: DWORD;
  NumSubKeys: DWORD;
  MaxSubKeyLen: DWORD;
  KeyName: string;
begin
  Result := False;
  if RegOpenKeyEx(RootKey, PChar(SubKey), 0, KEY_READ, RegKey) = ERROR_SUCCESS then
  begin
    if RegQueryInfoKey(RegKey, nil, nil, nil, @NumSubKeys, @MaxSubKeyLen, nil, nil, nil, nil, nil, nil) = ERROR_SUCCESS then
    begin
      SetLength(KeyName, MaxSubKeyLen + 1);
      if NumSubKeys <> 0 then
        for I := 0 to NumSubKeys - 1 do
        begin
          Size := MaxSubKeyLen + 1;
          RegEnumKeyEx(RegKey, I, PChar(KeyName), Size, nil, nil, nil, nil);
          Names.Add(PChar(KeyName));
        end;
      Result := True;
    end;
    RegCloseKey(RegKey);
  end
end;

function EnablePrivilege(PrivName: string; bEnable: Boolean): Boolean;
{
  ����/��ָֹ����ϵͳ��Ȩ��PrivName����Ҫ���õ���Ȩ����
  ����ֵ���ɹ����÷���True������False
}
var
  hToken: Cardinal;
  TP: TOKEN_PRIVILEGES;
  Dummy: Cardinal;
begin
  OpenProcessToken(GetCurrentProcess, TOKEN_ADJUST_PRIVILEGES or TOKEN_QUERY, hToken);
  TP.PrivilegeCount := 1;
  LookupPrivilegeValue(nil, pchar(PrivName), TP.Privileges[0].Luid);

  if bEnable then
    TP.Privileges[0].Attributes := SE_PRIVILEGE_ENABLED
  else
    TP.Privileges[0].Attributes := 0;
  AdjustTokenPrivileges(hToken, False, TP, SizeOf(TP), nil, Dummy);
  Result := GetLastError = ERROR_SUCCESS;
  CloseHandle(hToken);
end;

end.
