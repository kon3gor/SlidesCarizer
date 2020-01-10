using Google.Apis.Auth.OAuth2;
using Google.Apis.Slides.v1;
using Google.Apis.Slides.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace SlidesCatizer
{
    class Catizer
    {
        private static string[] Scopes = { SlidesService.Scope.Presentations };
        private static string ApplicationName = "SlidesCatizer";
        private static String presentationId;
        private static string baseimgurl;
        private static readonly String address = "https://api.github.com/graphql";
        private static SlidesService service;
        private static Random generator = new Random();
        private static int Count { get; set; }
        private static UserCredential credential;
        private static String github_token;

        static void Main(string[] args)
        {

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            using (var strings = new StreamReader(args[0]))
            {
                JToken param = JToken.Parse(strings.ReadToEnd());
                presentationId = param["presentation_id"].ToString();
                baseimgurl = param["img_base_url"].ToString();
                github_token = param["token"].ToString();
            }

            // Create Google Slides API service.
            service = new SlidesService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            BatchUpdatePresentationRequest body = new BatchUpdatePresentationRequest()
            {
                Requests = GenerateRequests()
            };
            _ = service.Presentations.BatchUpdate(body, presentationId).Execute();     
        }
        /*
         * Generate list of Request objsects for Google Docs API.
         * Each one has its own coordinates, and content, which in this case is an image from my github repo.
         */
        private static List<Request> GenerateRequests() {
            var list = new List<Request>();
            var coords = new List<List<int>>();
            Dimension emu4M = new Dimension()
            {
                Magnitude = 4000000.0,
                Unit = "EMU"
            };
            var req = new Request();

            //Get names of photos in the repo
            List<String> filenames = GetFilenames();
            //Get slide names from Google Docs API
            List <String> slidenames = GetSlideNames();
            //Calculate number of photos on each slide.
            Count = Count > filenames.Count ? Count % filenames.Count : filenames.Count % Count;
            foreach (String slidename in slidenames) {
                //Calcilate coordinates of photos for current slide
                coords = CalculateCoords();
                for (int i = 0; i < Count; i++)
                {
                    req.CreateImage = new CreateImageRequest()
                    {
                        Url = baseimgurl + filenames[i],
                        ElementProperties = new PageElementProperties()
                        {
                            PageObjectId = slidename,
                            Size = new Size()
                            {
                                Height = emu4M,
                                Width = emu4M
                            },
                            Transform = new AffineTransform()
                            {
                                TranslateX = coords[i][0],
                                TranslateY = coords[i][1],
                                ShearX = 0,
                                ShearY = 0,
                                ScaleX = 0.5,
                                ScaleY = 0.5,
                                Unit = "EMU"
                            }
                        },

                    };

                    list.Add(req);

                    req = new Request();
                }
                try
                {
                    filenames = filenames.GetRange(Count + 1, filenames.Count - Count - 1);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return list;
                } 
            }
            return list;

        }
        /*
         * Get photo names from the repo through the GitHub graphQL API.
         * Use RestSharp for making requests.
         * In fact, it's not a graphQL request.
         * TODO: make normal graphQL request.
         */
        private static List<String> GetFilenames() {

            var client = new RestClient(address);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Authorization", $"Bearer {github_token}");
            request.AddHeader("Content-Type", "application/json");
            //Really weird and work but it works
            request.AddParameter("undefined", "{\"query\": \"{repository(owner: \\\"kon3gor\\\", name: \\\"SlidesCatizer\\\") {filename: object(expression: \\\"master:images/\\\") {... on Tree {entries {name}}}}}\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            JToken resp = JToken.Parse(response.Content);

            var jsonlist = resp["data"]["repository"]["filename"]["entries"];
            List<String> filenames = new List<string>();
            foreach (var el in jsonlist)
            {
                filenames.Add(el["name"].ToString());
            }
            return filenames;

        }
        /*
         * Get slide names through the Google Docs API.
         */
        private static List<String> GetSlideNames() {

            List<String> slidenames = new List<String>();
            PresentationsResource.GetRequest request = service.Presentations.Get(presentationId);
            Presentation presentation = request.Execute();
            IList<Page> slides = presentation.Slides;
            Count = slides.Count;
            foreach (var slide in slides) {
                slidenames.Add(slide.ObjectId);
            }
            
            return slidenames;
        
        }
        /*
         * Calculate coordinates for each phtoto on the slide.
         * Max values was calculated with pain.
         */
        private static List<List<int>> CalculateCoords() {

            var coords = new List<List<int>>();
            int MAX_X = 8000000;
            int MAX_Y = 5000000;
            int x, y;

            for (int i = 0; i < Count; i++)
            {
                x = generator.Next(8) * 1000000;
                y = generator.Next(8) * 1000000;

                if (x >= MAX_X) {
                    x -= MAX_X;
                }
                if (y >= MAX_Y) {
                    y -= MAX_Y;
                }

                coords.Add(new List<int> { x, y });
            }

            return coords;

        }
    }
}