using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

[StructLayout(LayoutKind.Sequential)]
class PER_IO_DATA
{
    public string Data;
}

public class IOCPApiTest
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern SafeFileHandle CreateIoCompletionPort(IntPtr FileHandle, IntPtr ExistingCompletionPort, IntPtr CompletionKey, uint NumberOfConcurrentThreads);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetQueuedCompletionStatus(SafeFileHandle CompletionPort,
        out uint lpNumberOfBytesTransferred, out IntPtr lpCompletionKey,
        out IntPtr lpOverlapped, uint dwMilliseconds);
    [DllImport("Kernel32", CharSet = CharSet.Auto)]
    private static extern bool PostQueuedCompletionStatus(SafeFileHandle CompletionPort, uint dwNumberOfBytesTransferred, IntPtr dwCompletionKey, IntPtr lpOverlapped);

    public static unsafe void TestIOCPApi()
    {
        var CompletionPort = CreateIoCompletionPort(new IntPtr(-1), IntPtr.Zero, IntPtr.Zero, 1);
        if (CompletionPort.IsInvalid)
        {
            Console.WriteLine("CreateIoCompletionPort 出错:{0}", Marshal.GetLastWin32Error());
        }
        var thread = new Thread(ThreadProc);
        thread.Start(CompletionPort);

        var PerIOData = new PER_IO_DATA();
        var gch = GCHandle.Alloc(PerIOData);
        PerIOData.Data = "hi,我是蛙蛙王子，你是谁？";
        Console.WriteLine("{0}-主线程发送数据", Thread.CurrentThread.GetHashCode());
        PostQueuedCompletionStatus(CompletionPort, (uint)sizeof(IntPtr), IntPtr.Zero, (IntPtr)gch);

        var PerIOData2 = new PER_IO_DATA();
        var gch2 = GCHandle.Alloc(PerIOData2);
        PerIOData2.Data = "关闭工作线程吧";
        Console.WriteLine("{0}-主线程发送数据", Thread.CurrentThread.GetHashCode());
        PostQueuedCompletionStatus(CompletionPort, 4, IntPtr.Zero, (IntPtr)gch2);
        Console.WriteLine("主线程执行完毕");
        Console.ReadKey();
    }
    static void ThreadProc(object CompletionPortID)
    {
        var CompletionPort = (SafeFileHandle)CompletionPortID;

        while (true)
        {
            uint BytesTransferred;
            IntPtr PerHandleData;
            IntPtr lpOverlapped;
            Console.WriteLine("{0}-工作线程准备接受数据", Thread.CurrentThread.GetHashCode());
            GetQueuedCompletionStatus(CompletionPort, out BytesTransferred,
                                      out PerHandleData, out lpOverlapped, 0xffffffff);
            if (BytesTransferred <= 0)
                continue;
            GCHandle gch = GCHandle.FromIntPtr(lpOverlapped);
            var per_HANDLE_DATA = (PER_IO_DATA)gch.Target;
            Console.WriteLine("{0}-工作线程收到数据：{1}", Thread.CurrentThread.GetHashCode(), per_HANDLE_DATA.Data);
            gch.Free();
            if (per_HANDLE_DATA.Data != "关闭工作线程吧") continue;
            Console.WriteLine("收到退出指令，正在退出");
            CompletionPort.Dispose();
            break;
        }
    }

    public static int Main(String[] args)
    {
        TestIOCPApi();
        return 0;
    }
}