using FluentAssertions;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Sesión de ejercicio guiado (P-7): registra intención y momento, no el progreso. Su ciclo de vida es
/// simple —se abre, y se cierra por completarse o por abandono— y no admite cerrarse dos veces.
/// </summary>
public class SesionEjercicioTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void IniciarDejaLaSesionEnCurso()
    {
        var sesion = SesionEjercicio.Iniciar("ejercicio-1", Ahora);

        sesion.EnCurso.Should().BeTrue();
        sesion.Estado.Should().Be(EstadoSesionEjercicio.EnCurso);
        sesion.IniciadaEn.Should().Be(Ahora);
        sesion.FinalizadaEn.Should().BeNull();
    }

    [Fact]
    public void CompletarCierraLaSesion()
    {
        var sesion = SesionEjercicio.Iniciar("ejercicio-1", Ahora);

        sesion.Completar(Ahora.AddHours(1));

        sesion.Estado.Should().Be(EstadoSesionEjercicio.Completada);
        sesion.EnCurso.Should().BeFalse();
        sesion.FinalizadaEn.Should().Be(Ahora.AddHours(1));
    }

    [Fact]
    public void AbandonarCierraLaSesion()
    {
        var sesion = SesionEjercicio.Iniciar("ejercicio-1", Ahora);

        sesion.Abandonar(Ahora.AddMinutes(10));

        sesion.Estado.Should().Be(EstadoSesionEjercicio.Abandonada);
        sesion.EnCurso.Should().BeFalse();
    }

    [Fact]
    public void UnaSesionCerradaNoSeVuelveACerrar()
    {
        var sesion = SesionEjercicio.Iniciar("ejercicio-1", Ahora);
        sesion.Completar(Ahora);

        var acto = () => sesion.Abandonar(Ahora);

        acto.Should().Throw<InvalidOperationException>();
    }
}
