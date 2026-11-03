using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace AppForSEII.API.DTOs
{
    // Esta clase es un DTO (Data Transfer Object) utilizado para recoger los datos que el usuario 
    // envía desde el frontend al backend cuando quiere crear una nueva subasta.
    public class SubastaForCreateDTO
    {
        // 1. Constructor Parametrizado: Obliga a que al instanciar el objeto mediante código (ej. en tus tests), 
        // se pasen todos los datos obligatorios. Usa "throw new ArgumentNullException" como medida 
        // defensiva para que la aplicación falle rápido e indique el error exacto si se intentan pasar valores nulos.
        public SubastaForCreateDTO(string usuarioUserName, string nombre, string apellidos, string telefono, int metodoPago, IList<SubastaItemDTO> subastaItems)
        {
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            MetodoPago = metodoPago;
            
            
            SubastaItems = subastaItems ?? throw new ArgumentNullException(nameof(subastaItems));
        }
        // 2. Constructor Vacío: Es fundamental para que funcione ASP.NET Core. 
        // Cuando el controlador recibe un JSON en el POST, el framework usa este constructor para crear el objeto vacío 
        // y luego va inyectando las propiedades. Inicializa la lista para evitar errores "NullReferenceException".
        public SubastaForCreateDTO()
        {
            
            SubastaItems = new List<SubastaItemDTO>();
        }
        // 3. Propiedades y Data Annotations (Validaciones Automáticas):
        // Estas etiquetas entre corchetes [ ] le dicen a ASP.NET que valide el formato automáticamente.
        // Si el usuario envía algo que incumple esto, el ModelState falla y el controlador devuelve BadRequest.

        [EmailAddress]   // Valida que el texto tenga formato válido de correo electrónico (ej. nombre@dominio.com).
        [Required]   // Hace que este campo sea obligatorio en la petición.
        public string UsuarioUserName { get; set; } = "peter.jackson@uclm.es";    // Se le asigna un valor por defecto.

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]   // Fuerza la regla de negocio de tamaño.
        public string Nombre { get; set; } = "Peter";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tus apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
        public string Apellidos { get; set; } = "Jackson";

        [Phone]    // Valida el formato básico de un número de teléfono.
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "666555444";

        [Required]
        public int MetodoPago { get; set; }

        // Relación: Contiene la lista de los libros individuales que el usuario quiere añadir a la subasta.
        public IList<SubastaItemDTO> SubastaItems { get; set; } = new List<SubastaItemDTO>();

        // 4. Propiedad Calculada (Read-Only): 
        // No tiene un bloque 'set' porque su valor no se guarda físicamente desde fuera. Se calcula al vuelo 
        // usando LINQ (.Sum) para sumar el 'PrecioPuja' de cada elemento dentro de la lista de items.
        public decimal PrecioTotal
        {
            get
            {
                
                return SubastaItems.Sum(si => si.PrecioPuja);
                
            }
        }

        // 5. Método Auxiliar Protegido (protected): 
        // Al ser "protected", solo puede usarse en esta clase o en sus hijas (como SubastaDetailDTO).
        // Compara si la diferencia entre dos fechas es menor a 1 minuto, para evitar que los tests fallen 
        // al comparar objetos simplemente por una pequeña fracción de milisegundos de diferencia al guardarlos.

        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }
        // 6. Sobreescritura del método Equals:
        // Le indica a tus tests unitarios (y al sistema en general) cuándo dos DTOs se consideran exactamente iguales.
        public override bool Equals(object? obj)
        {
            return obj is SubastaForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Telefono == dTO.Telefono &&
                   MetodoPago == dTO.MetodoPago &&
                   // .SequenceEqual es MUY importante: Comprueba el contenido profundo de la lista. 
                   // Valida que ambas listas tengan los mismos elementos y en el mismo orden, 
                   // en lugar de comprobar simplemente si son la misma referencia de lista en memoria.
                   SubastaItems.SequenceEqual(dTO.SubastaItems) &&
                   PrecioTotal == dTO.PrecioTotal;
        }
        // 7. Sobreescritura de GetHashCode:
        // Obligatorio al sobreescribir Equals. Genera un identificador numérico (hash) único basado 
        // en las propiedades principales del objeto para operaciones de búsqueda o agrupado en memoria.
        public override int GetHashCode()
        {
    
            return HashCode.Combine(UsuarioUserName, Nombre, Apellidos, Telefono, MetodoPago);
        }
    }
}