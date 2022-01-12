using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Alura.CoisasAFazer.WebApp.Controllers;
using Alura.CoisasAFazer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class TarefasControllerEndpointCadastraTarefa
    {
        [Fact]
        public void DadaTarefaComInfosValidasDeveRetornarOk()
        {
            //arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            contexto.Categorias.Add(new Categoria(20, "Nova Categoria"));
            contexto.SaveChanges();
            var inMemoryRepo = new RepositorioTarefa(contexto);

            var controlador = new TarefasController(inMemoryRepo, mockLogger.Object);

            var modelFake = new CadastraTarefaVM();
            modelFake.IdCategoria = 20;
            modelFake.Titulo = "Nova Tarefa X";
            modelFake.Prazo = new DateTime(2022, 02, 15);

            //act
            //o retorno é IActionResult
            var result = controlador.EndpointCadastraTarefa(modelFake);

            //assert
            //confere se o retorno do método é o esperado (sucesso)
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void DadaTarefaComInfosValidasDeveRetornarOkSoComMoq()
        {
            //arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mockRepo = new Mock<IRepositorioTarefas>();

            //o controller chama o método Execute da classe ObtemCategoriaPorIdHandler
            //esse método chama o ObtemCategoriaPorId do repositório que retorna uma categoria
            //para simular uma categoria verdadeira, fazer o Setup com o retorno desejado
            var categ = new Categoria("categoria");
            mockRepo.Setup(s => s.ObtemCategoriaPorId(It.IsAny<int>())).Returns(categ);

            var controlador = new TarefasController(mockRepo.Object, mockLogger.Object);

            //os dados são usados no método no controller
            //se não forem passados, dará erro de referência (por não possuir os valores)
            var modelFake = new CadastraTarefaVM();
            modelFake.IdCategoria = 20;
            modelFake.Titulo = "Nova Tarefa X";
            modelFake.Prazo = new DateTime(2022, 02, 15);

            //act
            //o retorno é IActionResult
            var result = controlador.EndpointCadastraTarefa(modelFake);

            //assert
            //confere se o retorno do método é o esperado (sucesso)
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public void QuandoHpuverExcecaoRetornar500()
        {
            //arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mockRepo = new Mock<IRepositorioTarefas>();
            var categ = new Categoria("categoria");
            mockRepo.Setup(s => s.ObtemCategoriaPorId(It.IsAny<int>())).Returns(categ);
            mockRepo.Setup(s => s.IncluirTarefas(It.IsAny<Tarefa[]>())).Throws(new Exception("Erro ao incluir tarefa!"));

            var controlador = new TarefasController(mockRepo.Object, mockLogger.Object);

            var modelFake = new CadastraTarefaVM();
            modelFake.IdCategoria = 20;
            modelFake.Titulo = "Nova Tarefa X";
            modelFake.Prazo = new DateTime(2022, 02, 15);

            //act
            //o retorno é IActionResult
            var result = controlador.EndpointCadastraTarefa(modelFake);

            //assert
            Assert.IsType<StatusCodeResult>(result);
            var statusCodeRetornado = (result as StatusCodeResult).StatusCode;
            Assert.Equal(500, statusCodeRetornado);
        }
    }
}
