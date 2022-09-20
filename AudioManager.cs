using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNAFC
{
    public class AudioManager
    {
        public static Process? AudioManagerProcess;

        public bool Playing = false;
        public string FileName = "";
        public void Play(string filename = "", bool loop = false)
        {
            if (AudioManagerProcess == null && !Playing)
            {
                Playing = true;
                AudioManagerProcess = Process.Start("AudioManager.exe", @$"Data/Audio/{FileName}{filename}.wav {(loop == true ? "true" : "false")}");
            }
        }
        public void Stop()
        {
            if (AudioManagerProcess != null && Playing)
            {
                Playing = false;
                AudioManagerProcess.Kill();
                AudioManagerProcess = null;
            }
            
        }
    }
    
}
