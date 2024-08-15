using System;
using System.Management;
using System.ServiceProcess;
using System.Threading;

namespace SharpSMBTakeover
{
    internal class ServiceActions
    {

        static string lanmanService = "LanmanServer";
        static string srv2Service = "srv2";
        static string srvnetService = "srvnet";
      
        enum WMIErrorConst: uint
        {
            Success = 0,
            NotSupported= 1,
            AccessDenied = 2,
            DependentServicesRunning=3,
            InvalidServiceControl=4
        }

        ServiceController lanManSC;
        ServiceController srv2SC;
        ServiceController srvnetSC;
        bool reverseServiceOrder;
        static string StartModeToString(ServiceStartMode serviceStartMode)
        {
            switch (serviceStartMode)
            {
                case ServiceStartMode.Boot:
                    return "Boot";
                case ServiceStartMode.System:
                    return "System";
                case ServiceStartMode.Automatic:
                    return "Automatic";
                case ServiceStartMode.Disabled:
                    return "Disabled";
                case ServiceStartMode.Manual:
                    return "Manual";
                default:
                    return "";
            }
        }
        public ServiceActions(bool reverseServiceOrder)
        {
            this.reverseServiceOrder = reverseServiceOrder;
            lanManSC = new ServiceController(lanmanService);
            srv2SC = new ServiceController(srv2Service);
            srvnetSC = new ServiceController(srvnetService);
        }
        bool SetServiceStartUp(string serviceName, ServiceStartMode serviceStartMode)
        {

            string newServiceStartMode = StartModeToString(serviceStartMode);
            string path = "Win32_Service.Name='" + serviceName + "'";
            ManagementPath mPath = new ManagementPath(path);
            ManagementObject manObj = new ManagementObject(mPath);
            object[] parameters = new object[1];
            parameters[0] = newServiceStartMode;
            WMIErrorConst result = (WMIErrorConst) manObj.InvokeMethod("ChangeStartMode", parameters);
            if (result != WMIErrorConst.Success){
                if (result == WMIErrorConst.AccessDenied){
                    Console.WriteLine("[---] Error: Failed to change start mode: Access Denied");
                }else {
                    Console.WriteLine($"[---] Error: Failed to change start mode: WMI Error Number {result}");
                
                }
                return false;
            }
            else
            {
                Console.WriteLine($"[*] Successfully changed {serviceName} start mode to Disabled!");
            }
            return result == WMIErrorConst.Success;
        }
        public bool CheckIfAllServicesStopped()
        {
            return (lanManSC.Status != ServiceControllerStatus.Running) & (srv2SC.Status != ServiceControllerStatus.Running) & (srvnetSC.Status != ServiceControllerStatus.Running);
        }
        public bool CheckIfServicesRunning()
        {
            bool foundNonRunning = false;
            if (lanManSC.Status != ServiceControllerStatus.Running)
            {
                foundNonRunning = true;
            }
            if (srv2SC.Status != ServiceControllerStatus.Running)
            {
                foundNonRunning = true;
            }
            if (srvnetSC.Status != ServiceControllerStatus.Running)
            {
                foundNonRunning = true;
            }
            return !foundNonRunning;
        }
        public bool EnableServices()
        {
            ServiceController lanManSC = new ServiceController(lanmanService);
            Console.WriteLine($"[*] LanmanServer service start mode in state: {lanManSC.StartType}");
            if (lanManSC.StartType != ServiceStartMode.Automatic)
            {
                Console.WriteLine($"\t - Setting LanmanServer start type to Automatic");

                SetServiceStartUp(lanmanService, ServiceStartMode.Automatic);
            }
            Console.WriteLine($"[*] LanmanServer service status is:  {lanManSC.Status}");

            if (lanManSC.Status != ServiceControllerStatus.Running)
            {
                Console.WriteLine($"\t - Starting LanmanServer service");
                try
                {
                    lanManSC.Start();
                   
                    if (lanManSC.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("[*] Successfully started LanmanServer Service!");
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine("[---] Failed to start LanmanServer service!");
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
        
            ServiceController srv2SC = new ServiceController(srv2Service);
            Console.WriteLine($"[*] srv2 service status is:  {srv2SC.Status}");
            if (srv2SC.Status != ServiceControllerStatus.Running) {
                Console.WriteLine($"\t - Starting srv2 service");
                try
                {
                    srv2SC.Start();
                    if (srv2SC.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("[*] Successfully started srv2 Service!");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("[---] Failed to start srv2 service!");
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            ServiceController srvnetSC = new ServiceController(srvnetService);
            Console.WriteLine($"[*] srvnet service status is:  {srvnetSC.Status}");
            if (srvnetSC.Status != ServiceControllerStatus.Running)
            {
                Console.WriteLine($"\t - Starting srvnet service");
                try
                {
                    srvnetSC.Start();
                    if (srv2SC.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("[*] Successfully started srvnet Service!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[---] Failed to start srvnet service!");
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            
            return CheckIfServicesRunning();
        }
       public bool DisableService(string serviceName, ServiceController sc){
            bool neededToStop = sc.Status == ServiceControllerStatus.Running;
            Console.WriteLine($"[*] {serviceName} service running state: {sc.Status}");
            if (sc.Status == ServiceControllerStatus.Running){
                Console.WriteLine($"\t - Stopping {serviceName} service");
                try{
                    sc.Stop();
                }catch (Exception ex){
                    Console.WriteLine($"[---] Failed to stop {serviceName} service!");
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            bool stopped = (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending);
            if (stopped & neededToStop){
                Console.WriteLine($"[*] Successfully stopped the {serviceName} service!");
            }
            return stopped;
        }
        public bool DisableServices()
        {
            if (lanManSC.StartType != ServiceStartMode.Disabled)
            {
                Console.WriteLine($"\t - Setting LanmanServer start type to Disabled");
                SetServiceStartUp(lanmanService, ServiceStartMode.Disabled);
            }
            for (int i = 0; i < 3; i++)
            {
                if (reverseServiceOrder)
                {
                    DisableService("srvnet", srvnetSC);
                    DisableService("LanmanServer", lanManSC);
                    DisableService("srv2", srv2SC);
                }
                else
                {
                    DisableService("LanmanServer", lanManSC);
                    DisableService("srv2", srvnetSC);
                    DisableService("srvnet", srv2SC);
                }
              
                if (CheckIfAllServicesStopped()){
                    return true;
                }
            }
            return CheckIfAllServicesStopped();
        }
    }
}
