using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {
        public static void AddRotasEstudantes(this WebApplication app) 
        {
            var rotasEstudantes = app.MapGroup("estudantes");


            // Para criar se usa Post
            rotasEstudantes.MapPost("", 
                async (AddEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var jaExiste = await context.Estudantes.AnyAsync(estundante => estundante.Nome == request.Nome, ct);

                if (jaExiste)
                    return Results.Conflict("Já existe!");

                var novoEstudante = new Estudante(request.Nome);
                await context.Estudantes.AddAsync(novoEstudante, ct);
                await context.SaveChangesAsync(ct);

                var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(estudanteRetorno);
            });

            // Retornar todos os Estudantes Cadastrados
            rotasEstudantes.MapGet("", 
                async (AppDbContext context, CancellationToken ct) =>
            {
                var estudantes = await context
                .Estudantes
                .Where(estudante => estudante.Ativo)
                .Select(estudante => new EstudanteDto(estudante.Id, estudante.Nome))
                .ToListAsync(ct);

                return estudantes;
            });

            // Atualizar Nome Estudante
            rotasEstudantes.MapPut("{id:guid}", 
                async (Guid id ,UpdateEstudanteRequest request, AppDbContext context, CancellationToken ct) =>
            {
                var estudante = await context.Estudantes
                    .SingleOrDefaultAsync(estudante => estudante.Id.Equals(id), ct);

                if (estudante == null)
                    return Results.NotFound();

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(ct);
                return Results.Ok(new EstudanteDto(estudante.Id, estudante.Nome));
            });

            // Deletar
            rotasEstudantes.MapDelete("{id}",
                async (Guid id, AppDbContext context, CancellationToken ct) =>
                {
                    var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, ct);

                    if (estudante == null)
                        return Results.NotFound();

                    estudante.Desativar();

                    await context.SaveChangesAsync(ct);
                    return Results.Ok();
                });
        }
    }
}
