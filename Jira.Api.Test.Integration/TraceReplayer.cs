using Jira.Api.Remote;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira.Api.Test.Integration;

class TraceReplayer : IJiraRestClient
{
	private readonly Queue<string> _responses;

	public TraceReplayer(string traceFilePath)
	{
		var lines = File.ReadAllLines(traceFilePath).Where(line => !line.StartsWith("//") && !string.IsNullOrEmpty(line.Trim()));
		_responses = new Queue<string>(lines);
	}

	public RestClient RestSharpClient
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public JiraRestClientSettings Settings
	{
		get
		{
			return new JiraRestClientSettings();
		}
	}

	public string Url
	{
		get
		{
			return "http://testurl";
		}
	}

	public Task<RestResponse> ExecuteRequestAsync(RestRequest request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<JToken> ExecuteRequestAsync(Method method, string resource, object? requestBody, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Method: {method}. Url: {resource}");
		var response = JsonConvert.DeserializeObject(_responses.Dequeue());
		return Task.FromResult(JToken.FromObject(response));
	}

	public Task<T> ExecuteRequestAsync<T>(Method method, string resource, object? requestBody, CancellationToken cancellationToken)
	{
		Console.WriteLine($"Method: {method}. Url: {resource}");
		var result = JsonConvert.DeserializeObject<T>(_responses.Dequeue());
		return Task.FromResult(result);

	}

	public Task<byte[]?> DownloadDataAsync(string url, CancellationToken cancellationToken)
		=> throw new NotImplementedException();

	public Task DownloadAsync(string url, string fullFileName, CancellationToken cancellationToken)
		=> throw new NotImplementedException();
}
