using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

// Endpoint => Url
// local c/ https => https://localhost:5001
// local s/ https => http://localhost:5000
// produção => https://meuapp.azurewebsites.net/

[Route("v1/categories")] // => https://localhost:5001/categories
public class CategoryController : ControllerBase 
{
    [HttpGet]
    [Route("")] // => https://localhost:5001/categories (caso o Route não tenha nenhum nome declarado, será considerado a rota principal como a válida)
    [AllowAnonymous]
    [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
    // ResponseCache => Faz o cache do método (location diz onde será armazenado o cache)(Duration é a duração do cache)
    public async Task<ActionResult<List<Category>>> Get(
        [FromServices] DataContext context
    )
    // Task => utilizado para a progração assíncrona, para não travar a thread principal da aplicação
    // ActionResult => apresenta um result no formato que a tela espera, também é possível tipar ele
    // async => cria threads paralelas na aplicação para não travar a execução principal
    // <List<Category>> => retorna um array (lista) de objetos de categorias
    {
        var categories = await context.Categories.AsNoTracking().ToListAsync();
        // AsNoTracking => pega de forma mais simples os dados requisitados, já que não haverá manipulação dos mesmos, assim sendo a forma mais rápida
        // ToListAsync => executa a query no banco, sempre deixar por ultimo isso para não utilizar de forma massiva a memória do banco
        return Ok(categories);
    }

    [HttpGet]
    [Route("{id:int}")] // => https://localhost:5001/categories/id (o "{:int}" é uma restrição de rota, quer dizer que só aceita parametro do tipo declarado)
    [AllowAnonymous]
    public async Task<ActionResult<Category>> GetById(
        int id,
        [FromServices] DataContext context
    )
    {
        var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        if (category == null) {
            return NotFound(new { message = "Categoria não encontrada" });
        }

        return category;
    }

    [HttpPost]
    [Route("")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<List<Category>>> Post(
        [FromBody] Category model,
        [FromServices] DataContext context
    ) 
    // "[FromBody]" especifica que virá do corpo da requisição os dados
    // "[FromServices]" especifica que está usando o serviço de DataContext e setando no context
    // Utilizando a (Category model) é possível filtrar e validar o que será recebido compatível a model ja criada e configurada
    {
        if (!ModelState.IsValid) {// verifica a model se ela é válida ou não
            return BadRequest(ModelState); // se não é válida retorna um bad request, ou seja, requisição inválida, retornando também qual é o erro apresentado
        }

        try 
        { // tenta incluir no banco de dados
            context.Categories.Add(model); // está adicionando na memória virtual uma categoria modelada pela model Category
            await context.SaveChangesAsync(); // aguardar salvar as mudanças no banco de forma assincrona
            // quando executa o SaveChangesAsync é gerado um id automático e é preenchido no id do model
            return Ok(model); // "model" é o parâmetro criado tipado com uma model 
            // Ok => é um ActionResult retornando que está certa a requisição
        }
        catch
        {
            return BadRequest(new { message = "Não foi possível criar a categoria" });
        }
    }

    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<List<Category>>> Put(
        int id, 
        [FromBody] Category model,
        [FromServices] DataContext context
    )
    {
        // verifica se o id passado pela rota é o mesmo passado pelo body
        if (id != model.Id) {
            return NotFound(new { message = "Categoria não encontrada" }); // new {} é a criação de um objeto dinâmico, ou seja, sem tipagem
        }

        if (!ModelState.IsValid) {
            return BadRequest(ModelState); 
        }
        
        try {
            context.Entry<Category>(model).State = EntityState.Modified; // nessa linha diz que o estado da entrada da model está modificado, dizendo que algo mudou nessa model
            // assim o entity framework vai verificar por conta propriedade por propriedade para saber qual foi a mudança e assim salvar apenas a alteração no banco
            await context.SaveChangesAsync();
            return Ok(model);
        }
        catch (DbUpdateConcurrencyException) // erro de concorrência, tentando atualizar o dado ao mesmo tempo de outras formas 
        {
            return BadRequest(new { message = "Este registro já foi atualizado" });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Não foi possível atualizar a categoria" });
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<List<Category>>> Delete(
        int id,
        [FromServices] DataContext context
    )
    {
        var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
        // FirstOrDefaultAsync => vai buscar uma categoria, se encontrar mais de uma retornará a primeira, se não encontrar retornará nulo
        // x => x.Id == id => passa X como variável, retorna se x.Id é igual ao id passado na Url
        // ou seja, recupera a categoria do banco 
        if (category == null) {
            return NotFound(new { message = "Categoria não encontrada!" });
        }
        
        try 
        {
            context.Categories.Remove(category); // remove a categoria da memória virtual
            await context.SaveChangesAsync(); // remove a categoria do banco
            return Ok(new { message = "Categoria removida com sucesso" });
        }
        catch (Exception) 
        {
            return BadRequest(new { message = "Não foi possível remover a categoria" });
        }
    }
}