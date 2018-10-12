using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Knacka.Se.ProtobufGenerator
{
    internal static class Logger
    {
        internal static void LogIf(Func<bool> ifFunc, Func<string> messageFunc)
        {
            if (ifFunc())
            {
                var message = messageFunc();
                Log(message);
            }
        }

        internal static void Log(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {

                    IVsOutputWindow outWindow =
                    Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                    Guid paneGuid = VSConstants.GUID_OutWindowGeneralPane;
                    IVsOutputWindowPane pane = null;
                    if (outWindow != null)
                    {
                        outWindow.GetPane(ref paneGuid, out pane);

                        if (pane == null)
                        {
                            paneGuid = VSConstants.GUID_OutWindowDebugPane;
                            outWindow.GetPane(ref paneGuid, out pane);
                        }
                    }

                    if (pane != null)
                    {
                        pane.OutputString("From ProtobufGenerator: " + message + "\n");
                        pane.Activate();
                    }
                }
                catch (Exception)
                {
                }
            }

        }
    }
}
