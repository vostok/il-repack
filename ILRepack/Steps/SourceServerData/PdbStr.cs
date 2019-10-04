using System;
using System.Diagnostics;
using System.IO;

namespace ILRepacking.Steps.SourceServerData
{
    internal class PdbStr : IDisposable
    {
        private string _pdbStrPath = Path.GetTempFileName();
        private readonly bool broken = false;

        public PdbStr()
        {
            try
            {
                using (var resourceStream = typeof(PdbStr).Assembly.GetManifestResourceStream("ILRepacking.pdbstr.exe"))
                using (var fileStream = File.Create(_pdbStrPath))
                {
                    if (resourceStream == null)
                    {
                        broken = true;
                    }
                    else
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception)
            {
                broken = true;
            }
        }

        public string Read(string pdb)
        {
            return Execute($"-r -p:{pdb} -s:srcsrv");
        }

        public void Write(string pdb, string srcsrv)
        {
            var srcsrvFile = Path.GetTempFileName();
            File.WriteAllText(srcsrvFile, srcsrv);
            Execute($"-w -p:{pdb} -s:srcsrv -i:{srcsrvFile}");
            File.Delete(srcsrvFile);
        }

        private string Execute(string arguments)
        {
            if (broken)
                return string.Empty;

            var processInfo = new ProcessStartInfo
                              {
                                  RedirectStandardOutput = true,
                                  CreateNoWindow = true,
                                  UseShellExecute = false,
                                  FileName = _pdbStrPath,
                                  Arguments = arguments
                              };
            using (var process = Process.Start(processInfo))
            using (StreamReader reader = process.StandardOutput)
            {
                return reader.ReadToEnd();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            string file = _pdbStrPath;
            _pdbStrPath = null;
            SafeDeleteFile(file);
        }

        private void SafeDeleteFile(string filePath)
        {
            if (filePath != null)
            {
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch { }
            }
        }

        ~PdbStr()
        {
            string file = _pdbStrPath;
            _pdbStrPath = null;
            SafeDeleteFile(file);
        }
    }
}
