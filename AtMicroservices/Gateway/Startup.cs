using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; set; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddAuthentication(options =>
				{
					options.DefaultScheme = "alex_approves";
					//options.DefaultAuthenticateScheme = "CustomScheme";
					//options.DefaultChallengeScheme = "CustomScheme";
				})
				.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("alex_approves", o => { });

			services.AddOcelot(Configuration);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseAuthentication();

			app.UseOcelot().Wait();

			app.UseHttpsRedirection();
		}
	}

	public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		public BasicAuthenticationHandler(
			IOptionsMonitor<AuthenticationSchemeOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			var claims = new[] {
				new Claim("userId", "user_1"),
				new Claim("mail", "user_1@gmail.com")
			};
			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return AuthenticateResult.Success(ticket);
		}
	}
}
