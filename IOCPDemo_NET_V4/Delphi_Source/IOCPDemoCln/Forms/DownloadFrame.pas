unit DownloadFrame;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, BaseFrame, ExtCtrls, ToolWin, ComCtrls, ImgList, StdCtrls, Menus,
  CommCtrl, DownloadSocket, DateUtils, Math;

type
  TRecvStatus = (rsWait, rsRecving, rsCompleted, rsReRecv, rsError, rsCancel);
  TDownloadThread = class;
  TDownloadFile = record
    DirName: string;
    LocalFileName: string;
    RecvStatus: TRecvStatus;
    DownloadThread: TDownloadThread;
    Speed: Double;
  end;
  PDownloadFile = ^TDownloadFile;

  TFamDownload = class(TFamBase)
    pnlClient: TPanel;
    pnlBottom: TPanel;
    splBottom: TSplitter;
    tvDirecotory: TTreeView;
    splLeft: TSplitter;
    lvFiles: TListView;
    lvDownloadFiles: TListView;
    ilDirectory: TImageList;
    pnlFilesClient: TPanel;
    pnlFiles: TPanel;
    btnAddFiles: TButton;
    pmFiles: TPopupMenu;
    lblUploadThreadCount: TLabel;
    cbbThreadCount: TComboBox;
    ilState: TImageList;
    tmrExecuting: TTimer;
    pmUploadFile: TPopupMenu;
    mniDeleteUploadFile: TMenuItem;
    mniCancel: TMenuItem;
    lblPacketSize: TLabel;
    cbbPacketSize: TComboBox;
    mniRefresh: TMenuItem;
    mniSumSpeed: TMenuItem;
    procedure tvDirecotoryExpanding(Sender: TObject; Node: TTreeNode;
      var AllowExpansion: Boolean);
    procedure tvDirecotoryChanging(Sender: TObject; Node: TTreeNode;
      var AllowChange: Boolean);
    procedure lvDownloadFilesCustomDrawSubItem(Sender: TCustomListView;
      Item: TListItem; SubItem: Integer; State: TCustomDrawState;
      var DefaultDraw: Boolean);
    procedure btnAddFilesClick(Sender: TObject);
    procedure tmrExecutingTimer(Sender: TObject);
    procedure pmUploadFilePopup(Sender: TObject);
    procedure mniDeleteUploadFileClick(Sender: TObject);
    procedure lvDownloadFilesDeletion(Sender: TObject; Item: TListItem);
    procedure mniCancelClick(Sender: TObject);
    procedure mniRefreshClick(Sender: TObject);
    procedure mniSumSpeedClick(Sender: TObject);
  private
    {* ��ȡĳһ���ڵ��·�� *}
    function GetDirectory(ANode: TTreeNode): string;
  public
    {* ��ʼ��Ŀ¼�ṹ *}
    procedure InitDirectory;
  end;

  {* �����߳� *}
  TDownloadThread = class(TThread)
  private
    {* Զ�̷�����·�� *}
    FDirName: string;
    {* �����ļ��� *}
    FFileName: string;
    {* ʧ�ܳ��Դ��� *}
    FAttemptCount: Word;
    {* ���Լ�� *}
    FAttemptIntervalMS: Word;
    {* ListView���� *}
    FListItem: TListItem;
    {* Э����� *}
    FDownloadSocket: TDownloadSocket;
    {* �ļ����� *}
    FFileStream: TFileStream;
    {* �ļ��ܴ�С *}
    FTotalSize: Int64;
    {* ���մ�С�ͽ���ʱ�� *}
    FRecvSize: Int64;
    FRecvTimeMS: Cardinal;
    FBeginDT: TDateTime;
    {* ִ�в�����ʾ *}
    FInformation: string;
    {* ״̬ *}
    FRecvStatus: TRecvStatus;
    {* ÿ��Ӧ�ò㷢�Ͱ���С *}
    FPacketSize: Cardinal;
    {* �����ļ����� *}
    function RecvFile: Boolean;
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
    {* ȡ�� *}
    procedure Cancel;
    property DirName: string read FDirName write FDirName;
    property FileName: string read FFileName write FFileName;
    property AttemptCount: Word read FAttemptCount write FAttemptCount;
    property AttemptIntervalMS: Word read FAttemptIntervalMS write FAttemptIntervalMS;
    property ListItem: TListItem read FListItem write FListItem;
    property PacketSize: Cardinal read FPacketSize write FPacketSize;
  end;

implementation

uses DataMgrCtr, BasisFunction, DrawListViewProgress;

{$R *.dfm}

{ TFamUpload }

procedure TFamDownload.InitDirectory;
var
  slDirectory: TStringList;
  RootNode, ChildNode: TTreeNode;
  i: Integer;
begin
  DoubleBuffered := True;
  lvDownloadFiles.DoubleBuffered := True;
  if tvDirecotory.Items.Count > 0 then
    Exit;
  RootNode := tvDirecotory.Items.Add(nil, 'Root');
  RootNode.ImageIndex := 0;
  RootNode.SelectedIndex := 0;
  slDirectory := TStringList.Create;
  try
    if GDataMgrCtr.DownloadSocket.Dir('', slDirectory) then
    begin
      for i := 0 to slDirectory.Count - 1 do
      begin
        ChildNode := tvDirecotory.Items.AddChild(RootNode, slDirectory[i]);
        ChildNode.ImageIndex := 0;
        ChildNode.SelectedIndex := 0;
        ChildNode.HasChildren := True;
      end;
      RootNode.Expanded := True;
    end
    else
    begin
      ErrorBox(GDataMgrCtr.DownloadSocket.LastError);
    end;
  finally
    slDirectory.Free;
  end;
end;

procedure TFamDownload.tvDirecotoryExpanding(Sender: TObject;
  Node: TTreeNode; var AllowExpansion: Boolean);
var
  sParentDir: string;
  slDirectory: TStringList;
  ChildNode: TTreeNode;
  i: Integer;
begin
  inherited;
  if Node.HasChildren and (Node.Count = 0) then
  begin
    sParentDir := GetDirectory(Node);
    slDirectory := TStringList.Create;
    try
      Screen.Cursor := crHourGlass;
      try
        if GDataMgrCtr.DownloadSocket.Dir(sParentDir, slDirectory) then
        begin
          for i := 0 to slDirectory.Count - 1 do
          begin
            ChildNode := tvDirecotory.Items.AddChild(Node, slDirectory[i]);
            ChildNode.ImageIndex := 0;
            ChildNode.SelectedIndex := 0;
            ChildNode.HasChildren := True;
          end;
          if Node.Count = 0 then
            Node.HasChildren := False;
        end
        else
        begin
          ErrorBox(GDataMgrCtr.DownloadSocket.LastError);
        end;
      finally
        Screen.Cursor := crDefault;
      end;
    finally
      slDirectory.Free;
    end;
  end;
end;

function TFamDownload.GetDirectory(ANode: TTreeNode): string;
begin
  Result := '';
  while Assigned(ANode) and (ANode.Level <> 0) do
  begin
    Result := ANode.Text + PathDelim + Result;
    ANode := ANode.Parent;
  end;
end;

procedure TFamDownload.tvDirecotoryChanging(Sender: TObject; Node: TTreeNode;
  var AllowChange: Boolean);
var
  sDirName: string;
  slFiles: TStringList;
  i: Integer;
  ListItem: TListItem;
begin
  inherited;
  Screen.Cursor := crHourGlass;
  slFiles := TStringList.Create;
  try
    sDirName := GetDirectory(Node);
    if GDataMgrCtr.DownloadSocket.FileList(sDirName, slFiles) then
    begin
      lvFiles.Items.BeginUpdate;
      try
        lvFiles.Clear;
        for i := 0 to slFiles.Count - 1 do
        begin
          ListItem := lvFiles.Items.Add;
          ListItem.Caption := slFiles.Names[i];
          ListItem.SubItems.Add(GetFileSizeDisplay(StrToInt64(slFiles.ValueFromIndex[i])));
        end;
      finally
        lvFiles.Items.EndUpdate;
      end;
      if not btnAddFiles.Enabled then
        btnAddFiles.Enabled := True;
    end
    else
      ErrorBox(GDataMgrCtr.DownloadSocket.LastError);
  finally
    slFiles.Free;
    Screen.Cursor := crDefault;
  end;
end;

procedure TFamDownload.lvDownloadFilesCustomDrawSubItem(
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

procedure TFamDownload.btnAddFilesClick(Sender: TObject);
var
  i: Integer;
  ListItem: TListItem;
  DownloadFile: PDownloadFile;
  sPath: string;
begin
  inherited;
  if lvFiles.SelCount = 0 then
  begin
    InfoBox('Please Select Files');
    Exit;
  end;
  if SelectDirectoryEx(sPath, 'Select Directory', '') then
  begin
    lvDownloadFiles.Items.BeginUpdate;
    try
      for i := 0 to lvFiles.Items.Count - 1 do
      begin
        if not lvFiles.Items[i].Selected then
          Continue;
        ListItem := lvDownloadFiles.Items.Add;
        ListItem.Caption := lvFiles.Items[i].Caption;
        ListItem.SubItems.Add(lvFiles.Items[i].SubItems[0]);
        ListItem.SubItems.Add('0');
        ListItem.SubItems.Add('');
        ListItem.ImageIndex := 0;
        New(DownloadFile);
        DownloadFile.DirName := GetDirectory(tvDirecotory.Selected);
        DownloadFile.LocalFileName := sPath + ListItem.Caption;
        DownloadFile.RecvStatus := rsWait;
        DownloadFile.DownloadThread := nil;
        DownloadFile.Speed := 0;
        ListItem.Data := DownloadFile;
      end;
    finally
      lvDownloadFiles.Items.EndUpdate;
    end;
  end;
end;

procedure TFamDownload.tmrExecutingTimer(Sender: TObject);
var
  i, iCount, iThreadCount: Integer;
  DownloadFile: PDownloadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    if lvDownloadFiles.Items.Count > 0 then
    begin
      iCount := 0;
      for i := 0 to lvDownloadFiles.Items.Count - 1 do //��ȡ����ִ�е��������
      begin
        DownloadFile := lvDownloadFiles.Items[i].Data;
        if DownloadFile.RecvStatus in [rsRecving] then
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
            DownloadFile := lvDownloadFiles.Items[i].Data;
            if DownloadFile.RecvStatus <> rsWait then
              Continue;
            DownloadFile.RecvStatus := rsRecving;
            DownloadFile.DownloadThread := TDownloadThread.Create(True);
            DownloadFile.DownloadThread.DirName := DownloadFile.DirName;
            DownloadFile.DownloadThread.FileName := DownloadFile.LocalFileName;
            DownloadFile.DownloadThread.ListItem := lvDownloadFiles.Items[i];
            DownloadFile.DownloadThread.PacketSize := StrToInt(cbbPacketSize.Text) * 1024;
            DownloadFile.DownloadThread.Resume;
            Inc(iCount);
          end;
        end;
      end;
      for i := 0 to lvDownloadFiles.Items.Count - 1 do //�ͷ��Ѿ���ɵ������߳�
      begin
        DownloadFile := lvDownloadFiles.Items[i].Data;
        if DownloadFile.RecvStatus in [rsCompleted, rsError] then
        begin
          FreeAndNil(DownloadFile.DownloadThread);
        end;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamDownload.pmUploadFilePopup(Sender: TObject);
begin
  inherited;
  mniDeleteUploadFile.Enabled := lvDownloadFiles.SelCount > 0;
end;

procedure TFamDownload.mniDeleteUploadFileClick(Sender: TObject);
var
  i: Integer;
  DownloadFile: PDownloadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    for i := lvDownloadFiles.Items.Count - 1 downto 0 do
    begin
      if lvDownloadFiles.Items[i].Selected then
      begin
        DownloadFile := lvDownloadFiles.Items[i].Data;
        case DownloadFile.RecvStatus of //������ڽ����У���ȡ��
          rsRecving, rsReRecv: DownloadFile.DownloadThread.Cancel;
        end;
        FreeAndNilEx(DownloadFile.DownloadThread);
        lvDownloadFiles.Items[i].Delete;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamDownload.lvDownloadFilesDeletion(Sender: TObject;
  Item: TListItem);
begin
  inherited;
  if Item.Data <> nil then
  begin
    Dispose(PDownloadFile(Item.Data));
  end;
end;

procedure TFamDownload.mniCancelClick(Sender: TObject);
var
  i: Integer;
  DownloadFile: PDownloadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    for i := lvDownloadFiles.Items.Count - 1 downto 0 do
    begin
      if lvDownloadFiles.Items[i].Selected then
      begin
        DownloadFile := lvDownloadFiles.Items[i].Data;
        case DownloadFile.RecvStatus of //������ڽ����У���ȡ��
          rsRecving, rsReRecv: DownloadFile.DownloadThread.Cancel;
        end;
        FreeAndNilEx(DownloadFile.DownloadThread);
        DownloadFile.RecvStatus := rsCancel;
        lvDownloadFiles.Items[i].ImageIndex := 3;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamDownload.mniRefreshClick(Sender: TObject);
var
  AllowChange: Boolean;
begin
  inherited;
  tvDirecotoryChanging(tvDirecotory, tvDirecotory.Selected, AllowChange);
end;

{ TSendThread }

constructor TDownloadThread.Create(CreateSuspended: Boolean);
begin
  inherited Create(CreateSuspended);
  FAttemptCount := 3; //����3��
  FAttemptIntervalMS := 15 * 1000; //���Լ��15S
  FRecvStatus := rsRecving; //״̬��Ϊ������
end;

procedure TDownloadThread.Execute;
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
       if RecvFile then
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

function TDownloadThread.RecvFile: Boolean;
var
  iFileSize: Int64;
  iLen: Integer;
  LastRecvMS: Cardinal;
begin
  FDownloadSocket := TDownloadSocket.Create;
  try
    //����
    Result := FDownloadSocket.Connect(GDataMgrCtr.DownloadSocket.Host, GDataMgrCtr.DownloadSocket.Port)
      and FDownloadSocket.Login(GDataMgrCtr.User, GDataMgrCtr.Password);
    if not Result then //ʧ�ܣ����Ҳ��ǿͻ�ȡ������´�����Ϣ
    begin
      if not Terminated then
        Synchronize(UpdateErrorInformation);
      Exit;
    end;
    FFileStream := TFileStream.Create(FileName, fmCreate or fmOpenReadWrite or fmShareDenyWrite);
    try
      FFileStream.Position := FFileStream.Size; //�Ƶ��Ѵ����Сλ��
      //���ؿ�ʼ
      Result := FDownloadSocket.Download(DirName, ExtractFileName(FileName), FFileStream.Position, FPacketSize, iFileSize);
      if not Result then
      begin
        if not Terminated then
          Synchronize(UpdateErrorInformation);
        Exit;
      end;
      FTotalSize := FFileStream.Position + iFileSize;
      if FBeginDT = 0 then
        FBeginDT := Now;
      FRecvSize := 0;
      LastRecvMS := 0;
      while (not Terminated) and (FRecvSize < iFileSize) do //��������
      begin
        Result := FDownloadSocket.RecvData(iLen);
        if Result then //���ճɹ�д����
          FFileStream.Write(FDownloadSocket.RecvBuf[0], iLen)
        else //���򷵻�
        begin
          if not Terminated then
            Synchronize(UpdateErrorInformation);
          Exit;
        end;
        FRecvSize := FRecvSize + iLen;
        FRecvTimeMS := Max(MilliSecondsBetween(Now, FBeginDT), 0);
        if LastRecvMS = 0 then
          LastRecvMS := FRecvTimeMS;
        if LastRecvMS - FRecvTimeMS >= 1000 then
        begin
          //Synchronize(UpdateProgress);
          LastRecvMS := FRecvTimeMS;
        end;
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
    FDownloadSocket.Free;
  end;
end;

procedure TDownloadThread.UpdateErrorInformation;
begin
  if Assigned(FDownloadSocket) then
    FListItem.SubItems[2] := FDownloadSocket.LastError;
end;

procedure TDownloadThread.UpdateProgress;
var
  DownloadFile: PDownloadFile;
begin
  if FFileStream.Size > 0 then
    FListItem.SubItems[1] := IntToStr(Round(FFileStream.Position / FTotalSize * 100));
  if FRecvTimeMS > 0 then
  begin
    FListItem.SubItems[2] := GetSpeedDisplay(FRecvSize / FRecvTimeMS * 1000);
    DownloadFile := FListItem.Data;
    DownloadFile.Speed := FRecvSize / FRecvTimeMS * 1000; 
  end;
end;

procedure TDownloadThread.UpdateInformation;
begin
  FListItem.SubItems[2] := FInformation;
end;

procedure TDownloadThread.UpdateStatus;
begin
  case FRecvStatus of
    rsWait: FListItem.ImageIndex := 0;
    rsRecving: FListItem.ImageIndex := 1;
    rsCompleted: FListItem.ImageIndex := 2;
    rsReRecv: FListItem.ImageIndex := 3;
    rsError: FListItem.ImageIndex := 4;
  end;
  PDownloadFile(FListItem.Data).RecvStatus := FRecvStatus;
end;

procedure TDownloadThread.Cancel;
begin
  Terminate; //ֹͣ
  FDownloadSocket.Disconnect;
  WaitFor;
  FRecvStatus := rsCancel; //״̬��Ϊȡ��
end;

procedure TFamDownload.mniSumSpeedClick(Sender: TObject);
var
  SumSpeed: Double;
  i: Integer;
begin
  inherited;
  SumSpeed := 0;
  for i := 0 to lvDownloadFiles.Items.Count - 1 do
  begin
    SumSpeed := SumSpeed + PDownloadFile(lvDownloadFiles.Items[i].Data).Speed;
  end;
  InfoBox(GetSpeedDisplay(SumSpeed));
end;

end.
