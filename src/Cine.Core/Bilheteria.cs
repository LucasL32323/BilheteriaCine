namespace Cine.Core;

/// <summary>Bilheteria de uma sessão: controla lugares, cota de meia-entrada e valores cobrados.</summary>
public sealed class Bilheteria
{
    private readonly Sessao _sessao;
    private readonly PoliticaDeMeiaEntrada _politica;

    public Bilheteria(Sessao sessao, PoliticaDeMeiaEntrada? politica = null)
    {
        ArgumentNullException.ThrowIfNull(sessao);

        _sessao = sessao;
        _politica = politica ?? PoliticaDeMeiaEntrada.Padrao;
    }

    public int Vendidos { get; private set; }

    public int Disponiveis => _sessao.Lotacao - Vendidos;

    public int MeiasNaCota { get; private set; }

    public int CotaRestante => Math.Max(0, _politica.CotaMaxima(_sessao.Lotacao) - MeiasNaCota);

    public decimal Arrecadado { get; private set; }

    /// <summary>
    /// Vende todos os ingressos da compra ou nenhum. A verificação segue a ordem:
    /// lugares → acompanhante de PCD → cota de meia-entrada.
    /// </summary>
    public ResultadoDaCompra Comprar(IReadOnlyCollection<TipoDeIngresso> ingressos)
    {
        ArgumentNullException.ThrowIfNull(ingressos);
        if (ingressos.Count == 0)
        {
            throw new ArgumentException("A compra precisa ter ao menos um ingresso.", nameof(ingressos));
        }

        if (ingressos.Count > Disponiveis)
        {
            return ResultadoDaCompra.Recusada(MotivoDaRecusa.SemLugares);
        }

        var pessoasComDeficiencia = ingressos.Count(tipo => tipo == TipoDeIngresso.MeiaPcd);
        var acompanhantes = ingressos.Count(tipo => tipo == TipoDeIngresso.MeiaAcompanhantePcd);
        if (acompanhantes > pessoasComDeficiencia)
        {
            return ResultadoDaCompra.Recusada(MotivoDaRecusa.AcompanhanteSemPcd);
        }

        var meiasDaCota = ingressos.Count(_politica.ConsomeCota);
        if (meiasDaCota > CotaRestante)
        {
            return ResultadoDaCompra.Recusada(MotivoDaRecusa.CotaDeMeiaEsgotada);
        }

        var vendidos = ingressos
            .Select(tipo => new IngressoVendido(tipo, _sessao.PrecoDe(tipo)))
            .ToList();

        Vendidos += vendidos.Count;
        MeiasNaCota += meiasDaCota;
        Arrecadado += vendidos.Sum(ingresso => ingresso.Preco);

        return ResultadoDaCompra.Vendida(vendidos);
    }
}
