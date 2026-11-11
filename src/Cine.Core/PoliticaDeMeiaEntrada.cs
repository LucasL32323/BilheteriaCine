namespace Cine.Core;

/// <summary>
/// Regra da cota de meia-entrada. O padrão segue o art. 1º, §10, da Lei 12.933/2013: no máximo 40% dos ingressos
/// da sessão como meia para estudantes, jovens de baixa renda e pessoas com deficiência (e acompanhante).
/// A meia do idoso vem do Estatuto da Pessoa Idosa e, neste modelo, não consome a cota.
/// </summary>
public sealed record PoliticaDeMeiaEntrada
{
    public static PoliticaDeMeiaEntrada Padrao { get; } = new(
        0.40m,
        [
            TipoDeIngresso.MeiaEstudante,
            TipoDeIngresso.MeiaPcd,
            TipoDeIngresso.MeiaAcompanhantePcd,
            TipoDeIngresso.MeiaJovemBaixaRenda,
        ]);

    private readonly HashSet<TipoDeIngresso> _tiposNaCota;

    public PoliticaDeMeiaEntrada(decimal percentualDaCota, IEnumerable<TipoDeIngresso> tiposNaCota)
    {
        ArgumentNullException.ThrowIfNull(tiposNaCota);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(percentualDaCota, 0m);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percentualDaCota, 1m);

        _tiposNaCota = [.. tiposNaCota];
        if (_tiposNaCota.Contains(TipoDeIngresso.Inteira))
        {
            throw new ArgumentException("A inteira não pode consumir a cota de meia-entrada.", nameof(tiposNaCota));
        }

        PercentualDaCota = percentualDaCota;
    }

    public decimal PercentualDaCota { get; }

    /// <summary>Quantidade máxima de meias da cota em uma sessão (arredondada para baixo).</summary>
    public int CotaMaxima(int lotacao) => (int)Math.Floor(lotacao * PercentualDaCota);

    public bool ConsomeCota(TipoDeIngresso tipo) => _tiposNaCota.Contains(tipo);
}
