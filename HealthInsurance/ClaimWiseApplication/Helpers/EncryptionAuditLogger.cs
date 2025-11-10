using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace ClaimWise.Application.Helpers
{

    public static class EncryptionAuditLogger
    {
        private static readonly string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "encryption-audit.log");

        public static void LogDecryption(int claimId, string userId, string fileName)
        {
            var logEntry = $"[{DateTime.UtcNow}] Decryption: ClaimID={claimId}, UserID={userId}, File={fileName}";
            WriteLog(logEntry);
        }

        public static void LogEncryption(string fileName, int policyholderId)
        {
            var logEntry = $"[{DateTime.UtcNow}] Encryption: File={fileName}, PolicyholderID={policyholderId}";
            WriteLog(logEntry);
        }

        private static void WriteLog(string entry)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.AppendAllText(logPath, entry + Environment.NewLine);
        }
    }

}
