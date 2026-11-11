using Cine.Core;
using Xunit;

namespace Cine.Tests;

public class PoliticaDeMeiaEntradaTests
{
    [Theory]
    [InlineData(100, 40)]
    [InlineData(50, 20)]
    [InlineData(10, 4)]
    [InlineData(7, 2)]
    [InlineData(2, 0)]
    public void CotaMaxima_PoliticaPadrao_EQuarentaPorCentoArredondadoParaBaixo(int lotacao, int esperada)
    {
        var cota = PoliticaDeMeiaEntrada.Padrao.CotaMaxima(lotacao);

        Assert.Equal(esperada, cota);
    }

    [Theory]
    [InlineData(TipoDeIngresso.Inteira, false)]
    [InlineData(TipoDeIngresso.MeiaIdoso, false)]
    [InlineData(TipoDeIngresso.MeiaEstudante, true)]
    [InlineData(TipoDeIngresso.MeiaPcd, true)]
    [InlineData(TipoDeIngresso.MeiaAcompanhantePcd, true)]
    [InlineData(TipoDeIngresso.MeiaJovemBaixaRenda, true)]
    public void ConsomeCota_PoliticaPadrao_IdosoEInteiraFicamDeFora(TipoDeIngresso tipo, bool esperado)
    {
        var consome = PoliticaDeMeiaEntrada.Padrao.ConsomeCota(tipo);

        Assert.Equal(esperado, consome);
    }

    [Fact]
    public void Construtor_PoliticaPersonalizada_RespeitaPercentualETipos()
    {
        var politica = new PoliticaDeMeiaEntrada(0.50m, [TipoDeIngresso.MeiaEstudante, TipoDeIngresso.MeiaIdoso]);

        Assert.Equal(5, politica.CotaMaxima(10));
        Assert.True(politica.ConsomeCota(TipoDeIngresso.MeiaIdoso));
        Assert.False(politica.ConsomeCota(TipoDeIngresso.MeiaPcd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Construtor_PercentualForaDeZeroAUm_LancaArgumentOutOfRange(double percentual)
    {
        Action acao = () => _ = new PoliticaDeMeiaEntrada((decimal)percentual, [TipoDeIngresso.MeiaEstudante]);

        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void Construtor_InteiraNaCota_LancaArgumentException()
    {
        Action acao = () => _ = new PoliticaDeMeiaEntrada(0.40m, [TipoDeIngresso.Inteira]);

        Assert.Throws<ArgumentException>(acao);
    }
}
