unit RemoteStreamFrame;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, BaseFrame, StdCtrls, ComCtrls, Math, DateUtils, ExtCtrls,
  ImgList, RemoteStream, DefineUnit, CommCtrl;

type
  TRecvStatus = (rsWait, rsRecving, rsCompleted, rsReRecv, rsError, rsCancel);
  TRemoteStreamThread = class;
  TRemoteStreamFile = record
    Host: string;
    Port: Integer;
    RemoteFileName: string;
    LocalFileName: string;
    RecvStatus: TRecvStatus;
    RemoteStreamThread: TRemoteStreamThread;
  end;
  PRemoteStreamFile = ^TRemoteStreamFile;

  TFamRemoteStream = class(TFamBase)
    grpFileSvr: TGroupBox;
    lblSvrFileName: TLabel;
    lblLocateFileName: TLabel;
    edtRemoteFileName: TEdit;
    btnReadRomteStream: TButton;
    btnWriteRemoteStream: TButton;
    edtLocateFileName: TEdit;
    btnSizePosition: TButton;
    pbProgress: TProgressBar;
    lblSpeed: TLabel;
    mmoSvrFile: TMemo;
    pnlBotton: TPanel;
    btnAdd: TButton;
    lvDownloadFiles: TListView;
    tmrExecuting: TTimer;
    ilState: TImageList;
    lblUploadThreadCount: TLabel;
    cbbThreadCount: TComboBox;
    lblPacketSize: TLabel;
    cbbPacketSize: TComboBox;
    procedure btnReadRomteStreamClick(Sender: TObject);
    procedure btnWriteRemoteStreamClick(Sender: TObject);
    procedure btnSizePositionClick(Sender: TObject);
    procedure lvDownloadFilesCustomDrawSubItem(Sender: TCustomListView;
      Item: TListItem; SubItem: Integer; State: TCustomDrawState;
      var DefaultDraw: Boolean);
    procedure tmrExecutingTimer(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
    procedure lvDownloadFilesDeletion(Sender: TObject; Item: TListItem);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

  TRemoteStreamThread = class(TThread)
  private
    FHost: string;
    FPort: Integer;
    {* Զ�̷������ļ��� *}
    FRemoteFile: string;
    {* �����ļ��� *}
    FFileName: string;
    {* ʧ�ܳ��Դ��� *}
    FAttemptCount: Word;
    {* ���Լ�� *}
    FAttemptIntervalMS: Word;
    {* ListView���� *}
    FListItem: TListItem;
    {* Э����� *}
    FRemoteStream: TRemoteStream;
    {* �ļ����� *}
    FFileStream: TFileStream;
    {* �ļ��ܴ�С *}
    FTotalSize: Int64;
    {* ���մ�С�ͽ���ʱ�� *}
    FRecvSize: Int64;
    FRecvTimeMS: Cardinal;
    {* ִ�в�����ʾ *}
    FInformation: string;
    {* ״̬ *}
    FRecvStatus: TRecvStatus;
    {* ÿ��Ӧ�ò㷢�Ͱ���С *}
    FPacketSize: Cardinal;
    {* �����ļ����� *}
    function ReadRemoteStream: Boolean;
  protected
    procedure Execute; override;
    {* ���´�����Ϣ *}
    procedure UpdateErrorInformation;
    {* ���½��� *}
    procedure UpdateProgress;
    {* ������ʾ��Ϣ *}
    procedure UpdateInformation;
    {* ����״̬ *}
    procedure UpdateStatus;
  public
    constructor Create(CreateSuspended: Boolean);
    property Host: string read FHost write FHost;
    property Port: Integer read FPort write FPort;
    property RemoteFileName: string read FRemoteFile write FRemoteFile;
    property FileName: string read FFileName write FFileName;
    property AttemptCount: Word read FAttemptCount write FAttemptCount;
    property AttemptIntervalMS: Word read FAttemptIntervalMS write FAttemptIntervalMS;
    property ListItem: TListItem read FListItem write FListItem;
    property PacketSize: Cardinal read FPacketSize write FPacketSize;
  end;

const
  CIBufferLength = 1024 * 1024;

implementation

uses BasisFunction, DrawListViewProgress, DataMgrCtr;

{$R *.dfm}

function GetFileSizeDisplay(const AFileSize: Int64): string;
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
begin
  if ASpeed > 1024 * 1024 * 1024 then //����G
    Result := Format('%0.2f', [ASpeed / (1024 * 1024 * 1024)]) + ' GB/S'
  else if ASpeed > 1024 * 1024 then //����M
    Result := Format('%0.2f', [ASpeed / (1024 * 1024)]) + ' MB/S'
  else if ASpeed > 1024 then //����K
    Result := Format('%0.2f', [ASpeed / 1024]) + ' KB/S'
  else
    Result := Format('%0.2f', [ASpeed]) + ' B/S';
end;

procedure TFamRemoteStream.btnReadRomteStreamClick(Sender: TObject);
var
  sSvr, sSvrFileName, sLocateFileName: string;
  iPort, iLen: Integer;
  RemoteStream: TRemoteStream;
  FileStream: TFileStream;
  Buffer: array of Byte;
  dtTime: TDateTime;
  iSecond, iPosition, iSize: Int64;
begin
  sSvr := GDataMgrCtr.Host;
  iPort := GDataMgrCtr.Port;
  sSvrFileName := edtRemoteFileName.Text;
  sLocateFileName := edtLocateFileName.Text;
  RemoteStream := TRemoteStream.Create;
  FileStream := TFileStream.Create(sLocateFileName, fmCreate or fmOpenReadWrite or fmShareDenyWrite);
  try
    if not RemoteStream.Connect(sSvr, iPort) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
    if not RemoteStream.FileExists(sSvrFileName) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
    if not RemoteStream.OpenFile(sSvrFileName, rsmRead) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);

    FileStream.Size := 0;
    SetLength(Buffer, CIBufferLength);
    dtTime := Now;
    iPosition := RemoteStream.Position;
    iSize := RemoteStream.Size;
    while iPosition < iSize do
    begin
      iLen := RemoteStream.Read(Buffer[0], CIBufferLength);
      FileStream.Write(Buffer[0], iLen);
      iPosition := iPosition + iLen;
      pbProgress.Position := Floor(iPosition / iSize * 100);
      //pbProgress.Position := Floor(RemoteStream.Position / RemoteStream.Size * 100);
      Application.ProcessMessages;
    end;
    iSecond := SecondsBetween(Now, dtTime);
    if iSecond <= 0 then
      iSecond := 1;
    lblSpeed.Caption := GetSpeedDisplay(RemoteStream.Size / iSecond);
    
    if not RemoteStream.CloseFile then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
  finally
    FileStream.Free;
    RemoteStream.Free;
  end;
end;

procedure TFamRemoteStream.btnWriteRemoteStreamClick(Sender: TObject);
var
  sSvr, sSvrFileName, sLocateFileName: string;
  iPort, iLen: Integer;
  RemoteStream: TRemoteStream;
  FileStream: TFileStream;
  Buffer: array of Byte;
  dtTime: TDateTime;
  iSecond: Int64;
begin
  inherited;
  sSvr := GDataMgrCtr.Host;
  iPort := GDataMgrCtr.Port;
  sSvrFileName := edtRemoteFileName.Text;
  sLocateFileName := edtLocateFileName.Text;
  RemoteStream := TRemoteStream.Create;
  FileStream := TFileStream.Create(sLocateFileName, fmOpenRead or fmShareDenyWrite);
  try
    if not RemoteStream.Connect(sSvr, iPort) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
    if not RemoteStream.OpenFile(sSvrFileName, rsmReadWrite) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);

    SetLength(Buffer, CIBufferLength);
    dtTime := Now;
    while FileStream.Position < FileStream.Size do
    begin
      iLen := FileStream.Read(Buffer[0], CIBufferLength);
      RemoteStream.Write(Buffer[0], iLen);
      pbProgress.Position := Floor(FileStream.Position / FileStream.Size * 100);
      Application.ProcessMessages;
    end;
    iSecond := SecondsBetween(Now, dtTime);
    if iSecond <= 0 then
      iSecond := 1;
    lblSpeed.Caption := GetSpeedDisplay(FileStream.Size / iSecond);

    if not RemoteStream.CloseFile then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
  finally
    FileStream.Free;
    RemoteStream.Free;
  end;
end;

procedure TFamRemoteStream.btnSizePositionClick(Sender: TObject);
var
  sSvr, sSvrFileName: string;
  iPort: Integer;
  RemoteStream: TRemoteStream;
begin
  inherited;
  sSvr := GDataMgrCtr.Host;
  iPort := GDataMgrCtr.Port;
  sSvrFileName := edtRemoteFileName.Text;
  RemoteStream := TRemoteStream.Create;
  try
    if not RemoteStream.Connect(sSvr, iPort) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
    if not RemoteStream.FileExists(sSvrFileName) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
    if not RemoteStream.OpenFile(sSvrFileName, rsmReadWrite) then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);

    RemoteStream.Position := 20000;
    RemoteStream.Size := 5 * 1024 * 1024;

    if not RemoteStream.CloseFile then
      raise Exception.Create(RemoteStream.RemoteStreamSocket.LastError);
  finally
    RemoteStream.Free;
  end;
end;

procedure TFamRemoteStream.lvDownloadFilesCustomDrawSubItem(
  Sender: TCustomListView; Item: TListItem; SubItem: Integer;
  State: TCustomDrawState; var DefaultDraw: Boolean);
var
  BoundRect, Rect: TRect;
begin
  BoundRect := Item.DisplayRect(drBounds);
  //��ȡSubItem��Rect
  ListView_GetSubItemRect(lvDownloadFiles.Handle, Item.Index, SubItem, LVIR_LABEL, @Rect);
  if SubItem = 2 then //��ʾ״̬��
  begin
    if Item.Selected and (cdsFocused in State) then
      Sender.Canvas.Brush.Color := clHighlight;
    //��ʼ���������
    DrawSubItem(TListView(Sender),
      Item,
      SubItem,
      StrToFloatDef(Item.SubItems[1], 0),
      100,
      psSolid,
      True,
      RGB(0, 117, 0), //�����������ɫ
      RGB(0, 117, 0)); //��������ɫ
  end
  else if SubItem <= Item.SubItems.Count then
  begin
    if Item.Selected then
    begin
      if cdsFocused in State then
      begin
        lvDownloadFiles.Canvas.Brush.Color := clHighlight;
        lvDownloadFiles.Canvas.Font.Color := clWhite;
      end;
      lvDownloadFiles.Canvas.FillRect(Rect); //��ʼ��ѡ�б���
    end;
    DrawText(lvDownloadFiles.Canvas.Handle, PChar(Item.SubItems[SubItem-1]),
      Length(Item.SubItems[SubItem-1]), Rect,
      DT_VCENTER or DT_SINGLELINE or DT_END_ELLIPSIS or DT_LEFT);
  end;

  lvDownloadFiles.Canvas.Brush.Color := clWhite;

  if Item.Selected then //��ѡ�������
  begin
    if cdsFocused in State then//�ؼ��Ƿ��ڼ���״̬
    begin
      lvDownloadFiles.Canvas.Brush.Color := $00DAA07A; // $00E2B598; //clHighlight;
    end
    else
      lvDownloadFiles.Canvas.Brush.Color := $00E2B598; //$00DAA07A // clHighlight;
    lvDownloadFiles.Canvas.DrawFocusRect(BoundRect);
  end;

  DefaultDraw := False; //����ϵͳ����
end;

procedure TFamRemoteStream.tmrExecutingTimer(Sender: TObject);
var
  i, iCount, iThreadCount: Integer;
  RemoteStreamFile: PRemoteStreamFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    if lvDownloadFiles.Items.Count > 0 then
    begin
      iCount := 0;
      for i := 0 to lvDownloadFiles.Items.Count - 1 do //��ȡ����ִ�е��������
      begin
        RemoteStreamFile := lvDownloadFiles.Items[i].Data;
        if RemoteStreamFile.RecvStatus in [rsRecving] then
          Inc(iCount);
      end;
      iThreadCount := StrToInt(cbbThreadCount.Text);
      if iCount < iThreadCount then //�������ִ�е�����
      begin
        for i := 0 to lvDownloadFiles.Items.Count - 1 do
        begin
          if iCount >= iThreadCount then Break;
          if lvDownloadFiles.Items[i].ImageIndex = 0 then //�ȴ�״̬
          begin
            RemoteStreamFile := lvDownloadFiles.Items[i].Data;
            if RemoteStreamFile.RecvStatus <> rsWait then
              Continue;
            RemoteStreamFile.RecvStatus := rsRecving;
            RemoteStreamFile.RemoteStreamThread := TRemoteStreamThread.Create(True);
            RemoteStreamFile.RemoteStreamThread.Host := RemoteStreamFile.Host;
            RemoteStreamFile.RemoteStreamThread.Port := RemoteStreamFile.Port;
            RemoteStreamFile.RemoteStreamThread.RemoteFileName := RemoteStreamFile.RemoteFileName;
            RemoteStreamFile.RemoteStreamThread.FileName := RemoteStreamFile.LocalFileName;
            RemoteStreamFile.RemoteStreamThread.ListItem := lvDownloadFiles.Items[i];
            RemoteStreamFile.RemoteStreamThread.PacketSize := StrToInt(cbbPacketSize.Text) * 1024;
            RemoteStreamFile.RemoteStreamThread.Resume;
            Inc(iCount);
          end;
        end;
      end;
      for i := 0 to lvDownloadFiles.Items.Count - 1 do //�ͷ��Ѿ���ɵ������߳�
      begin
        RemoteStreamFile := lvDownloadFiles.Items[i].Data;
        if RemoteStreamFile.RecvStatus in [rsCompleted, rsError] then
        begin
          FreeAndNil(RemoteStreamFile.RemoteStreamThread);
        end;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamRemoteStream.btnAddClick(Sender: TObject);
var
  i: Integer;
  ListItem: TListItem;
  RemoteStreamFile: PRemoteStreamFile;
  sPath: string;
begin
  inherited;
  DoubleBuffered := True;
  lvDownloadFiles.DoubleBuffered := True;
  if mmoSvrFile.Lines.Count = 0 then
  begin
    InfoBox('Please Select Files');
    Exit;
  end;
  if SelectDirectoryEx(sPath, 'Select Directory', '') then
  begin
    lvDownloadFiles.Items.BeginUpdate;
    try
      for i := 0 to mmoSvrFile.Lines.Count - 1 do
      begin
        ListItem := lvDownloadFiles.Items.Add;
        ListItem.Caption := ExtractFileName(mmoSvrFile.Lines[i]);
        ListItem.SubItems.Add('');
        ListItem.SubItems.Add('0');
        ListItem.SubItems.Add('');
        ListItem.ImageIndex := 0;
        New(RemoteStreamFile);
        RemoteStreamFile.Host := GDataMgrCtr.Host;
        RemoteStreamFile.Port := GDataMgrCtr.Port;
        RemoteStreamFile.RemoteFileName := mmoSvrFile.Lines[i];
        RemoteStreamFile.LocalFileName := sPath + ExtractFileName(RemoteStreamFile.RemoteFileName);
        RemoteStreamFile.RecvStatus := rsWait;
        RemoteStreamFile.RemoteStreamThread := nil;
        ListItem.Data := RemoteStreamFile;
      end;
    finally
      lvDownloadFiles.Items.EndUpdate;
    end;
  end;
  tmrExecuting.OnTimer := tmrExecutingTimer;
end;

procedure TFamRemoteStream.lvDownloadFilesDeletion(Sender: TObject;
  Item: TListItem);
begin
  inherited;
  if Item.Data <> nil then
  begin
    Dispose(PRemoteStreamFile(Item.Data));
  end;
end;

{ TRemoteStreamThread }

constructor TRemoteStreamThread.Create(CreateSuspended: Boolean);
begin
  inherited Create(CreateSuspended);
  FAttemptCount := 3; //����3��
  FAttemptIntervalMS := 15 * 1000; //���Լ��15S
  FRecvStatus := rsRecving; //״̬��Ϊ������
end;

procedure TRemoteStreamThread.Execute;
var
  iAttemptCount, i: Integer;
begin
  inherited;
  iAttemptCount := 1;
  while (not Terminated) and (iAttemptCount <= FAttemptCount) do
  begin
    try
      FRecvStatus := rsRecving;
      Synchronize(UpdateStatus); //����Ϊ����״̬
       if ReadRemoteStream then
         FRecvStatus := rsCompleted
       else
         FRecvStatus := rsError;
      if (FRecvStatus = rsError) and (not Terminated) then //״̬Ϊʧ�ܣ�����û��ȡ����������
      begin
        for i := 0 to FAttemptIntervalMS div 1000 do
        begin
          if Terminated then Exit;
          Sleep(1000);
          FInformation := IntToStr(FAttemptIntervalMS div 1000 - (i + 1))
            + ' Seconds And Try To Send, Have Tried ' + IntToStr(iAttemptCount) + ' Times';
          Synchronize(UpdateInformation);
        end;
        Inc(iAttemptCount);
        FRecvStatus := rsReRecv; //����Ϊ��ͣ״̬
        Synchronize(UpdateStatus);
      end
      else
      begin
        Synchronize(UpdateStatus);
        Break;
      end;
    except
      on E: Exception do
      begin
        FRecvStatus := rsError; //���ʹ���״̬
        Synchronize(UpdateStatus);
        FInformation := 'Unknow Exception: ' + E.Message;
        Synchronize(UpdateInformation);
      end;
    end;
  end;
end;

function TRemoteStreamThread.ReadRemoteStream: Boolean;
var
  iFileSize: Int64;
  iLen: Integer;
  BeginDT: TDateTime;
  Buffer: array of Byte;
begin
  FRemoteStream := TRemoteStream.Create;
  try
    //����
    Result := FRemoteStream.Connect(FHost, FPort);
    if not Result then //ʧ�ܣ����Ҳ��ǿͻ�ȡ������´�����Ϣ
    begin
      if not Terminated then
        Synchronize(UpdateErrorInformation);
      Exit;
    end;
    FFileStream := TFileStream.Create(FileName, fmCreate or fmOpenReadWrite or fmShareDenyWrite);
    try
      FFileStream.Size := 0;
      FRemoteStream.OpenFile(FRemoteFile, rsmRead);
      FFileStream.Position := FFileStream.Size; //�Ƶ��Ѵ����Сλ��
      SetLength(Buffer, FPacketSize);
      FTotalSize := FRemoteStream.Size - FFileStream.Position;
      BeginDT := Now;
      FRecvSize := 0;
      iFileSize := FTotalSize;
      while (not Terminated) and (FRecvSize < iFileSize) do //��������
      begin
        iLen := FRemoteStream.Read(Buffer[0], FPacketSize);
        if iLen > 0 then //���ճɹ�д����
          FFileStream.Write(Buffer[0], iLen)
        else //���򷵻�
        begin
          if not Terminated then
            Synchronize(UpdateErrorInformation);
          Exit;
        end;
        FRecvSize := FRecvSize + iLen;
        FRecvTimeMS := Max(MilliSecondsBetween(Now, BeginDT), 0);
        Synchronize(UpdateProgress);
      end;
      Synchronize(UpdateProgress);
      if not Result then
      begin
        if not Terminated then
          Synchronize(UpdateErrorInformation);
        Exit;
      end;
    finally
      FFileStream.Free;
    end;
  finally
    FRemoteStream.Free;
  end;
end;

procedure TRemoteStreamThread.UpdateErrorInformation;
begin
  if Assigned(FRemoteStream) then
    FListItem.SubItems[2] := FRemoteStream.RemoteStreamSocket.LastError;
end;

procedure TRemoteStreamThread.UpdateProgress;
begin
  if FFileStream.Size > 0 then
    FListItem.SubItems[1] := IntToStr(Round(FFileStream.Position / FTotalSize * 100));
  if FRecvTimeMS > 0 then
    FListItem.SubItems[2] := GetSpeedDisplay(FRecvSize / FRecvTimeMS * 1000);
end;

procedure TRemoteStreamThread.UpdateInformation;
begin
  FListItem.SubItems[2] := FInformation;
end;

procedure TRemoteStreamThread.UpdateStatus;
begin
  case FRecvStatus of
    rsWait: FListItem.ImageIndex := 0;
    rsRecving: FListItem.ImageIndex := 1;
    rsCompleted: FListItem.ImageIndex := 2;
    rsReRecv: FListItem.ImageIndex := 3;
    rsError: FListItem.ImageIndex := 4;
  end;
  PRemoteStreamFile(FListItem.Data).RecvStatus := FRecvStatus;
end;

end.
