namespace servercore
{
    public enum TDisconnectType
    {
        Normal,     // disconnect normally
        Timeout,    // disconnect because of timeout
        Exception   // disconnect because of exception
    }

    public enum TSessionState
    {
        Active,    // state is active
        Inactive,  // session is inactive and will be closed
        Shutdown,  // session is shutdownling
        Closed     // session is closed
    }
}
