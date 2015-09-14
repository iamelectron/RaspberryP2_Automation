using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
 
namespace WindowsIoT_Raspberry_Pi
{
    class AzureDBmanager
    {

        piData item, item_default; 

       public AzureDBmanager()
        {
            item_default = new piData();
            item_default.commad = "NoCommand";
            item_default.Id = "xxxxxxxxxxxxxxxxxxxxxxxxxx"; //Replace with your DB id
            item_default.device_name = "No Device";
        }
        public async void update()
        {
            try
            { 
                await App.MobileService.GetTable<piData>().UpdateAsync(item_default);
            }
            catch (Exception ex) { }
    
        }

        public async Task<string[]>  read()
        { 
            try
            {
                List<piData> pp = await App.MobileService.GetTable<piData>().Where(x => x.Id ==
                    "45D961A0-B3E8-4C24-AEEF-7420FB9A232C").ToListAsync();  //Replace with your DB id

                string[] returndata = { pp[0].text, pp[0].commad, pp[0].device_name };
                 
                return returndata;
            }
            catch(Exception ex)
            {
                /////exception
            }
            return null;
        }

        //public async string getCommand()
        //{
        //    string command;
        //    piData new_command_reader = new piData { Id = "reader" };
        //    await MobileService.GetTable<piData>().ReadAsync();

        //}
    }

    public class piData
    {
        public string Id { get; set; }
        public string text { get; set; } 
       public string commad { get; set; } 
       public string device_name { get; set; }  
        public int device_count { get; set; }

    }
}
