using System.Collections.Generic;

namespace CumbyMinerScanV2.Utils;

public class LogHelper
{
    public static string ErrorFanLost = "ERROR_FAN_LOST";
    private static string ErrorTempTooHigh = "ERROR_TEMP_TOO_HIGH";
    private static string ErrorPowerLost = "ERROR_POWER_LOST";
    private static string ErrorNotEnoughChain = "Not enough chain";

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