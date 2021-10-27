namespace DotNet.Testcontainers.Containers
{
  using System.Text;
  using System.Threading.Tasks;
  using DotNet.Testcontainers.Configurations;
  using JetBrains.Annotations;
  using Microsoft.Extensions.Logging;

  /// <inheritdoc cref="TestcontainerDatabase" />
  [PublicAPI]
  public sealed class MsSqlTestcontainer : TestcontainerDatabase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlTestcontainer" /> class.
    /// </summary>
    /// <param name="configuration">The Testcontainers configuration.</param>
    /// <param name="logger">The logger.</param>
    internal MsSqlTestcontainer(ITestcontainersConfiguration configuration, ILogger logger)
      : base(configuration, logger)
    {
    }

    /// <inheritdoc />
    public override string ConnectionString
      => $"Server={this.Hostname},{this.Port};Database={this.Database};User Id={this.Username};Password={this.Password};";

    /// <summary>
    /// Executes a SQL script in the database container.
    /// </summary>
    /// <param name="scriptContent">The content of the SQL script to be executed.</param>
    /// <returns>Task that completes when the script has been executed.</returns>
    public override async Task<ExecResult> ExecScriptAsync(string scriptContent)
    {
      var tempScriptFile = this.GetTempScriptFile();

      await this.CopyFileAsync(tempScriptFile, Encoding.UTF8.GetBytes(scriptContent), 493)
        .ConfigureAwait(false);

      return await this.ExecAsync(new[] { "/opt/mssql-tools/bin/sqlcmd", "-b", "-r1", "-S", this.Hostname, "-U", this.Username, "-P", this.Password, "-i", tempScriptFile })
        .ConfigureAwait(false);
    }
  }
}
