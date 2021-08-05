﻿namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Newtonsoft.Json;

    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.VideoManagement;

    public class ApplicationHost
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public ApplicationHost(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public async Task<byte[]> GetBytes(string endpoint)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

            return bytes;
        }

        public async Task<BytesResponse> GetBytesAndContentType(string endpoint)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

            return new BytesResponse(
                    bytes,
                    response.Content.Headers.ContentType.MediaType,
                    response.IsSuccessStatusCode,
                    response.ReasonPhrase);
        }

        public async Task<PartialResponse> GetRange(string endpoint, string range)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Add("Range", range);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

            return PartialResponse.Parse(response.Content.Headers, bytes);
        }

        public async Task<T> Get<T>(string endpoint)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<VideoId> Post(string endpoint, byte[] content)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            ByteArrayContent byteArrayContent = new ByteArrayContent(content);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = byteArrayContent
            };
            byteArrayContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            VideoIdResponse videoIdResponse =
                    JsonConvert.DeserializeObject<VideoIdResponse>(responseString);

            return new VideoId(long.Parse(videoIdResponse.VideoId));
        }
    }
}