using Cine.Core;
using Xunit;

namespace Cine.Tests;

public class BilheteriaTests
{
    private const TipoDeIngresso Inteira = TipoDeIngresso.Inteira;
    private const TipoDeIngresso Estudante = TipoDeIngresso.MeiaEstudante;
    private const TipoDeIngresso Idoso = TipoDeIngresso.MeiaIdoso;
    private const TipoDeIngresso Pcd = TipoDeIngresso.MeiaPcd;
    private const TipoDeIngresso Acompanhante = TipoDeIngresso.MeiaAcompanhantePcd;

    [Fact]
    public void Comprar_UmaInteira_CobraOPrecoCheioEReduzOsLugares()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Inteira]);

        Assert.True(resultado.Sucesso);
        Assert.Equal(32.00m, resultado.Total);
        Assert.Equal(1, bilheteria.Vendidos);
        Assert.Equal(9, bilheteria.Disponiveis);
    }

    [Fact]
    public void Comprar_InteiraEMeia_SomaOsPrecosDeCadaIngresso()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Inteira, Estudante, Idoso]);

        Assert.True(resultado.Sucesso);
        Assert.Equal(32.00m + 16.00m + 16.00m, resultado.Total);
        Assert.Equal(3, resultado.Ingressos.Count);
        Assert.Equal(64.00m, bilheteria.Arrecadado);
    }

    [Fact]
    public void Comprar_PrecoComMeioCentavo_ArredondaIngressoPorIngresso()
    {
        var bilheteria = NovaBilheteria(lotacao: 10, precoInteira: 31.55m);

        var resultado = bilheteria.Comprar([Estudante, Estudante]);

        Assert.Equal(15.78m + 15.78m, resultado.Total);
    }

    [Fact]
    public void Comprar_MeiasAteACota_Aceita()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Estudante, Estudante, Estudante, Estudante]);

        Assert.True(resultado.Sucesso);
        Assert.Equal(4, bilheteria.MeiasNaCota);
        Assert.Equal(0, bilheteria.CotaRestante);
    }

    [Fact]
    public void Comprar_MeiasAlemDaCotaEmUmaCompra_RecusaTudoSemVenderNada()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Estudante, Estudante, Estudante, Estudante, Estudante]);

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.CotaDeMeiaEsgotada, resultado.Motivo);
        Assert.Equal(0, bilheteria.Vendidos);
        Assert.Equal(0m, bilheteria.Arrecadado);
    }

    [Fact]
    public void Comprar_CotaConsumidaEmComprasAnteriores_RecusaNovaMeia()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);
        bilheteria.Comprar([Estudante, Estudante, Estudante]);

        var recusada = bilheteria.Comprar([Estudante, Estudante]);
        var aceita = bilheteria.Comprar([Estudante]);

        Assert.False(recusada.Sucesso);
        Assert.Equal(MotivoDaRecusa.CotaDeMeiaEsgotada, recusada.Motivo);
        Assert.True(aceita.Sucesso);
    }

    [Fact]
    public void Comprar_CompraMistaQueEstouraACota_RecusaAInteiraTambem()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);
        bilheteria.Comprar([Estudante, Estudante, Estudante]);

        var resultado = bilheteria.Comprar([Inteira, Estudante, Estudante]);

        Assert.False(resultado.Sucesso);
        Assert.Equal(3, bilheteria.Vendidos);
    }

    [Fact]
    public void Comprar_Idoso_NaoConsomeACotaDasMeias()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);
        bilheteria.Comprar([Estudante, Estudante, Estudante, Estudante]);

        var idoso = bilheteria.Comprar([Idoso, Idoso]);
        var estudante = bilheteria.Comprar([Estudante]);

        Assert.True(idoso.Sucesso);
        Assert.False(estudante.Sucesso);
        Assert.Equal(4, bilheteria.MeiasNaCota);
    }

    [Fact]
    public void Comprar_SessaoPequenaComCotaZero_RecusaQualquerMeiaDaCota()
    {
        var bilheteria = NovaBilheteria(lotacao: 2);

        var resultado = bilheteria.Comprar([Estudante]);

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.CotaDeMeiaEsgotada, resultado.Motivo);
    }

    [Fact]
    public void Comprar_MaisIngressosQueLugares_RecusaPorFaltaDeLugares()
    {
        var bilheteria = NovaBilheteria(lotacao: 3);

        var resultado = bilheteria.Comprar([Inteira, Inteira, Inteira, Inteira]);

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.SemLugares, resultado.Motivo);
        Assert.Equal(0, bilheteria.Vendidos);
    }

    [Fact]
    public void Comprar_SessaoLotada_RecusaNovaCompra()
    {
        var bilheteria = NovaBilheteria(lotacao: 3);
        bilheteria.Comprar([Inteira, Inteira, Inteira]);

        var resultado = bilheteria.Comprar([Inteira]);

        Assert.Equal(MotivoDaRecusa.SemLugares, resultado.Motivo);
        Assert.Equal(0, bilheteria.Disponiveis);
    }

    [Fact]
    public void Comprar_SemLugaresECotaEstourada_InformaPrimeiroAFaltaDeLugares()
    {
        var bilheteria = NovaBilheteria(lotacao: 2);

        var resultado = bilheteria.Comprar([Estudante, Estudante, Estudante]);

        Assert.Equal(MotivoDaRecusa.SemLugares, resultado.Motivo);
    }

    [Fact]
    public void Comprar_PcdComAcompanhante_AceitaEAmbosConsomemCota()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Pcd, Acompanhante]);

        Assert.True(resultado.Sucesso);
        Assert.Equal(32.00m, resultado.Total);
        Assert.Equal(2, bilheteria.MeiasNaCota);
    }

    [Fact]
    public void Comprar_AcompanhanteSemPcdNaCompra_Recusa()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Acompanhante, Inteira]);

        Assert.False(resultado.Sucesso);
        Assert.Equal(MotivoDaRecusa.AcompanhanteSemPcd, resultado.Motivo);
    }

    [Fact]
    public void Comprar_DoisAcompanhantesParaUmPcd_Recusa()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);

        var resultado = bilheteria.Comprar([Pcd, Acompanhante, Acompanhante]);

        Assert.Equal(MotivoDaRecusa.AcompanhanteSemPcd, resultado.Motivo);
        Assert.Equal(0, bilheteria.Vendidos);
    }

    [Fact]
    public void Comprar_PoliticaPersonalizada_UsaOPercentualInformado()
    {
        var politica = new PoliticaDeMeiaEntrada(0.50m, [TipoDeIngresso.MeiaEstudante]);
        var bilheteria = new Bilheteria(new Sessao("Filme", 32m, 10), politica);

        var resultado = bilheteria.Comprar([Estudante, Estudante, Estudante, Estudante, Estudante]);

        Assert.True(resultado.Sucesso);
        Assert.Equal(0, bilheteria.CotaRestante);
    }

    [Fact]
    public void Comprar_ListaVazia_LancaArgumentException()
    {
        var bilheteria = NovaBilheteria(lotacao: 10);
        Action acao = () => bilheteria.Comprar([]);

        Assert.Throws<ArgumentException>(acao);
    }

    private static Bilheteria NovaBilheteria(int lotacao, decimal precoInteira = 32.00m) =>
        new(new Sessao("Filme", precoInteira, lotacao));
}
