using System;

namespace SharpSMBTakeover
{
    public class Program
    {

        enum Actions
        {
            None,
            Enable,
            Disable,
            Check
         
        }
        public static void Main(string[] args)
        {
            Console.WriteLine("\t SharpSMBTakeover");
            Console.WriteLine("\t @ret2desync (Original work by @zyn3rgy)");
            Console.WriteLine("\t C# port of the smbtakeover (https://github.com/zyn3rgy/smbtakeover) tool to disable/enable/check the services bound to TCP port 445 (SMB) on the current machine");
           
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }
            bool reverseServiceOrder = false;
            Actions serviceAction = Actions.None;
            for (int i = 0; i < args.Length;i++){
                switch (args[i]){
                    case "/enable":
                        serviceAction = Actions.Enable;
                        break;
                    case "/disable":
                        serviceAction = Actions.Disable;
                        break;
                    case "/reverse":
                        reverseServiceOrder = true;
                        break;
                    case "/check":
                        serviceAction = Actions.Check;
                        break;
                    default:
                        Console.WriteLine("Unknown Argument " + args[i]);
                        PrintHelp();
                        return;
                }
            }
            if (serviceAction == Actions.None){
                Console.WriteLine("Missing action, choices are /enable, /disable [/reverse], /check");
                PrintHelp();
                return;
            }
            ServiceActions sa = new ServiceActions(reverseServiceOrder);
            switch (serviceAction){
                case Actions.Enable:  
                    Console.WriteLine("[*] Going to enable all SMB services");
                    if (sa.EnableServices()){
                            Console.WriteLine("[**] All services started, successfully rebound to port 445.");
                    }
                    break;
                case Actions.Disable:
                    if (reverseServiceOrder){
                        Console.WriteLine("[*] Going to stop all services in the following order: srvnet, LanmanServer, srv2");
                    }else{
                        Console.WriteLine("[*] Going to stop all services in the following order: LanmanServer, srv2, srvnet");
                    }
                    if (sa.DisableServices()){
                        Console.WriteLine("[**] All services stopped, nothing bound to port 445.");
                    }
                    else
                    {
                        Console.WriteLine("[--] Failed to stop all services! Try re-run the tool or use the /reverse flag");
                    }
                    break;
                case Actions.Check:
                    Console.WriteLine("[*] Going to check if the srv2, LanmanServer, srvnet services are running");
                    if (sa.CheckIfServicesRunning()){
                        Console.WriteLine("[**] Services running, port 445 is bound.");
                    }else{
                        Console.WriteLine("[--] Services not running, port 445 is NOT bound.");
                    }
                    break;
                default:
                    Console.WriteLine("No action to perform, exiting");
                    return;
                }
            
            
        }
        static void PrintHelp()
        {
            Console.WriteLine("\t[*] Usage:");
            Console.WriteLine("\t  SharpSMBTakeover.exe.exe /disable [/reverse]");
            Console.WriteLine("\t \t Disables the services Lanmanserver, srv2 and srvnet service to unbind port 445. ");
            Console.WriteLine("\t \t By default does this in the order: Lanmanserver, srv2, srvnet, specifying /reverse argument will disable in this order: srv2, LanmanServer, srvnet.");
            Console.WriteLine("\t  SharpSMBTakeover.exe.exe /enable");
            Console.WriteLine("\t \t Re-Enables the Lanmanserver, srv2 and srvnet service to bind to port 445.");
            Console.WriteLine("\t  SharpSMBTakeover.exe.exe /check");
            Console.WriteLine("\t \t Checks if port 445 (SMB) is bound, i.e. Lanmanserver, srv2 and srvnet services are running.");
            Console.WriteLine("\t [*****] Sometimes the tool needs to be run twice to successfully stop the services. [*****]");


        }

    }
}
