using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AppForSEII.API.Models; 

namespace AppForSEII.API.DTOs
{
    // Objeto de Transferencia de Datos (DTO) usado para la CREACIÓN de una nueva compra.
    // Contiene los datos que el cliente envía al servidor para procesar el pedido.
    public class CompraForCreateDTO
    {
        // Constructor parametrizado.
        // Usamos el operador '??' como escudo: si nos envían un dato vacío (null), 
        // el 'throw' frena el programa en seco lanzando un error para evitar fallos futuros.           
        public CompraForCreateDTO(string usuarioUserName, string nombre, string apellidos, string direccion, string telefono, int metodoPagoId, string? codigoDescuento , IList<CompraItemDTO> compraItems)
        {
            UsuarioUserName = usuarioUserName ?? throw new ArgumentNullException(nameof(usuarioUserName));
            Nombre = nombre ?? throw new ArgumentNullException(nameof(nombre));
            Apellidos = apellidos ?? throw new ArgumentNullException(nameof(apellidos));
            Direccion = direccion ?? throw new ArgumentNullException(nameof(direccion));
            Telefono = telefono ?? throw new ArgumentNullException(nameof(telefono));
            //Admite nulos ya que al ser un int, no puede ser null, porque le pone 0 por defecto.
            MetodoPagoId = metodoPagoId;
            //Admite nulos ya que el ciente puede no poner un codigo de descuento
            CodigoDescuento = codigoDescuento;
            
           
             CompraItems = compraItems ?? throw new ArgumentNullException(nameof(compraItems));
        }

        // Constructor vacío obligatorio para que el framework pueda deserializar el JSON entrante.
        // Inicializamos la lista a una nueva instancia vacía para evitar errores de tipo NullReferenceException.           
        public CompraForCreateDTO()
        {
           
            CompraItems = new List<CompraItemDTO>();
        }
        // [EmailAddress] verifica automáticamente que el texto tenga el formato correcto de un correo electrónico (que contenga un '@' y un dominio).
        // [Required] hace que este campo sea obligatorio. Si no lo envían, la API rechaza la petición.
        [EmailAddress]
        [Required]
        public string UsuarioUserName { get; set; } = "peter.jackson@uclm.es";

        // AllowEmptyStrings = false impide que el usuario envíe un campo vacío (ej. "").
        // StringLength define los límites: el nombre no puede superar los 50 caracteres ni tener menos de 2. 
        // Si falla, lanza automáticamente el mensaje definido en "ErrorMessage".
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = "Peter";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tus apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener al menos 2 caracteres")]
        public string Apellidos { get; set; } = "Jackson";


        // DataType.MultilineText es una pista visual. Le dice a los sistemas que consuman esta API que este texto es largo, 
        // para que en la interfaz dibujen un cuadro de texto grande (textarea) en lugar de una línea simple.
        // Además, exige que la dirección tenga al menos 10 letras para asegurar que es una dirección real y no solo "Calle".
        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu dirección para el envío")]
        public string Direccion { get; set; } = "Albacete";


        // [Phone] revisa que el texto tenga formato telefónico válido (permite números, el signo '+', espacios o guiones).
        [Phone]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Por favor, establece tu teléfono")]
        public string Telefono { get; set; } = "666555444";

        // Obliga a enviar el ID del método de pago. Al ser un 'int', si no lo envían, C# intentará poner un 0, 
        // pero la etiqueta Required ayuda a formalizar que es un campo exigido por la API.
        [Required]
        public int MetodoPagoId { get; set; } 

        // Al no tener la etiqueta [Required], este campo es OPCIONAL (por eso lleva el string?).
        // PERO si el cliente decide enviarlo, el código que envíe tiene que tener obligatoriamente entre 5 y 10 caracteres.
        [StringLength(10, MinimumLength = 5, ErrorMessage = "El código de descuento debe tener entre 5 y 10 caracteres")]
        public string? CodigoDescuento { get; set; }


        // Representa los artículos del pedido.
        // Se inicializa por defecto como una lista vacía (= new List...) para evitar que 
        // la variable sea 'null' y provoque errores al intentar contar o recorrer sus elementos.
         public IList<CompraItemDTO> CompraItems { get; set; } = new List<CompraItemDTO>();


        // Propiedad declarada aquí para que también sea heredada por CompraDetailDTO.
        // No se pide en el constructor porque en la creación el servidor es el encargado de calcular este valor.
        public decimal PrecioTotal { get; set; } 


        // Método auxiliar para evitar que las pruebas fallen por diferencias de milisegundos en las fechas.
        // Se declara 'protected' para que las clases hijas (como CompraDetailDTO) también puedan utilizarlo.
        protected bool CompareDate(DateTime date1, DateTime date2)
        {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }

        // Sobrescritura de Equals para las pruebas unitarias.
        // Compara campo por campo. Para las listas (CompraItems), usa 'SequenceEqual' para comprobar 
        // que ambas listas tienen los mismos elementos y en el mismo orden.
        public override bool Equals(object? obj)
        {
            return obj is CompraForCreateDTO dTO &&
                   UsuarioUserName == dTO.UsuarioUserName &&
                   Nombre == dTO.Nombre &&
                   Apellidos == dTO.Apellidos &&
                   Direccion == dTO.Direccion &&
                   Telefono == dTO.Telefono &&
                   MetodoPagoId == dTO.MetodoPagoId &&
                   CodigoDescuento == dTO.CodigoDescuento &&
                   
                   
                    CompraItems.SequenceEqual(dTO.CompraItems) &&
                   
                   PrecioTotal == dTO.PrecioTotal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), UsuarioUserName, Direccion, Telefono);
        }
    }
}