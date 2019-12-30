using EasyVideoWin.CustomControls;
using EasyVideoWin.Helpers;
using EasyVideoWin.HttpUtils;
using EasyVideoWin.Model.CloudModel.CloudRest;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EasyVideoWin.Model.CloudModel
{
    
    public class CloudApiManager
    {
        #region -- Members --

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string REST_GET_ACS_API               = "/api/rest/v2.0/acsToken";
        private const string REST_GET_PARTY_NAME_API        = "/api/rest/v2.0/partName";
        
        private string _doradoZoneAddress;
        
        private static CloudApiManager _instance = new CloudApiManager();
        private static RestClient restClient = new RestClient();
        
        public delegate void HandleException(Exception e);
        public delegate void HandleErrorMessage(RestResponse response);
        
        #endregion

        #region -- Properties --

        public static CloudApiManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public string DoradoZoneAddress
        {
            get
            {
                return _doradoZoneAddress;
            }
            set
            {
                _doradoZoneAddress = value;
            }
        }

        public string AcsServerAddress { get; set; }
        public string Token { get; set; }
        #endregion

        #region -- Constructor --

        public CloudApiManager()
        {

        }

        #endregion

        #region -- Public Methods --
        
        public AcsRest GetAcsInfoByCallNumber(string confNumericId, ulong deviceId, HandleErrorMessage hdlErrMsg)
        {
            Dictionary<object, object> uriParams = new Dictionary<object, object>();
            uriParams.Add("confNumericId", confNumericId);
            uriParams.Add("deviceId", deviceId);
            string url = GetFullDoradoUrl(REST_GET_ACS_API, true, uriParams);
            log.InfoFormat("Get acs info from {0}", url);
            return GetObject<AcsRest>(url, null, hdlErrMsg);
        }

        public RestPartNameInfo GetPartyName(string confNumericId, ulong deviceId, HandleErrorMessage hdlErrMsg)
        {
            Dictionary<object, object> uriParams = new Dictionary<object, object>();
            uriParams.Add("confNumericId", confNumericId);
            uriParams.Add("deviceId", deviceId);
            string url = GetFullAcsUrl(REST_GET_PARTY_NAME_API, true, uriParams);
            return GetObject<RestPartNameInfo>(url, null, hdlErrMsg);
        }

        #endregion


        #region -- Private Method --

        private string GetFullDoradoUrl(string url, bool hasToken, Dictionary<object, object> uriParams)
        {
            return GetFullUrl(DoradoZoneAddress, url, hasToken, uriParams);
        }

        private string GetFullAcsUrl(string url, bool hasToken, Dictionary<object, object> uriParams)
        {
            return GetFullUrl(AcsServerAddress, url, hasToken, uriParams);
        }

        private string GetFullUrl(string serverAddress, string url, bool hasToken, Dictionary<object, object> uriParams)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(serverAddress).Append(url);

            bool addParam = false;
            if (hasToken)
            {
                sb.Append("?token=")
                    .Append(Token);
                addParam = true;
            }

            if (null != uriParams)
            {
                foreach (KeyValuePair<object, object> item in uriParams)
                {
                    if (!addParam)
                    {
                        sb.Append("?");
                        addParam = true;
                    }
                    else
                    {
                        sb.Append("&");
                    }
                    sb.Append(item.Key).Append("=")
                        .Append(item.Value);
                }
            }

            return sb.ToString();
        }
        
        private RESPONSE GetObject<RESPONSE>(string url)
        {
            return GetObject<RESPONSE>(url, null, null);
        }
        
        private RESPONSE GetObject<RESPONSE>(string url, HandleException handleException, HandleErrorMessage handleErrorMessage, bool isDoradoMsg = true)
        {
            RestResponse response = null;
            try
            {
                response = restClient.GetObject(url);
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    log.InfoFormat("Failed to get, status code: {0}, url: {1}", response.StatusCode, url);
                    if(handleErrorMessage == null)
                    {
                        HandleErrorMessageDefault(response, false, isDoradoMsg);
                    }
                    else
                    {
                        handleErrorMessage(response);
                    }
                    return default(RESPONSE);
                }

                return JsonConvert.DeserializeObject<RESPONSE>(response.Content);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to get object, exception:{0}", ex);
                log.InfoFormat("Failed to get, url:{0}", url);
                if (null == response)
                {
                    log.Info("The response is null for get object");
                }
                else
                {
                    log.InfoFormat("Failed to get object, the response is {0}", response.Content);
                }
                handleException?.Invoke(ex);
            }

            return default(RESPONSE);
        }

        private RESPONSE PutObject<REQUEST, RESPONSE>(string url, REQUEST req)
        {
            return PutObject<REQUEST, RESPONSE>(url, req, null, null);
        }
        
        private RESPONSE PutObject<REQUEST, RESPONSE>(string url, REQUEST req, HandleException handleException, HandleErrorMessage handleErrorMessage, bool isDoradoMsg = true)
        {
            string jsonData = JsonConvert.SerializeObject(req);
            RestResponse response = null;
            try
            {
                response = restClient.PutObject(url, jsonData);
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    log.InfoFormat("Failed to put, status code: {0}, url: {1}", response.StatusCode, url);
                    if (handleErrorMessage == null)
                    {
                        HandleErrorMessageDefault(response, false, isDoradoMsg);
                    }
                    else
                    {
                        handleErrorMessage(response);
                    }
                    return default(RESPONSE);
                }
                
                return JsonConvert.DeserializeObject<RESPONSE>(response.Content);
            }
            catch(Exception ex)
            {
                log.ErrorFormat("Failed to put object, exception:{0}", ex);
                log.InfoFormat("Failed to put, url:{0}", url);
                if (null == response)
                {
                    log.Info("The response is null for put object");
                }
                else
                {
                    log.InfoFormat("Failed to put object, the response is {0}", response.Content);
                }
                handleException?.Invoke(ex);
            }

            return default(RESPONSE);
        }

        private RESPONSE PostObject<REQUEST, RESPONSE>(string url, REQUEST req)
        {
            return PostObject<REQUEST, RESPONSE>(url, req, null, null);
        }

        private RESPONSE PostObject<REQUEST, RESPONSE>(string url, REQUEST req, HandleException handleException, HandleErrorMessage handleErrorMessage, bool isDoradoMsg = true)
        {
            string jsonData = JsonConvert.SerializeObject(req);
            RestResponse response = null;
            try
            {
                response = restClient.PostObject(url, jsonData);
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    log.InfoFormat("Failed to post, status code: {0}, url: {1}", response.StatusCode, url);
                    if (handleErrorMessage == null)
                    {
                        HandleErrorMessageDefault(response, false, isDoradoMsg);
                    }
                    else
                    {
                        handleErrorMessage(response);
                    }
                    return default(RESPONSE);
                }

                return JsonConvert.DeserializeObject<RESPONSE>(response.Content);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to post object, exception:{0}", ex);
                log.InfoFormat("Failed to post, url:{0}", url);
                if (null == response)
                {
                    log.Info("The response is null for post object");
                }
                else
                {
                    log.InfoFormat("Failed to post object, the response is {0}", response.Content);
                }
                handleException?.Invoke(ex);
            }

            return default(RESPONSE);
        }


        private bool DeleteObject(string url)
        {
            return DeleteObject(url, null, null);
        }

        private bool DeleteObject(string url, HandleException handleException, HandleErrorMessage handleErrorMessage, bool isDoradoMsg = true)
        {
            bool result = true;
            RestResponse response = null;
            try
            {
                response = restClient.DeleteObject(url);
                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    log.InfoFormat("Failed to delete, status code: {0}, url: {1}", response.StatusCode, url);
                    if (handleErrorMessage == null)
                    {
                        HandleErrorMessageDefault(response, false, isDoradoMsg);
                    }
                    else
                    {
                        handleErrorMessage(response);
                    }
                    result = false;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to delete object, exception:{0}", ex);
                log.InfoFormat("Failed to delete, url:{0}", url);
                if (null == response)
                {
                    log.Info("The response is null for delete object");
                }
                else
                {
                    log.InfoFormat("Failed to delete object, the response is {0}", response.Content);
                }
                handleException?.Invoke(ex);
                result = false;
            }
            return result;
        }

        private void HandleErrorMessageDefault(RestResponse response, bool showAllErrors, bool isDoradoMsg)
        {
            // do nothing        
        }

        
        #endregion

    }


}
