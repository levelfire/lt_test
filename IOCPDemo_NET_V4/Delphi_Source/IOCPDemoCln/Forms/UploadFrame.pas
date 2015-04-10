unit UploadFrame;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, BaseFrame, ExtCtrls, ToolWin, ComCtrls, ImgList, StdCtrls, Menus,
  CommCtrl, UploadSocket, DateUtils, Math;

type
  TSendStatus = (ssWait, ssSending, ssCompleted, ssReSend, ssError, ssCancel);
  TUploadThread = class;
  TUploadFile = record
    DirName: string;
    FileName: string;
    SendStatus: TSendStatus;
    UploadThread: TUploadThread;
    Speed: Double;
  end;
  PUploadFile = ^TUploadFile;

  TFamUpload = class(TFamBase)
    pnlClient: TPanel;
    pnlBottom: TPanel;
    splBottom: TSplitter;
    tvDirecotory: TTreeView;
    splLeft: TSplitter;
    lvFiles: TListView;
    lvUploadFiles: TListView;
    ilDirectory: TImageList;
    pnlFilesClient: TPanel;
    pnlFiles: TPanel;
    btnAddFiles: TButton;
    dlgOpen: TOpenDialog;
    pmDirectory: TPopupMenu;
    mniNew: TMenuItem;
    mniDelete: TMenuItem;
    pmFiles: TPopupMenu;
    mniDeleteFile: TMenuItem;
    lblUploadThreadCount: TLabel;
    cbbUploadThreadCount: TComboBox;
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
    procedure pmDirectoryPopup(Sender: TObject);
    procedure mniNewClick(Sender: TObject);
    procedure mniDeleteClick(Sender: TObject);
    procedure pmFilesPopup(Sender: TObject);
    procedure mniDeleteFileClick(Sender: TObject);
    procedure lvUploadFilesCustomDrawSubItem(Sender: TCustomListView;
      Item: TListItem; SubItem: Integer; State: TCustomDrawState;
      var DefaultDraw: Boolean);
    procedure btnAddFilesClick(Sender: TObject);
    procedure tmrExecutingTimer(Sender: TObject);
    procedure pmUploadFilePopup(Sender: TObject);
    procedure mniDeleteUploadFileClick(Sender: TObject);
    procedure lvUploadFilesDeletion(Sender: TObject; Item: TListItem);
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
  TUploadThread = class(TThread)
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
    FUploadSocket: TUploadSocket;
    {* �ļ����� *}
    FFileStream: TFileStream;
    {* ���ʹ�С�ͷ���ʱ�� *}
    FSendSize: Int64;
    FSendTimeMS: Cardinal;
    FBeginDT: TDateTime;
    {* ִ�в�����ʾ *}
    FInformation: string;
    {* ״̬ *}
    FSendStatus: TSendStatus;
    {* ÿ��Ӧ�ò㷢�Ͱ���С *}
    FPacketSize: Cardinal;
    {* �����ļ����� *}
    function SendFile: Boolean;
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

procedure TFamUpload.InitDirectory;
var
  slDirectory: TStringList;
  RootNode, ChildNode: TTreeNode;
  i: Integer;
begin
  DoubleBuffered := True;
  lvUploadFiles.DoubleBuffered := True;
  if tvDirecotory.Items.Count > 0 then
    Exit;
  RootNode := tvDirecotory.Items.Add(nil, 'Root');
  RootNode.ImageIndex := 0;
  RootNode.SelectedIndex := 0;
  slDirectory := TStringList.Create;
  try
    if GDataMgrCtr.UploadSocket.Dir('', slDirectory) then
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
      ErrorBox(GDataMgrCtr.UploadSocket.LastError);
    end;
  finally
    slDirectory.Free;
  end;
end;

procedure TFamUpload.tvDirecotoryExpanding(Sender: TObject;
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
        if GDataMgrCtr.UploadSocket.Dir(sParentDir, slDirectory) then
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
          ErrorBox(GDataMgrCtr.UploadSocket.LastError);
        end;
      finally
        Screen.Cursor := crDefault;
      end;
    finally
      slDirectory.Free;
    end;
  end;
end;

function TFamUpload.GetDirectory(ANode: TTreeNode): string;
begin
  Result := '';
  while Assigned(ANode) and (ANode.Level <> 0) do
  begin
    Result := ANode.Text + PathDelim + Result;
    ANode := ANode.Parent;
  end;
end;

procedure TFamUpload.tvDirecotoryChanging(Sender: TObject; Node: TTreeNode;
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
    if GDataMgrCtr.UploadSocket.FileList(sDirName, slFiles) then
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
      ErrorBox(GDataMgrCtr.UploadSocket.LastError);
  finally
    slFiles.Free;
    Screen.Cursor := crDefault;
  end;
end;

procedure TFamUpload.pmDirectoryPopup(Sender: TObject);
begin
  inherited;
  mniNew.Enabled := tvDirecotory.Selected <> nil;
  mniDelete.Enabled := (tvDirecotory.Selected <> nil) and (tvDirecotory.Selected.Level > 0);
end;

procedure TFamUpload.mniNewClick(Sender: TObject);
var
  sParentDir, sDirName: string;
  ChildNode: TTreeNode;
begin
  inherited;
  if tvDirecotory.Selected = nil then Exit;
  if InputBoxEx('Input Directory Name', '', '', sDirName) then
  begin
    Screen.Cursor := crHourGlass;
    try
      sParentDir := GetDirectory(tvDirecotory.Selected);
      if GDataMgrCtr.UploadSocket.CreateDir(sParentDir, sDirName) then
      begin
        if tvDirecotory.Selected.HasChildren and (tvDirecotory.Selected.Count = 0) then
          tvDirecotory.Selected.Expand(False)
        else
        begin
          ChildNode := tvDirecotory.Items.AddChild(tvDirecotory.Selected, sDirName);
          ChildNode.ImageIndex := 0;
          ChildNode.SelectedIndex := 0;
        end;
      end
      else
        ErrorBox(GDataMgrCtr.UploadSocket.LastError);
    finally
      Screen.Cursor := crDefault;
    end;
  end;
end;

procedure TFamUpload.mniDeleteClick(Sender: TObject);
var
  sParentDir, sDirName: string;
begin
  inherited;
  if (tvDirecotory.Selected = nil) or (tvDirecotory.Selected.Level = 0) then Exit;
  if AskBox(Format('Whether to delete the file folder [%s]', [tvDirecotory.Selected.Text])) then
  begin
    Screen.Cursor := crHourGlass;
    try
      sParentDir := GetDirectory(tvDirecotory.Selected.Parent);
      sDirName := tvDirecotory.Selected.Text;
      if GDataMgrCtr.UploadSocket.DeleteDir(sParentDir, sDirName) then
      begin
        tvDirecotory.Selected.Delete;
      end
      else
        ErrorBox(GDataMgrCtr.UploadSocket.LastError);
    finally
      Screen.Cursor := crDefault;
    end;
  end;
end;

procedure TFamUpload.pmFilesPopup(Sender: TObject);
begin
  inherited;
  mniDeleteFile.Enabled := lvFiles.SelCount > 0;
end;

procedure TFamUpload.mniDeleteFileClick(Sender: TObject);
var
  i: Integer;
  sDirName: string;
  slFiles: TStringList;
begin
  inherited;
  if lvFiles.SelCount = 0 then Exit;
  Screen.Cursor := crHourGlass;
  slFiles := TStringList.Create;
  try
    sDirName := GetDirectory(tvDirecotory.Selected);
    for i := 0 to lvFiles.Items.Count - 1 do
    begin
      if lvFiles.Items[i].Selected then
        slFiles.Add(lvFiles.Items[i].Caption);
    end;
    if GDataMgrCtr.UploadSocket.DeleteFile(sDirName, slFiles) then
      lvFiles.DeleteSelected
    else
      ErrorBox(GDataMgrCtr.UploadSocket.LastError);
  finally
    slFiles.Free;
    Screen.Cursor := crDefault;
  end;
end;

procedure TFamUpload.lvUploadFilesCustomDrawSubItem(
  Sender: TCustomListView; Item: TListItem; SubItem: Integer;
  State: TCustomDrawState; var DefaultDraw: Boolean);
var
  BoundRect, Rect: TRect;
begin
  BoundRect := Item.DisplayRect(drBounds);
  //��ȡSubItem��Rect
  ListView_GetSubItemRect(lvUploadFiles.Handle, Item.Index, SubItem, LVIR_LABEL, @Rect);
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
        lvUploadFiles.Canvas.Brush.Color := clHighlight;
        lvUploadFiles.Canvas.Font.Color := clWhite;
      end;
      lvUploadFiles.Canvas.FillRect(Rect); //��ʼ��ѡ�б���
    end;
    DrawText(lvUploadFiles.Canvas.Handle, PChar(Item.SubItems[SubItem-1]),
      Length(Item.SubItems[SubItem-1]), Rect,
      DT_VCENTER or DT_SINGLELINE or DT_END_ELLIPSIS or DT_LEFT);
  end;

  lvUploadFiles.Canvas.Brush.Color := clWhite;

  if Item.Selected then //��ѡ�������
  begin
    if cdsFocused in State then//�ؼ��Ƿ��ڼ���״̬
    begin
      lvUploadFiles.Canvas.Brush.Color := $00DAA07A; // $00E2B598; //clHighlight;
    end
    else
      lvUploadFiles.Canvas.Brush.Color := $00E2B598; //$00DAA07A // clHighlight;
    lvUploadFiles.Canvas.DrawFocusRect(BoundRect);
  end;

  DefaultDraw := False; //����ϵͳ����
end;

procedure TFamUpload.btnAddFilesClick(Sender: TObject);
var
  i: Integer;
  ListItem: TListItem;
  UploadFile: PUploadFile;
begin
  inherited;
  if dlgOpen.Execute() then
  begin
    lvUploadFiles.Items.BeginUpdate;
    try
      for i := 0 to dlgOpen.Files.Count - 1 do
      begin
        ListItem := lvUploadFiles.Items.Add;
        ListItem.Caption := ExtractFileName(dlgOpen.Files[i]);
        ListItem.SubItems.Add(GetFileSizeDisplay(FileSizeEx(dlgOpen.Files[i])));
        ListItem.SubItems.Add('0');
        ListItem.SubItems.Add('');
        ListItem.ImageIndex := 0;
        New(UploadFile);
        UploadFile.DirName := GetDirectory(tvDirecotory.Selected);
        UploadFile.FileName := dlgOpen.Files[i];
        UploadFile.SendStatus := ssWait;
        UploadFile.UploadThread := nil;
        UploadFile.Speed := 0;
        ListItem.Data := UploadFile;
      end;
    finally
      lvUploadFiles.Items.EndUpdate;
    end;
  end;
end;

procedure TFamUpload.tmrExecutingTimer(Sender: TObject);
var
  i, iCount, iUploadThreadCount: Integer;
  UploadFile: PUploadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    if lvUploadFiles.Items.Count > 0 then
    begin
      iCount := 0;
      for i := 0 to lvUploadFiles.Items.Count - 1 do //��ȡ����ִ�е��������
      begin
        UploadFile := lvUploadFiles.Items[i].Data;
        if UploadFile.SendStatus in [ssSending] then
          Inc(iCount);
      end;
      iUploadThreadCount := StrToInt(cbbUploadThreadCount.Text);
      if iCount < iUploadThreadCount then //�������ִ�е�����
      begin
        for i := 0 to lvUploadFiles.Items.Count - 1 do
        begin
          if iCount >= iUploadThreadCount then Break;
          if lvUploadFiles.Items[i].ImageIndex = 0 then //�ȴ�״̬
          begin
            UploadFile := lvUploadFiles.Items[i].Data;
            if UploadFile.SendStatus <> ssWait then
              Continue;
            UploadFile.SendStatus := ssSending;
            UploadFile.UploadThread := TUploadThread.Create(True);
            UploadFile.UploadThread.DirName := UploadFile.DirName;
            UploadFile.UploadThread.FileName := UploadFile.FileName;
            UploadFile.UploadThread.ListItem := lvUploadFiles.Items[i];
            UploadFile.UploadThread.PacketSize := StrToInt(cbbPacketSize.Text) * 1024;
            UploadFile.UploadThread.Resume;
            UploadFile.Speed := 0;
            Inc(iCount);
          end;
        end;
      end;
      for i := 0 to lvUploadFiles.Items.Count - 1 do //�ͷ��Ѿ���ɵ������߳�
      begin
        UploadFile := lvUploadFiles.Items[i].Data;
        if UploadFile.SendStatus in [ssCompleted, ssError] then
        begin
          FreeAndNil(UploadFile.UploadThread);
        end;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamUpload.pmUploadFilePopup(Sender: TObject);
begin
  inherited;
  mniDeleteUploadFile.Enabled := lvUploadFiles.SelCount > 0;
end;

procedure TFamUpload.mniDeleteUploadFileClick(Sender: TObject);
var
  i: Integer;
  UploadFile: PUploadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    for i := lvUploadFiles.Items.Count - 1 downto 0 do
    begin
      if lvUploadFiles.Items[i].Selected then
      begin
        UploadFile := lvUploadFiles.Items[i].Data;
        case UploadFile.SendStatus of //������ڷ����У���ȡ��
          ssSending, ssReSend: UploadFile.UploadThread.Cancel;
        end;
        FreeAndNilEx(UploadFile.UploadThread);
        lvUploadFiles.Items[i].Delete;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamUpload.lvUploadFilesDeletion(Sender: TObject;
  Item: TListItem);
begin
  inherited;
  if Item.Data <> nil then
  begin
    Dispose(PUploadFile(Item.Data));
  end;
end;

procedure TFamUpload.mniCancelClick(Sender: TObject);
var
  i: Integer;
  UploadFile: PUploadFile;
begin
  inherited;
  tmrExecuting.Enabled := False;
  try
    for i := lvUploadFiles.Items.Count - 1 downto 0 do
    begin
      if lvUploadFiles.Items[i].Selected then
      begin
        UploadFile := lvUploadFiles.Items[i].Data;
        case UploadFile.SendStatus of //������ڷ����У���ȡ��
          ssSending, ssReSend: UploadFile.UploadThread.Cancel;
        end;
        FreeAndNilEx(UploadFile.UploadThread);
        UploadFile.SendStatus := ssCancel;
        lvUploadFiles.Items[i].ImageIndex := 3;
      end;
    end;
  finally
    tmrExecuting.Enabled := True;
  end;
end;

procedure TFamUpload.mniRefreshClick(Sender: TObject);
var
  AllowChange: Boolean;
begin
  inherited;
  tvDirecotoryChanging(tvDirecotory, tvDirecotory.Selected, AllowChange);
end;

{ TSendThread }

constructor TUploadThread.Create(CreateSuspended: Boolean);
begin
  inherited Create(CreateSuspended);
  FAttemptCount := 3; //����3��
  FAttemptIntervalMS := 15 * 1000; //���Լ��15S
  FSendStatus := ssSending; //״̬��Ϊ������
  FPacketSize := 8 * 1024;
end;

procedure TUploadThread.Execute;
var
  iAttemptCount, i: Integer;
begin
  inherited;
  iAttemptCount := 1;
  while (not Terminated) and (iAttemptCount <= FAttemptCount) do
  begin
    try
      FSendStatus := ssSending;
      Synchronize(UpdateStatus); //����Ϊ����״̬
       if SendFile then
         FSendStatus := ssCompleted
       else
         FSendStatus := ssError;
      if (FSendStatus = ssError) and (not Terminated) then //״̬Ϊʧ�ܣ�����û��ȡ����������
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
        FSendStatus := ssReSend; //����Ϊ��ͣ״̬
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
        FSendStatus := ssError; //���ʹ���״̬
        Synchronize(UpdateStatus);
        FInformation := 'Unknow Exception: ' + E.Message;
        Synchronize(UpdateInformation);
      end;
    end;
  end;
end;

function TUploadThread.SendFile: Boolean;
var
  iFilePosition: Int64;
  Buff: array of Byte;
  iCount: Integer;
  LastSendMS: Cardinal;
begin
  FUploadSocket := TUploadSocket.Create;
  try
    //����
    Result := FUploadSocket.Connect(GDataMgrCtr.UploadSocket.Host, GDataMgrCtr.UploadSocket.Port)
      and FUploadSocket.Login(GDataMgrCtr.User, GDataMgrCtr.Password);
    if not Result then //ʧ�ܣ����Ҳ��ǿͻ�ȡ������´�����Ϣ
    begin
      if not Terminated then
        Synchronize(UpdateErrorInformation);
      Exit;
    end;
    //�ϴ���ʼ
    Result := FUploadSocket.Upload(DirName, ExtractFileName(FileName), iFilePosition);
    if not Result then
    begin
      if not Terminated then
        Synchronize(UpdateErrorInformation);
      Exit;
    end;
    FFileStream := TFileStream.Create(FileName, fmOpenRead or fmShareDenyWrite);
    try
      FFileStream.Position := iFilePosition; //�Ƶ��ϴ��ϴ�λ��
      if FBeginDT = 0 then
        FBeginDT := Now;
      LastSendMS := 0;
      SetLength(Buff, FPacketSize);
      while (not Terminated) and (FFileStream.Position < FFileStream.Size) do //��������
      begin
        iCount := FFileStream.Read(Buff[0], FPacketSize);
        Result := FUploadSocket.Data(Buff[0], iCount);
        if not Result then
        begin
          if not Terminated then
            Synchronize(UpdateErrorInformation);
          Exit;
        end;
        FSendSize := FSendSize + iCount;
        FSendTimeMS := Max(MilliSecondsBetween(Now, FBeginDT), 0);
        if LastSendMS = 0 then
          LastSendMS := FSendTimeMS;
        if LastSendMS - FSendTimeMS >= 1000 then //1S����һ��
        begin
          //Synchronize(UpdateProgress);
          LastSendMS := FSendTimeMS;
        end;
      end;
      Synchronize(UpdateProgress);
      Result := FUploadSocket.Eof(FFileStream.Size); //����EOF
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
    FUploadSocket.Free;
  end;
end;

procedure TUploadThread.UpdateErrorInformation;
begin
  if Assigned(FUploadSocket) then
    FListItem.SubItems[2] := FUploadSocket.LastError;
end;

procedure TUploadThread.UpdateProgress;
var
  UploadFile: PUploadFile;
begin
  if FFileStream.Size > 0 then
    FListItem.SubItems[1] := IntToStr(Round(FFileStream.Position / FFileStream.Size * 100));
  if FSendTimeMS > 0 then
  begin
    FListItem.SubItems[2] := GetSpeedDisplay(FSendSize / FSendTimeMS * 1000);
    UploadFile := FListItem.Data;
    UploadFile.Speed := FSendSize / FSendTimeMS * 1000;
  end;
end;

procedure TUploadThread.UpdateInformation;
begin
  FListItem.SubItems[2] := FInformation;
end;

procedure TUploadThread.UpdateStatus;
begin
  case FSendStatus of
    ssWait: FListItem.ImageIndex := 0;
    ssSending: FListItem.ImageIndex := 1;
    ssCompleted: FListItem.ImageIndex := 2;
    ssReSend: FListItem.ImageIndex := 3;
    ssError: FListItem.ImageIndex := 4;
  end;
  PUploadFile(FListItem.Data).SendStatus := FSendStatus;
end;

procedure TUploadThread.Cancel;
begin
  Terminate; //ֹͣ
  FUploadSocket.Disconnect;
  WaitFor;
  FSendStatus := ssCancel; //״̬��Ϊȡ��
end;

procedure TFamUpload.mniSumSpeedClick(Sender: TObject);
var
  SumSpeed: Double;
  i: Integer;
begin
  inherited;
  SumSpeed := 0;
  for i := 0 to lvUploadFiles.Items.Count - 1 do
  begin
    SumSpeed := SumSpeed + PUploadFile(lvUploadFiles.Items[i].Data).Speed;
  end;
  InfoBox(GetSpeedDisplay(SumSpeed));
end;

end.
