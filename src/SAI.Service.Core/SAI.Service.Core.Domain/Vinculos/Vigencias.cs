namespace SAI.Service.Core.Domain.Vinculos;

/// <summary>
/// Regla de exclusividad temporal de los vínculos (I-1, I-2, I-4): dos vínculos con la misma
/// <see cref="IVinculoTemporal.ClaveExclusividad"/> no pueden solapar en el tiempo. Como dos
/// intervalos abiertos siempre solapan, la regla implica también "a lo sumo uno vigente" por
/// clave. Es el único proceso escritor quien la aplica antes de abrir un vínculo nuevo.
/// </summary>
public static class Vigencias
{
    /// <summary>
    /// Verdadero si <paramref name="nuevo"/> puede coexistir con los <paramref name="existentes"/>
    /// sin violar la exclusividad temporal: ningún vínculo con la misma clave se solapa con él.
    /// </summary>
    public static bool AdmiteNuevo<T>(T nuevo, IEnumerable<T> existentes)
        where T : IVinculoTemporal
    {
        ArgumentNullException.ThrowIfNull(nuevo);
        ArgumentNullException.ThrowIfNull(existentes);

        return existentes
            .Where(e => e.ClaveExclusividad == nuevo.ClaveExclusividad)
            .All(e => !e.Vigencia.Solapa(nuevo.Vigencia));
    }
}
