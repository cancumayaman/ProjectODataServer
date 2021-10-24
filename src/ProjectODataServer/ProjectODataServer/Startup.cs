using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Sample.Data.DbContexts;
using Sample.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectODataServer
{
    public class Startup
    {
        public IWindsorContainer Container { get; } = new WindsorContainer();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Container);
            services.AddOptions();
            services.AddControllers();
            services.AddDbContext<SampleDataDbContext>(c => c
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .UseSqlite("Data Source=SampleData.db"));
            services.AddOData();
            Container.Register(
                Component.For<IConfiguration>()
                .Instance(Configuration)
                );
            Container.Install(FromAssembly.Containing<Startup>());
            var installAssemblies = Configuration
                .GetSection("InstallAssemblies")
                .Get<string[]>();
            if (installAssemblies != null && installAssemblies.Length > 0)
                foreach (var item in installAssemblies)
                {
                    Container.Install(FromAssembly.Named(item));
                }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Select().Filter().OrderBy().Count().MaxTop(10).Expand();
                endpoints.MapODataRoute("odata", "odata", GetEdmModel());
            });
        }
        private IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Category>("Category");
            odataBuilder.EntitySet<Product>("Product");
            return odataBuilder.GetEdmModel();
        }
    }

    public class GenerþcTypeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var types = new[] { typeof(Product), typeof(Category) };
            var sb = new StringBuilder();

            sb.AppendLine(@"using Microsoft.AspNet.OData;
            using Microsoft.AspNetCore.Mvc;
            using Sample.Data.DbContexts;
            using Sample.Data.Entities;
            using System.Linq;
            namespace ProjectODataServer.Controllers.OData
            {");

            foreach (var type in types)
            {
                sb.AppendLine($@"public class {type.Name}Controller : ControllerBase
	        {{
	        	private readonly SampleDataDbContext _db;
	        	public {type.Name}Controller(SampleDataDbContext db)
	        	{{
	        		_db = db;
	        	}}
	        	[EnableQuery]
	        	public IQueryable<{type.Name}> Get()
	        	{{
	        		return _db.Set<{type.Name}>();
	        	}}
	        }}
"           );
            }

            sb.AppendLine("}");

        }
    }
}
