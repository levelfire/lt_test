unit ThroughputSocket;

interface

uses
  IdTCPClient, BasisFunction, Windows, SysUtils, Classes, SyncObjs, DefineUnit,
  Types, BaseClientSocket, IdIOHandlerSocket, Messages;

type
  TThroughputScoket = class(TBaseClientSocket)
  private
    {* ���ݻ����� *}
    FBuffer: array of Byte;
    {* ���ݻ�������С *}
    FBufferSize: Integer;
    {* ѭ���������� *}
    FCount: Integer;
  public
    constructor Create; override;
    destructor Destroy; override;
    {* ���ӷ����� *}
    function Connect(const ASvr: string; APort: Integer): Boolean;
    {* �Ͽ����� *}
    procedure Disconnect;
    {* ѭ��������� *}
    function CyclePacket: Boolean;

    property BufferSize: Integer read FBufferSize write FBufferSize;
    property Count: Integer read FCount;
  end;

implementation

{ TThroughputScoket }

constructor TThroughputScoket.Create;
begin
  inherited;
  FBufferSize := 1024;
end;

destructor TThroughputScoket.Destroy;
begin
  inherited;
end;

function TThroughputScoket.Connect(const ASvr: string;
  APort: Integer): Boolean;
begin
  FHost := ASvr;
  FPort := APort;
  Result := inherited Connect(Char(sfThroughput));
end;

procedure TThroughputScoket.Disconnect;
begin
  inherited Disconnect;
end;

function TThroughputScoket.CyclePacket: Boolean;
var
  i: Integer;
  slRequest, slResponse: TStringList;
begin
  if FBufferSize > Length(FBuffer) then
  begin
    SetLength(FBuffer, FBufferSize);
    for i := Low(FBuffer) to High(FBuffer) do
    begin
      FBuffer[i] := 1;
    end;
  end;
  slRequest := TStringList.Create;
  slResponse := TStringList.Create;
  try
    FCount := FCount + 1;
    slRequest.Add(Format(CSFmtInt, [CSCount, FCount]));
    Result := ControlCommand(CSThroughputCommand[tcCyclePacket], slRequest, slResponse, FBuffer);
    if Result then
      FCount := StrToInt(slResponse.Values[CSCount]);
  finally
    slResponse.Free;
    slRequest.Free;
  end;
end;

end.
