using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DS.Sirius.Core.Common;
using DS.Sirius.DA.Definition;
using DS.Sirius.Core.Models;
using Newtonsoft.Json;

namespace DS.Sirius.DA.Implementation
{
    public class BattleNetRepository : IBattleNetRepository
    {
        #region Properties

        public string Region { get; set; }

       

        protected string BattleNetRegionCode
        {
            get { return Region ?? "eu"; }
        }


        protected Uri BattleNetApiBaseUri
        {
            get { return new Uri(string.Format(BattleNetConstants.BattleNetApiBaseUri, BattleNetRegionCode)); }
        }


        #endregion

        public BattleNetRepository(string region)
        {
            this.Region = region;
        }

        public void Dispose()
        {
            
        }

        public Career GetCareerByBattleTag(string battleTag)
        {
            try
            {

                var apiMethod = new Uri(BattleNetApiBaseUri, String.Format(BattleNetConstants.BattleNetApiProfile, battleTag)).ToString();

                var jsonResult = CallApiMethod(apiMethod);

                return (JsonConvert.DeserializeObject<Career>(jsonResult));
            }
            catch (Exception)
            {

                return null;
            }
            
        }

        public Hero GetHeroById(string battleTag, long id)
        {
            try
            {

                var apiMethod = new Uri(BattleNetApiBaseUri, String.Format(BattleNetConstants.BattleNetApiHero, battleTag, id.ToString())).ToString();

                var jsonResult = CallApiMethod(apiMethod);

                return (JsonConvert.DeserializeObject<Hero>(jsonResult));
            }
            catch (Exception)
            {

                return null;
            }
        }


        internal string CallApiMethod(string apiMethod)
        {
            string jsonResultString;

            var request = (HttpWebRequest)WebRequest.Create(apiMethod);
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
