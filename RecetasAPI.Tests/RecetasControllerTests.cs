using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetasAPI.Controllers;
using RecetasAPI.Data;
using RecetasAPI.Models;

namespace RecetasAPI.Tests
{
    public class RecetasControllerTests
    {
        private static RecetasDbContext CrearContexto()
        {
            var options = new DbContextOptionsBuilder<RecetasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new RecetasDbContext(options);
        }

        private static List<ValidationResult> Validar(object modelo)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
            return resultados;
        }

        // ---------------- Controlador ----------------

        [Fact]
        public async Task PostReceta_ConDatosValidos_CreaYRetornaLaReceta()
        {
            // Arrange
            using var context = CrearContexto();
            var controller = new RecetasController(context);
            var receta = new Receta { Nombre = "Huevos revueltos", Descripcion = "Desayuno rápido", TiempoPreparacion = 10 };

            // Act
            var resultado = await controller.PostReceta(receta, CancellationToken.None);

            // Assert
            var creado = Assert.IsType<CreatedAtActionResult>(resultado.Result);
            var recetaCreada = Assert.IsType<Receta>(creado.Value);
            Assert.Equal("Huevos revueltos", recetaCreada.Nombre);
            Assert.Equal(1, await context.Recetas.CountAsync());
        }

        [Fact]
        public async Task GetReceta_ConIdExistente_RetornaLaReceta()
        {
            using var context = CrearContexto();
            var receta = new Receta { Nombre = "Sopa de Tomate", TiempoPreparacion = 40 };
            context.Recetas.Add(receta);
            await context.SaveChangesAsync();
            var controller = new RecetasController(context);

            var resultado = await controller.GetReceta(receta.Id, CancellationToken.None);

            var recetaEncontrada = Assert.IsType<Receta>(resultado.Value);
            Assert.Equal("Sopa de Tomate", recetaEncontrada.Nombre);
        }

        [Fact]
        public async Task GetReceta_ConIdInexistente_RetornaNotFound()
        {
            using var context = CrearContexto();
            var controller = new RecetasController(context);

            var resultado = await controller.GetReceta(999, CancellationToken.None);

            Assert.IsType<NotFoundResult>(resultado.Result);
        }

        [Fact]
        public async Task DeleteReceta_ConIdExistente_EliminaLaReceta()
        {
            using var context = CrearContexto();
            var receta = new Receta { Nombre = "Pasta Carbonara", TiempoPreparacion = 30 };
            context.Recetas.Add(receta);
            await context.SaveChangesAsync();
            var controller = new RecetasController(context);

            var resultado = await controller.DeleteReceta(receta.Id, CancellationToken.None);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Equal(0, await context.Recetas.CountAsync());
        }

        [Fact]
        public async Task PostIngrediente_ConRecetaInexistente_RetornaBadRequest()
        {
            using var context = CrearContexto();
            var controller = new IngredientesController(context);
            var ingrediente = new Ingrediente { Nombre = "Albahaca", Cantidad = 5, UnidadMedida = "Hojas", RecetaId = 50 };

            var resultado = await controller.PostIngrediente(ingrediente, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(resultado.Result);
        }

        [Fact]
        public async Task GetPasosPreparacion_PorReceta_RetornaPasosOrdenados()
        {
            using var context = CrearContexto();
            var receta = new Receta { Nombre = "Ensalada César", TiempoPreparacion = 20 };
            context.Recetas.Add(receta);
            await context.SaveChangesAsync();
            context.PasosPreparacion.AddRange(
                new PasoPreparacion { Orden = 2, Descripcion = "Asar el pollo y cortarlo en tiras.", RecetaId = receta.Id },
                new PasoPreparacion { Orden = 1, Descripcion = "Lavar la lechuga y cortarla en trozos.", RecetaId = receta.Id });
            await context.SaveChangesAsync();
            var controller = new PasosPreparacionController(context);

            var resultado = await controller.GetPasosPreparacion(receta.Id, CancellationToken.None);

            var pasos = Assert.IsAssignableFrom<IEnumerable<PasoPreparacion>>(resultado.Value).ToList();
            Assert.Equal(2, pasos.Count);
            Assert.Equal(1, pasos[0].Orden);
            Assert.Equal(2, pasos[1].Orden);
        }

        // ---------------- Validaciones del modelo ----------------

        [Fact]
        public void Receta_ConNombreMenorA3Caracteres_FallaValidacionDeModelo()
        {
            var receta = new Receta { Nombre = "Té", TiempoPreparacion = 5 };

            var resultados = Validar(receta);

            Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Receta.Nombre)));
        }

        [Fact]
        public void Ingrediente_ConNombreMayorA50Caracteres_FallaValidacionDeModelo()
        {
            var ingrediente = new Ingrediente { Nombre = new string('a', 51), Cantidad = 1, UnidadMedida = "Unidad", RecetaId = 1 };

            var resultados = Validar(ingrediente);

            Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(Ingrediente.Nombre)));
        }

        [Fact]
        public void PasoPreparacion_ConDescripcionMenorA10Caracteres_FallaValidacionDeModelo()
        {
            var paso = new PasoPreparacion { Descripcion = "Mezclar", Orden = 1, RecetaId = 1 };

            var resultados = Validar(paso);

            Assert.Contains(resultados, r => r.MemberNames.Contains(nameof(PasoPreparacion.Descripcion)));
        }
    }
}
