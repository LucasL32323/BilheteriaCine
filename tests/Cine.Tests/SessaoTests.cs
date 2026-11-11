using Cine.Core;
using Xunit;

namespace Cine.Tests;

public class SessaoTests
{
    public static TheoryData<TipoDeIngresso, decimal, decimal> Precos => new()
    {
        { TipoDeIngresso.Inteira, 32.00m, 32.00m },
        { TipoDeIngresso.MeiaEstudante, 32.00m, 16.00m },
        { TipoDeIngresso.MeiaIdoso, 25.00m, 12.50m },
        { TipoDeIngresso.MeiaPcd, 32.00m, 16.00m },
        { TipoDeIngresso.MeiaAcompanhantePcd, 32.00m, 16.00m },
        { TipoDeIngresso.MeiaJovemBaixaRenda, 32.00m, 16.00m },
        { TipoDeIngresso.MeiaEstudante, 31.55m, 15.78m },
        { TipoDeIngresso.MeiaIdoso, 0.05m, 0.03m },
    };

    [Theory]
    [MemberData(nameof(Precos))]
    public void PrecoDe_TipoDeIngresso_AplicaMeiaEArredondaParaCentavo(TipoDeIngresso tipo, decimal precoInteira, decimal esperado)
    {
        var sessao = new Sessao("Filme", precoInteira, 100);

        var preco = sessao.PrecoDe(tipo);

        Assert.Equal(esperado, preco);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-5, 100)]
    [InlineData(32, 0)]
    [InlineData(32, -1)]
    public void Construtor_PrecoOuLotacaoNaoPositivos_LancaArgumentOutOfRange(int preco, int lotacao)
    {
        Action acao = () => _ = new Sessao("Filme", preco, lotacao);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void Construtor_PrecoComMaisDeDuasCasas_LancaArgumentOutOfRange()
    {
        Action acao = () => _ = new Sessao("Filme", 32.505m, 100);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void Construtor_FilmeEmBranco_LancaArgumentException()
    {
        Action acao = () => _ = new Sessao("  ", 32m, 100);

        Assert.Throws<ArgumentException>(acao);
    }
}
