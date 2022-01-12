using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Testes
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DadaTarefaDeveIncluirNoBanco()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar XUnit", new Core.Models.Categoria("Estudo"), new DateTime(2022,12,31));

            var repo = new RepositorioFake();
            var mockLog = new Mock<ILogger<CadastraTarefaHandler>>();

            var handle = new CadastraTarefaHandler(repo, mockLog.Object);

            //act
            handle.Execute(comando);

            //assert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar XUnit").FirstOrDefault();
            Assert.NotNull(tarefa);

        }

        [Fact]
        public void QuandoExceptionForLancadaIsSuccessDeveSerFalso()
        {
            //arrange
            var comando = new CadastraTarefa("Estudar XUnit", new Core.Models.Categoria("Estudo"), new DateTime(2022, 12, 31));

            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Erro!"));
            var repo = mock.Object;

            var mockLog = new Mock<ILogger<CadastraTarefaHandler>>();

            var handle = new CadastraTarefaHandler(repo, mockLog.Object);

            //act
            var resultado = handle.Execute(comando);

            //assert
            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void QuandoExceptionForLancadaConferirLog()
        {
            //arrange
            var mensagemDeErro = "Erro!";
            var excecaoEsperada = new Exception(mensagemDeErro);
            var comando = new CadastraTarefa("Estudar XUnit", new Core.Models.Categoria("Estudo"), new DateTime(2022, 12, 31));

            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada);
            var repo = mock.Object;

            var mockLog = new Mock<ILogger<CadastraTarefaHandler>>();

            var handle = new CadastraTarefaHandler(repo, mockLog.Object);

            //act
            var resultado = handle.Execute(comando);

            //assert
            mockLog.Verify(l => l.Log(
                    LogLevel.Error, //nível do Log - LogError
                    It.IsAny<EventId>(),
                    It.Is<object>((v, t) => true),
                    excecaoEsperada,
                    It.Is<Func<object, Exception, string>>((v, t) => true)
                    ), Times.Once()
                );

        }



        delegate void CapturaMensagemLog(LogLevel level, EventId eventId, object state, Exception exception, Func<object, Exception, string> function);

        [Fact]
        public void TestarLogs()
        {
            //arrange
            var titulo = "Estudar XUnit";
            var comando = new CadastraTarefa(titulo, new Categoria("Estudo"), new DateTime(2022, 12, 31));

            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;

            LogLevel levelCapturado = LogLevel.Warning;
            string mensagemCapturada = string.Empty;

            CapturaMensagemLog captura = (level, eventId, state, exception, func) =>
            {
                levelCapturado = level;
                mensagemCapturada = func(state, exception);
            };

            var mockLog = new Mock<ILogger<CadastraTarefaHandler>>();
            mockLog.Setup(l => l.Log(
                It.IsAny<LogLevel>(), //nível do Log 
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()
                )).Callback(captura);

            var handle = new CadastraTarefaHandler(repo, mockLog.Object);

            //act
            handle.Execute(comando);

            //assert
            Assert.Equal(LogLevel.Warning, levelCapturado);
            Assert.Contains(titulo, mensagemCapturada);
        }



        [Fact]
        public void QuandoExecuteForChamadoInsereTarefas()
        {
            //arrange
            var categ = new Categoria("Dummy");
            var tarefas = new List<Tarefa>
            {
                new Tarefa(100, "tarefa A", categ, new DateTime(2020-12-31), null, StatusTarefa.Criada),
                new Tarefa(105, "tarefa B", categ, new DateTime(2020-12-30), null, StatusTarefa.Criada),
                new Tarefa(107, "tarefa C", categ, new DateTime(2020-12-25), null, StatusTarefa.Criada)
            };

            var mock = new Mock<IRepositorioTarefas>();
            mock.Setup(r => r.ObtemTarefas(It.IsAny<Func<Tarefa, bool>>()))
                .Returns(tarefas);
            var repo = mock.Object;

            var comando = new GerenciaPrazoDasTarefas();
            var handle = new GerenciaPrazoDasTarefasHandler(repo);

            //act
            handle.Execute(comando);

            //assert
            //verifica quantas vezes o método foi chamado nessa execução
            //mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Exactly(3));
            mock.Verify(r => r.AtualizarTarefas(It.IsAny<Tarefa[]>()), Times.Once());
        }

        [Fact]
        public void QuantasVezesFoiChamado()
        {
            //arrange
            var idCategoria = 20;
            var mock = new Mock<IRepositorioTarefas>();
            var repo = mock.Object;

            var comando = new ObtemCategoriaPorId(idCategoria);
            var handle = new ObtemCategoriaPorIdHandler(repo);

            //act
            handle.Execute(comando);

            //assert
            mock.Verify(r => r.ObtemCategoriaPorId(idCategoria), Times.Once());
        }
    }
}
