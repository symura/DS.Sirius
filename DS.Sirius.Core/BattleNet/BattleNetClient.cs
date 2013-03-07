using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace DS.Sirius.Core.BattleNet
{
    public class BattleNetClient
    {
        private static BattleNetClient _instance;


        public static BattleNetClient Current
        {
            get { return _instance ?? (_instance = new BattleNetClient()); }
        }

        public BattleNetClient()
        {

        }

        private const string BattleNetBaseUri = "http://eu.battle.net/api/d3/";
        private const string BattleNet_Api_Profile = "profile/{0}/";
        private const string BattleNet_Api_Hero = "profile/{0}/hero/{1}";

        #region Async methods

        //public void GetCareerAsync(string profileName, Action<Career> action)
        //{
        //    profileName = profileName.Replace("#", "-");
        //    try
        //    {
        //        var apiMethod = new Uri(new Uri(BattleNetBaseUri), String.Format(BattleNet_Api_Profile, profileName)).ToString();

        //        CallApiMethodAsync(apiMethod, jsonResult =>
        //            {
        //                try
        //                {
        //                    var career = JsonConvert.DeserializeObject<Career>(jsonResult);


        //                    action(career);
        //                }
        //                catch (Exception ex)
        //                {
        //                    action(null);
        //                }
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        action(null);
        //    }

        //}

        //public async Task<Career> GetCareerAsync(string profileName)
        //{
        //    try
        //    {
        //        profileName = profileName.Replace("#", "-");
        //        var apiMethod = new Uri(new Uri(BattleNetBaseUri), String.Format(BattleNet_Api_Profile, profileName)).ToString();

        //        var jsonResult = CallApiMethod(apiMethod);

        //        return (await JsonConvert.DeserializeObjectAsync<Career>(jsonResult));                                   
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }
            
        //}

        //public async Task<Hero> GetHeroAsync(string profileName, int heroID)
        //{
        //    try
        //    {
        //        profileName = profileName.Replace("#", "-");
        //        var apiMethod = new Uri(new Uri(BattleNetBaseUri), String.Format(BattleNet_Api_Hero, profileName, heroID.ToString())).ToString();

        //        var jsonResult = CallApiMethod(apiMethod);

        //        return (await JsonConvert.DeserializeObjectAsync<Hero>(jsonResult));
        //    }
        //    catch (Exception)
        //    {

        //        return null;
        //    }

        //}




        #endregion

        internal void CallApiMethodAsync(string apiMethod, Action<string> action)
        {
            try
            {

                var request = (HttpWebRequest)WebRequest.Create(apiMethod);
                //request.Headers["user-agent"] = "D3TestClient";
                request.AllowAutoRedirect = true;

               

                request.BeginGetResponse(delegate(IAsyncResult result)
                {
                    try
                    {
                        WebResponse response;

                        // in some cases the Battle.Net community api may return an exception (404/500 error)
                        // we just ignore it and try to read the content of the response
                        try
                        {
                            response = request.EndGetResponse(result);
                        }
                        catch (WebException ex)
                        {
                            response = ex.Response;
                        }

                        using (var stream = response.GetResponseStream())
                        {
                            var streamReader = new StreamReader(stream);
                            var jsonResultString = streamReader.ReadToEnd();

                            //var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResultString);
                            //if (apiResponse == null)
                            //{
                            //    jsonResultString = String.Empty;
                            //}
                            //else
                            //{
                            //    if (!apiResponse.IsValid)
                            //    {
                            //        var reasonCaption = AppResources.ResourceManager.GetString(String.Format("UI_ApiResponseError_{0}_Caption", apiResponse.ReasonType)) ?? AppResources.UI_Common_Error_NoData_Caption;
                            //        var reasonText = AppResources.ResourceManager.GetString(String.Format("UI_ApiResponseError_{0}_Text", apiResponse.ReasonType)) ?? AppResources.UI_Common_Error_NoData_Text;
                            //        jsonResultString = String.Empty;
                            //    }
                            //}

                            action(jsonResultString);

                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }, null);
            }
            catch (Exception ex)
            {
                action(null);
            }
        }

        internal string CallApiMethod(string apiMethod)
        {
            string jsonResultString = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(apiMethod);
                //request.Headers["user-agent"] = "D3TestClient";
            request.AllowAutoRedirect = true;

             WebResponse response = request.GetResponse();

             using (var stream = response.GetResponseStream())
             {
                 var streamReader = new StreamReader(stream);
                jsonResultString = streamReader.ReadToEnd();

             }

             return jsonResultString;
        }


    }
}
