namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareConstants
    {
        public enum SessionResultCodes
        {   
            Closed = 0,  
            Opening = 1,
            OpenedAndPending = 2,
            Opened = 3,
            OpenedAndDownloadComplete = 4,
            Closing = 5,
            Aborting = 6,
            AbortingAndWaitingWriteEvent = 7,
        }

        public enum ControlIds
        {
            SendMtu = 0,
            ReadyToDownload = 1,
            Unknown
        }

        public enum UpdateIds
        {
            Percent = 0,
            StateChange = 1,
            Unknown
        }
    }
}