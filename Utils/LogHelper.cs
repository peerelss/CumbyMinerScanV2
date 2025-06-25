using System.Collections.Generic;

namespace CumbyMinerScanV2.Utils;

public class LogHelper
{
    public static string ErrorFanLost = "ERROR_FAN_LOST";
    public static string ErrorTempTooHigh = "ERROR_TEMP_TOO_HIGH";
    public static string ErrorPowerLost = "ERROR_POWER_LOST";
    public static string ErrorNotEnoughChain = "Not enough chain";
    public static string ErrorOffline = "Offline";
    public static string ErrorUnknown = "Unknown";
    public static string ErrorNetIssue = "NetIssue";
    public static string NormalOk = "Ok";

    private static List<string> ErrorList =
    [
        ErrorFanLost,
        ErrorTempTooHigh,
        ErrorPowerLost, ErrorNotEnoughChain
    ];

    public static List<string> ParseLog(string log)
    {
        string[] logList = log.Split('\n');
        foreach (string logLine in logList)
        {
            foreach (string error in ErrorList)
            {
                if (logLine.Contains(error))
                {
                    return [error, logLine];
                }
            }
        }

        return ["unknown error", ""];
    }
}