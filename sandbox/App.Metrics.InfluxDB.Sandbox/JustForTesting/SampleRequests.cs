﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Scheduling;

namespace App.Metrics.InfluxDB.Sandbox.JustForTesting
{
    public static class SampleRequests
    {
        private static readonly Uri ApiBaseAddress = new Uri("http://localhost:1111/");

        public static void Run(CancellationToken token)
        {
            var randomBufferGenerator = new RandomBufferGenerator(50000);
            var scheduler = new DefaultTaskScheduler();
            var httpClient = new HttpClient
            {
                BaseAddress = ApiBaseAddress
            };

            Task.Run(
                () => scheduler.Interval(
                    TimeSpan.FromSeconds(1),
                    TaskCreationOptions.None,
                    async () =>
                    {
                        var satisfied = httpClient.GetAsync("api/satisfying", token);
                        var tolerating = httpClient.GetAsync("api/tolerating", token);
                        var frustrating = httpClient.GetAsync("api/frustrating", token);

                        await Task.WhenAll(satisfied, tolerating, frustrating);
                    },
                    token),
                token);

            Task.Run(
                () => scheduler.Interval(
                    TimeSpan.FromSeconds(3),
                    TaskCreationOptions.None,
                    async () =>
                    {
                        var randomStatusCode = httpClient.GetAsync("api/randomstatuscode", token);
                        var exceptionThrower = httpClient.GetAsync("api/exceptionThrowing", token);

                        await Task.WhenAll(randomStatusCode, exceptionThrower);
                    },
                    token),
                token);

            Task.Run(
                () => scheduler.Interval(
                    TimeSpan.FromSeconds(3),
                    TaskCreationOptions.None,
                    async () =>
                    {
                        await httpClient.GetAsync("api/test", token);
                    },
                    token),
                token);

            Task.Run(
                () => scheduler.Interval(
                    TimeSpan.FromSeconds(2),
                    TaskCreationOptions.None,
                    async () =>
                    {
                        var putBytes = new ByteArrayContent(randomBufferGenerator.GenerateBufferFromSeed());
                        var putFormData = new MultipartFormDataContent { { putBytes, "put-file", "rnd-put" } };
                        var putRequest = httpClient.PutAsync("api/file", putFormData, token);

                        var postBytes = new ByteArrayContent(randomBufferGenerator.GenerateBufferFromSeed());
                        var postFormData = new MultipartFormDataContent { { postBytes, "post-file", "rnd-post" } };
                        var postRequest = httpClient.PostAsync("api/file", postFormData, token);

                        await Task.WhenAll(putRequest, postRequest);
                    },
                    token),
                token);
        }
    }
}
