using Main.Api.Twitch;
using Main.Data;
using Main.Models;
using Main.Models.Dao;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Main.Controllers
{
	public class CommonController : Controller
	{
		public bool customerLogin = false;

		public CommonController(ref MainContext context, HttpRequest request)
		{
			var optionsBuilder = new DbContextOptions<MainContext>();
			context = new MainContext(optionsBuilder);
			context.Value = new Dictionary<string, object>();
		}


	}
}
