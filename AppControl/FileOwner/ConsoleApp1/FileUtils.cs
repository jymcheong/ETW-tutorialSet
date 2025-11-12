using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class FileUtils
    {
        public static string GetOwner(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // Optionally skip reparse points (symlinks)
                if ((fileInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    return string.Empty;

                FileSecurity fileSecurity = fileInfo.GetAccessControl();
                IdentityReference sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                return ntAccount?.Value ?? string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
