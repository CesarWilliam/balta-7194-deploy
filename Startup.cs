using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Data;

namespace Shop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(); // => serviço de utilização do CORS

            services.AddResponseCompression(options => 
            {
                options.Providers.Add<GzipCompressionProvider>(); // zip das informações contidas no json, assim compactando as mesmas para enviar ao html
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new [] { "application/json" });
            });

            services.AddControllers();

            var key = Encoding.ASCII.GetBytes(Settings.Secret); // => transformando a chave em formato de bites
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => 
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            }); 

            // ==> Criação do banco virtual
            services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));  
            // configurando o uso do banco com o DataContext. "Database" foi o nome criado aleatório, pode ser qualquer nome
            // UseInMemoryDatabase => cria o banco em memória virtual da aplicação

            // ==> Criação do banco SQL
            // services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));
            // AddDbContext => cria a injeção de dependencia dentro da aplicação, fazendo as conexões com o banco
            // UseSqlServer => cria o banco SQL server 
            // GetConnectionString => pega a conexão criada no appsettings.json, passando o nome configurado

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger(); // => permite e especificação do uso da documentação

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1"); // endpoint do Swagger
            });

            app.UseRouting();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()); // ==> configura o CORS para autorizar o uso de localhost nas requisições

            app.UseAuthentication(); // => autentica o usuário
            app.UseAuthorization(); // => diz quais as roles dos usuários

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

// dotnet new webapi -o Shop => comando utlizado para criar uma paste e um projeto dentro dela com o nome qualquer
// dotnet add package Microsoft.EntityFrameworkCore.InMemory => cria o banco em memória virtual da aplicação
// dotnet add package Microsoft.EntityFrameworkCore.SqlServer => cria o naco SQL
// dotnet watch run => comando para acompanhar as modificações da aplicação 

// ==> pacotes para configuração e criação do banco de dados
// dotnet tool install --global dotnet-ef => instalação de uma ferramente do dotnet, é um pacote onde permite o acesso a todos os comandos do EntityFramework
// dotnet add package Microsoft.EntityFrameworkCore.Design => o pacote ajuda a informar como o ef vai gerar o banco na migração dele
// dotnet ef migrations add InitialCreate => gera a migração do banco, criando uma pasta "Migrations" no projeto (a migração é gerada a partir das models pré definidas)
// dotnet ef database update => executa todas as migrações no banco

// ==> Pacotes de autenticação e autorização
// dotnet add package Microsoft.AspNetCore.Authentication => pacote de autenticação de api
// dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer => pacote de desencriptar para o formato Jwt(JSON Web Token)

// ==> Pacote de documentação
// dotnet add package Swashbuckle.AspNetCore -v 5.0.0-rc4 => pacote de documentação do Swagger