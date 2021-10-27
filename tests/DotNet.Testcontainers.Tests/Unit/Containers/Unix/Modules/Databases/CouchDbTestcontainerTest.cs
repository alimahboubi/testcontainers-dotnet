namespace DotNet.Testcontainers.Tests.Unit
{
  using System;
  using System.Net;
  using System.Threading.Tasks;
  using DotNet.Testcontainers.Tests.Fixtures;
  using Xunit;

  [Collection(nameof(Testcontainers))]
  public sealed class CouchDbTestcontainerTest : IClassFixture<CouchDbFixture>
  {
    private readonly CouchDbFixture couchDbFixture;

    public CouchDbTestcontainerTest(CouchDbFixture couchDbFixture)
    {
      this.couchDbFixture = couchDbFixture;
    }

    [Fact]
    public async Task ConnectionEstablished()
    {
      // Given
      var database = this.couchDbFixture.Connection.Database;

      // When
      var response = await database.PutAsync()
        .ConfigureAwait(false);

      // Then
      Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task ExecScriptInRunningContainer()
    {
      // Given
      const string script = @"
        #!/bin/bash
        curl -v -X PUT http://couchdb:couchdb@127.0.0.1:5984/mydatabase/
        curl -v -X PUT http://couchdb:couchdb@127.0.0.1:5984/mydatabase/""001"" -d '{ "" name "" : "" MyName "" }'
      ";

      var docIdRequestBuilder = new UriBuilder(this.couchDbFixture.Container.ConnectionString);
      docIdRequestBuilder.Path = "mydatabase/001";

      // When
      var result = await this.couchDbFixture.Container.ExecScriptAsync(script)
        .ConfigureAwait(false);

      // Then
      string response;

      using (var client = new WebClient())
      {
        response = client.DownloadString(docIdRequestBuilder.Uri);
      }

      Assert.Equal(0, result.ExitCode);
      Assert.Contains("MyName", response);
    }
  }
}
