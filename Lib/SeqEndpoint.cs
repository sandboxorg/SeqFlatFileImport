﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SeqFlatFileImport
{
    internal interface ISeqEndpoint
    {
        void Write(RawEvent entry);
        IResult Flush();
    }

    internal class SeqEndpoint : ISeqEndpoint
    {
        public const string DefaultUri = "http://localhost:5341/";
        private const int BatchSize = 1000;

        private readonly Uri _uri;
        private readonly string _batchId;
        private readonly string _apiKey;
        private List<RawEvent> _events = new List<RawEvent>(BatchSize);
        private readonly List<Task<IResult>> _sendTasks = new List<Task<IResult>>();

        public SeqEndpoint(string uri = DefaultUri, string apiKey = null, string batchId = null)
        {
            _uri = new Uri(new Uri(uri ?? DefaultUri), "/api/events/raw");
            _batchId = batchId ?? DateTimeOffset.Now.ToString();
            _apiKey = apiKey;
        }

        public void Write(RawEvent evt)
        {
            evt.Properties["BatchId"] = _batchId;
            _events.Add(evt);
            if (_events.Count == BatchSize)
            {
                var events = _events;
                _events = new List<RawEvent>(BatchSize);
                _sendTasks.Add(Task.Run(async () => await Send(events)));
            }
        }

        private async Task<IResult> Send(List<RawEvent> events)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new {Events = events});
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(_uri, content);
                    if (response.IsSuccessStatusCode)
                        return Result.Success();
                    return Result.Failed(response.StatusCode + " " + response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return Result.Failed(ex.Message);
            }
        }

        public IResult Flush()
        {
            if (_events.Count > 0)
                _sendTasks.Add(Send(_events));
            var results = Task.WhenAll(_sendTasks.ToArray()).Result;
            return Result.From(results);
        }
    }

   
}