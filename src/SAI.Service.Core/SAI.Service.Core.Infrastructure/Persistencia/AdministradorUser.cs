using Microsoft.AspNetCore.Identity;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Usuario de ASP.NET Core Identity para el <b>administrador único</b> del servicio
/// (ADR-16). El sistema es monousuario por diseño: existe a lo sumo una fila en
/// <c>AspNetUsers</c>. No se agregan campos propios en la Etapa 1; se deja el tipo
/// derivado para poder extenderlo sin una nueva migración de identidad de base.
/// </summary>
public class AdministradorUser : IdentityUser
{
}
