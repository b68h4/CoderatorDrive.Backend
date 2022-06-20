using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace Depo
{
    public class DriveApiService
    {
        public string[] Scopes = { DriveService.Scope.DriveReadonly, DriveService.Scope.Drive };

        private ClientSecrets secrets { get; set; }
    
        public DriveService service { get; set; }
       
        public DriveApiService()
        {
       
            secrets = new ClientSecrets()
            {
                ClientId = "CLIENTID",
                ClientSecret = "CLIENTSECRET"
            };
            var auth = GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, Scopes, "CoderatorDrive",
                new CancellationToken(), new FileDataStore("data", true), new PromptCodeReceiver()).Result;
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = auth,
                ApplicationName = "CoderatorDrive",
            });

        }


    }
}
