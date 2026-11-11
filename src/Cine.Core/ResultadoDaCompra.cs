namespace Cine.Core;

public enum MotivoDaRecusa
{
    Nenhum,
    SemLugares,
    CotaDeMeiaEsgotada,
    AcompanhanteSemPcd,
}

public readonly record struct IngressoVendido(TipoDeIngresso Tipo, decimal Preco);

/// <summary>Resultado de uma compra: tudo ou nada. Em caso de recusa nenhum ingresso é vendido.</summary>
public sealed record ResultadoDaCompra
{
    private ResultadoDaCompra(bool sucesso, MotivoDaRecusa motivo, IReadOnlyList<IngressoVendido> ingressos)
    {
        Sucesso = sucesso;
        Motivo = motivo;
        Ingressos = ingressos;
    }

    public bool Sucesso { get; }

    public MotivoDaRecusa Motivo { get; }

    public IReadOnlyList<IngressoVendido> Ingressos { get; }

    public decimal Total => Ingressos.Sum(ingresso => ingresso.Preco);

    internal static ResultadoDaCompra Vendida(IReadOnlyList<IngressoVendido> ingressos) =>
        new(true, MotivoDaRecusa.Nenhum, ingressos);

    internal static ResultadoDaCompra Recusada(MotivoDaRecusa motivo) =>
        new(false, motivo, []);
}
