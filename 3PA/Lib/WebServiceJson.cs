using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using _3PA.MainFeatures;

namespace _3PA.Lib {
    public class WebServiceJson {

        #region fields

        /// <summary>
        /// Url on which to to the request
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Json read on the url
        /// </summary>
        public string JsonResponse { get; private set; }

        /// <summary>
        /// Status code of the request, use this to know if it went ok
        /// </summary>
        public HttpStatusCode StatusCodeResponse { get; private set; }

        /// <summary>
        /// Status description of the request
        /// </summary>
        public string StatusDescriptionResponse { get; private set; }

        /// <summary>
        /// The request that will be sent
        /// </summary>
        public StringBuilder JsonRequest { get; private set; }

        /// <summary>
        /// Exception caught during the request, will be null if all went ok
        /// </summary>
        public Exception ResponseException { get; private set; }

        /// <summary>
        /// subscribe to this to do an action when the request ends
        /// </summary>
        public event Action<WebServiceJson> OnRequestEnded;

        /// <summary>
        /// subscribe to this to modify the HttpRequest before executing it (for instance to add headers)
        /// </summary>
        public event Action<HttpWebRequest> OnInitHttpWebRequest;

        /// <summary>
        /// Post or get
        /// </summary>
        public WebRequestMethod Method { get; private set; }

        /// <summary>
        /// Set the request timeout
        /// </summary>
        public int TimeOut {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        #endregion

        #region private fields

        private int _timeOut = 2000;

        private JavaScriptSerializer _jsSerializer;

        private HttpWebRequest _httpRequest;

        #endregion

        #region life and death

        /// <summary>
        /// Constructor
        /// </summary>
        public WebServiceJson(WebRequestMethod method, string url) {
            Method = method;
            Url = url;
            StatusCodeResponse = HttpStatusCode.BadRequest;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Append name: "value" to the request
        /// </summary>
        public void AddToReq(string name, string value) {
            if (JsonRequest == null)
                JsonRequest = new StringBuilder("{");
            else
                JsonRequest.Append(",");

            if (_jsSerializer == null)
                _jsSerializer = new JavaScriptSerializer();

            JsonRequest.Append("\"");
            JsonRequest.Append(name);
            JsonRequest.Append("\": ");
            JsonRequest.Append(_jsSerializer.Serialize(value));
        }

        /// <summary>
        /// Call this method to send the request/receive the response
        /// </summary>
        public void Execute() {
            Task.Factory.StartNew(() => {
                try {
                    if (JsonRequest != null && !JsonRequest.EndsWith("}"))
                        JsonRequest.Append("}");

                    // init request
                    _httpRequest = WebRequest.Create(Url) as HttpWebRequest;
                    if (_httpRequest != null) {
                        _httpRequest.Proxy = Config.Instance.GetWebClientProxy();
                        _httpRequest.Method = Method.ToString().ToUpper();
                        _httpRequest.ContentType = "application/json";
                        _httpRequest.UserAgent = Config.GetUserAgent;
                        _httpRequest.ReadWriteTimeout = TimeOut;
                        _httpRequest.Timeout = TimeOut;
                    }
                    if (OnInitHttpWebRequest != null)
                        OnInitHttpWebRequest(_httpRequest);

                    if (Method == WebRequestMethod.Post) {
                        if (JsonRequest == null) {
                            ResponseException = new Exception("No request set!");
                        } else {
                            // start posting
                            if (PostRequest()) {

                                // get the response
                                GetResponse();
                            }
                        }

                    } else if (Method == WebRequestMethod.Get) {
                        // get the response
                        GetResponse();
                    }

                    if (OnRequestEnded != null)
                        OnRequestEnded(this);

                } catch (WebException e) {
                    ResponseException = e;
                    HandleWebException(e);
                } catch (Exception e) {
                    ResponseException = e;
                    StatusCodeResponse = HttpStatusCode.BadRequest;
                }
            });
        }

        /// <summary>
        /// Fill an object based on the response json
        /// </summary>
        public T Deserialize<T>() {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            return jsSerializer.Deserialize<T>(JsonResponse);
        }

        /// <summary>
        /// Fill an object based on the response json
        /// </summary>
        public List<T> DeserializeArray<T>() {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            return jsSerializer.Deserialize<List<T>>(JsonResponse);
        }

        /// <summary>
        /// Prepare the json request string by serializing an object
        /// </summary>
        public void Serialize<T>(T obj) {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            JsonRequest = new StringBuilder(jsSerializer.Serialize(obj));
        }

        #endregion

        #region private methods

        private bool PostRequest() {
            try {
                // post in UTF8  
                var byteArray = Encoding.UTF8.GetBytes(JsonRequest.ToString());

                using (Stream writer = _httpRequest.GetRequestStream()) {
                    writer.Write(byteArray, 0, byteArray.Length);
                    writer.Flush();
                }
                return true;
            } catch (WebException e) {
                ResponseException = e;
                HandleWebException(e);
            } catch (Exception e) {
                ResponseException = e;
                StatusCodeResponse = HttpStatusCode.BadRequest;
            }
            return false;
        }

        private void GetResponse() {
            try {
                using (HttpWebResponse httpWebResponse = _httpRequest.GetResponse() as HttpWebResponse) {
                    if (httpWebResponse != null) {
                        StatusCodeResponse = httpWebResponse.StatusCode;
                        var responseStream = httpWebResponse.GetResponseStream();
                        if (responseStream != null) {
                            using (StreamReader reader = new StreamReader(responseStream))
                                JsonResponse = reader.ReadToEnd();
                        }
                    }
                }
            } catch (WebException e) {
                ResponseException = e;
                HandleWebException(e);
            } catch (Exception e) {
                ResponseException = e;
                StatusCodeResponse = HttpStatusCode.BadRequest;
            }
        }
        
        private void HandleWebException(WebException wex) {
            HttpWebResponse hwr = wex.Response as HttpWebResponse;
            if (hwr != null) {
                StatusCodeResponse = hwr.StatusCode;
                StatusDescriptionResponse = hwr.StatusDescription;
            }
        }
        
        #endregion

        #region WebRequestMethod

        public enum WebRequestMethod {
            Post,
            Get
        }

        #endregion

    }

}
