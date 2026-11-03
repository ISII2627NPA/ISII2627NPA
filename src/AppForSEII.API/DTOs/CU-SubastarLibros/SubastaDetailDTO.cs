using System;
using System.Collections.Generic;
using AppForSEII.API.Models;

namespace AppForSEII.API.DTOs
{
    
    // Esta clase representa los datos detallados de una subasta que se enviarán al cliente (Frontend).
    // Hereda (:) de 'SubastaForCreateDTO', lo que significa que automáticamente tiene todas sus propiedades 
    // (UsuarioUserName, Nombre, Apellidos, Telefono, MetodoPago y SubastaItems) sin tener que escribirlas de nuevo.
    public class SubastaDetailDTO:SubastaForCreateDTO
    {
        // 1. Constructor: Se encarga de inicializar el objeto cuando lo creamos (usando 'new SubastaDetailDTO(...)').
        // Recibe como parámetros tanto los datos exclusivos del Detail (id, fecha, precio total) 
        // como los datos básicos que necesita la clase padre (SubastaForCreateDTO).
        public SubastaDetailDTO(int id, string usuarioUserName, string nombre, string apellidos, string telefono, 
            DateTime fechaSubasta, decimal precioSubasta, int metodoPago, 
            IList<SubastaItemDTO> subastaItems)
            // 'base(...)' llama al constructor de la clase padre (SubastaForCreateDTO) y le pasa sus datos correspondientes
            // para que ella se encargue de inicializarlos.
            : base(usuarioUserName,
                   nombre,
                   apellidos,
                   telefono,
                   metodoPago,
                   subastaItems)
        {
            // Inicializamos las propiedades exclusivas de esta clase con los valores recibidos por parámetro.
            Id = id;
            FechaSubasta = fechaSubasta;
            PrecioSubasta = precioSubasta;
        }
        // 2. Propiedades exclusivas del Detail (las que no tiene el DTO de creación)

        public int Id { get; set; }   // Identificador único de la subasta (generado por la base de datos).

        public DateTime FechaSubasta { get; set; }   // Fecha y hora en la que se realizó la subasta.

        public decimal PrecioSubasta { get; set; }    // Precio total de la subasta (la suma de las pujas de todos los items).

        // 3. Sobreescritura del método Equals: 
        // Sirve para que los tests unitarios (Assert.Equal) sepan cómo comparar dos objetos de este tipo y decidir si son iguales.
        public override bool Equals(object? obj)
        {
            // a) Comprueba que el objeto a comparar ('obj') no sea nulo y sea de tipo 'SubastaDetailDTO'.
            // Si lo es, lo guarda en la variable 'dTO'.
            return obj is SubastaDetailDTO dTO &&
                   // b) Llama al Equals de la clase padre (SubastaForCreateDTO) para comprobar que los datos básicos 
                   // (Nombre, Apellidos, etc.) también coincidan.
                   base.Equals(obj) &&
                   // c) Compara la fecha de la subasta. Usa un método 'CompareDate' (probablemente heredado de alguna clase base tuya) 
                   // porque las fechas en C# a veces fallan al compararse directamente por culpa de los milisegundos.
                   CompareDate(FechaSubasta, dTO.FechaSubasta) &&
                   // d) Compara que el precio total sea exactamente el mismo.
                   PrecioSubasta == dTO.PrecioSubasta &&
                   // e) Compara que el ID coincida.
                   Id == dTO.Id;   
        }
        // 4. Sobreescritura del método GetHashCode:
        // Siempre que se sobreescribe el 'Equals', es obligatorio sobreescribir el 'GetHashCode'.
        // Genera un número entero único (código hash) para este objeto basado en sus datos.
        public override int GetHashCode()
        {
            // Combina el código hash de la clase padre con el ID y la Fecha de esta clase.
            // Esto es muy útil cuando se guardan estos objetos en diccionarios o listas HashSet para búsquedas rápidas.
            return HashCode.Combine(base.GetHashCode(), Id, FechaSubasta);
            
        }
    }
    
}